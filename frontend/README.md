# Frontend

Cliente simple en React + TypeScript (Vite) para subir un archivo de audio al
backend (`POST /api/audio`) y mostrar el resumen que devuelve.

## Instalar y correr

```bash
npm install
npm run dev
```

Esto levanta el servidor de desarrollo en `http://localhost:5173`.

Para correr los tests (Vitest + React Testing Library):

```bash
npm run test
```

Para generar el build de producción:

```bash
npm run build
```

## Configurar VITE_API_URL

La URL del backend se lee de la variable de entorno `VITE_API_URL`. Copia
`.env.example` a `.env` (ya viene creado con el valor por defecto) y ajústalo
si el backend corre en otra URL:

```bash
cp .env.example .env
```

```
VITE_API_URL=http://localhost:5262
```

El backend debe estar corriendo y tener habilitado CORS para
`http://localhost:5173` (ya configurado en `Program.cs`).
