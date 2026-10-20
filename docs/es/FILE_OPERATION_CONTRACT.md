# Contrato de operaciones para las adaptaciones

[English](../FILE_OPERATION_CONTRACT.md) · [Índice en español](README.md)

Uso este contrato para decidir si un adaptador nativo puede declarar `CanMove`. En 0.3.0 solo lo permite la implementación existente de Windows. El adaptador de producción macOS/Linux rechaza las modificaciones; una llamada portátil a `File.Move` o las pruebas compartidas no bastan como evidencia.

| Límite | Comportamiento exigido antes de activar una adaptación |
| --- | --- |
| Rutas e identidad | Resolver pertenencia mediante identidades verificadas de archivos/directorios y comportamiento por volumen. No deducir mayúsculas por el nombre del sistema ni normalizar Unicode a ciegas. Rechazar alias ambiguos, enlaces, montajes no admitidos, marcadores de nube y volúmenes ausentes. |
| Destino | No sobrescribir archivos existentes, incluidos alias por mayúsculas/Unicode. Reservar o crear atómicamente; comprobar existencia antes no evita una sustitución posterior. |
| Origen | Verificar contenido esperado e identidad. Proteger frente a escritores, sustituciones de rutas y directorios durante la operación. El modo de compartición administrado no garantiza exclusión portátil. |
| Mismo volumen | Conservar identidad y metadatos admitidos y verificar el resultado. Documentar límites del sistema de archivos. |
| Entre volúmenes | Copiar sin sobrescribir, vaciar buffers y verificar antes de borrar el original verificado. Conservar fechas, permisos y metadatos extendidos requeridos o rechazar la transferencia no soportada. |
| Diario y recuperación | Guardar Pending antes de modificar. Confirmar estado e historial juntos. Tras una interrupción inspeccionar rutas e identidades; conservar copias ambiguas y solicitar atención en vez de borrar por suposición. |
| Capacidades | Rechazar antes de crear directorios, modificar archivos o registrar operaciones. Desactivar botones no basta. La vista previa restringida tampoco reproduce recuperaciones. |

Conservo en 0.3.0 la política de rutas e implementación nativa aceptada de Windows. `PathSafety` sigue comparando según el sistema operativo: es una limitación explícita, no una política por volumen terminada. Difiero el resolvedor por volumen y los adaptadores de producción a sus hitos, para probarlos juntos en variantes de APFS y ext4. La supervisión conserva avisos que difieren en mayúsculas; no decide identidad del sistema de archivos.

Exijo pruebas nativas desechables de escritores, sustituciones, enlaces, cambios de montaje, colisiones en el mismo y distinto volumen, alias Unicode/mayúsculas, permisos, disco lleno, copias interrumpidas, reinicio y recuperación. Los adaptadores de pruebas compartidas solo ejercitan diario y flujo. Las cinco pruebas exclusivas de Windows y su aceptación previa constituyen evidencia separada.

## Perfiles y copias

Conservo esquema 1 y ubicación del perfil Windows. Las muestras fijas de 0.2.0 comprueban ambos idiomas, reglas propias, archivos e historial sin producir la muestra con el serializador nuevo. Verifico que una copia de la base cerrada abre con los mismos datos.

Para una copia real cierro Dowlume completamente, copio todo el directorio del perfil (incluidos archivos auxiliares SQLite restantes) y respaldo aparte los archivos gestionados. Una copia de la base no restaura contenido ni deshace movimientos. Restauro con la aplicación cerrada, primero en un perfil desechable del mismo sistema mediante `--data-directory`; reviso vista previa e historial antes de supervisar. No copio únicamente una base SQLite activa ni prometo portabilidad entre sistemas.

## Presupuestos iniciales de rendimiento

Los defino como umbrales de investigación para futuras implementaciones nativas aceptadas, no como resultados medidos: inicio interactivo en 3 segundos; vista previa de 1.000 archivos locales completos de 4 KiB en 5 segundos; primera página de 10.000 operaciones en 1 segundo; memoria residente en reposo inferior a 300 MiB; sin sondeo continuo en reposo fuera del intervalo configurado. Registro sistema, CPU, almacenamiento, compilación, ejecuciones frías/calientes y mediana de cinco repeticiones. El hash depende del tamaño del contenido; los archivos grandes requieren medir rendimiento por volumen de datos.

La prueba de arranque del paquete 0.3.0 falla a los 60 segundos y comprueba ventana/cierre. Ese límite no es una medición de rendimiento. La aceptación estable debe incorporar bases medidas, regresiones de carga y cancelación ágil antes de afirmar que cumplo estos presupuestos.
