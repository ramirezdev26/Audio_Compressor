# Load test: audio upload

Script `audio-upload.js` sube `sample-files/sample3.mp3` al endpoint `POST /api/audio`
usando k6, para comparar el comportamiento del servidor antes y después de mover
el procesamiento a background.

## Requisitos

- [k6](https://k6.io/docs/get-started/installation/) instalado.
- La API corriendo (por defecto en `http://localhost:5262`).

## Cómo correrlo

Desde `backend/AudioProcessor.Api`:

```bash
k6 run load-tests/audio-upload.js
```

Para apuntar a otra URL:

```bash
API_URL=http://localhost:5000 k6 run load-tests/audio-upload.js
```

## Cambiar usuarios virtuales (VUs) y duración

Editar el bloque `options` en `audio-upload.js`:

```js
export const options = {
  vus: 5,        // usuarios virtuales concurrentes
  duration: '30s',
};
```

También se puede sobreescribir desde la línea de comandos sin tocar el archivo:

```bash
k6 run --vus 20 --duration 60s load-tests/audio-upload.js
```

Usa valores bajos (ej. `vus: 5`) para probar el endpoint síncrono "antes" del cambio,
y valores más altos (ej. `vus: 20-50`) para ver cómo el endpoint asíncrono "después"
responde rápido aunque el procesamiento pesado siga corriendo en background.
