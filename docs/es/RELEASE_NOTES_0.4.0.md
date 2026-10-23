# Dowlume 0.4.0

[English](../RELEASE_NOTES_0.4.0.md) · [Español](#)

Publiqué el hito macOS Apple Silicon como [v0.4.0](https://github.com/mglezz/dowlume/releases/tag/v0.4.0) el 23 de octubre de 2026 tras confirmar el funcionamiento final en macOS y Windows. Promocioné los paquetes de aplicación exactos probados, sin recompilarlos. Registro revisión, hashes, controles automáticos y confirmación manual en la [aceptación de 0.4.0](RELEASE_CANDIDATE_0.4.0.md).

## Cambios

- Habilito organización, supervisión, envejecimiento/revisión, Conservar, Posponer, Mover, Deshacer elegible y recuperación del diario en macOS 15+ ARM64, para carpetas APFS locales del mismo volumen.
- Uso renombrado atómico exclusivo, descriptores verificados de archivos/carpetas e identidad persistente de volumen/archivo. Conservo los archivos ambiguos y muestro su ubicación observada en Actividad cuando está disponible.
- Añado menús nativos de aplicación/Dock, Comando-coma para Ajustes, reapertura y salida ordenada. Cerrar la ventana termina tras esperar al trabajo en curso; no hay modo de bandeja en macOS.
- Añado inicio de sesión opcional para una `.app` instalada y copio el perfil cerrado de vista previa 0.3.0 al nuevo perfil `Dowlume`, sin eliminar el original.
- Empaqueto adaptador nativo, icono, recursos inglés/español y guías sin conexión. Pruebo el binario macOS extraído, incluido el flujo nativo completo y las pestañas en ambos idiomas.
- Conservo las funciones de Windows y pruebo la actualización desde el instalador publicado 0.3.0. Linux sigue como vista previa restringida para el hito 0.5.0.

Clasifico `.dmg` y `.pkg` como Instaladores en perfiles nuevos. Conservo las reglas migradas; puedes añadir esas extensiones en Ajustes si faltaban. Los paquetes de aplicaciones son directorios y no se importan como descargas.

## Límites

Rechazo movimientos entre volúmenes, enlaces duros o simbólicos, almacenamiento de red o que no sea APFS y ubicaciones conocidas gestionadas por la nube. La detección no certifica todos los proveedores de sincronización; solo admito carpetas locales ordinarias fuera de esos servicios. Los escritores POSIX no están obligados a respetar bloqueos cooperativos. Verifico tras renombrar y conservo los archivos si detecto una carrera; no prometo excluir cualquier escritor ni revertir automáticamente. Consulta el [contrato](FILE_OPERATION_CONTRACT.md).

La papelera nativa sigue desactivada. Excluyo Mac Intel. Aplazo Developer ID, notarización y firma de editor Windows hasta la estabilización final; macOS tiene firma ad hoc. La instalación puede requerir la aprobación explícita del sistema para un editor sin firma. No elimino la cuarentena ni desactivo Gatekeeper.

Para instalación, migración, respaldo y desinstalación, consulta la [guía](USER_GUIDE.md#macos-040).
