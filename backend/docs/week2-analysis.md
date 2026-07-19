# Análisis: ¿Es momento de aplicar hilos?

Por ahora, no. Cada request a `POST /api/audio` sigue siendo una sola tarea
(comprimir un archivo), no varias tareas que se puedan correr en paralelo
dentro del mismo request. Además, ASP.NET Core (Kestrel) ya atiende cada
request en un hilo del thread pool por defecto, así que ya existe
concurrencia básica entre múltiples clientes sin que nosotros tengamos que
manejar hilos manualmente.

Tendría sentido introducir hilos/paralelismo cuando, para un mismo audio,
tengamos que ejecutar varias tareas independientes al mismo tiempo (por
ejemplo: comprimir + extraer transcripción + generar resumen), algo que
vendrá en semanas posteriores del proyecto. Agregar hilos ahora, sin ese
escenario, solo agregaría complejidad y riesgo de race conditions sin
ninguna ganancia real de rendimiento.
