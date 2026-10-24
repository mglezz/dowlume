# Hoja de ruta de Dowlume

[English](../../development/roadmap.md) · [Índice en español](../README.md)

La versión 0.4.0 ofrece los flujos Windows x64 y macOS Apple Silicon dentro del [alcance publicado](../user/platform-support.md). Linux sigue como vista previa restringida. El siguiente hito es el soporte nativo Linux y, después, la estabilización multiplataforma.

Los hitos definen alcance previsto, no fechas. Pueden hacer falta más versiones 0.x antes de 1.0. Compilar en una plataforma no basta para declararla compatible: requiere aceptación nativa.

## Versiones previstas hacia 1.0

| Hito | Resultado | Criterios de aceptación |
| --- | --- | --- |
| 0.5.0 · Linux | Flujo Linux compatible | Una versión Ubuntu LTS identificada, x64 y GNOME inicialmente; cobertura X11/Wayland explícita; Descargas, permisos, gestor de archivos, inicio opcional, alternativa sin bandeja, seguridad nativa y paquete aceptado con dependencias documentadas. |
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
