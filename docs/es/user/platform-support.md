# Compatibilidad por plataforma

[English](../../user/platform-support.md) · [Español](#)

Publico [0.4.0](../releases/acceptance/0.4.0.md) como estable para macOS Apple Silicon y Windows dentro del alcance siguiente. Confirmé rc.1 manualmente en ambas plataformas y acepté la corrección de cierre de rc.2 mediante regresiones nativas y automatización del paquete final. Los binarios macOS/Linux 0.3.0 siguen siendo vistas previas restringidas y Linux 0.4.0 publicado continúa restringido. El código de 0.5.0 activa las operaciones Linux dentro del alcance candidato siguiente; supera CI nativa y automatización del paquete, con aceptación manual final pendiente. Consulta las [notas](../releases/0.4.0.md).

| Plataforma | Implementación actual | Distribución y aceptación |
| --- | --- | --- |
| Windows x64 | Organización, supervisión, revisión, Deshacer elegible, recuperación, Explorador, bandeja e inicio de sesión | Instalador y ZIP portátil; superé actualización automatizada desde 0.3.0 fijada por hash con confirmación funcional manual de rc.1 y aceptación automática de regresiones y paquetes de la corrección de cierre de rc.2. |
| macOS 15+ Apple Silicon | Flujo admitido completo en APFS local del mismo volumen; diálogos/Finder, menús de aplicación/Dock, reapertura/salida, inicio opcional, ambos idiomas y migración de perfil de vista previa | `.app` autocontenida en ZIP con firma ad hoc. CI nativa usa macOS 15; también pruebo localmente en 26.6.2. Registro la confirmación funcional final y sus límites de evidencia por separado de la automatización. |
| macOS Intel | Excluido | Sin paquete x64 ni declaración de soporte. |
| Ubuntu 24.04 x64 (candidato 0.5.0) | Flujo nativo en ext4 local del mismo montaje, recuperación por identidad, Descargas XDG, inicio opcional y gestor de archivos; sin bandeja | Preparados `.deb` y `.tar.gz` autocontenidos. Superadas ejecución nativa y prueba del portable extraído en CI; pendientes instalación/actualización y GNOME X11/Wayland reales. Consulta la [aceptación Linux](../../development/linux-acceptance.md). |
| Fedora / Arch x64 | Mismo portable glibc y restricciones ext4, sin lista cerrada de distribuciones | Sin validar; todavía sin RPM/AUR. Se rechazan Btrfs/XFS y otros sistemas de archivos. Se necesita evidencia separada antes de ampliar el soporte oficial. |

Mantengo la papelera nativa desactivada en todos los sistemas. Conservo evidencia histórica y no repito pruebas retiradas de capacidad. No importo perfiles entre sistemas operativos.

## Prioridades funcionales y firma aplazada

Completo los flujos Windows, macOS Apple Silicon y Linux antes de la firma de editor y notarización macOS en estabilización final. La firma no es requisito de los hitos funcionales 0.4.0/0.5.0. Conservo firma ad hoc en macOS, declaro su estado y la verifico antes/después de extraer. No elimino cuarentena ni desactivo Gatekeeper. Registro por separado seguridad nativa, aceptación funcional y restricciones de instalación.

## Alcance macOS

Admito carpetas APFS locales ordinarias fuera de servicios de sincronización. Origen y destino deben estar en el mismo volumen. Rechazo transferencias entre volúmenes, enlaces simbólicos/duros, archivos no regulares, rutas/atributos conocidos de nube, almacenamiento de red/no APFS y volúmenes ausentes antes de preparar el movimiento. No reconocer una carpeta como nube no certifica su uso con sincronización. Uso identidad nativa persistente para pertenencia, renombrado exclusivo sin sobrescribir e identidad más SHA-256 para recuperación. Los bloqueos cooperativos no excluyen cualquier escritor POSIX; las carreras detectadas conservan archivos y requieren atención. Consulta [implementación](../architecture/macos.md) y [contrato](../architecture/file-operations.md).

Guardo datos Windows en `%LOCALAPPDATA%\Dowlume`, macOS en `~/Library/Application Support/Dowlume` y Linux 0.5.0 en `$XDG_DATA_HOME/Dowlume` o `~/.local/share/Dowlume`. El perfil Linux anterior `Dowlume.Preview` permanece intacto sin migración automática. La primera apertura macOS copia un perfil cerrado `Dowlume.Preview` solo si no existe el nuevo; conservo el origen. `--data-directory` evita migración y registro de inicio de sesión.

Cerrar la ventana macOS termina tras esperar al trabajo. Ocultar nativo y reabrir desde Dock funcionan mientras está abierta. El inicio opcional usa un LaunchAgent por usuario para una app instalada, con marca de propiedad y argumentos literales. Se aplica al siguiente inicio de sesión y los ajustes del sistema pueden anularlo. Windows conserva bandeja e inicio por registro. Linux cierra tras completar el trabajo activo y no tiene bandeja. El inicio opcional usa `$XDG_CONFIG_HOME/autostart/com.mglezz.dowlume.desktop` (por defecto `~/.config/autostart`) y la supervisión empieza en pausa. Consulta la [implementación y límites Linux](../../architecture/linux.md).

## Evidencias y paquetes

Ejecuto pruebas compartidas en Windows x64, macOS ARM64 y Ubuntu x64. Los adaptadores de muestras no prueban seguridad nativa. Pruebas macOS separadas recorren lectura/movimiento en imágenes APFS desechables de ambas políticas de mayúsculas, incluido volumen lleno y carreras. Las exclusivas de Windows se omiten explícitamente fuera de él. Las muestras de esquema 1 conservan idiomas, ajustes, archivos e historial.

```sh
python3 scripts/Build-PlatformPreview.py --runtime osx-arm64 --output artifacts/macos-package
# En Ubuntu 24.04 x64:
python3 scripts/Build-Linux.py --output artifacts/linux-candidate
python3 scripts/Test-Linux.py --output artifacts/linux-native --archive artifacts/linux-candidate/Dowlume-0.5.0-linux-x64.tar.gz
```

Compilo en cada plataforma nativa con SDK .NET 10.0.401 y Python 3.13 en CI. macOS requiere herramientas de línea de comandos Xcode para adaptador C, icono, firma y archivo. Linux CI instala `xvfb`, `xauth`, `libx11-6`, `libice6`, `libsm6`, `libfontconfig1`, `libgl1`, `libegl1`, `libxrandr2`, `libxi6`, `libxcursor1`, `libxrender1`, `libxext6` y `libxfixes3`. Mostrar carpetas en Linux también requiere `/usr/bin/xdg-open` (`xdg-utils`) y gestor de archivos.

La salida debe ser nueva. Cada archivo incluye runtime, idiomas, guías sin conexión, revisión y datos de compilación. Los informes incluyen SHA-256 del archivo, estado del árbol y campos explícitos de aceptación manual/máquina limpia. El binario macOS extraído prueba ciclo nativo, supervisión, reapertura del diario y pestañas inglés/español en un perfil nuevo desechable. Solo compilaciones limpias confirmadas son candidatas; la automatización no implica aceptación de inicio real de sesión ni máquina limpia.
