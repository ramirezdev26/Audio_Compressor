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

La aplicación resuelve esto con un pipeline automático: **subir → comprimir
→ transcribir → resumir**, y devuelve el resultado en una sola respuesta.

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
| `AudioProcessor.Application` | Casos de uso e interfaces (`UploadAudioService`, `IFileStore`, `IAudioRepository`, `IAudioCompressor`, `IAudioTranscriber`, `ITextSummarizer`) |
| `AudioProcessor.Infrastructure` | Implementaciones concretas: SQLite (EF Core), almacenamiento local de archivos, FFmpeg, Whisper.net, Ollama |
| `AudioProcessor.Api` | Punto de entrada, controllers, Swagger |

**Pipeline de procesamiento** (secuencial por ahora, ver `backend/docs/`):

```
Cliente → POST /api/audio → guarda original
                          → comprime a AAC (FFmpeg)
                          → transcribe a texto (Whisper.net, modelo tiny, local)
                          → resume el texto (Ollama, llama3.2:1b, local, ≤ 50 caracteres)
                          → persiste todo en SQLite
                          → responde con id, urls, tiempos y resumen
```

Cada paso mide su propio tiempo de ejecución (`Stopwatch`) y lo loguea y
persiste, como base para las mediciones de rendimiento que se piden en
semanas posteriores del curso (comparación secuencial vs. paralelo).

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

## Documentación adicional

- [`backend/docs/week2-analysis.md`](./backend/docs/week2-analysis.md) — análisis de compresión de audio.
- [`backend/docs/week3-threading-explanation.md`](./backend/docs/week3-threading-explanation.md) — análisis de dónde tendría sentido aplicar hilos en el pipeline.
- [`backend/docs/week4-frontend-analysis.md`](./backend/docs/week4-frontend-analysis.md) — por qué el frontend usa `async/await` sin bloquear el hilo principal.

## Estado del proyecto

En desarrollo activo como parte del Capstone del curso. El pipeline
funciona de punta a punta de forma secuencial; la paralelización
(hilos, `Task`, TPL) del pipeline está planificada para las semanas
siguientes del curso, según el análisis en `week3-threading-explanation.md`.