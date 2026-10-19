# Modelo de dominio

[English](../../requirements/DOMAIN_MODEL.md) · [Índice en español](../README.md)

## Alcance y método

Analizo Dowlume como un sistema de organización local de descargas, revisión y operaciones con historial. Examino qué archivos pueden organizarse, dónde corresponden, cuándo revisarlos y qué puedo concluir tras una interrupción.

Reconstruí el modelo del MVP Windows en la revisión `a4b1c9a` el 15 de octubre de 2026 a partir del comportamiento, reglas, documentación y pruebas. No lo presento como rediseño, esquema de base de datos ni diagrama de clases C#. No afirmo entrevistas con expertos o verificación formal. La actualización de contención del 17–18 de octubre desactiva la papelera y mantiene valores históricos legibles. El idioma de interfaz es una preferencia de presentación, sin cambios en las reglas del dominio.

Sigo la progresión de conceptos candidatos, asociaciones y atributos del material [mmasias/idsw1](https://github.com/mmasias/idsw1/blob/main/temario/contenidos/00004-MdD.md), y las vistas complementarias de [mmasias/hdC](https://github.com/mmasias/hdC/blob/main/docs/UMLdocs/mdd.md). Añado actividades y secuencias para explicar el comportamiento concreto de Dowlume.

Incluyo una carpeta local a la vez, clasificación por extensión, vista previa, consentimiento, antigüedad, revisión, decisiones, historial, deshacer y reconciliación. Excluyo red/nube, eliminación automática permanente, clasificación por contenido, detección de duplicados y supervisión simultánea de varias carpetas. Mantengo catálogos de actores, especificación de requisitos y casos de uso como trabajo separado.

## Conceptos candidatos y seleccionados

No convierto cada sustantivo en una clase. Distingo entidades, valores, estados, acciones, contexto externo y mecanismos técnicos.

| Concepto | Significado y decisión |
| --- | --- |
| Carpeta de descargas (`Downloads Folder`) | Límite de descubrimiento directo y contexto de origen. Puede persistir en el historial tras cambiar la carpeta activa. |
| Política de organización (`Organization Policy`) | Carpeta elegida, exclusiones y seis reglas; no representa todos los ajustes técnicos de la aplicación. |
| Categoría (`Category`) | Documentos, Imágenes, Vídeos, Comprimidos, Instaladores u Otros. Son instancias de un concepto, no seis subclases ni directorios físicos. |
| Regla de categoría (`Category Rule`) | Extensiones, nombre de carpeta y días hasta revisión. Permite variar el tratamiento sin cambiar la identidad de la categoría. |
| Archivo descargado (`Downloaded File`) | Candidato transitorio o archivo registrado durante un episodio de gestión. Conserva identidad a lo largo de sus decisiones. |
| Zona de revisión (`Review Area`) | `Expiring Files`, bajo la carpeta de descargas, para esperar una decisión; no es categoría ni calendario de eliminación. |
| Vista previa (`Preview`) | Conjunto transitorio de propuestas ligado a una política; puede estar vacío y no se conserva como autorización histórica. |
| Propuesta de organización (`Organization Proposal`) | Origen observado, categoría y destino propuestos, y contenido observado. No reserva el destino ni autoriza por sí sola. |
| Operación (`Operation`) | Intento con intención, resultado y posible referencia a lo deshecho; puede existir antes del primer registro confirmado del archivo. |
| Condición de archivo (`File Condition`) | Valor inmutable de hechos anteriores o previstos de una operación; no es otra copia física ni entidad independiente. |

Conservar, Posponer, Mover, Papelera y Deshacer son tipos de acción, no objetos duraderos propios. Edad, nombre, ruta, extensión y huella son valores. Usuario, navegador, sistema operativo y papelera son contexto externo. Excluyo del modelo watcher, canal, semáforo, SQLite, repositorio, modelos de vista y bandeja; sus mecanismos están en [Arquitectura](../ARCHITECTURE.md).

## Identidad y límites

La identidad pertenece a un episodio de gestión. Nombre y hash no son identidades globales: dos archivos pueden compartirlos y una reimportación puede iniciar otro episodio.

Una ruta registrada expresa conocimiento, no presencia garantizada. Un archivo Missing conserva su última ruta; uno Recycled conserva su ubicación histórica, no una dirección navegable en la papelera. Un movimiento puede haber ocurrido físicamente mientras el estado registrado sigue siendo anterior.

La primera organización inicia la antigüedad. Mantengo ese instante al completar, recuperar, revisar o deshacer; no es la fecha del navegador ni el primer evento del watcher. Una nueva tentativa de organización fallida puede iniciar otro episodio.

El consentimiento autoriza archivos previsualizados, futuros y todavía incompletos de esa carpeta durante la supervisión. No lo modelo como entidad persistida: tras reiniciar exijo vista previa y confirmación de nuevo. La carpeta de origen expresa descubrimiento, no propiedad física actual.

## Vistas UML compartidas

Mantengo las imágenes y fuentes originales en inglés, con vocabulario equivalente arriba, para evitar dos modelos divergentes. Cada enlace abre el SVG completo y su fuente editable. En [Reproducción de diagramas](diagrams/domain-model/README.md) explico el proceso.

| Vista | SVG | Fuente |
| --- | --- | --- |
| Clases iniciales | [Imagen](../../requirements/diagrams/domain-model/svg/classes-initial.svg) | [PlantUML](../../requirements/diagrams/domain-model/src/classes-initial.puml) |
| Clases intermedias | [Imagen](../../requirements/diagrams/domain-model/svg/classes-intermediate.svg) | [PlantUML](../../requirements/diagrams/domain-model/src/classes-intermediate.puml) |
| Clases detalladas | [Imagen](../../requirements/diagrams/domain-model/svg/classes-detailed.svg) | [PlantUML](../../requirements/diagrams/domain-model/src/classes-detailed.puml) |
| Objetos de ejemplo | [Imagen](../../requirements/diagrams/domain-model/svg/objects-review.svg) | [PlantUML](../../requirements/diagrams/domain-model/src/objects-review.puml) |
| Estados del archivo | [Imagen](../../requirements/diagrams/domain-model/svg/states-file.svg) | [PlantUML](../../requirements/diagrams/domain-model/src/states-file.puml) |
| Estados de operación | [Imagen](../../requirements/diagrams/domain-model/svg/states-operation.svg) | [PlantUML](../../requirements/diagrams/domain-model/src/states-operation.puml) |
| Actividad de organización | [Imagen](../../requirements/diagrams/domain-model/svg/activity-organization.svg) | [PlantUML](../../requirements/diagrams/domain-model/src/activity-organization.puml) |
| Actividad de revisión | [Imagen](../../requirements/diagrams/domain-model/svg/activity-review.svg) | [PlantUML](../../requirements/diagrams/domain-model/src/activity-review.puml) |
| Secuencia de organización | [Imagen](../../requirements/diagrams/domain-model/svg/sequence-organization.svg) | [PlantUML](../../requirements/diagrams/domain-model/src/sequence-organization.puml) |
| Secuencia de recuperación | [Imagen](../../requirements/diagrams/domain-model/svg/sequence-recovery.svg) | [PlantUML](../../requirements/diagrams/domain-model/src/sequence-recovery.puml) |

Las tres vistas de clases refinan un mismo MVP, no versiones de la aplicación. La inicial distingue archivo, descubrimiento, clasificación y zona de espera. La intermedia añade política, reglas y operaciones con multiplicidades. La detallada incorpora propuestas, atributos y condiciones históricas.

La instantánea de objetos muestra una revisión válida con fechas y relaciones coherentes; no implica envejecimiento anticipado ni decisión automática. Las actividades explican alternativas y las secuencias muestran el orden conceptual de observaciones, consentimiento e intención, sin modelar llamadas entre clases técnicas.

## Ciclo del archivo

| Evento | Estado resultante |
| --- | --- |
| Organización confirmada | Organized, con categoría, ubicación y origen de antigüedad. |
| Vence antigüedad o aplazamiento | InReview en la zona de revisión. Kept no vence. |
| Conservar | Kept en la carpeta actualmente configurada para su categoría. |
| Posponer | Snoozed en su categoría, con plazo de siete días desde la decisión. |
| Mover | Moved, fuera de la gestión; el destino puede ser una subcarpeta permitida dentro de Descargas. |
| Papelera histórica | Recycled sigue siendo legible, pero no creo esa transición ni garantizo restauración. |
| Falta un archivo gestionado | Missing, salvo bloqueo por operación sin resolver. Devolverlo físicamente no restaura automáticamente su estado anterior. |
| Deshacer Organizar | Regresa a la ubicación inicial como Kept. |
| Deshacer Revisión | Regresa a la ubicación previa como Snoozed durante siete días. |
| Deshacer Conservar, Posponer o Mover | Restaura los hechos anteriores de InReview y su ubicación. |

Ningún estado final implica destrucción física. Moved todavía puede admitir Deshacer. Renombrados y ediciones externas no crean transiciones automáticas inventadas.

## Ciclo de la operación

Separo resultado del intento y estado del archivo: puede haber un archivo Organized con una revisión Pending o NeedsAttention. Una primera organización puede quedar incierta antes de crear el registro confirmado del archivo.

| Evidencia de recuperación | Resultado |
| --- | --- |
| Solo origen con contenido esperado | Failed; permite una nueva tentativa de la acción original. |
| Solo destino con contenido esperado | Completed; confirma hechos previstos sin mover otra vez. |
| Ambos, ninguno, contenido distinto o inspección fallida | NeedsAttention; conservo copias y bloqueo acciones afectadas. |
| Papelera histórica con origen coincidente | Failed; el envío sigue desactivado. |
| Papelera histórica con origen ausente o cambiado | NeedsAttention; no repito ni infiero éxito por otro destino. |

Cancelar antes de registrar no crea una operación. Reintentar crea un intento nuevo; no borro el fallo anterior. Deshacer también crea una operación y mantiene Completed en la original.

## Procesos y escenarios

La organización comienza con política, vista previa, confirmación, comprobaciones por propuesta y supervisión. La vista previa no mueve archivos recién descubiertos ni crea categorías, pero puede reconciliar registros interrumpidos previos; no promete cero cambios al historial.

Una vista previa vacía permite autorizar descargas futuras. Omite candidatos que ya no estén listos y rechaza contenido cambiado. Recalculo destinos porque la vista previa no reserva nombres. Un fallo en el lote inicial impide empezar la supervisión, pero conserva las acciones anteriores ya completadas: el lote no es atómico.

En revisión, la espera puede ser indefinida. Conservar y Posponer ejecutan la decisión seleccionada sin confirmación adicional; Mover requiere carpeta y confirmación. Cancelar mantiene InReview. Siempre compruebo disponibilidad, contenido, rutas y operaciones pendientes.

En el ejemplo de colisión, un `report.pdf` nuevo no sobrescribe `Documents/report.pdf`: propongo `report (1).pdf`, vuelvo a comprobar tras el consentimiento y registro intención antes de mover. Si cambia la ocupación, puede cambiar el sufijo. Si cambia el contenido, rechazo la propuesta.

En recuperación, la evidencia del origen y destino decide el resultado. Confirmar historial y hechos representa una actualización lógica, no una supuesta transacción distribuida entre disco y base de datos.

## Reglas de negocio

Conservo los identificadores originales para poder cruzar ambas versiones.

| Regla | Restricción |
| --- | --- |
| **DM-01 — Alcance** | Una carpeta local configurada a la vez, descubrimiento no recursivo y conservación del historial de carpetas previas. |
| **DM-02 — Ubicaciones** | Carpeta existente y local; rechazo raíces, red, sistema, datos de aplicaciones, enlaces, junctions y rutas de nube con reparse points. Admito carpetas temporales desechables de prueba. |
| **DM-03 — Disponibilidad** | Excluyo extensiones parciales, exclusiones configuradas, nombres que empiezan por `~` y rutas gestionadas o bloqueadas. Un evento no demuestra descarga completa. |
| **DM-04 — Clasificación** | Seis categorías fijas; extensiones sin distinguir mayúsculas, como máximo una regla por extensión; desconocidas a Other. Pueden configurarse extensiones explícitas para Other. |
| **DM-05 — Política** | Nombres seguros, únicos sin distinguir mayúsculas y distintos de `Expiring Files`; plazos enteros de 1 a 3650 días. Los cambios no renombran carpetas ni recategorizan registros existentes. |
| **DM-06 — Consentimiento** | Vista previa antes de empezar, política todavía coincidente y contenido verificado otra vez. Confirmar cubre llegadas actuales, futuras e incompletas. Reiniciar no restaura la autorización. |
| **DM-07 — Conservación** | No sobrescribo destinos ni realizo eliminación permanente de archivos gestionados. Uso sufijos libres y rechazo destinos inválidos. No acepto automáticamente contenido editado. |
| **DM-08 — Antigüedad** | Organized vence cuando `now >= firstOrganizedAt + current category reviewAfterDays`; Snoozed cuando `now >= snoozedUntil`. Actúo en una reconciliación posterior, no necesariamente en el instante exacto. |
| **DM-09 — Revisión** | Traslado a un nombre disponible conservando categoría y fecha inicial. InReview no caduca. Las decisiones requieren un archivo seleccionado actualmente en revisión. Papelera desactivada. |
| **DM-10 — Decisiones** | Conservar excluye futuras revisiones; Posponer añade siete días. Ambos vuelven a la carpeta de categoría actual. Mover deja de gestionar, incluso en una subcarpeta permitida de Descargas, pero no admite la raíz ni el directorio de revisión actual como destino. |
| **DM-11 — Papelera** | Rechazo antes de acceso a archivos o registro. Mantengo tipos históricos; ausencia no prueba éxito ni recuperación. No repito ni deshago envíos nativos. Cuarentena pendiente. |
| **DM-12 — Registro** | Intención antes de actuar, hechos y finalización confirmados juntos después. Antes del registro se puede cancelar sin intento; después debe completarse o conservar un resultado reconciliable. |
| **DM-13 — Recuperación** | Solo evidencia inequívoca con contenido esperado permite Completed o Failed. NeedsAttention conserva copias y bloquea sujeto y rutas registradas. No reserva todos los destinos futuros ni elimina copias ambiguas. |
| **DM-14 — Deshacer** | Exijo la última acción completada compatible, hechos actuales coincidentes, contenido listo e intacto, retorno libre dentro de la raíz actual y ausencia de bloqueos o deshacer previo. No deshago Recycle ni Undo. |
| **DM-15 — Consecuencias** | Deshacer Organizar restaura Kept; Revisión restaura Snoozed siete días; Conservar/Posponer/Mover restauran InReview. Mantengo la fecha inicial. Tras Deshacer no se habilita automáticamente otra acción anterior. |
| **DM-16 — Cambios externos** | No reparo automáticamente Missing ni renombrados por redescubrimiento. Rechazo contenido que no coincide con la huella esperada. Seguimiento de renombrados, aceptación de ediciones y deduplicación quedan fuera. |

Los plazos iniciales son Documentos 30, Imágenes 60, Vídeos 30, Comprimidos 14, Instaladores 7 y Otros 30 días. Son valores de configuración, no propiedades inmutables de las categorías.

## Decisiones de modelado y evidencia

Distingo clasificación de ubicación, candidatos de registros confirmados, estado del archivo de resultado del intento, e intención de hechos completados. Las rutas y huellas son valores; las copias físicas ambiguas se explican por ubicaciones observadas, sin añadir una función de duplicados.

No infiero historial duradero de vista previa, registro persistente de consentimiento ni repositorio conceptual de carpetas. Las condiciones Before e IntendedAfter pertenecen a una operación sin implicar destrucción del archivo. El episodio no promete continuidad de identidad tras renombrado externo o reimportación.

Contrasté el modelo con [AppSettings](../../../Dowlume/Dowlume.Core/AppSettings.cs), [TrackedFile](../../../Dowlume/Dowlume.Core/TrackedFile.cs), [DownloadOrganizer](../../../Dowlume/Dowlume.Infrastructure/DownloadOrganizer.cs), [MainViewModel](../../../Dowlume/Dowlume.App/ViewModels/MainViewModel.cs) y las pruebas de dominio, supervisión, seguridad y escritorio. La [tabla original de evidencias](../../requirements/DOMAIN_MODEL.md#evidence-and-consistency-review) mantiene el detalle de enlaces por regla.

Reviso multiplicidades, hechos opcionales, condiciones de transición, resultados de recuperación y fechas del escenario. Las pruebas existentes y la sintaxis PlantUML no constituyen demostración exhaustiva. Registro renderizado, revisión visual y validación de enlaces por separado en [Validación](../VALIDATION.md).
