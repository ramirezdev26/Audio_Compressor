# Arquitectura y Pipeline

## Arquitectura (Clean Architecture, 4 capas)

```mermaid
flowchart TD
    Client["Cliente Web<br/>(React + Vite)"] -->|POST /api/audio| Api

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

- **Api:** recibe el request HTTP.
- **Application:** orquesta el caso de uso (subir, comprimir, transcribir, resumir).
- **Domain:** entidad `AudioFile`, sin dependencias externas.
- **Infrastructure:** implementaciones concretas (SQLite, disco local, FFmpeg, Whisper.net, Ollama).

Regla de dependencias: `Api → Application → Domain`, e `Infrastructure` implementa interfaces definidas en `Application`.

## Pipeline de procesamiento

```mermaid
sequenceDiagram
    participant U as Usuario
    participant A as API
    participant F as FFmpeg
    participant W as Whisper (local)
    participant O as Ollama (local)
    participant DB as SQLite

    U->>A: Sube audio
    A->>A: Guarda archivo original
    A->>F: Comprime a AAC
    F-->>A: Audio comprimido
    A->>W: Transcribe audio
    W-->>A: Texto
    A->>O: Resume texto (≤ 50 caracteres)
    O-->>A: Resumen
    A->>DB: Guarda todo (urls, tiempos, transcripción, resumen)
    A-->>U: Responde con resultado
```

Todo el pipeline es **secuencial** por ahora (cada paso espera al anterior). Compresión y transcripción no dependen entre sí, así que son candidatas a paralelizarse más adelante; el resumen sí depende de la transcripción, por lo que ese paso no se puede paralelizar con los anteriores.
