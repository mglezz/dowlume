# Investigación de la capacidad de la Papelera

[English](../RECYCLE_CAPACITY_INVESTIGATION.md) · [Índice en español](README.md)

**Estado:** conservo los fallos históricos y mantengo desactivada la papelera nativa en v0.1.0. No he implementado la cuarentena recuperable. No modifico la máquina de evidencia, checkpoints, exportaciones ni informes, y no repito las pruebas que perdieron contenido.

## Decisión de contención

El 17 de octubre de 2026 autoricé continuar la preparación de la versión con la papelera desactivada. Rechazo la acción desde interfaz, API, ejecutor interno y adaptador antes de tocar archivos o crear registros. Excluí de compilación el adaptador y callbacks COM y conservé sus bytes en el [archivo histórico](archive/native-recycle/README.md).

Mantengo Conservar, Posponer, Mover y Deshacer compatible. Los tipos y estados históricos conservan su identidad serializada. Inspecciono los envíos antiguos: si el origen está intacto, marco el intento como fallido; si falta o cambió, requiere atención. No repito la operación, no deduzco éxito por un destino registrado y no afirmo haber recuperado contenido ausente.

El 18 de octubre completé aceptación y publicación del candidato con esa contención. Los fallos nativos no se convierten en éxitos; cualquier sustitución futura necesita diseño y aceptación propios.

## Candidato y resultados originales

Las pruebas nativas usaron `cf42326f4560515b1574ebe7794ef645a551bc21`, Windows 11 Pro x64 26300 y cuenta estándar `DowlumeTester`. El arnés autónomo cargó las bibliotecas Core, Infrastructure y Monitoring del ZIP verificado, sin .NET compartido. El instalador tenía SHA-256 `73C465CBFC4A47562D4859B9EF9AE0338EF66C9D836C5B07599E4CD37C93F507`; el archivo del arnés, `3583A8649689EAE32E97882FB0ED61C320515387F4647CCBD2168C4A48951F82`.

| Caso | Resultado registrado |
| --- | --- |
| Muestra de 64 KiB, envío ordinario y restauración | Superado; hash restaurado coincidente. |
| Papelera desactivada, muestra de 64 KiB | Superado; rechazo y origen conservado. |
| Muestra de 2 MiB con capacidad de 1 MiB | Fallo: excepción después de desaparecer el origen; sin origen ni elemento recuperable visible en los inventarios finales. |
| Dos muestras de 768 KiB con capacidad de 1 MiB | Fallo: desapareció la muestra anterior mientras se aceptaba la nueva. |
| Restauración de la política original del invitado | Informada como satisfactoria. |

El arnés exigía una papelera inicialmente vacía, generaba datos ficticios aleatorios y verificaba hashes. Intentaba restaurar las muestras restantes después de cada caso, sin vaciar la papelera. Por ello separo las observaciones inmediatas de los inventarios finales.

## Dos ejecuciones y orden de restauración

Conservo dos ejecuciones existentes, no una repetición nueva. Sus informes sobredimensionados corresponden a `2026-10-16T18:46:21.5742316Z` y `2026-10-16T18:49:40.815674Z`. Ambos registran fallo del adaptador, `System.IO.IOException`, origen ausente y hash nulo. Distingo el HRESULT administrado `-2146232800` del resultado shell `00270008`.

Al revisar arnés y worker comprobé que la existencia del origen se registraba antes de intentar restaurarlo. La restauración buscaba el contenido por hash, rechazaba una ruta ocupada e invocaba únicamente `undelete` sobre la ruta original. No había un destino alternativo ni limpieza de archivos. La restauración de la muestra entrante explica que reaparezca después; no explica que la muestra anterior ya hubiera desaparecido antes.

Los inventarios finales solo contenían las muestras ordinaria, de papelera desactivada y de capacidad próxima. No quedaban las muestras sobredimensionadas ni las anteriores expulsadas. La eliminación permanente nativa y la expulsión por capacidad son las principales explicaciones, pero no tengo una traza completa de callbacks ni prueba de borrado físico irreversible. El código `00270008` por sí solo no identifica la condición fallida.

El recolector inicial leía el espacio de nombres del shell. Amplié el diagnóstico de solo lectura para inventariar entradas directas, incluidas ocultas, en `$Recycle.Bin` del usuario y volumen de las muestras. Una lectura incompleta nunca equivale a una papelera vacía. No cambio permisos, restauro, borro ni recorro todo el disco.

## Metadatos y contenido

El inventario completo encontró cuatro archivos `$I`, `desktop.ini` y ningún contenido `$R`. Los registros `$I8YR4X7.bin`, `$I73L38B.bin`, `$IEB7VE0.bin` y `$I5N7735.bin` corresponden a las muestras ordinarias o cercanas a capacidad que sí se restauraron. No corresponden al contenido perdido.

