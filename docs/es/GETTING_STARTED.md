# Preparar el entorno de Dowlume

[English](../../GETTING_STARTED.md) · [Índice en español](README.md)

## Requisitos

Desarrollo y distribuyo la aplicación para Windows x64. Utilizo un SDK .NET **10.0.4xx**: [`global.json`](../../global.json) fija `10.0.400` con `rollForward: latestPatch`; CI utiliza `10.0.401`. Para generar el instalador también necesito Inno Setup 6.

Mantengo identificadores, comentarios, ramas y mensajes Conventional Commits en inglés. La interfaz y la documentación están disponibles en inglés y español. El instalador y el ZIP portátil publicados de 0.2.0 incluyen el selector de idioma.

## Compilar y probar

Desde la raíz del repositorio:

```powershell
dotnet restore Dowlume/Dowlume.slnx
dotnet build Dowlume/Dowlume.slnx -c Release --no-restore
dotnet test Dowlume/Dowlume.slnx -c Release --no-build
dotnet run --project Dowlume/Dowlume.App -c Release
```

Mis pruebas utilizan carpetas temporales, nunca las descargas personales. Las pruebas de regresión del envío a la papelera comprueban que se rechaza sin modificar archivos; no ejecutan las antiguas pruebas nativas de capacidad. Conservo sus evidencias y no las repito.

Para probar la interfaz, elige una carpeta desechable. También puedes aislar los datos de la aplicación:

```powershell
dotnet run --project Dowlume/Dowlume.App -c Release -- --data-directory C:\Dowlume-Test\Profile
```

En macOS, la carpeta temporal del sistema puede atravesar enlaces simbólicos que el proyecto rechaza. Para ejecutar comprobaciones de desarrollo, utiliza una carpeta temporal real con `TMPDIR`; esto no convierte macOS en una plataforma de distribución aceptada.

## Desarrollo nativo experimental

También compilo en macOS 15 Apple Silicon y Ubuntu 24.04 x64. Estos sistemas ofrecen vista previa y preferencias; bloqueo modificaciones, recuperación, ocultar en bandeja e inicio automático. Excluyo los Mac Intel. Detallo [paquetes y dependencias](PLATFORM_SUPPORT.md).

```sh
mkdir -p artifacts/test-tmp
TMPDIR="$PWD/artifacts/test-tmp" dotnet test Dowlume/Dowlume.slnx -c Release
```

Fuera de Windows, las pruebas compartidas usan un adaptador de muestras; las omisiones explícitas no cuentan como aceptación nativa. Conservo la ubicación del perfil Windows y separo los experimentales. Documento [copias y límites de operaciones](FILE_OPERATION_CONTRACT.md).

## Siguientes pasos

Explico el flujo completo en la [guía de uso](USER_GUIDE.md), los conceptos en el [modelo de dominio](requirements/DOMAIN_MODEL.md) y los límites de las operaciones en [Arquitectura](ARCHITECTURE.md).

Trabajo con una sola rama permanente, `main`, ramas temporales, Conventional Commits y revisión con CI antes de integrar. Documento el proceso en [Contribuciones](CONTRIBUTING.md) y [Ramas](BRANCHES.md). Generar un paquete local no equivale a publicarlo ni sustituye la [aceptación](RELEASE_CHECKLIST.md).
