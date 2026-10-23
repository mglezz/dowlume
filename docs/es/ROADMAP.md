# Hoja de ruta de Dowlume

[English](../../ROADMAP.md) · [Índice en español](README.md)

**Estado actual:** publiqué [v0.4.0 para macOS Apple Silicon y Windows x64](https://github.com/mglezz/dowlume/releases/tag/v0.4.0) el 23 de octubre de 2026 tras controles automáticos y confirmación funcional final en ambas plataformas. Linux sigue como vista previa restringida para 0.5.0. Registro la [aceptación de 0.4.0](RELEASE_CANDIDATE_0.4.0.md).

Publiqué [v0.1.0 para Windows x64](https://github.com/mglezz/dowlume/releases/tag/v0.1.0) el 18 de octubre de 2026 tras completar aceptación automatizada, confirmar pruebas humanas y revisión independiente, y autorizar su publicación. Mantengo las evidencias en [Validación](VALIDATION.md).

## Versiones previstas hacia 1.0

Después de 0.2.0 priorizo el funcionamiento seguro en varias plataformas. Estas versiones expresan alcance previsto, no fechas ni soporte ya disponible. Corrijo defectos en cada hito y publico versiones de parche cuando hace falta. Mantengo Windows durante las adaptaciones y admito versiones 0.x adicionales si no se cumplen los criterios. Utilizo candidatas antes de declarar estables los paquetes de una nueva plataforma.

| Objetivo | Resultado previsto | Criterios para completarlo |
| --- | --- | --- |
| 0.3.0 | Bases comunes; Windows sigue siendo el producto soportado | CI nativa en Windows, macOS y Linux; capacidades y adaptadores explícitos; contratos de archivos y política de rutas revisados; pruebas de compatibilidad de perfiles; paquetes experimentales reproducibles; matriz de soporte y riesgos nativos pendientes documentados. Compilar no concede soporte de producción a una adaptación. |
| 0.4.0 | macOS soportado dentro de una matriz definida | Aceptación nativa de archivos y recuperación, Finder y carpetas, permisos y su denegación, ciclo de vida e inicio de sesión opcional, interfaz en ambos idiomas y paquetes de instalación/actualización aceptados con su estado de firma documentado. Solo incluyo Apple Silicon. Intel queda fuera de esta hoja de ruta y podría reconsiderarse en una entrega posterior. |
| 0.5.0 | Linux soportado dentro de una matriz definida | Una versión concreta de Ubuntu LTS, x64 y GNOME como destino inicial; cobertura explícita de X11/Wayland y del backend; descubrimiento de Descargas configuradas, permisos, gestor de archivos, inicio opcional, alternativa sin bandeja, aceptación nativa de archivos y paquete aceptado con dependencias del sistema documentadas. Otras distribuciones, escritorios y arquitecturas requieren evidencia propia. |
| 0.6.x–0.9.x, si hacen falta | Estabilización y cierre funcional | Resolver diferencias entre plataformas, migraciones, usabilidad, accesibilidad y regresiones de rendimiento. Decidir expresamente el alcance de recuperación/cuarentena. Abordar firma de editor y notarización macOS tras completar las funciones de las plataformas soportadas. No añado versiones solo para rellenar números. |
| 1.0.0 | Producto fiable dentro de su contrato de soporte | Ningún defecto conocido sin resolver de pérdida de datos, sobrescritura, recuperación insegura o seguridad crítica; matriz aceptada, actualización de perfiles, instrucciones de respaldo/recuperación, accesibilidad y ambos idiomas, presupuestos de rendimiento medidos, dependencias soportadas, procedencia de distribución y aceptación de los paquetes finales en todos los destinos soportados. |

«Soportado» significa que las funciones documentadas funcionan en las versiones, arquitecturas, sistemas de archivos y sesiones de escritorio indicados. No significa todas las distribuciones Linux, todas las versiones de macOS ni convenciones visuales nativas idénticas. En 0.3.0 fijo la matriz inicial según las dependencias .NET/Avalonia reales y los entornos de prueba disponibles; añadir un destino exige aceptación, no solo otro identificador de runtime. Detallo el [alcance y la evidencia por plataforma](PLATFORM_SUPPORT.md).

Integré y publiqué las bases de 0.3.0 tras superar los controles nativos automáticos y [confirmar el funcionamiento final Windows](RELEASE_CANDIDATE_0.3.0.md). Completé la aceptación macOS en 0.4.0; la de Linux queda para 0.5.0.

### Publicada: 0.4.0 para Apple Silicon

Acepté [0.4.0-rc.1](https://github.com/mglezz/dowlume/releases/tag/v0.4.0-rc.1) en macOS y Windows y publiqué los mismos paquetes como 0.4.0. He implementado operaciones APFS del mismo volumen y recuperación, ciclo completo y supervisión, menús nativos/reapertura/salida, inicio opcional, migración, ambos idiomas y empaquetado autocontenido. Verifico carreras, colisiones, permisos y volumen lleno con ambas políticas APFS. El paquete extraído ejecuta una prueba del flujo nativo. Registro evidencia y comprobaciones manuales del paquete exacto en [aceptación de 0.4.0](RELEASE_CANDIDATE_0.4.0.md).

Registré la confirmación funcional final por separado de la automatización; conservo su alcance y límites de evidencia en el registro de aceptación. Priorizo funcionalidad macOS/Linux conservando Windows y aplazo firma de editor, Developer ID y notarización hasta estabilización final. La firma no bloquea los hitos funcionales 0.4.0/0.5.0; documento su estado real. Intel queda fuera del alcance.

### Primera secuencia de implementación: 0.3.0

He implementado en el código de 0.3.0 CI nativa, adaptadores, rechazo de modificaciones experimentales, regresiones fijas de perfiles 0.2.0 y paquetes nativos de vista previa. Defino umbrales iniciales de rendimiento en el [contrato](FILE_OPERATION_CONTRACT.md); las bases medidas de las adaptaciones estables siguen pendientes. La aceptación final de paquetes es independiente de la implementación.


Entrego este trabajo en PRs acotados, con comprobaciones superadas antes de integrar cada uno:

1. **CI nativa y cobertura visible:** ejecuto las pruebas compartidas de dominio, persistencia, supervisión e interfaz sin ventana en los tres sistemas. Separo pruebas nativas de Windows y contratos portables, indico las pruebas no aplicables y evito confundir retornos anticipados con validación nativa. Establezco ubicaciones desechables compatibles con las reglas de rutas de cada plataforma.
2. **Servicios y selección de capacidades:** separo descubrimiento de carpetas, mostrar en el gestor de archivos, ubicación de perfiles, inicio de sesión y comportamiento de ventana/bandeja mediante contratos pequeños. Selecciono adaptadores al construir la aplicación y mantengo las comprobaciones del sistema fuera del dominio. Oculto o explico acciones no disponibles; nunca oculto la única ventana sin una forma funcional de recuperarla.
3. **Base de seguridad de archivos:** defino el contrato por volumen y bloqueo las modificaciones nativas no aceptadas. Conservo la política Windows en 0.3.0 e implemento la resolución por volumen junto con cada adaptación antes de activar sus operaciones; detallo el [contrato revisado](FILE_OPERATION_CONTRACT.md). Defino identidad, colisiones Unicode/mayúsculas, movimientos sin sobrescritura, carreras de escritura/sustitución, enlaces y montajes, conservación de metadatos y recuperación de copias interrumpidas entre volúmenes. Demuestro los contratos con muestras desechables y rechazo operaciones no soportadas; no presento la alternativa actual fuera de Windows como segura sin validar.
4. **Persistencia y prevención de regresiones:** verifico actualización de perfiles de 0.2.0, conservación de idiomas e historial, respaldo/restauración, instancia única y cierre limpio. Llevar un perfil a otro sistema es una migración distinta porque sus rutas dependen de la plataforma; no prometo portabilidad copiando archivos SQLite.
5. **Paquetes experimentales y revisión:** genero artefactos macOS/Linux identificables con commit, hashes y limitaciones; compruebo inicio, dependencias nativas y ciclo de vida. Registro versiones, arquitectura, sistema de archivos, escritorio/backend, resultados y bloqueos antes de cerrar 0.3.0. Los paquetes experimentales quedan para muestras desechables hasta superar la aceptación nativa de seguridad.

Centro esta primera versión en las bases de la adaptación. No incorporo por defecto detección de duplicados, clasificación nueva, actualización automática ni supervisión de varias carpetas. Reviso la evidencia de 0.3.0 antes de abrir la aceptación estable de macOS.

### Calidad en todos los hitos

Pruebo vista previa y cancelación, organización/supervisión, Conservar/Posponer/Mover, Deshacer compatible, historial, reinicio y recuperación de operaciones interrumpidas en ambos idiomas. Incluyo orígenes bloqueados o cambiantes, destinos ocupados, disco lleno, permisos denegados, volúmenes no disponibles y copias interrumpidas. Conservo copias ambiguas y bloqueo acciones cuya seguridad no pueda establecer.

Durante 0.3.0 fijo cargas representativas y presupuestos medibles de inicio, vista previa, historial, memoria y supervisión; después controlo regresiones en cada destino soportado. Repito las comprobaciones de Windows afectadas por cambios compartidos y no doy por aceptados nuevos archivos con las pruebas de una versión anterior.

Mantengo la papelera nativa desactivada durante las adaptaciones. Antes de 1.0 implemento una cuarentena recuperable diseñada y aceptada por separado, o excluyo expresamente papelera/cuarentena de las funciones soportadas y ajusto interfaz y documentación. No reactivo una función insegura para declarar paridad. Conservo intactas las evidencias, checkpoints y pruebas de capacidad fallidas.

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

## Ideas adicionales sin versión asignada

No asigno fechas ni versiones a estas ideas; no amplían por defecto los hitos de plataforma:

- Detección de duplicados y análisis del almacenamiento.
- [Cuarentena recuperable](RECYCLE_CAPACITY_INVESTIGATION.md), en el mismo volumen y sin expulsión automática.
- Reglas más completas y organización virtual.
- Supervisión simultánea de varias carpetas.
- Seguimiento de renombrados externos y aceptación explícita de contenido editado.
- Consultas incrementales e índices para perfiles grandes.
- Idiomas adicionales al inglés y español. La accesibilidad forma parte de los criterios de versión anteriores.

Mantengo fuera del MVP la clasificación por contenido, sincronización en la nube, scripts arbitrarios y eliminación permanente automática. Los binarios futuros requieren nueva aceptación; las pruebas de v0.1.0 no los aprueban automáticamente.