Verifiqué bytes y hashes y decodifiqué los cuatro registros. Los metadatos conservan tamaño, fecha y ruta original, no el contenido ni su hash. La investigación del [formato de Joachim Metz](https://github.com/libyal/dtformats/blob/main/documentation/Windows%20Recycle.Bin%20file%20formats.asciidoc) fundamenta esa interpretación. Los hashes esperados provienen de los informes de las muestras, no de los metadatos.

`Read-WindowsRecycleMetadata.ps1` comprueba un manifiesto, abre identificadores que impiden escritura/eliminación y muestra JSON sin escribir informes ni llamar al adaptador. Conservo rutas y metadatos completos en almacenamiento privado.

## Acceso al checkpoint y límites de recuperación

Preservé tres checkpoints. El del fallo se creó a las `2026-10-16T18:49:38.5401180Z`, antes del segundo informe sobredimensionado; no asumo que incluya ambas ejecuciones. Los checkpoints anteriores preceden la generación de datos aleatorios y no contienen necesariamente las muestras buscadas.

La primera inspección de la exportación no pudo enumerar NTFS por cifrado BitLocker. No interpreto esa lista vacía como evidencia negativa de recuperación. Después inspeccioné una copia física independiente, sin montar ni arrancar, cuyo SHA-256 se mantuvo en `3AE5611E04F6031DF15EC3E4F6B78323C3B1D8E0B29650892461866751AC65C5`, con 25.337.790.464 bytes.

Encontré una clave clara existente en la partición 3 y desbloqueé el volumen en memoria, sin publicar material de claves ni modificar protectores o sectores. La inspección confirmó las muestras supervivientes de la primera ejecución; no encontró el contenido sobredimensionado ni la muestra anterior. La segunda carpeta no estaba en ese checkpoint, de forma coherente con su fecha.

También recorrí registros válidos y eliminados de la MFT buscando nombres y tamaños pertinentes. No encontré candidatos cuyo contenido pudiera verificar. No fue una búsqueda de todos los clusters ni descarta registros reutilizados, restos sin nombre o contenido fuera de esa imagen. **Pude acceder al checkpoint; no recuperé el contenido ausente.** Crear un protector nuevo en el invitado actual no lo añadiría retrospectivamente al checkpoint antiguo.

Conservo versiones de herramientas, hashes, tiempos, tablas y límites completos en el [registro original](../RECYCLE_CAPACITY_INVESTIGATION.md). No instalé herramientas en el invitado de evidencia ni escribí resultados recuperados sobre su disco.

## Contrato del adaptador retirado

El código archivado pedía reciclaje cuando fuera posible y aceptaba confirmaciones del shell. El bit `0x80` del callback no reservaba capacidad ni garantizaba retención. Comprobar la muestra entrante después de la operación no podía revertir una eliminación ni demostrar conservación de muestras anteriores.

Ni un aviso, ni comprobar espacio libre, ni consultar capacidad antes de llamar al shell garantizan recuperación duradera. Pueden existir carreras y políticas que eliminen contenido posteriormente. Por ello no uso la retención de la Papelera como garantía de recuperación de Dowlume.

## Cuarentena recuperable propuesta

Propongo **Mover a recuperación**, con una carpeta `Dowlume Recovery` en el mismo volumen, excluida del descubrimiento y reservada frente a nombres de categoría. Explicaría que conserva archivos y no libera espacio.

1. Validar hash, identidad, volumen, rutas y bloqueos; elegir un destino único que no exista.
2. Registrar origen, destino, hash, identificador y estado Pendiente antes de mover.
3. Renombrar mediante el identificador verificado en el mismo volumen, sin shell delete ni alternativa copiar/eliminar. Si falla la seguridad o el espacio de metadatos, conservar origen.
4. Verificar y confirmar estado e historial. Tras una interrupción, preservar copias ambiguas.
5. Ofrecer Restaurar explícito, con hash verificado y rechazo de destinos ocupados; conservar flujos alternativos y metadatos.
6. Conservar indefinidamente, sin caducidad, expulsión por capacidad, limpieza automática ni eliminación al desinstalar.

Usaría tipo de operación y estado propios sin reinterpretar registros históricos. Actualizaría juntos dominio, interfaz, compatibilidad del historial, recuperación, guías y aceptación. No sería una copia de seguridad frente a avería de disco o procesos hostiles.

## Regresión y aceptación futuras

Comprobaría independencia de la política de papelera, origen intacto ante falta de espacio/permisos, colisiones, rutas ocupadas, enlaces, cambios de volumen, contenido y flujos alternativos. Probaría concurrencia, interrupciones antes y después del renombrado y persistencia tras reinicio, cambio de carpeta, actualización y desinstalación.

Haría estas pruebas en un invitado independiente con manifiestos duraderos y hashes antes y después de cada operación. Empezaría con fallos simulados; cualquier prueba real de volumen lleno usaría un volumen desechable dedicado. Conservaría fallos y resultados nuevos por separado. No repetiría los sondeos fallidos en la máquina preservada para probar cambios especulativos de flags.
