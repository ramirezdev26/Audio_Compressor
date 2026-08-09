# Semana 4: Frontend y programación asíncrona en el navegador

## Qué se implementó

Un frontend mínimo en React + TypeScript (Vite) con un único formulario para
subir un archivo de audio a `POST /api/audio` y mostrar el resumen que
devuelve el backend. Del lado del backend solo se agregó una política CORS
de desarrollo para permitir que `http://localhost:5173` llame a la API.

## Por qué async/await + fetch

Subir un archivo implica esperar una respuesta del backend que puede tardar
varios segundos (comprime el audio, lo transcribe y genera un resumen). Si
esa espera se hiciera de forma bloqueante, la pestaña del navegador se
congelaría: no se podría hacer click en nada, ni siquiera repintar la
pantalla, hasta que la respuesta llegara.

`fetch` es asíncrono por diseño: devuelve una `Promise` inmediatamente y
delega la espera de la red al motor del navegador, fuera del hilo principal
de JavaScript. `async`/`await` no cambia ese comportamiento, solo da una
sintaxis secuencial para trabajar con esa promesa sin bloquear nada: al
llegar a un `await`, la función se "pausa" y le devuelve el control al
Event Loop, que sigue procesando otros eventos (clicks, renders, etc.)
mientras la petición de red sigue en curso en segundo plano. Cuando la
promesa se resuelve, el Event Loop retoma la función justo donde quedó.

## Por qué el estado de "loading" es la evidencia de que la UI no se bloquea

El componente maneja cuatro estados (`idle`, `loading`, `success`, `error`)
con `useState`. Apenas se llama a `handleSubmit`, el estado pasa a
`loading` y React re-renderiza mostrando "Subiendo archivo..." y
deshabilitando el botón, todo esto antes de que `fetch` haya recibido
respuesta alguna del servidor.

Si el `fetch` bloqueara el hilo principal, ese re-render nunca se vería:
la UI se quedaría congelada en el estado anterior hasta que la petición
terminara, y el mensaje de carga aparecería y desaparecería en el mismo
instante (o ni siquiera se llegaría a pintar). Que el usuario efectivamente
vea "Subiendo archivo..." en pantalla mientras la petición sigue pendiente
en la red es la prueba visual de que el hilo principal quedó libre para
seguir renderizando y atendiendo eventos durante toda la espera. El test
de Vitest (`App.test.tsx`) verifica exactamente esto: mockea `fetch` con
una promesa que nunca se resuelve y comprueba que el texto de carga
aparece en el DOM mientras esa promesa sigue pendiente.
