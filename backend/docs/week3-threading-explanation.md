# Semana 3: Extractor de resumen de audio

## Qué se implementó

Esta semana se agregó el extractor de resúmenes pedido por la actividad. Como
el proyecto todavía no transcribía audio a texto, fue necesario agregar
también un paso previo de transcripción, ya que el resumen necesita texto de
entrada:

1. **Transcripción (audio → texto):** `IAudioTranscriber` /
   `WhisperAudioTranscriber`, usando **Whisper.net** con el modelo `tiny`
   (el más liviano de la familia Whisper), corriendo 100% local. Whisper
   requiere WAV 16kHz mono como entrada, así que el audio original se
   convierte primero con FFmpeg (reutilizando Xabe.FFmpeg, ya presente en el
   proyecto desde la semana 2) antes de pasarlo al modelo.
2. **Resumen (texto → resumen ≤ 50 caracteres):** `ITextSummarizer` /
   `OllamaTextSummarizer`, usando **Ollama** con el modelo `llama3.2:1b`
   corriendo localmente, llamado por HTTP a través de `IHttpClientFactory`
   (`AddHttpClient`). El prompt le pide al modelo un resumen en español de
   máximo 50 caracteres, pero el resultado también se trunca explícitamente
   en código (`OllamaTextSummarizer`) como salvaguarda, porque no se puede
   confiar en que un modelo de lenguaje respete un límite de caracteres solo
   porque se lo pedimos en el prompt.

El modelo `ggml-tiny.bin` de Whisper se descarga automáticamente en el
primer arranque de la API (mismo patrón que ya existía para el binario de
FFmpeg) y se guarda en disco para no volver a descargarlo en cada arranque.

`UploadAudioService` ahora encadena, después de comprimir: transcribir el
audio original y luego resumir esa transcripción, midiendo cada paso con
`Stopwatch` y logueando el tiempo igual que ya se hacía para la compresión.
El resultado (`Summary`, `SummaryTimeMs`, `Transcript`, `TranscriptionTimeMs`)
se persiste en `AudioFile` y se expone en la respuesta de
`POST /api/audio`.

## Por qué modelos livianos y locales

- **Costo:** no hay presupuesto para usar una API de pago (OpenAI, etc.) en
  un proyecto de curso: Whisper `tiny` y Ollama con `llama3.2:1b` corren
  gratis en la máquina local.
- **Tamaño de la tarea:** transcribir audios cortos y generar un resumen de
  50 caracteres no necesita un modelo grande; `tiny` y `1b` son más que
  suficientes y arrancan/responden mucho más rápido que sus variantes
  grandes.
- **Consistencia con el resto del proyecto:** desde la semana 1 el proyecto
  evita dependencias externas de pago y prioriza herramientas que corren
  localmente (FFmpeg también corre local). Mantener todo local evita además
  tener que manejar API keys o conectividad externa en el flujo de subida.

## ¿Es momento de aplicar hilos?

Todavía no, con el mismo criterio del análisis de la semana 2
(`docs/week2-analysis.md`): `UploadAsync` sigue siendo una sola tarea por
request, ejecutada de forma secuencial con `async`/`await` normal, sin
`Thread`, `Task.Run` ni `Parallel.*`. ASP.NET Core ya da concurrencia básica
entre requests distintos usando el thread pool, así que no hace falta
paralelismo manual para eso.

Donde sí empieza a tener sentido pensar en paralelismo es dentro de un mismo
request: comprimir y transcribir el audio son pasos independientes entre sí
(ninguno necesita el resultado del otro), por lo que en teoría podrían
correr en paralelo. Resumir, en cambio, depende directamente del resultado
de la transcripción, así que ese paso no puede paralelizarse con los
anteriores — tiene que esperar a que la transcripción termine.

Introducir hilos ahora, solo para paralelizar compresión + transcripción,
agregaría complejidad (coordinar dos tareas asíncronas, manejar sus
excepciones por separado) por una ganancia de rendimiento modesta en audios
cortos. Es un cambio que tiene más sentido reservar para cuando el curso
explícitamente pida aplicar hilos/paralelismo, evitando además introducir
race conditions sin necesidad real todavía.
