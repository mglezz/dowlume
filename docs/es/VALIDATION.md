# Registro de validación

[English](../VALIDATION.md) · [Índice en español](README.md)

Mantengo registros fechados y vinculados a revisiones. Un pendiente en una entrada antigua describe ese momento; las entradas posteriores registran su cierre sin alterar evidencias. Las rutas bajo `artifacts/` identifican material privado conservado y pueden no existir en una clonación nueva.

Esta página recoge en español la cronología y los límites de los resultados. Conservo las tablas originales, comandos exactos, identificadores de ejecuciones y rutas detalladas en el [registro inglés](../VALIDATION.md).

## 19 de octubre de 2026: localización y revisión documental

Añadí selección inglés/español por perfil, con inglés inicial para perfiles antiguos y sin cambiar nombres de carpetas, contenido, reglas ni historial persistido. Comprobé reinicio, cambios con ajustes pendientes, fallo al guardar y reintento, actualización de pantallas y bandeja, errores traducidos y cancelación de confirmación en español tras cambiar el idioma con una vista previa existente. Comparé claves y argumentos de ambos recursos y las etiquetas del dominio.

En macOS con SDK .NET 10.0.401 superé la compilación Release y **74 pruebas, sin omisiones**, además de formato, auditoría de dependencias y `git diff --check`. Usé una carpeta temporal real mediante `TMPDIR` porque la predeterminada de macOS atraviesa un enlace simbólico; no relajé las protecciones. Inspeccioné capturas de las cinco pestañas españolas a 880 × 680 y corregí el ajuste de encabezados. Las capturas privadas quedan ignoradas porque incluyen rutas locales.

Añadí 20 páginas españolas y comprobé enlaces y encabezados de las 40 páginas de ambos idiomas. Recuperé la documentación de publicación pendiente mediante el [PR #23](https://github.com/mglezz/dowlume/pull/23), integrado tras superar CI, y contrasté los metadatos públicos de v0.1.0. Añadí guía rápida española y comprobación del recurso español al empaquetar.

Esto no equivale a aceptación manual de Windows ni a publicar otra versión. Los binarios y etiqueta v0.1.0 permanecen intactos y en inglés. Los controles nativos de carpetas siguen Windows; un fallo anterior a leer preferencias usa inglés y los mensajes históricos desconocidos conservan su texto. Los futuros paquetes localizados necesitan aceptación específica.

## 18 de octubre de 2026: publicación y aceptación final

Autoricé la publicación de v0.1.0 y su etiqueta después de completar los controles. Se publicó a las `2026-10-18T17:00:48Z` desde `94ba177ea0da5e4809f87a59edfe5c7893b65537`. Los seis archivos públicos se descargaron sin autenticación y coincidieron con tamaños, hashes locales, digests de GitHub y manifiesto publicado.

Confirmé la aceptación humana y revisión independiente de esos artefactos. La confirmación no aporta identidades de revisores ni registros manuales individuales adicionales. La distingo de las pruebas automatizadas.

El candidato limpio con papelera desactivada superó 69 pruebas, seguridad, empaquetado y primer uso en el anfitrión. Repetí aceptación en un Windows 11 Pro 26300 separado, cuenta estándar y sin .NET compartido. Conservo informes recuperados en modo de solo lectura y hashes del kit. La máquina de evidencia permaneció intacta. La [ficha del candidato](RELEASE_CANDIDATE_0.1.0.md) registra el alcance.

## 17 de octubre: inspección y contención

Revisé las dos ejecuciones existentes y el orden de restauración del arnés. No encontré limpieza ni destinos alternativos que explicasen las muestras ausentes. Correlacioné los cuatro metadatos supervivientes con muestras que sí habían sido restauradas, no con el contenido perdido.

Una copia verificada del checkpoint permitió acceso en memoria mediante una clave clara de BitLocker existente. La inspección de archivos y registros MFT no recuperó las muestras. Distingo acceso al volumen de recuperación del contenido y conservo los límites de la búsqueda.

Decidí desactivar toda ejecución nativa de papelera en v0.1.0. Archivé fuentes originales sin cambiar bytes, mantuve los estados históricos y añadí regresiones de rechazo y conservación. No transformé los fallos nativos en pruebas superadas.

## 16 de octubre: Windows, historial y fallos de capacidad

Registré la preparación de Hyper-V, ISO oficial verificada e instalación posterior de Windows limpio. La aceptación automatizada del paquete `cf42326` fue satisfactoria. Las pruebas nativas ordinarias de reciclaje/restauración y rechazo con papelera desactivada pasaron, pero fallaron los casos de muestra sobredimensionada y expulsión por capacidad. Conservo dos ejecuciones, informes y tres checkpoints.

También registré mejoras de bandeja, inicio con Windows, empaquetado, diagnóstico, análisis de seguridad y protección ante carreras. Las pruebas entre discos NVMe/SATA incluyeron movimiento, deshacer e interrupción de copia, con origen y copia parcial conservados.

La búsqueda de Actividad añadió filtros por ruta, acción y estado, páginas de 50 y navegación estable frente a nuevas operaciones. Las pruebas correspondientes mantienen las comprobaciones de Deshacer y el contador global de atención.

## 15 de octubre: dominio y documentación

Analicé el dominio de la revisión `a4b1c9a` y preparé diez vistas UML con fuentes PlantUML, SVG, comprobación de sintaxis y revisión visual. No afirmo entrevistas con expertos ni verificación formal. Más tarde actualicé las vistas afectadas por la desactivación de la papelera.

Generé las capturas de la interfaz Avalonia con datos ficticios. Son ilustraciones de pantalla, no evidencia de aceptación de operaciones de archivos ni de diálogos nativos. Las capturas compartidas son de la interfaz inglesa anterior al selector.

## 14 de octubre: candidato inicial

Conservo la entrada inicial de compilación, dominio, supervisión, operaciones, recuperación, interfaz y empaquetado como evidencia de aquella revisión. Los resultados de versiones posteriores no cambian retrospectivamente sus limitaciones.

## Interpretar los resultados

No utilizo pruebas unitarias para dar por completadas aceptación humana, equipo limpio, firma o hardware real. Las pruebas sin pantalla no sustituyen bandeja, diálogos y escalado en Windows. La ausencia de avisos de seguridad no demuestra ausencia de todo defecto. Los cambios posteriores, incluida la localización, necesitan sus propias comprobaciones y aceptación antes de distribuir nuevos paquetes.
