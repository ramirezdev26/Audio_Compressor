# Audio Compressor — Capstone (Programming 7)

Aplicación cliente-servidor para procesar archivos de audio: el usuario sube
un audio desde un cliente web, el servidor lo comprime, lo transcribe y
genera un resumen corto del contenido, todo corriendo con herramientas
locales (sin APIs de pago).

**Autor:** Santiago Ramírez
**Curso:** CSPR-471 Programming 7 — Jala University
**Profesor:** Leopoldo Flores
**Repositorio:** [gitlab.com/jala-university1/cohort-2/ES.CSPR-471.GA.T2.26.M1/SB/santiago.ramirez/capstone](https://gitlab.com/jala-university1/cohort-2/ES.CSPR-471.GA.T2.26.M1/SB/santiago.ramirez/capstone/-/blob/main/README.md?ref_type=heads)

## Caso de uso

Un usuario tiene un archivo de audio (nota de voz, grabación de una idea,
entrevista, etc.) y quiere:
1. Guardarlo de forma comprimida (ahorrar espacio).
2. Obtener rápidamente de qué trata sin tener que escucharlo completo.

La aplicación resuelve esto con un pipeline automático que corre en segundo
plano: al subir el audio, el servidor responde de inmediato y **comprime,
filtra el ruido y transcribe/resume en tres ramas en paralelo**; el resultado
se consulta después desde la lista de audios procesados.

## Demo — Evaluación de Medio Término

- 🎥 [Video demo (3 min)](https://jalauniv-my.sharepoint.com/:v:/g/personal/santiago_ramirez_jala_university/IQAJZYbccH2BSpb6FaB3_avTAZAsK_IPWqkaSRdHM1C2vBI?e=Ai77d6&nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJTdHJlYW1XZWJBcHAiLCJyZWZlcnJhbFZpZXciOiJTaGFyZURpYWxvZy1MaW5rIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXcifX0%3D)
- 📊 Diapositivas: [`docs/midterm-deck.pptx`](./docs/midterm-deck.pptx)
- 🏗️ Diagrama de arquitectura y pipeline: [`docs/architecture.md`](./docs/architecture.md)

## Arquitectura

El proyecto está dividido en dos partes independientes dentro de este mismo
repositorio (monorepo):

```
Audio_Compressor/
├── backend/     API en ASP.NET Core (C#) con Clean Architecture
└── frontend/    Cliente web en React + TypeScript (Vite)
```

**Backend — Clean Architecture (4 capas):**

| Capa | Responsabilidad |
|---|---|
| `AudioProcessor.Domain` | Entidades del negocio (`AudioFile`) |
| `AudioProcessor.Application` | Casos de uso e interfaces (`UploadAudioService`, `IFileStore`, `IAudioRepository`, `IAudioCompressor`, `IAudioFilter`, `IAudioTranscriber`, `ITextSummarizer`) |
| `AudioProcessor.Infrastructure` | Implementaciones concretas: SQLite (EF Core), almacenamiento local de archivos, FFmpeg, Whisper.net, Ollama |
| `AudioProcessor.Api` | Punto de entrada, controllers, Swagger |

**Pipeline de procesamiento** (en segundo plano, 3 ramas en paralelo, ver
`backend/docs/` y [`docs/architecture.md`](./docs/architecture.md)):

```
Cliente → POST /api/audio → guarda el archivo original
                          → responde de inmediato con 202 Accepted { id, message }
                          → (en background) espera un slot libre según el límite
                            de concurrencia (Processing:MaxConcurrentJobs)
                          → al conseguir el slot, corre en paralelo (Parallel.Invoke):
                              • comprime a AAC (FFmpeg)
                              • filtra ruido (FFmpeg, filtro afftdn)
                              • transcribe (Whisper.net, modelo tiny) y luego
                                resume (Ollama, llama3.2:1b, local, ≤ 50 caracteres)
                          → persiste todo en SQLite

Cliente → GET /api/audio → lista de audios procesados (urls original/comprimido/
                            filtrado, tiempos y resumen)
```

Cada rama mide su propio tiempo de ejecución (`Stopwatch`) y lo loguea y
persiste, como base para las mediciones de rendimiento que se piden en el
curso (comparación secuencial vs. paralelo).

El número de jobs que corren al mismo tiempo en segundo plano está limitado
por un `SemaphoreSlim` compartido, configurable con la clave
`Processing:MaxConcurrentJobs` en `appsettings.json` (por defecto
`Environment.ProcessorCount`), para no agotar la RAM de la máquina si llegan
muchas subidas seguidas.

Un diagrama visual de esta arquitectura y del pipeline está disponible en
[`docs/architecture.md`](./docs/architecture.md).

## Cómo correr el proyecto completo

Necesitas el backend y el frontend corriendo al mismo tiempo, en dos
terminales distintas.

### 1. Backend

Requisitos: .NET SDK 8.0, herramienta `dotnet-ef`, y
[Ollama](https://ollama.com) corriendo localmente con el modelo `llama3.2:1b`
descargado.

```bash
cd backend
dotnet restore
dotnet ef database update --project AudioProcessor.Infrastructure --startup-project AudioProcessor.Api
dotnet run --project AudioProcessor.Api
```

La API queda en `http://localhost:5262` (el puerto puede variar, revisa la
consola). FFmpeg y el modelo de Whisper se descargan automáticamente en el
primer arranque si no están presentes.

Instrucciones completas y detalle de endpoints en
[`backend/README.md`](./backend/README.md).

### 2. Frontend

Requisitos: Node.js.

```bash
cd frontend
npm install
cp .env.example .env   # ajusta VITE_API_URL si el backend corre en otro puerto
npm run dev
```

El cliente queda en `http://localhost:5173`.

Instrucciones completas en [`frontend/README.md`](./frontend/README.md).

## Probar la API sin frontend

El backend incluye:
- **Swagger UI**, disponible en `/swagger` cuando corre en modo desarrollo.
- Un archivo [`backend/AudioProcessor.Api/AudioProcessor.Api.http`](./backend/AudioProcessor.Api/AudioProcessor.Api.http)
  con requests listas para ejecutar desde VS Code (extensión REST Client) o
  Rider/Visual Studio.

## Cómo probar el límite de concurrencia

Para que el throttling se note aunque no llegue una carga masiva de subidas,
bajá el valor de `Processing:MaxConcurrentJobs` en
`backend/AudioProcessor.Api/appsettings.json` (por ejemplo a `3`). Con el
backend corriendo, generá varias subidas simultáneas con el script de carga
de k6 en
[`backend/AudioProcessor.Api/load-tests/audio-upload.js`](./backend/AudioProcessor.Api/load-tests/audio-upload.js)
y revisá en la consola del backend los logs `waiting for a processing slot`
/ `acquired a processing slot`: nunca debería haber más jobs corriendo al
mismo tiempo que el límite configurado.

## Documentación adicional

- [`backend/docs/week2-analysis.md`](./backend/docs/week2-analysis.md) — análisis de compresión de audio.
- [`backend/docs/week3-threading-explanation.md`](./backend/docs/week3-threading-explanation.md) — análisis de dónde tendría sentido aplicar hilos en el pipeline.
- [`backend/docs/week4-frontend-analysis.md`](./backend/docs/week4-frontend-analysis.md) — por qué el frontend usa `async/await` sin bloquear el hilo principal.

## Estado del proyecto

En desarrollo activo como parte del Capstone del curso. El pipeline corre en
segundo plano con sus tres ramas (compresión, filtro de ruido y
transcripción/resumen) en paralelo vía `Parallel.Invoke`, y un límite de
concurrencia configurable evita que un burst de subidas agote la RAM de la
máquina bajo carga. Detalle de la evolución semana a semana en
`backend/docs/` y del pipeline actual en
[`docs/architecture.md`](./docs/architecture.md).
