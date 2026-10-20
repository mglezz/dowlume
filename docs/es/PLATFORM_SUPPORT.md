# Compatibilidad de plataformas

[English](../PLATFORM_SUPPORT.md) · [Índice en español](README.md)

Conservo [0.2.0](RELEASE_CANDIDATE_0.2.0.md) como versión aceptada de Windows mientras preparo 0.3.0. El código de 0.3.0 incluye adaptadores, CI nativa y paquetes experimentales. No declaro soporte estable de macOS/Linux: sus operaciones sobre archivos siguen bloqueadas. Consulta la [hoja de ruta](ROADMAP.md) y las [notas de 0.3.0](RELEASE_NOTES_0.3.0.md).

## Matriz

| Destino | Alcance de 0.3.0 | Requisito pendiente |
| --- | --- | --- |
| Windows x64 | Operaciones, ubicación del perfil, Explorer, bandeja e inicio de sesión existentes; instalador y ZIP portátil | Repetir la aceptación afectada sobre los paquetes finales de 0.3.0, incluida actualización desde 0.2.0. |
| macOS 15, Apple Silicon | CI ARM64 nativa; `.app` autocontenida en ZIP; vista previa, ajustes, ambos idiomas, historial y Finder; perfil separado | 0.4.0: seguridad nativa, APFS sensible/insensible a mayúsculas y Unicode, permisos, ciclo de vida, distribución firmada/notarizada y aceptación de instalación/actualización. Otras versiones de macOS requieren evidencia propia. |
| macOS Intel | Excluido | Sin implementación, CI ni paquete x64. Podría reconsiderarlo posteriormente con aceptación propia. |
| Ubuntu 24.04 x64 | CI nativa; `.tar.gz` autocontenido; funciones de vista previa; Descargas XDG; abrir carpeta contenedora; arranque X11 bajo Xvfb | 0.5.0: seguridad ext4, escritorio GNOME, permisos, inicio automático, integración y actualización; XWayland y Wayland nativo requieren pruebas separadas. |
| Otros destinos Linux | Excluidos inicialmente | Evidencia propia para otras distribuciones, escritorios y arquitecturas. |

Los archivos macOS/Linux son vistas previas de desarrollo, no instaladores aceptados. El bundle macOS tiene firma ad hoc, sin Developer ID ni notarización. Verifico la firma antes y después de extraer el ZIP y ejecuto la aplicación extraída con un perfil desechable. Linux arranca bajo Xvfb: esto no valida una sesión real de GNOME, diálogos, gestor de archivos ni accesibilidad. Ambos informes dejan explícitamente pendientes la aceptación manual y la de equipo limpio. No pido desactivar la seguridad del sistema.

