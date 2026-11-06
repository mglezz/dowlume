# Hoja de ruta de Dowlume

[English](../../development/roadmap.md) · [Índice en español](../README.md)

La versión 0.5.0 ofrece los flujos Windows x64, macOS Apple Silicon y Ubuntu 24.04 LTS x64 con GNOME y ext4 dentro del [alcance publicado](../user/platform-support.md). La siguiente etapa es la estabilización multiplataforma. El [registro 0.5.0](../releases/acceptance/0.5.0.md) conserva la confirmación manual y los límites de la automatización.

Los hitos definen alcance previsto, no fechas. Pueden hacer falta más versiones 0.x antes de 1.0. Compilar en una plataforma no basta para declararla compatible: requiere aceptación nativa.

## Camino a 0.6.0

El código actual incorpora organización periódica opcional con un intervalo de 1 a 1440 minutos, vista previa y confirmación explícitas, pasadas secuenciales y arranque pausado. La búsqueda en Revisión y la selección de pestañas más clara también están disponibles después de 0.5.0. Estos cambios todavía no constituyen un paquete 0.6.0 publicado.

Las siguientes prioridades propuestas son un resumen de organización más claro (última pasada y próxima revisión), una acción para organizar una sola vez sin supervisión en segundo plano y estadísticas de espacio por categoría. La detección de duplicados debería comenzar con un informe o vista previa, sin eliminación automática. Son propuestas para continuar, no funciones implementadas.

Antes de publicar 0.6.0, hay que validar la actualización desde 0.5.0, ambos idiomas, navegación por teclado, pausa y cierre durante una pasada, suspensión y reanudación, y los paquetes exactos en todas las plataformas compatibles. Se mantienen los criterios de seguridad de operaciones con archivos.

## Versiones previstas hacia 1.0

| Hito | Resultado | Criterios de aceptación |
| --- | --- | --- |
| 0.6.x–0.9.x · Estabilización, si hace falta | Cerrar diferencias entre plataformas | Migraciones, usabilidad, accesibilidad, rendimiento, alcance de recuperación, firma Windows y firma Developer ID y notarización macOS. |
| 1.0.0 | Producto fiable dentro del contrato publicado | Sin defectos conocidos pendientes de pérdida de datos, sobrescritura, recuperación insegura o seguridad crítica; plataformas, actualizaciones, copias de seguridad, accesibilidad, idiomas, presupuestos medidos, dependencias y paquetes aceptados. |

La fiabilidad Windows sigue siendo un requisito durante el desarrollo Linux. macOS se dirige a Apple Silicon; Intel queda fuera del plan. Otras distribuciones, escritorios y arquitecturas Linux requieren evidencia propia. La firma de editor y la notarización se aplazan a la estabilización; cada candidato debe indicar su firma y sus límites reales de instalación.

## Calidad en todos los hitos

Comprueba vista previa y cancelación, organización y supervisión, Conservar/Posponer/Mover, Deshacer, historial, reinicio y recuperación en ambos idiomas. Los cambios en operaciones requieren pruebas desechables de archivos ocupados o cambiantes, colisiones, permisos, volúmenes ausentes, falta de espacio e interrupciones. Conserva las copias ambiguas.

Mide arranque, vista previa, historial, memoria y supervisión con cargas representativas. Los umbrales del [contrato](../architecture/file-operations.md) son objetivos de investigación; las bases de rendimiento nativas siguen pendientes.

La papelera nativa continúa desactivada. Antes de 1.0 debe existir una cuarentena recuperable diseñada y aceptada por separado, o excluirse explícitamente ambas funciones del alcance, con interfaz y documentación coherentes. Los fallos históricos no se dan por resueltos sin evidencia.

## Trabajo completado

| Versión | Alcance | Registro |
| --- | --- | --- |
| 0.1.0 | Organización, revisión, historial, recuperación y escritorio Windows; papelera desactivada | [Notas](../releases/0.1.0.md) · [Aceptación](../releases/acceptance/0.1.0.md) |
| 0.2.0 | Interfaz y documentación en inglés y español | [Notas](../releases/0.2.0.md) · [Aceptación](../releases/acceptance/0.2.0.md) |
| 0.3.0 | Servicios de plataforma, CI nativa, compatibilidad y vistas previas restringidas | [Notas](../releases/0.3.0.md) · [Aceptación](../releases/acceptance/0.3.0.md) |
| 0.5.0 | Flujo Linux ext4, integración XDG, instalador Ubuntu y portable, conservación del menú al actualizar | [Notas](../releases/0.5.0.md) · [Aceptación](../releases/acceptance/0.5.0.md) |
| 0.4.0 | Flujo APFS, escritorio macOS, inicio opcional, migración y paquete autocontenido | [Notas](../releases/0.4.0.md) · [Aceptación](../releases/acceptance/0.4.0.md) |

También está implementado el historial con búsqueda, filtros y paginación. La [validación histórica](../archive/validation.md) no aprueba binarios nuevos.

## Ideas adicionales sin versión asignada

- Duplicados y análisis del espacio ocupado.
- Cuarentena recuperable en el mismo volumen, con restauración explícita y sin expulsión automática.
- Reglas de clasificación más ricas y organización virtual.
- Supervisión simultánea de varias carpetas.
- Seguimiento de cambios externos de nombre y aceptación explícita de contenido editado.
- Consultas incrementales y búsquedas indexadas para perfiles grandes.
- Más idiomas.

La clasificación por contenido, la sincronización en la nube, los scripts arbitrarios y el borrado permanente automático quedan fuera del alcance actual.
