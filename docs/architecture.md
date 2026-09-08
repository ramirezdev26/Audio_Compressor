# Arquitectura y Pipeline

## Arquitectura (Clean Architecture, 4 capas)

```mermaid
flowchart TD
    Client["Cliente Web<br/>(React + Vite)"] -->|POST /api/audio| Api
    Client -->|GET /api/audio| Api

    subgraph Backend["Backend (.NET)"]
        Api["Api<br/>AudioController"]
        App["Application<br/>UploadAudioService"]
        Domain["Domain<br/>AudioFile"]
        Infra["Infrastructure<br/>SQLite · FileStore · FFmpeg · Whisper · Ollama"]

        Api --> App
        App --> Domain
        App -.->|usa interfaces| Infra
    end
```

- **Api:** recibe el request HTTP (`POST /api/audio` para subir, `GET /api/audio` para listar los audios ya procesados).
- **Application:** orquesta el caso de uso (subir, comprimir, filtrar ruido, transcribir, resumir).
- **Domain:** entidad `AudioFile`, sin dependencias externas.
- **Infrastructure:** implementaciones concretas (SQLite, disco local, FFmpeg, Whisper.net, Ollama).

Regla de dependencias: `Api → Application → Domain`, e `Infrastructure` implementa interfaces definidas en `Application`.

## Pipeline de procesamiento

```mermaid
sequenceDiagram
    participant U as Usuario
    participant A as API
    participant S as Semáforo (Processing:MaxConcurrentJobs)
    participant F as FFmpeg
    participant W as Whisper (local)
    participant O as Ollama (local)
    participant DB as SQLite

    U->>A: POST /api/audio (sube archivo)
    A->>A: Guarda archivo original
    A-->>U: 202 Accepted { id, message: "Procesando en segundo plano" }

    Note over A: Procesamiento en background (Task.Run)
    A->>S: Espera slot libre
    S-->>A: Slot obtenido

    par Comprimir
        A->>F: Comprime a AAC
        F-->>A: Audio comprimido
    and Filtrar ruido
        A->>F: Filtro afftdn
        F-->>A: Audio filtrado
    and Transcribir y resumir
        A->>W: Transcribe audio
        W-->>A: Texto
        A->>O: Resume texto (≤ 50 caracteres)
        O-->>A: Resumen
    end

    A->>S: Libera slot
    A->>DB: Guarda registro completo (urls, tiempos, transcripción, resumen)

    U->>A: GET /api/audio
    A-->>U: Lista de audios procesados
```

El procesamiento corre en segundo plano (`Task.Run`, fire-and-forget): la API
responde con `202 Accepted` apenas guarda el archivo original, y el resultado
completo se consulta después con `GET /api/audio`. Las tres ramas (compresión,
filtro de ruido y transcripción+resumen) corren en paralelo con
`Parallel.Invoke` una vez que consiguen un slot; el resumen sí depende del
texto transcrito, así que no se paraleliza con la transcripción.

Sin un límite de concurrencia, un burst de subidas simultáneas dispara tantos
jobs en paralelo como uploads lleguen, y cada uno consume CPU y RAM a la vez
(dos procesos FFmpeg + Whisper + Ollama) — esto agotó la RAM de la máquina en
una prueba de carga con k6 documentada en el laboratorio de la semana 6. Por
eso el número de jobs que corren al mismo tiempo está limitado por un
`SemaphoreSlim` compartido, configurable con `Processing:MaxConcurrentJobs`
en `appsettings.json` (por defecto `Environment.ProcessorCount`).
