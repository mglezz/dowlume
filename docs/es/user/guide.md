# Usar Dowlume

[English](../../user/guide.md) · [Índice en español](../README.md)

Dowlume organiza descargas completas en Windows x64, macOS Apple Silicon y Ubuntu 24.04 LTS x64 con GNOME. La [versión 0.6.0](https://github.com/mglezz/dowlume/releases/tag/v0.6.0) incluye instaladores Windows/Ubuntu y paquetes portables con .NET incluido. Consulta las [instrucciones macOS](#macos-040), las [instrucciones Linux](#linux-060) y la [matriz de plataformas](platform-support.md). Empieza con copias de archivos de ejemplo en una carpeta desechable.

## Instalar y abrir

Elige el instalador Windows para una instalación habitual, o **Portable** para descargar la aplicación completa en un ZIP desde la publicación de GitHub. Los paquetes Windows de v0.6.0 no tienen firma de editor: utiliza una fuente de confianza y compara el SHA-256 con las [sumas de comprobación](../releases/0.6.0.md#sumas-de-comprobación).

Ejecuta `Dowlume-0.6.0-win-x64-setup.exe` sin permisos de administrador. Para la versión portátil, extrae todo el ZIP y ejecuta `Dowlume.App.exe` junto a sus archivos. No necesita una instalación separada de .NET.

Incluyo `START-HERE.txt` para consulta sin conexión y un acceso **Dowlume quick start** en el menú Inicio. Cierra también cualquier instancia oculta en la bandeja antes de actualizar o desinstalar; el instalador se niega a continuar mientras la aplicación está abierta. Conservo los archivos organizados y el perfil local al desinstalar.

## Actualizar sin desinstalar (0.6.0)

El instalador de Windows ya permite instalar una versión nueva sobre la existente, sin desinstalar primero. Cierra Dowlume, ejecuta el nuevo instalador y conserva la carpeta de instalación. Se mantienen el perfil y los archivos organizados.

La versión 0.6.0 añade **Ajustes → Buscar actualizaciones**. Pausa la organización para acceder a Ajustes. La consulta contacta con el servicio público de publicaciones de GitHub solo cuando lo solicitas, sin cuenta, credenciales, nombres de archivos ni rutas; GitHub recibe los datos normales de conexión, como tu dirección IP. Solo ofrece versiones estables más recientes, excluyendo borradores y versiones preliminares. Un fallo de red no impide organizar archivos localmente.

En una instalación registrada de Windows, **Descargar e instalar actualización…** pide confirmación, descarga el instalador oficial a una carpeta temporal independiente, verifica el tamaño y SHA-256 publicados, pausa la organización y sale antes de abrir el instalador interactivo. Completa la instalación conservando la carpeta existente. No hace falta desinstalar. La suma verifica la coherencia con los metadatos de GitHub; no es una firma de editor. Los paquetes Windows siguen sin estar firmados. Si falta el instalador o su suma, se ofrece la publicación oficial. Si el instalador no se abre, vuelve a abrir Dowlume y reinténtalo o utiliza la publicación oficial. Los instaladores descargados quedan en la carpeta temporal para la limpieza habitual del sistema.

Las copias portables de Windows, macOS, Linux y los perfiles de prueba personalizados ofrecen **Abrir publicación e instrucciones**. En Ubuntu, `sudo apt install ./<nuevo-paquete>.deb` actualiza el paquete. Para copias portables, cierra la aplicación anterior y extrae el archivo completo en otra carpeta permanente; conserva el perfil y actualiza los registros de inicio o menú que apunten a la ubicación anterior. El botón no sustituye automáticamente aplicaciones portables ni ejecuta órdenes Linux con privilegios.

La versión 0.5.0 no contiene este botón: el primer salto a 0.6.0 requiere el instalador habitual. El registro de aceptación distingue las pruebas automatizadas de la confirmación del mantenedor.

## Elegir idioma

Pausa la supervisión y abre **Settings → Language**. Selecciona **Español** o **English**. El cambio se aplica y se guarda inmediatamente para ese perfil. Al reiniciar se conserva; los perfiles antiguos empiezan en inglés.

Cambio pestañas, botones, confirmaciones, bandeja, categorías, estados, fechas y mensajes conocidos. Mantengo los nombres de archivos y carpetas, las rutas y los datos históricos. La carpeta `Expiring Files` conserva ese nombre. No guardo otros ajustes pendientes al cambiar de idioma. Los mensajes históricos desconocidos mantienen su texto original y los controles nativos del selector de carpetas siguen el idioma de Windows.

**El instalador y el ZIP portátil publicados de 0.6.0 incluyen el selector.** Los paquetes anteriores de 0.1.0 siguen en inglés.

## Primer uso

1. Abre **Ajustes** y elige una carpeta local existente. Sugiero inicialmente la carpeta Descargas conocida por Windows.
2. Ajusta nombres de carpetas, extensiones, exclusiones y plazos de revisión. Guarda los ajustes.
3. Abre **Vista previa** y pulsa **Actualizar vista previa**. No mueve los archivos ni crea las carpetas de categoría.
4. Comprueba los destinos, pulsa **Organizar y supervisar** y confirma. La autorización incluye los archivos mostrados, las descargas futuras y las que terminen más tarde.
5. Consulta **Resumen** y **Actividad** para ver el resultado.

![Vista previa inglesa de v0.1.0 con archivos ficticios](../../images/classification.png)

*Conservo las capturas de v0.1.0 en inglés como referencia histórica. [Procedencia](../images/README.md).*

Solo descubro archivos directamente en la carpeta elegida. Las subcarpetas no se reorganizan salvo que actives expresamente la agrupación de carpetas de 0.7.0. Creo las categorías y `Expiring Files` cuando hacen falta. Si un nombre está ocupado, añado un sufijo numérico; no sobrescribo el destino.

En la versión publicada 0.6.0, la supervisión empieza en pausa en cada arranque; 0.7.0 añade la preferencia descrita abajo. Para continuar, revisa la vista previa y confirma otra vez. **Pausar** detiene la supervisión y espera a la operación activa. **Ocultar en la bandeja** mantiene la aplicación abierta; cerrar la ventana detiene la supervisión.

El icono puede aparecer entre los iconos ocultos del área de notificación. Un clic restaura la ventana; el menú contextual ofrece **Abrir Dowlume** y **Salir**. Windows permite fijarlo en el área visible.

En **Ajustes**, **Iniciar Dowlume al entrar en Windows** se aplica inmediatamente a tu cuenta y está desactivado inicialmente. En 0.6.0 empieza en pausa; en 0.7.0 sigue la preferencia guardada de inicio automático. Desactívalo antes de mover una copia portátil y actívalo de nuevo desde su ubicación final. Windows también puede desactivar aplicaciones de inicio. La desinstalación elimina la entrada solo si pertenece a esa instalación.

### Ventana al iniciar Windows (desarrollo de 0.7.0)

Si has activado el inicio con Windows, Dowlume inicia por defecto en el área de notificación sin mostrar la ventana. Activa **Mostrar la ventana al iniciar Windows** en Ajustes si prefieres verla al iniciar sesión. La preferencia se guarda inmediatamente, sin guardar cambios pendientes en carpetas o reglas. La apertura manual muestra la ventana. Usa el icono de la bandeja para abrir o salir de una instancia en segundo plano; la monitorización sigue la preferencia guardada de inicio automático descrita abajo. Si falla la creación de la bandeja, se muestra la ventana para mantener accesible la aplicación.

La actualización conserva una entrada de inicio existente y añade el argumento de inicio de sesión; no activa el inicio automático si estaba desactivado. Una copia portable actualiza su propia entrada antigua la próxima vez que se abre normalmente. Los paquetes publicados de 0.6.0 todavía no incluyen esta opción.

## Apariencia y monitorización persistente (desarrollo de 0.7.0)

Estas novedades, la preferencia de ventana al iniciar Windows y las reglas personalizadas pertenecen a la próxima **0.7.0**, no a los paquetes publicados de 0.6.0.

En **Ajustes → Apariencia**, elige **Sistema**, **Claro** u **Oscuro**. La elección se aplica y guarda al instante, incluidos los diálogos. Sistema sigue al sistema operativo. Pausa la monitorización para acceder a Ajustes.

La cabecera incluye un interruptor **Encendido / Apagado**. Los perfiles nuevos y actualizados empiezan apagados. Primero elige una carpeta, actualiza Vista previa y confirma **Organizar y supervisar**. Para reanudar tras reiniciar Dowlume o iniciar sesión, marca **Reanudar la monitorización al abrir Dowlume**, guarda y confirma la activación inicial. Iniciar la aplicación con Windows es una preferencia independiente.

El inicio automático usa las últimas reglas guardadas y confirmadas. Cambiar la ruta, clasificación, exclusiones, intervalos o agrupación de carpetas exige otra vista previa y confirmación. Cambiar solo el tema o la preferencia de inicio automático no invalida las reglas. Si la ruta no está disponible o es inválida, la monitorización queda apagada y se muestra un error. **Apagado** detiene ambos monitores y desactiva la reanudación automática; cerrar la aplicación conserva la preferencia. El modo continuo revisa las descargas existentes al reanudar; el periódico vuelve a esperar su intervalo completo.

## Agrupar carpetas sueltas (desarrollo de 0.7.0)

Activa **Ajustes → Agrupar carpetas sueltas** si quieres mover las carpetas completas del primer nivel a una carpeta contenedora, llamada `Folders` por defecto. Elige un nombre portable distinto de las categorías y un intervalo de **20–1440 minutos**. La opción empieza desactivada. Guarda, revisa las candidatas en Vista previa y confirma la organización. Su temporizador es independiente del de archivos; apagar la monitorización detiene ambos.

Las carpetas deben permanecer sin cambios entre dos revisiones programadas y no contener elementos modificados recientemente. El primer movimiento posible es tras dos intervalos (40 minutos con el valor predeterminado); los cambios reinician la espera. Reiniciar la aplicación también reinicia las observaciones. Se mueve el directorio completo dentro del mismo volumen compatible; nunca se fusiona, elimina ni reclasifica su contenido. Si el nombre ya existe en el destino, se deja el origen en su sitio.

Se protegen las categorías, `Expiring Files`, la contenedora y los antiguos destinos registrados en el historial. Se omiten carpetas ocultas o del sistema, enlaces, marcadores de nube, descargas incompletas o excluidas, contenidos inaccesibles y árboles que superen el límite de inspección de 10 000 entradas. Mantén las carpetas importantes de trabajo fuera de la raíz supervisada. La estabilidad observada no garantiza que otro programa no reabra un archivo; los procesos POSIX pueden ignorar los bloqueos de acceso compartido.

Los movimientos aparecen en **Actividad**, con ambas rutas, pero no entran en revisión por antigüedad ni tienen Deshacer automático. Para restaurar una carpeta, apaga la monitorización y devuélvela completa con el explorador. Los movimientos interrumpidos requieren inspeccionar ambas rutas manualmente: nunca se repiten ni eliminan automáticamente. Una carpeta con una incidencia pendiente en el historial queda excluida de futuras agrupaciones.

## Carpetas y reglas personalizadas (desarrollo de 0.7.0)

Pausa la organización y abre **Ajustes → Carpetas y clasificación**. Puedes editar las carpetas predefinidas o pulsar **Añadir carpeta** (hasta 50 personalizadas). Pon un nombre único, elige los días hasta la revisión y selecciona cómo clasificar:

- **Extensión:** una lista separada por comas, como `.png, .mov, .mp4`. Retira primero la extensión de su carpeta anterior; se rechazan duplicados aunque cambien las mayúsculas.
- **El nombre contiene:** un fragmento literal como `contrato` coincide con `Mi contrato firmado.pdf`.
- **El nombre empieza por:** `ABC` coincide con `ABC-123.pdf`, pero no con `Informe ABC.pdf`.

Se compara el nombre sin su última extensión, sin distinguir mayúsculas y normalizando Unicode. Se busca texto literal, sin expresiones regulares ni comodines. Las reglas por nombre tienen prioridad sobre las extensiones. Se rechazan patrones idénticos y carpetas de destino duplicadas. Dos patrones distintos pueden coincidir: `ABC contrato.pdf` encaja en ambos ejemplos. Una ambigüedad bloquea la vista previa y el inicio; si aparece en una descarga posterior, ese archivo permanece intacto y el error indica las carpetas implicadas. El orden de las reglas no decide el destino. Las descargas incompletas y las exclusiones tienen prioridad sobre cualquier regla. **Otros** conserva su función para archivos sin coincidencia.

Guarda y actualiza la vista previa antes de iniciar. Editar reglas no renombra carpetas físicas ni mueve en bloque archivos existentes. **Eliminar regla** solo elimina una regla personalizada, nunca sus archivos o historial. Los archivos registrados mantienen su identidad de categoría; la revisión y el retorno usan la regla actual si existe, o la carpeta y el plazo registrados si se eliminó la regla personalizada. Deshacer sigue disponible cuando procede. Conserva una copia de seguridad y prueba los cambios con archivos desechables. Los paquetes publicados de 0.6.0 todavía no incluyen estas opciones.

## Organización periódica (0.6.0)

La versión 0.6.0 incluye **Ajustes → Organizar por intervalos en lugar de continuamente**. Es opcional y está desactivada por defecto. Elige un intervalo entre 1 y 1440 minutos (10 por defecto), guarda, actualiza la vista previa y pulsa **Organizar y supervisar** para confirmar.

Este modo no organiza nada inmediatamente, ni siquiera los archivos de la vista previa. La primera pasada espera el intervalo elegido. Cada pasada revisa los archivos directamente en la carpeta elegida y realiza las comprobaciones habituales de antigüedad y recuperación. Las nuevas descargas pueden organizarse en pasadas posteriores; se mantienen las protecciones para archivos incompletos, ocupados, recientes, excluidos y ya registrados. No se importan subcarpetas existentes. Actividad y Deshacer siguen disponibles.

El modo periódico no utiliza un vigilante de cambios del sistema de archivos. Después de cada pasada completa espera otro intervalo: las pasadas lentas no se solapan ni acumulan ejecuciones pendientes. La aplicación debe seguir abierta; no es una tarea programada del sistema operativo. La suspensión del equipo puede retrasar las revisiones. **Pausar** cancela las siguientes revisiones y espera a que una operación ya registrada termine de forma segura. Cambiar el intervalo o el modo exige una nueva vista previa. En 0.6.0, reiniciar deja la organización pausada. En 0.7.0 se aplica la preferencia guardada de inicio automático.

## Revisar archivos

Desactivé la papelera nativa en v0.1.0 porque las [pruebas de capacidad](../archive/recycle-capacity-investigation.md) no conservaron los archivos de forma fiable. Puedes mantenerlos en revisión o usar las decisiones disponibles. La cuarentena recuperable es una propuesta, todavía no una función.

Calculo la antigüedad desde la primera organización, no desde la descarga o modificación. Al vencer el plazo de la categoría, traslado el archivo a `Expiring Files`.

En la versión 0.6.0, busca por nombre o ruta actual
para filtrar la lista mientras escribes. La búsqueda no distingue mayúsculas y
trata los signos de puntuación literalmente. **Limpiar** restaura la lista completa.
El contador muestra los archivos visibles y el total pendiente de revisión; buscar
no cambia el total del resumen ni mueve archivos. Si la búsqueda oculta el archivo
seleccionado, se borra la selección antes de elegir otra acción. La búsqueda es
temporal y se restablece al reiniciar la aplicación.

Selecciona un archivo en **Revisión**:

- **Conservar:** lo devuelve a su categoría y excluye futuras revisiones por antigüedad.
- **Posponer 7 días:** lo devuelve a su categoría y aplaza la revisión siete días.
- **Mover a…:** permite elegir un destino y confirmar; dejo de gestionar el archivo.
- **Enviar a la papelera…:** permanece desactivado y muestra una explicación. No ejecuta eliminación nativa.
- **Mostrar en el Explorador:** permite inspeccionar el archivo sin ejecutarlo.

La revisión es una zona de espera sin plazo de eliminación automática.

## Buscar una acción antigua

En **Actividad** muestro el historial completo del perfil, incluidas carpetas elegidas anteriormente, en orden inverso de registro y páginas de 50 operaciones.

Busca parte de un nombre o de una ruta de origen o destino. Puedes filtrar por acción y estado, pulsar **Buscar / actualizar** o Intro en el cuadro de búsqueda. Ignoro las diferencias de mayúsculas, incluidas letras acentuadas; `%` y `_` se buscan literalmente. **Limpiar** elimina filtros y vuelve a la primera página.

**Anterior** y **Siguiente** permiten recorrer el historial. La primera página incorpora nuevas operaciones; las antiguas mantienen su posición frente a nuevas llegadas. Sus estados sí se actualizan, por lo que un resultado filtrado puede reducirse tras recuperar una operación. Conservas la selección mientras siga en la página mostrada.

El contador de atención del Resumen incluye todas las operaciones Pendiente y Requiere atención, además de los archivos ausentes de la carpeta actual. Los filtros no reducen ese contador. Las fechas se muestran en hora local con el formato del idioma seleccionado.

## Deshacer y recuperar

Selecciona una operación completada y pulsa **Deshacer acción seleccionada…**. Compruebo que el contenido no ha cambiado, que la ubicación original está disponible y que no hay una acción posterior completada sobre ese archivo.

Deshacer la organización inicial devuelve el archivo a la raíz como Conservado, evitando que vuelva a organizarse inmediatamente. Deshacer el traslado por antigüedad lo devuelve a su categoría pospuesto siete días. No permito deshacer los envíos históricos a la papelera: Windows puede no conservar su contenido.

Registro la intención antes de mover. **Reintentar operaciones interrumpidas** compara las rutas y el contenido:

| Estado observado | Resultado |
| --- | --- |
| Solo queda el origen y su contenido coincide | Registro Fallida; puedes reintentar la acción original. |
| Solo queda el destino y coincide su contenido (también identidad nativa registrada en macOS) | Completo el registro interrumpido. |
| Existen ambos, ninguno, el contenido difiere o falla la inspección | Registro Requiere atención y conservo los archivos. |

Copia las rutas desde Actividad y revísalas en Explorador o Finder. Conserva el contenido que necesites. Para movimientos interrumpidos, reintenta cuando quede una copia inequívoca con el contenido esperado en una de las rutas registradas.

No repito envíos históricos a la papelera ni deduzco su éxito por la ausencia del origen. Un origen intacto permite marcarlos como fallidos; el contenido ausente o cambiado sigue requiriendo atención. Reintentar no restaura contenido perdido ni garantiza que Windows lo guardase.

Si editas un archivo registrado, rechazo las operaciones basadas en su huella anterior. Puedes moverlo fuera de la carpeta gestionada para administrarlo por tu cuenta. Para importarlo de nuevo intencionadamente, devuélvelo a la raíz con otro nombre y revisa la vista previa. No sigo automáticamente cambios de nombre externos.

## Ajustes y datos

- Aplico las reglas a futuras clasificaciones y a los plazos actuales. Cambiar un nombre de categoría en Ajustes no renombra la carpeta existente.
- Superviso una carpeta a la vez. Conservo el historial de las anteriores y recupero sus registros en el Resumen al volver a elegirlas.
- En Windows guardo base de datos y diagnóstico en `%LOCALAPPDATA%\Dowlume`.
- La base de datos contiene rutas personales y huellas de contenido. El diagnóstico contiene eventos y tipos o códigos de error, sin rutas ni mensajes de excepción.
- Para una copia de seguridad, cierra Dowlume y copia todo el directorio de datos, incluidos los archivos auxiliares de SQLite.
- La desinstalación conserva archivos organizados y base de datos.
- `Dowlume.App.exe --data-directory <ruta-absoluta>` permite un perfil de pruebas aislado. No ejecutes perfiles distintos contra la misma carpeta.

## Límites

Windows x64, macOS Apple Silicon y Ubuntu 24.04 LTS x64 con GNOME son los destinos estables publicados. El [flujo Linux](#linux-060) requiere ext4 local del mismo montaje. Rechazo recursos de red, raíces de unidades, carpetas del sistema, enlaces simbólicos, junctions y marcadores de nube con reparse points. No ejecuto la aplicación como servicio.

En Windows compruebo antigüedad de escritura, dos observaciones estables y apertura exclusiva antes de considerar una descarga lista. Es una comprobación conservadora, sin garantía de que otro programa no vuelva a escribir después. Excluyo descargas parciales conocidas. Evita descargar directamente en las carpetas de categoría.

Los movimientos en Windows mantienen abierto el identificador del archivo verificado. Entre volúmenes, copio con Windows, verifico el flujo principal del destino y elimino el origen mediante su identificador verificado. No es una operación atómica: una interrupción puede dejar dos copias para inspección manual. Los archivos grandes pueden retrasar la pausa o la salida mientras termina la operación.

Mis protecciones cubren cambios ordinarios del sistema de archivos, no un proceso malicioso con los mismos privilegios del usuario. Mantén copias de seguridad de los archivos valiosos.

## macOS 0.4.0

Dirijo esta versión a Apple Silicon, macOS 15+ y carpetas APFS locales ordinarias fuera de sincronizadores. Rechazo movimientos entre volúmenes, enlaces, red y carpetas conocidas de nube. El destino debe estar en el mismo volumen APFS; dos volúmenes del mismo disco siguen siendo distintos. Mantengo desactivada la papelera nativa.

Cierra la aplicación anterior, extrae el ZIP y coloca `Dowlume.app` completa en Aplicaciones antes de abrirla o activar el inicio de sesión. Incluyo runtime, español y guías dentro del paquete. Uso firma ad hoc, sin Developer ID ni notarización. macOS puede bloquear la primera apertura; si ofrece **Abrir igualmente** en **Ajustes del Sistema → Privacidad y seguridad**, úsalo solo tras comprobar procedencia y hash. No pido desactivar Gatekeeper ni quitar cuarentena. Si la política del sistema impide abrir, registro esa limitación de instalación en la aceptación.

Permite acceso solo a las carpetas seleccionadas para organizar. Si se deniega, muestro un error y conservo los archivos; revisa **Ajustes del Sistema → Privacidad y seguridad → Archivos y carpetas** y reintenta. No necesito acceso total al disco. El aviso depende de la carpeta y los permisos ya concedidos.

**Comando-coma** abre Ajustes. Ocultar nativo y reabrir desde Dock recuperan la misma aplicación. Cerrar la ventana o **Salir** espera al trabajo y termina; no hay modo de bandeja macOS. El inicio automático se aplica a una `.app` instalada al siguiente inicio de sesión; la supervisión continúa pausada y los ajustes macOS pueden anularlo. Desactívalo antes de mover o desinstalar la app. Para desinstalar, sal y elimina la app; conservo perfil y archivos gestionados.

Guardo datos en `~/Library/Application Support/Dowlume`. En la primera apertura predeterminada copio el perfil cerrado `Dowlume.Preview` si no existe el nuevo, conservo el original y retengo idioma/ajustes/historial. Cierra antes la vista previa. Un perfil nuevo existente tiene prioridad; un destino incompleto o error de migración exige inspección, sin fusión automática. Respalda el perfil completo cerrado y los archivos antes de actualizar. `--data-directory` no migra el perfil personal ni modifica el inicio de sesión.

Si detecto una carrera de archivos/carpetas, conservo los objetos y puedo mostrar NeedsAttention. Actividad incluye origen, destino previsto y, cuando está disponible, ubicación observada del objeto abierto original. Conserva esos archivos antes de reintentar recuperación. Los bytes iguales no bastan para completar una operación macOS: debe coincidir también la identidad registrada de volumen/archivo. No puedo excluir cualquier escritor POSIX; mantén archivos editados activamente o sincronizados fuera de la carpeta gestionada.

## Linux 0.6.0

La [versión 0.6.0](https://github.com/mglezz/dowlume/releases/tag/v0.6.0)
admite Ubuntu 24.04 LTS x64 / GNOME con ext4 local.
Comprueba las [sumas de los paquetes](../releases/0.6.0.md#sumas-de-comprobación)
y empieza con archivos desechables.

Cierra Dowlume antes de instalar o actualizar. En Ubuntu ejecuta
`sudo apt install ./dowlume_0.6.0_amd64.deb`; instala la aplicación y declara sus
dependencias nativas. Abre Dowlume desde el menú de aplicaciones. Las actualizaciones
conservan el perfil y los archivos gestionados; respalda antes el perfil. Para quitar
el paquete, desactiva el inicio de sesión en Ajustes, cierra la app y ejecuta
`sudo apt remove dowlume`. El perfil y los archivos permanecen.

Para Portable, extrae todo el `.tar.gz` en una carpeta permanente de tu usuario y
arranca `./Dowlume.App`. `python3 register-menu.py` registra opcionalmente el menú;
`python3 register-menu.py --remove` elimina la entrada de esa copia; una copia antigua no puede eliminar la de una nueva. Para actualizar, cierra la app,
extrae la nueva versión en otra carpeta, compruébala y registra su entrada.
Vuelve a activar el inicio de sesión desde la copia nueva si lo utilizas. No sobrescribas
binarios en ejecución. `START-HERE.es.txt` incluye las dependencias nativas; .NET va incluido.

El perfil usa `$XDG_DATA_HOME/Dowlume`, normalmente `~/.local/share/Dowlume`.
El perfil anterior `Dowlume.Preview` se conserva sin migración automática. El inicio
opcional se activa en Ajustes y utiliza XDG Autostart en `$XDG_CONFIG_HOME/autostart`
(normalmente `~/.config/autostart`). La supervisión empieza en pausa. Cerrar la ventana
completa el trabajo activo y sale; no hay bandeja. Mostrar abre la carpeta contenedora.

Los movimientos requieren ext4 local del mismo montaje, con UUID y fecha de creación
verificables. Se rechazan enlaces, Btrfs/XFS, red/FUSE/overlay y movimientos entre montajes.
Elige carpetas fuera de sincronización y escrituras externas simultáneas. El mismo
portable puede funcionar en Fedora/Arch con glibc, bibliotecas nativas y ext4 compatibles;
siguen sin validación oficial. Consulta el [registro de aceptación](../releases/acceptance/0.6.0.md)
para distinguir la confirmación del mantenedor de las pruebas automáticas de los paquetes finales y sus límites de evidencia.
