# Procedimiento de publicación

[English](../../development/releasing.md) · [Índice en español](../README.md)

La aceptación corresponde a una revisión y unos binarios concretos. Compilar, crear un borrador o haber validado una versión anterior no valida un paquete nuevo. Conserva la revisión, los hashes, el entorno y los resultados en `docs/releases/acceptance/<version>.md`. La [lista completada de 0.1.0](../archive/release-checklist-0.1.0.md) es un registro histórico.

## Preparar el candidato

1. Actualiza la versión en `src/Dowlume.App/Dowlume.App.csproj` y redacta las notas inglesas en `docs/releases/<version>.md`. Enlaza su traducción desde `docs/es/releases/<version>.md`.
2. Revisa las instrucciones del paquete, la guía, las plataformas y la versión de referencia para actualizar instalaciones.
3. Ejecuta la compilación, las pruebas, el formato, `python3 scripts/check_repository.py`, `python3 -m unittest discover -s tests/packaging` y la auditoría de dependencias. Registra las comprobaciones nativas pendientes.
4. Genera desde una revisión confirmada y limpia. `BUILD-INFO.json` debe indicar `DirtyWorkingTree: false`. Usa un directorio nuevo para cada candidato.

Los títulos y textos de las publicaciones, commits e instrucciones principales se escriben en inglés. La documentación española es una traducción opcional enlazada, sin concatenar ambos idiomas en el texto principal.

## Generar paquetes reproducibles

En Windows, con el SDK requerido e Inno Setup 6:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Release.ps1 -RequireInstaller
```

Utiliza `-InnoCompiler` para una ubicación personalizada y `-OutputDirectory` para un directorio de salida nuevo. La salida incluye ZIP portátil, instalador, kit de aceptación, notas, metadatos y sumas SHA-256.

En cada sistema nativo, con Python 3.13 y las dependencias indicadas en [plataformas](../user/platform-support.md):

```sh
python3 scripts/Build-PlatformPreview.py --runtime osx-arm64 --output artifacts/macos-candidate
# En Ubuntu x64:
python3 scripts/Build-Linux.py --output artifacts/linux-candidate
python3 scripts/Test-Linux.py --output artifacts/linux-native --archive artifacts/linux-candidate/Dowlume-0.5.0-linux-x64.tar.gz
```

macOS necesita las herramientas Xcode. El script comprueba la aplicación extraída con un perfil desechable. Incluye guías de usuario seleccionadas y traducciones separadas; excluye archivos históricos y herramientas de desarrollo. No publica la versión.

## Antes de publicar

Completa la [aceptación Windows](windows-acceptance.md), las pruebas nativas y la aceptación de escritorio macOS. Distingue las pruebas automáticas de las confirmaciones manuales, el inicio de sesión real y la instalación limpia. Conserva la máquina virtual de investigación y no ejecutes las pruebas retiradas de capacidad de la papelera.

Verifica idiomas, vista previa, organización, revisión, Deshacer, recuperación, actualizaciones y conservación de archivos. Documenta la firma real, los límites de instalación y las plataformas aceptadas. Solicita la aprobación del responsable para publicar esos binarios concretos y conserva los paquetes probados sin recompilarlos.

## Tras publicar

Etiqueta la revisión aceptada y adjunta únicamente el instalador Windows y los ZIP de aplicación Windows/macOS, nombrados `-portable.zip`. Los informes, kits de aceptación y metadatos permanecen en los artefactos de CI o en el registro del candidato. Publica los hashes en las notas enlazadas y verifica las descargas públicas. Las candidatas Linux 0.5.0 siguen en prerelease hasta completar la [matriz Linux](../../development/linux-acceptance.md); pueden incluir `.deb`, `.tar.gz`, sumas y evidencia técnica. No se marcan como última versión estable. Renombrar un ZIP no debe modificar sus bytes ni su hash; actualiza los enlaces actuales y conserva los nombres originales en los registros históricos. GitHub añade sus propios enlaces al código fuente. Actualiza los enlaces del README, el índice de versiones, la guía y la hoja de ruta. No conviertas resultados fallidos o pendientes en aprobaciones.
