# Audio Processor - Backend

API para el procesamiento de archivos de audio (Proyecto Capstone - Programming 7).

## Arquitectura

Proyecto organizado con Clean Architecture:

- **AudioProcessor.Domain** → Entidades del negocio (AudioFile).
- **AudioProcessor.Application** → Casos de uso e interfaces (UploadAudioService, IFileStore, IAudioRepository).
- **AudioProcessor.Infrastructure** → Implementaciones concretas (SQLite con EF Core, almacenamiento local de archivos).
- **AudioProcessor.Api** → Punto de entrada, controllers.

## Requisitos

- .NET SDK 8.0
- Herramienta `dotnet-ef` instalada globalmente:
```bash
  dotnet tool install --global dotnet-ef
```
- [Ollama](https://ollama.com) instalado y corriendo localmente, con el modelo `llama3.2:1b` descargado (se usa para generar el resumen del audio):
```bash
  ollama pull llama3.2:1b
  ollama serve
```
  La URL base de Ollama es configurable en `appsettings.json` (clave `Ollama:BaseUrl`, por defecto `http://localhost:11434`).

## Cómo correr el entorno de desarrollo

1. Restaurar dependencias:
```bash
   dotnet restore
```

2. Aplicar migraciones (crea la base de datos SQLite):
```bash
   dotnet ef database update --project AudioProcessor.Infrastructure --startup-project AudioProcessor.Api
```

3. Correr el proyecto:
```bash
   dotnet run --project AudioProcessor.Api
```

4. La API queda disponible en `http://localhost:5262` (el puerto puede variar, revisa la consola).

## Endpoint disponible

### `POST /api/audio`

Recibe un archivo de audio y lo almacena.

**Request (curl):**
```bash
curl -X POST http://localhost:5262/api/audio \
  -F "file=@/ruta/a/tu/archivo.mp3"
```

**Response (200 OK):**
```json
{
  "id": "0d4b4b61-5496-48f4-929c-dbba8f4ec826",
  "url": "/files/0d4b4b61-5496-48f4-929c-dbba8f4ec826.mp3",
  "compressedUrl": "/files/0d4b4b61-5496-48f4-929c-dbba8f4ec826_compressed.aac",
  "compressionTimeMs": 812,
  "summary": "Resumen breve del contenido del audio",
  "summaryTimeMs": 430
}
```

El archivo se guarda en `AudioProcessor.Api/FileStore/` y el registro completo (incluyendo la transcripción y el resumen) se guarda en la base de datos SQLite (`audio.db`). El audio se transcribe localmente con Whisper.net (modelo `tiny`) y luego se resume con Ollama (`llama3.2:1b`), truncando el resultado a un máximo de 50 caracteres.