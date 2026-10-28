# Usar Dowlume

[English](../../user/guide.md) · [Índice en español](../README.md)

Dowlume organiza descargas completas en Windows x64 y macOS Apple Silicon. La [versión 0.4.0](https://github.com/mglezz/dowlume/releases/tag/v0.4.0) incluye la aplicación para macOS, un instalador por usuario y un ZIP portátil para Windows, con .NET incluido. Consulta las [instrucciones de macOS](#macos-040) para instalar la aplicación en ese sistema. Empieza con copias de archivos de ejemplo en una carpeta desechable.

## Instalar y abrir

Elige el instalador Windows para una instalación habitual, o **Portable** para descargar la aplicación completa en un ZIP desde la publicación de GitHub. Los paquetes Windows de v0.4.0 no tienen firma de editor: utiliza una fuente de confianza y compara el SHA-256 con las [sumas de comprobación](../releases/0.4.0.md#sumas-de-comprobación).

Ejecuta `Dowlume-0.4.0-win-x64-setup.exe` sin permisos de administrador. Para la versión portátil, extrae todo el ZIP y ejecuta `Dowlume.App.exe` junto a sus archivos. No necesita una instalación separada de .NET.

Incluyo `START-HERE.txt` para consulta sin conexión y un acceso **Dowlume quick start** en el menú Inicio. Cierra también cualquier instancia oculta en la bandeja antes de actualizar o desinstalar; el instalador se niega a continuar mientras la aplicación está abierta. Conservo los archivos organizados y el perfil local al desinstalar.

## Elegir idioma

Pausa la supervisión y abre **Settings → Language**. Selecciona **Español** o **English**. El cambio se aplica y se guarda inmediatamente para ese perfil. Al reiniciar se conserva; los perfiles antiguos empiezan en inglés.

Cambio pestañas, botones, confirmaciones, bandeja, categorías, estados, fechas y mensajes conocidos. Mantengo los nombres de archivos y carpetas, las rutas y los datos históricos. La carpeta `Expiring Files` conserva ese nombre. No guardo otros ajustes pendientes al cambiar de idioma. Los mensajes históricos desconocidos mantienen su texto original y los controles nativos del selector de carpetas siguen el idioma de Windows.

**El instalador y el ZIP portátil publicados de 0.4.0 incluyen el selector.** Los paquetes anteriores de 0.1.0 siguen en inglés.

## Primer uso

1. Abre **Ajustes** y elige una carpeta local existente. Sugiero inicialmente la carpeta Descargas conocida por Windows.
2. Ajusta nombres de carpetas, extensiones, exclusiones y plazos de revisión. Guarda los ajustes.
3. Abre **Vista previa** y pulsa **Actualizar vista previa**. No mueve los archivos ni crea las carpetas de categoría.
4. Comprueba los destinos, pulsa **Organizar y supervisar** y confirma. La autorización incluye los archivos mostrados, las descargas futuras y las que terminen más tarde.
5. Consulta **Resumen** y **Actividad** para ver el resultado.

![Vista previa inglesa de v0.1.0 con archivos ficticios](../../images/classification.png)

*Conservo las capturas de v0.1.0 en inglés como referencia histórica. [Procedencia](../images/README.md).*

Solo descubro archivos directamente en la carpeta elegida. No importo ni reorganizo subcarpetas existentes. Creo las categorías y `Expiring Files` cuando hacen falta. Si un nombre está ocupado, añado un sufijo numérico; no sobrescribo el destino.

La supervisión empieza en pausa en cada arranque. Para continuar, revisa la vista previa y confirma otra vez. **Pausar** detiene la supervisión y espera a la operación activa. **Ocultar en la bandeja** mantiene la aplicación abierta; cerrar la ventana detiene la supervisión.

El icono puede aparecer entre los iconos ocultos del área de notificación. Un clic restaura la ventana; el menú contextual ofrece **Abrir Dowlume** y **Salir**. Windows permite fijarlo en el área visible.

En **Ajustes**, **Iniciar Dowlume al entrar en Windows** se aplica inmediatamente a tu cuenta y está desactivado inicialmente. La supervisión sigue empezando en pausa. Desactívalo antes de mover una copia portátil y actívalo de nuevo desde su ubicación final. Windows también puede desactivar aplicaciones de inicio. La desinstalación elimina la entrada solo si pertenece a esa instalación.

## Revisar archivos

Desactivé la papelera nativa en v0.1.0 porque las [pruebas de capacidad](../archive/recycle-capacity-investigation.md) no conservaron los archivos de forma fiable. Puedes mantenerlos en revisión o usar las decisiones disponibles. La cuarentena recuperable es una propuesta, todavía no una función.

Calculo la antigüedad desde la primera organización, no desde la descarga o modificación. Al vencer el plazo de la categoría, traslado el archivo a `Expiring Files`.

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

Windows x64 y macOS Apple Silicon son los destinos estables publicados. La [candidata Linux 0.5.0](#linux-050-preliminar) activa el flujo nativo en ext4 local, con aceptación manual pendiente. Rechazo recursos de red, raíces de unidades, carpetas del sistema, enlaces simbólicos, junctions y marcadores de nube con reparse points. No ejecuto la aplicación como servicio.

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

## Linux 0.5.0 preliminar

La [prerelease 0.5.0-rc.1](https://github.com/mglezz/dowlume/releases/tag/v0.5.0-rc.1)
es para pruebas, inicialmente en Ubuntu 24.04 LTS x64 / GNOME con ext4 local.
Comprueba las [sumas de los paquetes](../releases/0.5.0.md#sumas-de-comprobación)
y empieza con archivos desechables.

Cierra Dowlume antes de instalar o actualizar. En Ubuntu ejecuta
`sudo apt install ./dowlume_0.5.0_amd64.deb`; instala la aplicación y declara sus
dependencias nativas. Abre Dowlume desde el menú de aplicaciones. Las actualizaciones
conservan el perfil y los archivos gestionados; respalda antes el perfil. Para quitar
el paquete, desactiva el inicio de sesión en Ajustes, cierra la app y ejecuta
`sudo apt remove dowlume`. El perfil y los archivos permanecen. La aceptación de
instalación limpia, actualización y desinstalación sigue pendiente.

Para Portable, extrae todo el `.tar.gz` en una carpeta permanente de tu usuario y
arranca `./Dowlume.App`. `python3 register-menu.py` registra opcionalmente el menú;
`python3 register-menu.py --remove` elimina esa entrada. Para actualizar, cierra la app,
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
siguen sin validación oficial. Consulta el [registro de aceptación](../releases/acceptance/0.5.0.md)
para separar automatización de pruebas pendientes GNOME X11/Wayland, inicio de sesión,
instalación e interrupción/reinicio.
