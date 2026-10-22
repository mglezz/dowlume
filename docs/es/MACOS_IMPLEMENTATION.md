# Implementación macOS hacia 0.4.0

[English](../MACOS_IMPLEMENTATION.md) · [Índice en español](README.md)

Mi objetivo es macOS 15 en Apple Silicon. La versión publicada sigue siendo 0.3.0; este trabajo inicia el desarrollo de 0.4.0 y no activa modificaciones nativas de archivos ni constituye una nueva versión. Intel queda fuera del alcance.

## Implementado: lectura e identidad nativas

Uso `MacFileHandle` en el cálculo del hash de vista previa y en la lectura verificada de producción. Abro una ruta absoluta con `O_NOFOLLOW_ANY`, `O_CLOEXEC` y `O_NONBLOCK` de Darwin y examino el descriptor mediante `fstat`. Así rechazo enlaces simbólicos en cualquier componente durante la propia apertura y archivos no regulares sin esperar a un escritor de FIFO. Rechazo el indicador dataless cuando lo observo; esto no acepta proveedores de nube ni garantiza evitar la descarga de contenido durante la apertura.

Registro identidad de dispositivo/inodo, número de enlaces duros, modo, tamaño, fechas de modificación/cambio/creación e indicadores. Comparo metadatos tras leer y vuelvo a abrir la ruta para comprobar que describe el mismo objeto. Excluyo la fecha de acceso porque la lectura puede actualizarla. Consulto `_PC_CASE_SENSITIVE` en un descriptor abierto para las pruebas nativas por volumen; no normalizo nombres Unicode ni deduzco identidad comparando cadenas.

Restrinjo el adaptador a macOS 15+ ARM64. Uso la estructura `stat` Darwin ARM64 del SDK local y contrasto sus campos relevantes con la utilidad `stat` del sistema. Verifico los indicadores de apertura con las [definiciones Darwin de Apple](https://github.com/apple-oss-distributions/xnu/blob/main/bsd/sys/fcntl.h) y la estructura con [sys/stat.h](https://github.com/apple-oss-distributions/xnu/blob/main/bsd/sys/stat.h).

## Comprobaciones nativas reproducibles

Ejecuto diez pruebas `MacNativeRead`: hash de producción y metadatos nativos, enlaces finales/antecesores, directorios/FIFO, sustitución por contenido idéntico, sustitución de directorio antecesor, escritura concurrente restaurando longitud/mtime, alias por mayúsculas/Unicode, identidad de enlaces duros, permisos denegados y rutas inválidas/ausentes. Las demás plataformas omiten estas pruebas explícitamente.

Después de compilar Release, ejecuto:

```sh
dotnet build Dowlume/Dowlume.slnx -c Release
python3 scripts/Test-MacFileReads.py --output artifacts/mac-file-reads
```

El directorio de salida debe ser nuevo. Ejecuto como usuario normal en Apple Silicon, con las herramientas de línea de comandos de Xcode y el SDK del repositorio. El script crea dos imágenes dinámicas privadas de 256 MiB, una APFS y otra APFS sensible a mayúsculas, activa permisos de propietario y ejecuta las pruebas en ambas. Nunca recibe un volumen ni una carpeta Descargas del usuario. Solo desmonta sus propias imágenes y elimina sus directorios temporales; si falla el desmontaje, conserva la muestra y da la ejecución por fallida. No fuerza desmontajes.

Conservo resultados TRX por volumen y `ACCEPTANCE.mac-file-reads.json`, con commit, cambios locales, sistema, arquitectura y SDK. Las pruebas ausentes u omitidas hacen fallar la aceptación. El trabajo macOS de CI ejecuta el mismo script y adjunta sus informes junto con las regresiones completas. Las comprobaciones locales superaron las diez pruebas en ambas variantes APFS sobre macOS 26.6.2 ARM64 y SDK 10.0.401; el informe identifica esa ejecución como compilación de desarrollo con cambios locales. CI aporta evidencia independiente de macOS 15 para el cambio confirmado. Los resultados locales no amplían la matriz soportada.

## Límites pendientes

Son comprobaciones de lectura, no una transacción ni un bloqueo contra escritores POSIX. Un escritor puede modificar el contenido tras verificarlo. No inmovilizo directorios antecesores, demuestro pertenencia ante cambios de montaje, valido almacenamiento de red/nube ni establezco todavía una operación segura de renombrado/borrado. Los enlaces duros son alias observables, no destinos aceptados para modificar. `PathSafety` mantiene su comparación según el sistema operativo. Conservo `CanMove` desactivado en macOS y bloqueo organización, cambios desde revisión, Deshacer y recuperación automática.

Mi secuencia restante es:

1. Definir y validar el protocolo de modificación nativo: pertenencia por volumen, protección frente a escritores/sustituciones, destinos atómicos sin sobrescritura, metadatos, interrupciones y recuperación del diario. Las operaciones entre volúmenes necesitan evidencia independiente o rechazo explícito.
2. Completar la aceptación de escritorio: permisos y denegación, Finder/selección de carpetas, Dock/menús/reapertura/cierre, inicio de sesión opcional, ambos idiomas y migración del perfil.
3. Medir rendimiento, generar la candidata exacta y aceptar instalación/actualización y todo el flujo soportado antes de publicar 0.4.0.

Difiero la firma Developer ID y la notarización a la estabilización final multiplataforma, junto con la firma de editor Windows. Primero completo y valido los flujos de la aplicación en macOS y Linux conservando Windows. Este trabajo de distribución no bloquea el desarrollo funcional ni la aceptación de la versión 0.4.0. Conservo la firma ad hoc de los paquetes macOS de desarrollo y documento su estado real de firma y las limitaciones de instalación. Retomaré la [distribución Developer ID](https://developer.apple.com/developer-id/) en la fase final de distribución. No elimino cuarentena ni desactivo Gatekeeper en los scripts o instrucciones.

Conservo la implementación Windows y los artefactos publicados. El reciclaje nativo sigue desactivado y mantengo intactas las evidencias históricas e investigaciones de capacidad.
