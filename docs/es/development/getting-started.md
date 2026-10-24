# Preparar el entorno de Dowlume

[English](../../development/getting-started.md) · [Índice en español](../README.md)

## Requisitos

Desarrollo para Windows x64 y macOS 15+ Apple Silicon; para macOS necesito las herramientas de línea de comandos Xcode. Linux sigue como vista previa restringida. Utilizo un SDK .NET **10.0.4xx**: [`global.json`](../../../global.json) fija `10.0.400` con `rollForward: latestPatch`; CI utiliza `10.0.401`. Para generar el instalador también se necesita Inno Setup 6. Las comprobaciones del repositorio requieren Python 3.10+; CI utiliza Python 3.13 para los scripts de empaquetado.

Mantengo identificadores, comentarios, ramas y mensajes Conventional Commits en inglés. La interfaz y la documentación están disponibles en inglés y español. Los paquetes publicados 0.4.0 incluyen el selector de idioma introducido en 0.2.0.

## Compilar y probar

Desde la raíz del repositorio:

```powershell
dotnet restore Dowlume.slnx
dotnet build Dowlume.slnx -c Release --no-restore
dotnet test Dowlume.slnx -c Release --no-build
dotnet format Dowlume.slnx --verify-no-changes --no-restore
python3 scripts/check_repository.py
python3 -m unittest discover -s tests/packaging
dotnet run --project src/Dowlume.App -c Release
```

Mis pruebas utilizan carpetas temporales, nunca las descargas personales. Las pruebas de regresión del envío a la papelera comprueban que se rechaza sin modificar archivos; no ejecutan las antiguas pruebas nativas de capacidad. Conservo sus evidencias y no las repito.

Para probar la interfaz, elige una carpeta desechable. También puedes aislar los datos de la aplicación:

```powershell
dotnet run --project src/Dowlume.App -c Release -- --data-directory C:\Dowlume-Test\Profile
```

En macOS, la carpeta temporal del sistema puede atravesar enlaces simbólicos que el proyecto rechaza. Para ejecutar comprobaciones de desarrollo, utiliza una carpeta temporal real con `TMPDIR`; esto no convierte macOS en una plataforma de distribución aceptada.

## Desarrollo nativo

Habilito el flujo macOS 0.4.0 en APFS local del mismo volumen. Añado menús nativos, Finder, migración e inicio de sesión para la app empaquetada. Ubuntu 24.04 x64 sigue restringido. Excluyo Intel. Detallo [paquetes y dependencias](../user/platform-support.md).

```sh
mkdir -p artifacts/test-tmp
TMPDIR="$PWD/artifacts/test-tmp" dotnet test Dowlume.slnx -c Release
```

Fuera de Windows, las pruebas compartidas usan un adaptador de muestras; las omisiones explícitas no cuentan como aceptación nativa. Conservo el perfil Windows, migro el perfil cerrado de vista previa macOS al iniciar sin ruta personalizada y separo el perfil Linux. Documento [copias y límites de operaciones](../architecture/file-operations.md).

Tras compilar Release, ejecuto APFS desechable con `python3 scripts/Test-MacFileReads.py --output artifacts/mac-file-operations`. Genero y pruebo el archivo macOS extraído con `python3 scripts/Build-PlatformPreview.py --runtime osx-arm64 --output artifacts/macos-package`. Las salidas deben ser nuevas. Los perfiles personalizados no registran inicio de sesión ni migran datos personales.

## Siguientes pasos

Explico el flujo completo en la [guía de uso](../user/guide.md), los conceptos en el [modelo de dominio](../requirements/domain-model.md) y los límites de las operaciones en [Arquitectura](../architecture/overview.md).

Trabajo con una sola rama permanente, `main`, ramas temporales, Conventional Commits y revisión con CI antes de integrar. Documento el proceso en [Contribuciones](contributing.md) y [Ramas](branches.md). Generar un paquete local no equivale a publicarlo ni sustituye la [aceptación](releasing.md).
