# Aceptación de Dowlume 0.4.0

[English](../RELEASE_CANDIDATE_0.4.0.md) · [Español](#)

Publiqué [0.4.0](https://github.com/mglezz/dowlume/releases/tag/v0.4.0) para macOS Apple Silicon y Windows el 23 de octubre de 2026 tras aceptar el candidato exacto en ambas plataformas. La firma sigue aplazada. Distingo a continuación evidencia automática y confirmación funcional manual final.

## Alcance

Dirijo esta versión a macOS 15+ ARM64 y carpetas APFS locales ordinarias del mismo volumen. Incluyo vista previa, organización, supervisión, revisión, Conservar, Posponer, Mover, Deshacer elegible, recuperación, historial, ajustes, inglés/español, Finder, menús nativos e inicio opcional. Rechazo operaciones entre volúmenes, nube, red y enlaces; Linux sigue restringido y la papelera desactivada. Consulta [notas](RELEASE_NOTES_0.4.0.md) y [garantías](FILE_OPERATION_CONTRACT.md).

## Evidencia automatizada

Ejecuto batería Release completa, formato, analizadores/dependencias, matriz nativa en dos imágenes APFS nuevas y flujo del paquete extraído. Los informes identifican revisión, estado del árbol, sistema y hashes. No uso paquetes de desarrollo con cambios sin confirmar como candidatos.

Las pruebas locales en macOS 26.6.2 ARM64, SDK 10.0.401, superaron el flujo admitido y ambas políticas APFS, incluido volumen desechable de 256 MiB lleno. Verifico conservación ante carreras/colisiones, atributos extendidos/permisos/fechas, rechazo de enlaces duros, disponibilidad nativa, permisos denegados, supervisión/cierre, recuperación con base reabierta y rechazo entre volúmenes antes del diario. CI aporta evidencia separada de macOS 15, Windows y Linux. Conservo intactas versiones históricas y pruebas retiradas de papelera.

El script macOS firma ad hoc, extrae el ZIP, vuelve a verificarlo y prueba su ejecutable real con un perfil desechable nuevo obligatorio. El informe cubre pestañas en ambos idiomas, persistencia, ciclo nativo y Deshacer, base reabierta y cierre del supervisor. No afirma instalación manual, inicio real de sesión ni máquina limpia.

## Entrega de la candidata — 2026-10-23

Publiqué primero [v0.4.0-rc.1](https://github.com/mglezz/dowlume/releases/tag/v0.4.0-rc.1) como versión preliminar. Genero todos los paquetes desde la revisión limpia [`e51f59356612a77c87a5ada69300b523264415a4`](https://github.com/mglezz/dowlume/commit/e51f59356612a77c87a5ada69300b523264415a4), tras integrar [implementación macOS #33](https://github.com/mglezz/dowlume/pull/33) y [corrección del cierre #34](https://github.com/mglezz/dowlume/pull/34). Los commits posteriores de documentación no modifican estos binarios. Después promocioné los paquetes de aplicación aceptados a 0.4.0 sin recompilarlos.

| Comprobación de la revisión final | Resultado |
| --- | --- |
| [Compilación, pruebas y formato](https://github.com/mglezz/dowlume/actions/runs/39914334936) | macOS: 107 superadas / 8 omitidas; Windows: 86 / 29; Linux: 82 / 33. Ningún fallo. Las omisiones corresponden a otras plataformas. |
| Matriz APFS nativa, misma ejecución | 25/25 superadas con cada política de mayúsculas; macOS 15.7.9 ARM64. |
| [Seguridad](https://github.com/mglezz/dowlume/actions/runs/39914334926) | Superada. |
| [Paquetes nativos](https://github.com/mglezz/dowlume/actions/runs/39953171400) | Superados el flujo macOS extraído y el arranque de la vista previa restringida Linux. |
| [Paquete Windows](https://github.com/mglezz/dowlume/actions/runs/39953167507) | Superados actualización desde el instalador aceptado 0.3.0, idiomas, arranque portátil, protección con app abierta y desinstalación. |

Descarté el intento anterior desde `032df79`: una prueba de escritorio eliminaba su perfil temporal antes de que el cierre asíncrono liberara SQLite. Corregí la limpieza para esperar al cierre real e impedí que una actualización inicial tardía reactivara el temporizador durante la salida. La ejecución final indicada supera las pruebas con esa corrección.

Adjunto información de compilación, informes de aceptación, `Dowlume-0.4.0-validation.zip` y `SHA256SUMS.all.txt` a la publicación. El informe local del archivo extraído es `ACCEPTANCE.osx-arm64-local.json`; separo su resultado automático de la instalación manual y del inicio real de sesión.

### SHA-256 de los paquetes

| Archivo / File | SHA-256 |
| --- | --- |
| `Dowlume-0.4.0-osx-arm64.zip` | `27cb13f73fdd034164c5aafa5b428e957912658ce9abfb1ef903febb7300de71` |
| `Dowlume-0.4.0-win-x64-setup.exe` | `24e13f5773b5e07010bf31afdc6a9041635c9f89e999f4fd978857d33ea47732` |
| `Dowlume-0.4.0-win-x64.zip` | `f31c9a3cab722c1e630b4688426ae32b0d1710780cca15e711af03dd7b2e86f0` |
| `Dowlume-0.4.0-linux-x64-preview.tar.gz` | `1e21bdb6795c2d27549c51a9cb1c504821d5334d050b97959d31ff980a7a1d21` |

## Lista del paquete exacto

Solicité comprobar el candidato identificado por hash con esta lista. Registro debajo la confirmación global; no invento mediciones ni evidencias separadas para cada punto:

- macOS: extraer e instalar `.app` completa, revisar avisos Gatekeeper/permisos con la firma ad hoc real y verificar acceso ordinario/denegado con muestras desechables.
- macOS: actualizar un perfil cerrado de vista previa 0.3.0 conservando ajustes, idioma e historial; verificar reinicio, selector/Finder, Comando-coma, Ocultar/reabrir desde Dock, rechazo de segunda instancia, cancelar confirmaciones y Salir supervisando.
- macOS: activar inicio automático desde la app instalada, cerrar/iniciar sesión, confirmar apertura pausada, desactivarlo y verificar retirada del registro propio. Desinstalar conserva perfil y archivos.
- Windows: repetir actualización desde 0.3.0, idiomas tras reiniciar, organización/revisión/Deshacer, bandeja y cierre con el paquete nuevo. Separo automatización alojada y confirmación manual.

No reutilizo la confirmación de 0.3.0. El 23 de octubre de 2026 confirmé que el nuevo candidato funcionaba correctamente en macOS y Windows y continué con la publicación estable.

## Aceptación y publicación estable — 2026-10-23

Tras recibir la lista del candidato exacto, confirmé las pruebas satisfactorias en Mac y Windows y autoricé continuar la publicación. Es una confirmación funcional global, no un resultado registrado por separado para cada punto. No aporté nuevas versiones detalladas del sistema, evidencia de máquina limpia, capturas ni registros por paso; no deduzco esos datos. Los JSON automáticos conservan sus campos originales `ManualDesktopAcceptance: false` porque describen automatización, no esta confirmación posterior.

Publico [v0.4.0](https://github.com/mglezz/dowlume/releases/tag/v0.4.0) desde la misma revisión `e51f593` y conservo idénticos el instalador, archivos de aplicación, kit de aceptación, información de compilación y evidencia automática. Solo finalizo las notas independientes y sus manifiestos de hashes afectados tras la aceptación. La documentación dentro de los paquetes aceptados describe el estado previo a publicar; este registro y las notas independientes recogen el estado final. Conservo la candidata como evidencia histórica y marco 0.4.0 como última estable.
