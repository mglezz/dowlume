# Revisión de calidad del código — 2026-11-02

[English](../../development/code-quality-review.md) · [Documentación](../README.md)

La base es `f308eb9`, posterior a la publicación de 0.5.0. Se revisan capas, reglas
del dominio, organización, persistencia, supervisión, integración de escritorio,
traducción y aceptación de paquetes. Es una revisión de mantenibilidad con pruebas
de regresión, no una demostración formal, auditoría completa de seguridad o garantía
de perfección. Los binarios publicados de 0.5.0 conservan su revisión original.

Se aplican nombres claros, responsabilidades concretas, errores explícitos, poco
acoplamiento y eliminación de conocimiento duplicado, siguiendo criterios de
[Clean Code](https://www.informit.com/store/clean-code-a-handbook-of-agile-software-craftsmanship-9780132639408)
y la [explicación del autor sobre nombres y funciones](https://www.informit.com/articles/article.aspx?p=3222335).
El número de líneas no determina por sí solo la responsabilidad de una clase.

## Cambios realizados

- Unificar los escenarios casi idénticos de aceptación macOS/Linux en
  `NativePackageAcceptance`, con etapas de idiomas, archivos, supervisión e informe.
  Se conservan las comprobaciones nativas, los nombres de informes y el requisito de
  perfil aislado. Se recorren las pestañas existentes y se exige navegación no vacía.
- Sustituir la comparación de JSON de la vista previa por comparación explícita de
  valores en `AppSettings` y `CategoryRule`, conservando orden y mayúsculas. Las
  pruebas comprueban valores equivalentes y cambios; la prueba de escritorio impide
  organizar o abrir confirmación con reglas distintas de las previsualizadas.
- Liberar la conexión SQLite si falla su apertura/configuración antes de devolverla.
  Se conserva la propiedad de los recursos en las rutas correctas. La liberación
  excepcional se revisa estáticamente; no se afirma inyección de un fallo PRAGMA.
- Comprobar el ejecutable registrado antes de retirar el inicio automático de
  macOS/Linux. Las pruebas nativas comprueban que desactivar una copia antigua
  conserva byte a byte la entrada de la copia nueva, como ya hace Windows.

## Decisiones conservadas

Core contiene reglas y contratos sin UI ni SQLite; Infrastructure implementa
persistencia y operaciones nativas; Monitoring depende de Core y recibe callbacks.
La composición de servicios permanece en la aplicación.

Las comprobaciones repetidas de identidad, contenido y rutas protegen frente a
cambios concurrentes. Windows, APFS y ext4 tienen contratos diferentes; no se unifican
por similitud textual. Los semáforos y su liberación explícita hacen visible el orden
entre cancelación, diario y movimiento. Los errores esperados de disponibilidad,
los registros sin datos privados y las excepciones de las fronteras UI tienen
tratamientos distintos y justificados.

Se conservan valores de enums persistidos, esquema 1, mensajes del diario y traducción.
Cambiar esos contratos solo por estilo exigiría migración y evidencia de compatibilidad.

## Compromisos de diseño pendientes de evolución

`MainViewModel` sigue coordinando ajustes, comandos, refresco y cierre con servicios
concretos. Una futura extracción por funciones debe reducir acoplamiento de verdad;
repartir métodos en archivos parciales no basta. Debe preservar consentimiento y cierre.

Los registros de ajustes contienen listas y arrays mutables: no son profundamente
inmutables. La comparación nueva no cambia igualdad de registros ni propiedad de
colecciones. Los nuevos consumidores no deben modificar ajustes compartidos con la
supervisión activa. Un modelo inmutable requiere trabajo de compatibilidad del perfil
y del editor de ajustes.

Organización y recuperación todavía cargan colecciones completas; la paginación de
actividad no limita todo el coste de las consultas JSON. El alcance sigue siendo
carpetas personales, sin afirmar rendimiento para millones de registros.

## Validación

Antes del merge se requieren las comprobaciones de contribución, pruebas compartidas
y nativas y paquetes en ambos sistemas POSIX. Windows por sí solo no verifica inicio
automático ni paquetes macOS/Linux. Esta revisión no implica nuevas pruebas manuales
ni una nueva publicación.
