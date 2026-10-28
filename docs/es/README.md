<div align="center">

# Dowlume

**Un poco de orden para tu carpeta de Descargas.**

Encuentra lo que necesitas. Revisa lo que olvidaste. Decide qué conservar.

[Descargar Dowlume](https://github.com/mglezz/dowlume/releases/tag/v0.4.0) · [Guía de uso](user/guide.md) · [English](../../README.md)

</div>

Las descargas se acumulan: un recibo, una foto, un instalador que necesitaste una vez. Dowlume les da un sitio y te ayuda a volver a los archivos que quizá habías olvidado.

Organiza las descargas completas en carpetas y lleva los archivos antiguos a una zona de revisión. Tú decides cuáles conservar y cuáles mover a otro lugar. Todo funciona en tu equipo, sin cuentas, sincronización en la nube ni telemetría.

![Dowlume con descargas organizadas y archivos pendientes de revisión](../images/overview.png)

*Interfaz inglesa con archivos ficticios. [Procedencia de las capturas](images/README.md).*

## Una rutina sencilla

| Comprueba los destinos | Deja que se organice | Decide a tu ritmo |
| --- | --- | --- |
| La vista previa muestra dónde irá cada archivo antes de empezar. | Las descargas completas se ordenan en seis categorías con reglas configurables. | Los archivos antiguos aparecen en **Expiring Files**. Puedes conservarlos, posponer la revisión o moverlos. |

Tú decides cuándo empieza la supervisión. Dowlume respeta las subcarpetas existentes, guarda un historial y permite deshacer los movimientos compatibles. No hay una fecha límite de borrado automático. El envío a la papelera está desactivado.

## Descargar

Los paquetes de **Dowlume 0.4.0** incluyen el runtime; no necesitas instalar .NET por separado.

| Plataforma | Descarga |
| --- | --- |
| macOS 15+ · Apple Silicon | [Portable](https://github.com/mglezz/dowlume/releases/download/v0.4.0/Dowlume-0.4.0-osx-arm64-portable.zip) |
| Windows · x64 | [Instalador (recomendado)](https://github.com/mglezz/dowlume/releases/download/v0.4.0/Dowlume-0.4.0-win-x64-setup.exe) · [Portable](https://github.com/mglezz/dowlume/releases/download/v0.4.0/Dowlume-0.4.0-win-x64-portable.zip) |

Empieza con copias de unos pocos archivos en una carpeta local y consulta la vista previa. La interfaz empieza en inglés; puedes seleccionar español en **Settings → Language**.

macOS utiliza firma ad hoc sin notarización; los paquetes Windows no tienen firma de editor. Consulta la [instalación](user/guide.md#instalar-y-abrir) y comprueba las [sumas SHA-256](releases/0.4.0.md#sumas-de-comprobación). Los movimientos en macOS requieren APFS local en el mismo volumen. La [prerelease 0.5.0-rc.1](https://github.com/mglezz/dowlume/releases/tag/v0.5.0-rc.1) permite probar el flujo Linux, instalador Ubuntu y paquetes portables. Supera automatización nativa y de paquetes; la [aceptación manual Linux](../development/linux-acceptance.md) sigue pendiente antes de publicar estable. La [matriz de plataformas](user/platform-support.md) detalla el alcance.

## Documentación en español

- [Guía de uso](user/guide.md): instalación, organización, revisión y recuperación.
- [Notas de 0.4.0](releases/0.4.0.md): cambios de la versión y su [aceptación](releases/acceptance/0.4.0.md).
- [Preparación del entorno](development/getting-started.md), [contribuciones](development/contributing.md) y [ramas](development/branches.md).
- [Arquitectura](architecture/overview.md), [contrato de archivos](architecture/file-operations.md) e [implementación macOS](architecture/macos.md).
- [Modelo de dominio](requirements/domain-model.md) y [hoja de ruta](development/roadmap.md).
- [Publicación](development/releasing.md), [aceptación Windows](development/windows-acceptance.md) y [preparación Hyper-V](development/hyper-v-acceptance.md).
- Registros históricos: [validación](archive/validation.md), [seguridad](archive/security-review.md) e [investigación de la papelera](archive/recycle-capacity-investigation.md).

El inglés es el idioma principal del proyecto. El [índice completo](../README.md) reúne también las convenciones del repositorio y el historial de versiones.

Dowlume es código abierto bajo la [licencia MIT](../../LICENSE).
