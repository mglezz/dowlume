# Implementación de macOS para 0.4.0

[English](../MACOS_IMPLEMENTATION.md) · [Español](#)

Dirijo esta versión a macOS 15+ en Apple Silicon, con carpetas APFS locales del mismo volumen. Implemento el flujo admitido completo: vista previa, organización, supervisión, envejecimiento/revisión, Conservar, Posponer, Mover, Deshacer elegible, historial, ajustes, ambos idiomas y recuperación. Conservo Windows y mantengo desactivadas las mutaciones en Linux. Excluyo Intel y papelera nativa.

## Operaciones nativas

Abro rutas con `O_NOFOLLOW_ANY`, `O_CLOEXEC` y `O_NONBLOCK` de Darwin, inspecciono `stat` ARM64 y rechazo enlaces, archivos no regulares, marcadores sin datos y destinos de mutación con enlaces duros. Compruebo APFS, montaje local, rutas conocidas de nube y atributos de proveedor. No puedo reconocer todos los sincronizadores; las carpetas admitidas deben estar fuera de estos servicios.

Preparo el movimiento antes de escribir Pending: abro origen y carpetas padre existentes, exijo el mismo volumen APFS, verifico SHA-256 e identidad y adquiero un `flock` cooperativo. La identidad persistente combina UUID de volumen, inodo y fecha de nacimiento. Creo componentes de destino relativos a los descriptores retenidos y llamo a `renameatx_np` con `RENAME_EXCL`. Nunca sustituyo destinos existentes, incluidos alias APFS de mayúsculas/Unicode. Sincronizo las carpetas y verifico identidad, contenido y rutas padre. El renombrado conserva el objeto original y sus metadatos; no recurro a copiar/borrar.

Uso un pequeño adaptador C ARM64 para la llamada variádica `fcntl(F_GETPATH)`, compilado con el SDK de macOS e incluido en el paquete. Si una operación falla, registro la ruta observada del objeto abierto cuando está disponible para localizar archivos desde Actividad tras una sustitución de carpeta.

Los bloqueos cooperativos no excluyen cualquier escritor POSIX. Una sustitución justo antes del renombrado puede mover el objeto sustituto al destino; la verificación detecta la discrepancia, conserva los objetos y registra NeedsAttention. Nunca borro ni revierto usando una ruta incierta. Excluyo del modelo admitido la mutación hostil por el mismo usuario y no prometo congelar el espacio de nombres. Detallo este límite en el [contrato](FILE_OPERATION_CONTRACT.md).

La recuperación nunca repite el movimiento. Solo completo el diario interrumpido si no existe origen y el destino coincide en identidad y contenido. Un origen intacto sin destino pasa a Failed y permite reintentar. Una identidad ausente o archivos ambiguos requieren atención; en macOS no doy por completados registros antiguos solo por sus bytes. Mantengo compatibilidad con esquema SQLite 1 mediante campos JSON opcionales.

## Escritorio y migración

Uso selectores nativos y mostrar en Finder. El menú incluye Abrir y Ajustes (Comando-coma), junto a Ocultar/Servicios/Salir nativos; la reapertura desde Dock restaura la ventana. Cerrar o salir espera al trabajo, cierra confirmaciones y libera el bloqueo de perfil. No ofrezco ocultar en bandeja en macOS.

Para una `.app` instalada, el inicio opcional crea un LaunchAgent por usuario con argumentos literales y marca de propiedad. No sustituyo ni elimino trabajos ajenos. Se aplica al siguiente inicio de sesión; los ajustes de macOS pueden desactivarlo. Las ejecuciones de desarrollo o con perfil personalizado no pueden cambiar el inicio de sesión personal.

Guardo el perfil en `~/Library/Application Support/Dowlume`. Antes de crearlo por primera vez copio el perfil cerrado `Dowlume.Preview` mediante una carpeta temporal privada, verifico la base copiada y renombro la carpeta completa al destino. Conservo el original, rechazo origen abierto o enlaces y no fusiono con un destino existente. La supervisión comienza pausada.

## Comprobaciones reproducibles

```sh
mkdir -p artifacts/test-tmp
TMPDIR="$PWD/artifacts/test-tmp" dotnet test Dowlume/Dowlume.slnx -c Release
python3 scripts/Test-MacFileReads.py --output artifacts/mac-file-operations
python3 scripts/Build-PlatformPreview.py --runtime osx-arm64 --output artifacts/macos-package
```

Los directorios de salida deben ser nuevos. El script crea dos imágenes privadas dispersas de 256 MiB, APFS y APFS sensible a mayúsculas, activa propiedad y ejecuta como usuario ordinario. Cubre lectura, identidad, enlaces/FIFO, alias, sustituciones, escritores, colisiones, permisos, ciclo completo/reinicio/recuperación, supervisión, rechazo entre volúmenes y un volumen desechable realmente lleno. Solo desmonta sus imágenes, no fuerza desmontajes y conserva la prueba si falla el desmontaje. No repite investigaciones retiradas de capacidad de papelera.

Conservo TRX y `ACCEPTANCE.mac-file-reads.json` con revisión, estado del árbol, sistema, arquitectura y SDK. CI ejecuta lo mismo en macOS 15. La evidencia local en macOS 26.6.2 complementa, sin sustituir, ese entorno base.

El empaquetado verifica firma ad hoc antes y después de extraer el ZIP. La aplicación autocontenida extraída exige un perfil desechable nuevo, recorre las pestañas en inglés/español, verifica persistencia del idioma, ejecuta organización/revisión/Posponer/Conservar/Mover/Deshacer y reapertura del diario, inicia supervisión real y espera a su cierre. El informe vincula las pruebas al hash del archivo. No afirma aceptación humana de escritorio, inicio real de sesión ni máquina limpia; los registro en la [aceptación](RELEASE_CANDIDATE_0.4.0.md).

Aplazo Developer ID y notarización hasta estabilización final. Documento firma ad hoc y límites de instalación; los scripts no quitan cuarentena ni desactivan Gatekeeper. Conservo versiones históricas, evidencias e investigaciones retiradas.
