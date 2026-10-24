# Aceptación de versiones en Windows

[English](../../development/windows-acceptance.md) · [Índice en español](../README.md)

Este procedimiento se aplica a cada nuevo candidato. Los ejemplos usan 0.4.0 y el instalador 0.3.0 como referencia de actualización. Los registros anteriores no validan binarios nuevos.

El [registro de aceptación de 0.4.0](../releases/acceptance/0.4.0.md) distingue los resultados documentados para esa versión. Registra SHA-256, commit de `BUILD-INFO.json`, compilación de Windows, persona que prueba, fecha y resultado. Un control sin resultado no es una prueba superada.

## Entorno limpio

Utilizo una máquina virtual nueva o un equipo de repuesto con Windows 11 x64, cuenta estándar y sin .NET moderno instalado por separado. Documento la procedencia de la imagen. Un equipo de desarrollo o un runner con SDK no satisface este requisito.

Conservo intactos la máquina virtual, exportaciones y checkpoints de la investigación. No los restauro para probar otra versión. No cambio políticas de papelera ni ejecuto pruebas nativas de capacidad. En [Hyper-V](hyper-v-acceptance.md) explico la preparación.

Descargo el instalador, ZIP, información de compilación y checksums del candidato. Comparo cada archivo con `Get-FileHash -Algorithm SHA256`. Un hash identifica el archivo, pero no sustituye confiar en su origen. No publico un paquete con `DirtyWorkingTree: true`.

## Prueba automatizada de paquetes

El kit incluye `Test-WindowsPackage.ps1`, `Test-WindowsTray.ps1` y `Test-WindowsLocalization.ps1`; los mantengo juntos. En Windows PowerShell, sin instalar .NET:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Test-WindowsPackage.ps1 -Installer .\Dowlume-0.4.0-win-x64-setup.exe -Archive .\Dowlume-0.4.0-win-x64.zip -PreviousInstaller .\Dowlume-0.3.0-win-x64-setup.exe -ReportPath .\package-result.json -RequireNoSharedDotNet -RequireLocalization
```

El script rechaza instalaciones o entradas de inicio existentes. Crea instalación, perfil y muestra únicos; comprueba lanzamiento portátil, instalación, reinstalación, instancia duplicada, bloqueo de cambios mientras la aplicación está abierta, bandeja, entradas de inicio y conservación de datos tras desinstalar. Necesita escritorio interactivo.

Usa el instalador publicado de 0.3.0 como base de actualización. Para elegir otra base añado `-PreviousInstaller <ruta>`. Sin ese argumento, compruebo una reinstalación del mismo paquete. Conservo ambos hashes y los informes. Si falla, inspecciono los registros retenidos antes de repetir; no sobrescribo una instalación residual.

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

## Idioma y actualización de 0.3.0

Registro los hashes del instalador y ZIP exactos y los resultados observados antes de publicar:

1. En una cuenta desechable, configuro una carpeta en 0.2.0, organizo un archivo ficticio, pauso y compruebo su entrada en Actividad. Cierro, instalo 0.3.0 y verifico carpeta, ajustes, historial, contenido intacto y supervisión en pausa.
2. Elijo Español en Settings > Language. Reviso las cinco pestañas, confirmaciones y cancelaciones, explicación de la papelera desactivada y menú de bandeja. Compruebo que cambiar idioma no mueve archivos ni renombra las carpetas existentes.
3. Cierro y vuelvo a abrir; verifico que persiste el español. Vuelvo a English y reinicio para verificar el inglés. Compruebo ambos idiomas al tamaño mínimo, con el escalado de Windows elegido y con teclado.
4. Extraigo el ZIP portátil en una carpeta nueva, conservando `es` y ambas guías. Abro, cambio idioma, cierro y vuelvo a abrir. Compruebo lo mismo sin instalar .NET por separado en el equipo limpio.
5. Registro versión de Windows, escalado, fecha y problemas. El runner comprueba el funcionamiento del paquete, pero no sustituye esta revisión humana ni demuestra un entorno limpio.

Con `-RequireLocalization` compruebo recursos y guías, cambio inglés → español → inglés en la aplicación instalada y reinicio después de cada cambio para verificar la persistencia.

## Seguridad nativa

Pruebo movimiento y deshacer entre dos volúmenes locales físicos. Interrumpo únicamente el proceso aislado durante una copia: deben quedar origen intacto y copia parcial, estado Requiere atención y rechazo de nuevas acciones hasta resolver. Si solo queda origen o una copia ya completa, la prueba de interrupción no es concluyente.

```powershell
dotnet run --project tools/FileOperationAcceptance -c Release -- verify E:\Dowlume.Acceptance artifacts/acceptance/cross-volume.json
```

Elijo una carpeta escribible de un segundo volumen fijo. El arnés usa muestras únicas de 64 MiB y 512 MiB, conserva carpetas e informes y no simula un corte eléctrico. Necesita SDK, por lo que no forma parte de la prueba de equipo sin .NET.

Para la papelera ejecuto solo regresiones de rechazo y conservación, nunca las pruebas de capacidad retiradas. Mantengo las evidencias originales de sus fallos.

## Publicación

Registro evidencia vinculada a los hashes, resuelvo fallos, obtengo revisión independiente y repito las pruebas afectadas si hay cambios. El 20 de octubre de 2026 confirmé que todas las funcionalidades funcionaban correctamente y publiqué 0.2.0 con los mismos siete archivos aceptados del borrador. La copia del procedimiento incluida en el kit conserva el estado pendiente de cuando lo compilé; el [registro de aceptación](../releases/acceptance/0.2.0.md) actualiza ese estado. Para 0.3.0 repito la aceptación Windows afectada sobre sus hashes exactos. macOS/Linux son vistas previas restringidas, con informes separados de arranque y sin aceptación estable.
