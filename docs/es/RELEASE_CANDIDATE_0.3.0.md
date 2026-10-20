# Dowlume 0.3.0 — aceptación del candidato

[English](../RELEASE_CANDIDATE_0.3.0.md) · [Índice en español](README.md)

Preparé el borrador de 0.3.0 el 20 de octubre de 2026 desde el commit limpio `6b8a33d7b43f8da5889d17dd2506b8c4c7a5e647`, integrado mediante el [PR #29](https://github.com/mglezz/dowlume/pull/29), tras las bases de CI nativa del [PR #28](https://github.com/mglezz/dowlume/pull/28). **La aceptación humana final en Windows está pendiente.** Conservo [0.2.0](https://github.com/mglezz/dowlume/releases/tag/v0.2.0) como versión estable más reciente. El borrador es visible para quienes mantienen el repositorio; los artefactos de los workflows enlazados conservan los mismos candidatos.

## Evidencia del código final

| Comprobación | Resultado registrado |
| --- | --- |
| [Compilación y pruebas nativas](https://github.com/mglezz/dowlume/actions/runs/39459672054) | Windows: 84 superadas y una nativa Linux omitida. macOS: 79 superadas y seis nativas omitidas. Linux: 80 superadas y cinco nativas Windows omitidas. Sin fallos. Formato correcto. |
| [Seguridad](https://github.com/mglezz/dowlume/actions/runs/39459672727) | Superada. |
| [Paquete Windows](https://github.com/mglezz/dowlume/actions/runs/39459676032) | Superado en `Microsoft Windows NT 10.0.26100.0`: base 0.2.0 verificada, actualización, arranque/cierre portátil, recursos/guías, cambio de idioma y reinicio, rechazo de segunda instancia, restauración de bandeja, bloqueo de actualización/desinstalación con la app abierta, conservación/retirada de inicio automático y conservación de perfil/muestras. |
| [Paquetes nativos](https://github.com/mglezz/dowlume/actions/runs/39459680677) | macOS 15.7.9 ARM64 y Ubuntu 24.04 x64: arranque y cierre correcto desde los archivos extraídos. Firma ad hoc verificada antes/después en macOS; X11 bajo Xvfb en Linux. |
| Procedencia | Los tres archivos de compilación identifican `6b8a33d7b43f8da5889d17dd2506b8c4c7a5e647` y `DirtyWorkingTree: false`. Verifiqué los hashes de paquetes y archivos subidos. |

Revisé durante el desarrollo la ventana macOS en español y su cierre; corregí pestañas recortadas e instrucciones inadecuadas para la vista previa. Es revisión de interfaz de desarrollo, no aceptación independiente final. Los informes nativos dejan pendientes aceptación manual y equipo limpio. Las pruebas compartidas fuera de Windows usan un adaptador exclusivo de pruebas; las modificaciones y recuperación de producción siguen bloqueadas. Detallo el [alcance](PLATFORM_SUPPORT.md) y el [contrato](FILE_OPERATION_CONTRACT.md).

## Aceptación Windows pendiente

Pruebo estos hashes exactos siguiendo el [procedimiento actual](WINDOWS_ACCEPTANCE.md): actualización desde 0.2.0 con muestras ficticias, conservación de ajustes/historial/contenido, vista previa/cancelación, organización/pausa, revisión/Deshacer compatible, idiomas tras reiniciar, bandeja y cierre. Registro compilación de Windows, escalado, fecha y resultados. El runner tiene .NET compartido; su informe no demuestra un equipo limpio. Conservo esa limitación hasta obtener un resultado separado.

No reutilizo la aceptación humana de 0.2.0, altero la máquina/checkpoints de evidencias ni repito pruebas retiradas de papelera. Promuevo los mismos archivos solo tras confirmar su funcionamiento final; recompilar exige repetir las comprobaciones afectadas. macOS/Linux continúan como vistas previas restringidas y los Mac Intel quedan excluidos.

## Archivos candidatos

Conservo la lista de hashes Windows y las listas nativas separadas. El borrador contiene estos 15 archivos; no he reemplazado archivos aceptados anteriores.

| Archivo | SHA-256 |
| --- | --- |
| `BUILD-INFO.json` | `19c0366bd1e321858acd2c0532691833519a7d6d823a0e14dc5805ec0b8404f7` |
| `Dowlume-0.3.0-win-x64-setup.exe` | `1e72330b1ed1c662cdbab9fa93511626f736488067d261f07683443a08826cd3` |
| `Dowlume-0.3.0-win-x64.zip` | `bbd6e769a54a6961970237215a4a3d463fe1b8f300515575b8262a3ffeb74c89` |
| `Dowlume-0.3.0-windows-acceptance.zip` | `c418ab9e2fad2af10216ecfbaa0a7078d58a2ee78d65fe529e349ff3b8fa64ad` |
| `RELEASE_NOTES_0.3.0.es.md` | `ba0d35947925b7da53101a6f9048f4fb82d5a580030385332a386e69fb650e3b` |
| `RELEASE_NOTES_0.3.0.md` | `29784970c06eb73728aa821e9cf7b82b707956ec19042785585b6b8d2af10abe` |
| `SHA256SUMS.txt` | `ce80faca44e1a6d9544c441b29842036772dfb97b6164dd91cbef1913298de3b` |
| `ACCEPTANCE.osx-arm64.json` | `f510aba9d30e2eb20bb5c05e7e7e5b929192a825feccb517415cd73f6f3ad3d8` |
| `BUILD-INFO.osx-arm64.json` | `080e375e99e5ef9bb7dc7cf77f0d9bb5836a84c8cb3f2e5e92661466e6b9d14a` |
| `Dowlume-0.3.0-osx-arm64-preview.zip` | `97da16c0124c69ed6aaee7c569c765dc427e49115cf0422d9af87f94e6ba4376` |
| `SHA256SUMS.osx-arm64.txt` | `54365b1b2c6359f937e93fa9a9c86370afa90ed3552f9b63df6544abb0522b24` |
| `ACCEPTANCE.linux-x64.json` | `700b3c6fb493655ef71f801727f6bb5a0ddbd2ee4303eca7f10b12905a7499c0` |
| `BUILD-INFO.linux-x64.json` | `d3aae37537e75812c5f38c4f06b87dec8d9a4d8ab1507c7239555b730fb4128c` |
| `Dowlume-0.3.0-linux-x64-preview.tar.gz` | `36c1fd404d5521281ebb5bc169902c0dbe2cca7eda7ba43f060fe6ac7984e96b` |
| `SHA256SUMS.linux-x64.txt` | `8229cde91f0116220de253632c6f9aa6113de4bfd31d4041407b74c8ef08f380` |
