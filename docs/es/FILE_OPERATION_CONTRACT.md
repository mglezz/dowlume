# Contrato de operaciones para las adaptaciones

[English](../FILE_OPERATION_CONTRACT.md) · [Índice en español](README.md)

Uso este contrato para habilitar operaciones nativas. En 0.4.0 conservo Windows y habilito macOS Apple Silicon en APFS local del mismo volumen. Linux sigue restringido. Distingo exclusión nativa Windows de detección y conservación ante carreras POSIX en macOS; no equiparo ambas garantías.

| Límite | Comportamiento exigido antes de activar una adaptación |
| --- | --- |
| Rutas e identidad | Resolver pertenencia mediante identidades verificadas de archivos/directorios y comportamiento por volumen. No deducir mayúsculas por el nombre del sistema ni normalizar Unicode a ciegas. Rechazar alias ambiguos, enlaces, montajes no admitidos, marcadores de nube y volúmenes ausentes. |
| Destino | No sobrescribir archivos existentes, incluidos alias por mayúsculas/Unicode. Reservar o crear atómicamente; comprobar existencia antes no evita una sustitución posterior. |
| Origen | Verificar contenido esperado e identidad. Declarar y probar garantías nativas ante escritores y sustituciones; la ambigüedad detectada debe conservar archivos para inspección. El modo de compartición administrado no garantiza exclusión portátil. |
| Mismo volumen | Conservar identidad y metadatos admitidos y verificar el resultado. Documentar límites del sistema de archivos. |
| Entre volúmenes | Copiar sin sobrescribir, vaciar buffers y verificar antes de borrar el original verificado. Conservar fechas, permisos y metadatos extendidos requeridos o rechazar la transferencia no soportada. |
| Diario y recuperación | Guardar Pending antes de modificar. Confirmar estado e historial juntos. Tras una interrupción inspeccionar rutas e identidades; conservar copias ambiguas y solicitar atención en vez de borrar por suposición. |
| Capacidades | Rechazar antes de crear directorios, modificar archivos o registrar operaciones. Desactivar botones no basta. La vista previa restringida tampoco reproduce recuperaciones. |

## macOS 0.4.0

Resuelvo rutas existentes mediante UUID de volumen/inodo/nacimiento y comparo nombres ausentes de forma conservadora con la política de mayúsculas del volumen abierto. El renombrado exclusivo del núcleo decide las colisiones, no una comparación de cadenas. Retengo descriptores de origen/padres, verifico contenido antes y después y registro identidad antes de modificar. Rechazo transferencias entre volúmenes antes de Pending o de crear carpetas.

`flock` es cooperativo. Un escritor POSIX arbitrario o una sustitución de rutas del mismo usuario puede competir con las últimas comprobaciones. Una sustitución de origen justo antes de renombrar puede mover el sustituto; lo detecto después y conservo ambos objetos, sin revertir ni borrar por ruta. Registro la ruta observada del objeto origen cuando está disponible. No prometo exclusión obligatoria de escritura, espacio de nombres inmóvil ni seguridad frente a modificaciones hostiles del mismo usuario. Rechazo rutas/atributos conocidos de nube, pero solo admito carpetas ordinarias fuera de cualquier sincronizador. Renombrar en el mismo volumen conserva metadatos del objeto original. La recuperación exige identidad nativa registrada y contenido, nunca solo hashes.

Verifico lectura/movimiento en imágenes APFS normales y sensibles a mayúsculas, incluidas colisiones, escritores, sustituciones de origen/carpetas, permisos denegados, volumen lleno, rechazo entre volúmenes, supervisión y ciclo/recuperación con base reabierta. Detallo pruebas y paquetes en [implementación macOS](MACOS_IMPLEMENTATION.md). Los adaptadores de muestras y resultados históricos Windows son evidencia separada.

## Perfiles y copias

Conservo esquema 1 y ubicación del perfil Windows. Las muestras fijas de 0.2.0 comprueban ambos idiomas, reglas propias, archivos e historial sin producir la muestra con el serializador nuevo. Verifico que una copia de la base cerrada abre con los mismos datos.

Para una copia real cierro Dowlume completamente, copio todo el directorio del perfil (incluidos archivos auxiliares SQLite restantes) y respaldo aparte los archivos gestionados. Una copia de la base no restaura contenido ni deshace movimientos. Restauro con la aplicación cerrada, primero en un perfil desechable del mismo sistema mediante `--data-directory`; reviso vista previa e historial antes de supervisar. No copio únicamente una base SQLite activa ni prometo portabilidad entre sistemas.

## Presupuestos iniciales de rendimiento

Los defino como umbrales de investigación para futuras implementaciones nativas aceptadas, no como resultados medidos: inicio interactivo en 3 segundos; vista previa de 1.000 archivos locales completos de 4 KiB en 5 segundos; primera página de 10.000 operaciones en 1 segundo; memoria residente en reposo inferior a 300 MiB; sin sondeo continuo en reposo fuera del intervalo configurado. Registro sistema, CPU, almacenamiento, compilación, ejecuciones frías/calientes y mediana de cinco repeticiones. El hash depende del tamaño del contenido; los archivos grandes requieren medir rendimiento por volumen de datos.

La prueba de arranque del paquete 0.3.0 falla a los 60 segundos y comprueba ventana/cierre. Ese límite no es una medición de rendimiento. La aceptación estable debe incorporar bases medidas, regresiones de carga y cancelación ágil antes de afirmar que cumplo estos presupuestos.
