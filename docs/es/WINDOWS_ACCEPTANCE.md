# Aceptación de versiones en Windows

[English](../WINDOWS_ACCEPTANCE.md) · [Índice en español](README.md)

Utilicé este procedimiento para los paquetes publicados de v0.1.0. Repito las comprobaciones afectadas para cada binario posterior. Registro SHA-256, commit de `BUILD-INFO.json`, compilación de Windows, persona que prueba, fecha y resultado. Un control sin resultado no es una prueba superada.

## Entorno limpio

Utilizo una máquina virtual nueva o un equipo de repuesto con Windows 11 x64, cuenta estándar y sin .NET moderno instalado por separado. Documento la procedencia de la imagen. Un equipo de desarrollo o un runner con SDK no satisface este requisito.

Conservo intactos la máquina virtual, exportaciones y checkpoints de la investigación. No los restauro para probar otra versión. No cambio políticas de papelera ni ejecuto pruebas nativas de capacidad. En [Hyper-V](HYPER_V_ACCEPTANCE.md) explico la preparación.

Descargo el instalador, ZIP, información de compilación y checksums del candidato. Comparo cada archivo con `Get-FileHash -Algorithm SHA256`. Un hash identifica el archivo, pero no sustituye confiar en su origen. No publico un paquete con `DirtyWorkingTree: true`.

## Prueba automatizada de paquetes

El kit incluye `Test-WindowsPackage.ps1` y `Test-WindowsTray.ps1`; los mantengo juntos. En Windows PowerShell, sin instalar .NET:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Test-WindowsPackage.ps1 -Installer .\Dowlume-0.1.0-win-x64-setup.exe -Archive .\Dowlume-0.1.0-win-x64.zip -ReportPath .\package-result.json -RequireNoSharedDotNet
```

El script rechaza instalaciones o entradas de inicio existentes. Crea instalación, perfil y muestra únicos; comprueba lanzamiento portátil, instalación, reinstalación, instancia duplicada, bloqueo de cambios mientras la aplicación está abierta, bandeja, entradas de inicio y conservación de datos tras desinstalar. Necesita escritorio interactivo.

Para probar actualización desde otro candidato añado `-PreviousInstaller <ruta>`. Sin ese argumento, compruebo una reinstalación del mismo paquete. Conservo ambos hashes y los informes. Si falla, inspecciono los registros retenidos antes de repetir; no sobrescribo una instalación residual.

## Primer uso con una persona

Pido a alguien que no conozca la implementación que pruebe, sin ayuda de desarrollo:

1. Abrir el acceso del menú Inicio y la guía sin conexión.
2. Crear una carpeta desechable con documentos, imágenes, comprimidos y un `.crdownload`; guardarla en Ajustes.
3. Actualizar la vista previa, comprobar destinos y verificar que los archivos siguen intactos.
4. Cancelar Organizar y supervisar; comprobar que no hay movimientos. Repetir confirmando y verificar hashes.
5. Añadir una muestra completa, esperar su organización, pausar y comprobar que la siguiente no se mueve.
6. Ocultar en bandeja, restaurar con clic y menú, probar desde ventana minimizada, revisar iconos ocultos y salir. Al reabrir, comprobar la pausa.
7. Activar inicio con Windows en la cuenta desechable, cerrar sesión y volver; repetir desactivado. Verificar conservación al actualizar y retirada al desinstalar. No cerrar la sesión de trabajo del anfitrión.
8. Buscar la muestra en Actividad y deshacer una acción compatible, verificando contenido y rechazo de destinos ocupados.
9. Actualizar con el asistente, reabrir y comprobar ajustes e historial; desinstalar y verificar conservación de muestras y perfil.
10. Con una muestra en Revisión, comprobar explicación y botón de papelera desactivado con ratón y teclado. Probar Conservar, Posponer, Mover, Explorador y Deshacer.
11. Para paquetes posteriores con localización, cambiar inglés/español, reiniciar, verificar persistencia, textos, bandeja, diálogos, teclado y escalado; comprobar el tamaño mínimo sin recortes.

Registro textos confusos, errores y problemas de navegación. Solo adjunto capturas revisadas que no revelen rutas privadas.

## Seguridad nativa

Pruebo movimiento y deshacer entre dos volúmenes locales físicos. Interrumpo únicamente el proceso aislado durante una copia: deben quedar origen intacto y copia parcial, estado Requiere atención y rechazo de nuevas acciones hasta resolver. Si solo queda origen o una copia ya completa, la prueba de interrupción no es concluyente.

```powershell
dotnet run --project scripts/FileOperationAcceptance -c Release -- verify E:\Dowlume.Acceptance artifacts/acceptance/cross-volume.json
```

Elijo una carpeta escribible de un segundo volumen fijo. El arnés usa muestras únicas de 64 MiB y 512 MiB, conserva carpetas e informes y no simula un corte eléctrico. Necesita SDK, por lo que no forma parte de la prueba de equipo sin .NET.

Para la papelera ejecuto solo regresiones de rechazo y conservación, nunca las pruebas de capacidad retiradas. Mantengo las evidencias originales de sus fallos.

## Publicación

Relaciono la aceptación con los hashes, resuelvo fallos, obtengo revisión independiente y autorizo explícitamente cada publicación. v0.1.0 ya está publicada; los cambios de idioma necesitan validación para sus futuros paquetes. macOS y Linux quedan fuera de este procedimiento.
