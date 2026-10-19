# Usar Dowlume

[English](../USER_GUIDE.md) · [Índice en español](README.md)

Diseñé Dowlume para organizar descargas en Windows. Publiqué [v0.1.0 para Windows x64](https://github.com/mglezz/dowlume/releases/tag/v0.1.0) como instalador por usuario y ZIP portátil con .NET incluido. Empieza con copias de archivos de ejemplo en una carpeta desechable.

## Instalar y abrir

Descarga el instalador o el ZIP desde la publicación de GitHub. Los paquetes de v0.1.0 no están firmados: utiliza una fuente de confianza y compara el SHA-256 con `SHA256SUMS.txt`.

Ejecuta `Dowlume-0.1.0-win-x64-setup.exe` sin permisos de administrador. Para la versión portátil, extrae todo el ZIP y ejecuta `Dowlume.App.exe` junto a sus archivos. No necesita una instalación separada de .NET.

Incluyo `START-HERE.txt` para consulta sin conexión y un acceso **Dowlume quick start** en el menú Inicio. Cierra también cualquier instancia oculta en la bandeja antes de actualizar o desinstalar; el instalador se niega a continuar mientras la aplicación está abierta. Conservo los archivos organizados y el perfil local al desinstalar.

## Elegir idioma

En compilaciones del código actual, pausa la supervisión y abre **Settings → Language**. Selecciona **Español** o **English**. El cambio se aplica y se guarda inmediatamente para ese perfil. Al reiniciar se conserva; los perfiles antiguos empiezan en inglés.

Cambio pestañas, botones, confirmaciones, bandeja, categorías, estados, fechas y mensajes conocidos. Mantengo los nombres de archivos y carpetas, las rutas y los datos históricos. La carpeta `Expiring Files` conserva ese nombre. No guardo otros ajustes pendientes al cambiar de idioma. Los mensajes históricos desconocidos mantienen su texto original y los controles nativos del selector de carpetas siguen el idioma de Windows.

**Los paquetes publicados de v0.1.0 no incluyen el selector y siguen en inglés.** Para usarlo hasta una publicación posterior, [compila el código actual](GETTING_STARTED.md).

## Primer uso

1. Abre **Ajustes** y elige una carpeta local existente. Sugiero inicialmente la carpeta Descargas conocida por Windows.
2. Ajusta nombres de carpetas, extensiones, exclusiones y plazos de revisión. Guarda los ajustes.
3. Abre **Vista previa** y pulsa **Actualizar vista previa**. No mueve los archivos ni crea las carpetas de categoría.
4. Comprueba los destinos, pulsa **Organizar y supervisar** y confirma. La autorización incluye los archivos mostrados, las descargas futuras y las que terminen más tarde.
5. Consulta **Resumen** y **Actividad** para ver el resultado.

![Vista previa inglesa de v0.1.0 con archivos ficticios](../images/classification.png)

*Conservo las capturas de v0.1.0 en inglés como referencia histórica. [Procedencia](images/README.md).*

Solo descubro archivos directamente en la carpeta elegida. No importo ni reorganizo subcarpetas existentes. Creo las categorías y `Expiring Files` cuando hacen falta. Si un nombre está ocupado, añado un sufijo numérico; no sobrescribo el destino.

La supervisión empieza en pausa en cada arranque. Para continuar, revisa la vista previa y confirma otra vez. **Pausar** detiene la supervisión y espera a la operación activa. **Ocultar en la bandeja** mantiene la aplicación abierta; cerrar la ventana detiene la supervisión.

El icono puede aparecer entre los iconos ocultos del área de notificación. Un clic restaura la ventana; el menú contextual ofrece **Abrir Dowlume** y **Salir**. Windows permite fijarlo en el área visible.

En **Ajustes**, **Iniciar Dowlume al entrar en Windows** se aplica inmediatamente a tu cuenta y está desactivado inicialmente. La supervisión sigue empezando en pausa. Desactívalo antes de mover una copia portátil y actívalo de nuevo desde su ubicación final. Windows también puede desactivar aplicaciones de inicio. La desinstalación elimina la entrada solo si pertenece a esa instalación.

## Revisar archivos

Desactivé la papelera nativa en v0.1.0 porque las [pruebas de capacidad](RECYCLE_CAPACITY_INVESTIGATION.md) no conservaron los archivos de forma fiable. Puedes mantenerlos en revisión o usar las decisiones disponibles. La cuarentena recuperable es una propuesta, todavía no una función.

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
| Solo queda el destino y su contenido coincide | Completo el registro interrumpido. |
| Existen ambos, ninguno, el contenido difiere o falla la inspección | Registro Requiere atención y conservo los archivos. |

Copia las rutas desde Actividad y revísalas en el Explorador. Conserva el contenido que necesites. Para movimientos interrumpidos, reintenta cuando quede una copia inequívoca con el contenido esperado en una de las rutas registradas.

No repito envíos históricos a la papelera ni deduzco su éxito por la ausencia del origen. Un origen intacto permite marcarlos como fallidos; el contenido ausente o cambiado sigue requiriendo atención. Reintentar no restaura contenido perdido ni garantiza que Windows lo guardase.

Si editas un archivo registrado, rechazo las operaciones basadas en su huella anterior. Puedes moverlo fuera de la carpeta gestionada para administrarlo por tu cuenta. Para importarlo de nuevo intencionadamente, devuélvelo a la raíz con otro nombre y revisa la vista previa. No sigo automáticamente cambios de nombre externos.

## Ajustes y datos

- Aplico las reglas a futuras clasificaciones y a los plazos actuales. Cambiar un nombre de categoría en Ajustes no renombra la carpeta existente.
- Superviso una carpeta a la vez. Conservo el historial de las anteriores y recupero sus registros en el Resumen al volver a elegirlas.
- Guardo base de datos y diagnóstico en `%LOCALAPPDATA%\Dowlume`.
- La base de datos contiene rutas personales y huellas de contenido. El diagnóstico contiene eventos y tipos o códigos de error, sin rutas ni mensajes de excepción.
- Para una copia de seguridad, cierra Dowlume y copia todo el directorio de datos, incluidos los archivos auxiliares de SQLite.
- La desinstalación conserva archivos organizados y base de datos.
- `Dowlume.App.exe --data-directory <ruta-absoluta>` permite un perfil de pruebas aislado. No ejecutes perfiles distintos contra la misma carpeta.

## Límites

Distribuyo para Windows x64. Rechazo recursos de red, raíces de unidades, carpetas del sistema, enlaces simbólicos, junctions y marcadores de nube con reparse points. No ejecuto la aplicación como servicio.

Compruebo antigüedad de escritura, dos observaciones estables y apertura exclusiva antes de considerar una descarga lista. Es una comprobación conservadora, sin garantía de que otro programa no vuelva a escribir después. Excluyo descargas parciales conocidas. Evita descargar directamente en las carpetas de categoría.

Los movimientos en Windows mantienen abierto el identificador del archivo verificado. Entre volúmenes, copio con Windows, verifico el flujo principal del destino y elimino el origen mediante su identificador verificado. No es una operación atómica: una interrupción puede dejar dos copias para inspección manual. Los archivos grandes pueden retrasar la pausa o la salida mientras termina la operación.

Mis protecciones cubren cambios ordinarios del sistema de archivos, no un proceso malicioso con los mismos privilegios del usuario. Mantén copias de seguridad de los archivos valiosos.
