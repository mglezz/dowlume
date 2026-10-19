# Hoja de ruta de Dowlume

[English](../../ROADMAP.md) · [Índice en español](README.md)

Publiqué [v0.1.0 para Windows x64](https://github.com/mglezz/dowlume/releases/tag/v0.1.0) el 18 de octubre de 2026 tras completar aceptación automatizada, confirmar pruebas humanas y revisión independiente, y autorizar su publicación. Mantengo las evidencias en [Validación](VALIDATION.md).

## Trabajo completado

| Etapa | Resultado |
| --- | --- |
| Base | Solución C#, Avalonia, Core, Infrastructure, Monitoring, xUnit, CI, diagnóstico y licencia MIT. |
| Detección | Carpeta configurable, eventos del sistema, cola limitada, comprobaciones conservadoras, exclusiones y recorridos periódicos. |
| Clasificación | Seis categorías, reglas editables, vista previa, consentimiento, colisiones sin sobrescritura y registro SQLite. |
| Ciclo de vida | Primera organización, plazos, `Expiring Files`, Conservar, Posponer siete días, Mover y Deshacer compatible. |
| Escritorio | Resumen, Vista previa, Revisión, Actividad, Ajustes, diálogos, bandeja y cierre ordenado. |
| Robustez | Recuperación de interrupciones, protección de copias ambiguas y pruebas de cambios, bloqueos y carreras. |
| Distribución | ZIP autónomo e instalador por usuario, aceptación Windows y publicación de v0.1.0. |
| Historial | Búsqueda por nombre/ruta, filtros y páginas de operaciones antiguas. |
| Idiomas, v0.2.0 publicada | Selector inglés/español persistente y documentación paralela. Conservo intactos los binarios anteriores de v0.1.0. |

Publiqué [v0.2.0](https://github.com/mglezz/dowlume/releases/tag/v0.2.0) el 20 de octubre de 2026 tras superar las pruebas automáticas y confirmar que todas las funcionalidades del paquete final funcionaban correctamente. Registro los archivos exactos en la [aceptación de 0.2.0](RELEASE_CANDIDATE_0.2.0.md).

## Controles de v0.1.0

Completé integración y CI del commit `94ba177`, empaquetado limpio, verificación de hashes, pruebas de actualización y primer uso en el equipo anfitrión y en un Windows 11 independiente sin .NET compartido. Registré la aceptación humana y revisión independiente, pruebas entre discos NVMe/SATA y la decisión de publicar sin firma.

Desactivé la papelera nativa tras los fallos de capacidad. Conservo la máquina virtual, checkpoints, informes y contenido restante; los fallos siguen siendo fallos. Los seis archivos públicos coinciden con los artefactos aceptados. El [listado de publicación](RELEASE_CHECKLIST.md) conserva el detalle.

## Posibles siguientes pasos

No asigno fechas ni versiones a estas ideas:

- Detección de duplicados y análisis del almacenamiento.
- [Cuarentena recuperable](RECYCLE_CAPACITY_INVESTIGATION.md), en el mismo volumen y sin expulsión automática.
- Reglas más completas y organización virtual.
- Supervisión simultánea de varias carpetas.
- Seguimiento de renombrados externos y aceptación explícita de contenido editado.
- Consultas incrementales e índices para perfiles grandes.
- [Integraciones macOS/Linux](PLATFORM_SUPPORT.md), otros idiomas y revisión adicional de accesibilidad.

Mantengo fuera del MVP la clasificación por contenido, sincronización en la nube, scripts arbitrarios y eliminación permanente automática. Los binarios futuros requieren nueva aceptación; las pruebas de v0.1.0 no los aprueban automáticamente.