La [matriz de Avalonia](https://docs.avaloniaui.net/docs/supported-platforms) y los [requisitos de distribución de Apple](https://developer.apple.com/documentation/security/notarizing-macos-software-before-distribution) orientan el soporte futuro; no demuestran la seguridad nativa de Dowlume. Mantengo la matriz más limitada anterior hasta disponer de evidencia directa.

## Bases implementadas

Selecciono inicio, gestor de archivos, perfil y bandeja en `DesktopPlatform`. Windows conserva `%LOCALAPPDATA%\Dowlume`; macOS utiliza `~/Library/Application Support/Dowlume.Preview`; Linux usa los datos locales de .NET, normalmente `$XDG_DATA_HOME/Dowlume.Preview` o `~/.local/share/Dowlume.Preview`. No importo perfiles Windows automáticamente: las rutas guardadas dependen de su sistema original.

Leo `user-dirs.dirs` de Linux como datos acotados y expando únicamente la forma admitida de `$HOME`; nunca lo ejecuto como script. Windows conserva su API de carpetas conocidas; macOS sugiere actualmente `~/Downloads` y permite elegir otra carpeta. Los comandos del gestor reciben argumentos literales sin shell. Linux abre la carpeta contenedora porque no existe una selección de archivos portátil.

No ofrezco ocultar en bandeja ni inicio automático en las vistas previas. Si falla la creación de la bandeja Windows, conservo una ventana visible y registro el error. Al cerrar espero el trabajo pendiente y libero el bloqueo del perfil.

`IFileMover` separa coordinación y modificación. `SafeFileMover` rechaza movimientos fuera de Windows antes de crear directorios. El organizador también bloquea organización, reconciliación, decisiones de revisión, Deshacer y recuperación antes de registrar operaciones o modificar archivos. La vista previa no reproduce operaciones pendientes en estos sistemas. Las preferencias y el perfil SQLite aislado sí se pueden guardar. El [contrato de operaciones](FILE_OPERATION_CONTRACT.md) recoge el trabajo nativo pendiente.

## Evidencia e interpretación de pruebas

Ejecuto pruebas compartidas en máquinas nativas Windows x64, macOS ARM64 y Ubuntu x64. Cinco pruebas exclusivas de Windows figuran como omitidas fuera de Windows; la regresión Linux de nombres diferenciados por mayúsculas se omite explícitamente en otros sistemas. Fuera de Windows, las pruebas de coordinación utilizan un adaptador desechable exclusivo del ensamblado de pruebas: no lo distribuyo ni demuestra protección frente a carreras nativas. Pruebo por separado el rechazo del adaptador de producción. Las muestras fijas del esquema 1 comprueban ajustes inglés/español de 0.2.0, reglas propias, historial y copias de bases cerradas.

Cada paquete incluye revisión, estado limpio/modificado, runtime, entorno, hashes e informe de arranque desde el archivo. Solo considero candidatos los paquetes construidos desde código confirmado y limpio. CI conserva entorno/SDK e informes TRX. Repito las pruebas Windows contra el instalador 0.2.0 fijado por hash. La aceptación manual anterior no aprueba binarios nuevos.

## Compilación y dependencias

Compilo en el sistema nativo con el SDK .NET 10 del repositorio y Python 3.13:

```sh
python3 scripts/Build-PlatformPreview.py --runtime osx-arm64 --output artifacts/macos-preview
# En Ubuntu 24.04 x64:
python3 scripts/Build-PlatformPreview.py --runtime linux-x64 --output artifacts/linux-preview
```

La carpeta de salida no debe existir. El script empaqueta desde un directorio aislado, extrae el archivo y comprueba arranque y cierre nativos. macOS necesita las herramientas de línea de comandos de Xcode para `codesign` y `ditto`. CI Linux instala `xvfb`, `xauth`, `libx11-6`, `libice6`, `libsm6`, `libfontconfig1`, `libgl1`, `libegl1`, `libxrandr2`, `libxi6`, `libxcursor1`, `libxrender1`, `libxext6` y `libxfixes3`. Mostrar carpetas requiere además `/usr/bin/xdg-open` (`xdg-utils`) y un gestor de escritorio funcional. Incluir .NET no elimina las dependencias nativas.

## Requisitos de las adaptaciones

Antes de activar modificaciones valido identidad por volumen, Unicode/mayúsculas, colisiones, no sobrescritura, escritores y sustituciones, enlaces/montajes, metadatos, permisos denegados, disco lleno y recuperación interrumpida entre volúmenes. Antes de distribuir versiones estables acepto ambos idiomas, diálogos, gestor, reinicio, cierre, rechazo de instancias duplicadas, inicio opcional, instalación, actualización y desinstalación sobre los paquetes exactos. Empiezo Linux con Ubuntu 24.04 GNOME; una prueba X11 no acepta Wayland nativo.

Mantengo desactivada la papelera nativa. Conservo evidencias y checkpoints y no repito las pruebas de capacidad retiradas. Red, marcadores de nube, enlaces e importación de perfiles entre sistemas siguen fuera de soporte.
