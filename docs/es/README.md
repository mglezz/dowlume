# Dowlume

[English](../../README.md) · [Español](README.md)

**Tus descargas, bajo control.**

Desarrollo Dowlume como una aplicación de escritorio para Windows que organiza las descargas completas, lleva los archivos antiguos a una zona de revisión y deja la decisión final en tus manos. Mantengo todos los datos en el equipo, sin cuentas, sincronización en la nube ni telemetría.

**Estado:** publiqué [v0.3.0 para Windows x64](https://github.com/mglezz/dowlume/releases/tag/v0.3.0) el 20 de octubre de 2026. Puedes descargar el [instalador](https://github.com/mglezz/dowlume/releases/download/v0.3.0/Dowlume-0.3.0-win-x64-setup.exe) o el [ZIP portátil](https://github.com/mglezz/dowlume/releases/download/v0.3.0/Dowlume-0.3.0-win-x64.zip) y comprobar sus [sumas SHA-256](https://github.com/mglezz/dowlume/releases/download/v0.3.0/SHA256SUMS.txt). Estos paquetes incluyen .NET y no están firmados.

**Idiomas:** mantengo el inglés como idioma inicial y incluyo español en v0.2.0. En **Settings → Language**, selecciona **Español**; después encontrarás el selector en **Ajustes → Idioma**. Pausa primero la supervisión. El cambio es inmediato y se guarda para ese perfil. No cambia nombres de carpetas, archivos ni registros. **El instalador y ZIP portátil de 0.3.0 conservan el selector introducido en 0.2.0**. Conservo intactos los archivos originales de 0.1.0, que siguen en inglés. Consulta las [notas de 0.2.0](RELEASE_NOTES_0.2.0.md).

Desactivé el envío nativo a la Papelera en v0.1.0 tras comprobar que las pruebas de capacidad no preservaban los archivos de forma fiable. Mantengo disponibles Conservar, Posponer, Mover a y las operaciones de Deshacer compatibles. Documento los fallos y la propuesta de cuarentena en la [investigación](RECYCLE_CAPACITY_INVESTIGATION.md).

[Guía de uso](USER_GUIDE.md) · [Preparación del entorno](GETTING_STARTED.md) · [Hoja de ruta](ROADMAP.md) · [Contribuciones](CONTRIBUTING.md)

![Resumen de Dowlume con seis archivos de ejemplo](../images/overview.png)

*Conservo esta captura de la interfaz inglesa de v0.1.0 con datos ficticios. Es anterior al selector de idioma. [Procedencia de las imágenes](images/README.md).*

**Próximos hitos:** el código de 0.3.0 incorpora CI nativa, adaptadores y vistas previas restringidas para macOS 15 Apple Silicon y Ubuntu 24.04 x64. Mantengo bloqueadas las modificaciones de archivos en estas vistas previas. Planifico soporte aceptado macOS en 0.4.0, Linux en 0.5.0 y estabilización hacia 1.0. Windows x64 sigue siendo la plataforma soportada. Son objetivos con criterios de aceptación, no soporte ya disponible ni fechas prometidas; consulta la [hoja de ruta](ROADMAP.md) y el [alcance por plataforma](PLATFORM_SUPPORT.md).

He publicado las bases de 0.3.0 con archivos macOS/Linux claramente identificados como experimentales. Registro [aceptación y hashes](RELEASE_CANDIDATE_0.3.0.md) por separado de las declaraciones de soporte estable.

## Por qué lo desarrollo

Las carpetas de descargas acumulan documentos útiles, instaladores de un solo uso y archivos fáciles de olvidar. Creé Dowlume para organizar lo que termina de descargarse, volver a mostrar los archivos antiguos y permitir una decisión consciente. Me centro en que las operaciones puedan reconciliarse cuando se interrumpe una descarga, un movimiento o la aplicación.

| Vista previa | Organización | Revisión | Recuperación |
| --- | --- | --- | --- |
| Comprueba los destinos antes de confirmar. | Clasifica en seis categorías configurables. | Conserva, pospone o mueve desde `Expiring Files`. | Consulta el historial, deshaz acciones compatibles y revisa interrupciones. |

No elimino archivos automáticamente. La supervisión empieza en pausa en cada arranque y no importo las subcarpetas existentes. En Actividad incluyo búsqueda por nombre o ruta, filtros y paginación para consultar operaciones antiguas.

## Tecnologías y decisiones

Utilizo **C#, .NET 10, Avalonia con MVVM, SQLite y xUnit**. Separo la interfaz, las reglas, la supervisión y el acceso al sistema de archivos. Compruebo la estabilidad de las descargas, verifico contenido mediante SHA-256 y registro cada intención antes de mover. Si quedan copias ambiguas, las conservo para su inspección. Explico estas decisiones en [Arquitectura](ARCHITECTURE.md).

Windows x64 es mi plataforma de distribución. Ofrezco bases experimentales para [macOS Apple Silicon y Linux](PLATFORM_SUPPORT.md); compilar y abrir la aplicación no equivale a validar sus operaciones nativas.

## Ejecutar desde el código

Instala un SDK de .NET de la familia **10.0.4xx**. `global.json` fija `10.0.400` y permite el último parche de esa familia; CI usa `10.0.401`.

```powershell
dotnet restore Dowlume/Dowlume.slnx
dotnet run --project Dowlume/Dowlume.App -c Release
```

Empieza con una carpeta desechable y archivos de ejemplo. En [Preparación del entorno](GETTING_STARTED.md) detallo las pruebas; en [Publicación](RELEASE_CHECKLIST.md), la generación y aceptación de paquetes.

## Documentación en español

| Documento | Contenido |
| --- | --- |
| [Guía de uso](USER_GUIDE.md) | Instalación, idioma, organización, revisión, historial y recuperación |
| [Preparación del entorno](GETTING_STARTED.md) | SDK, compilación y pruebas |
| [Contribuciones](CONTRIBUTING.md) y [ramas](BRANCHES.md) | Convenciones, revisión y commits |
| [Modelo de dominio](requirements/DOMAIN_MODEL.md) | Conceptos, reglas y vistas UML compartidas |
| [Arquitectura](ARCHITECTURE.md) | Componentes, procesamiento y límites de seguridad |
| [Hoja de ruta](ROADMAP.md) y [plataformas](PLATFORM_SUPPORT.md) | Trabajo realizado y posibilidades futuras |
| [Validación](VALIDATION.md) y [seguridad](SECURITY_REVIEW.md) | Resultados, procedencia y limitaciones |
| [Notas de v0.3.0](RELEASE_NOTES_0.3.0.md) | Comportamiento de los binarios publicados |
| [Aceptación de 0.3.0](RELEASE_CANDIDATE_0.3.0.md) y [registro histórico de 0.1.0](RELEASE_CHECKLIST.md) | Identidad de los artefactos y controles completados |
| [Aceptación en Windows](WINDOWS_ACCEPTANCE.md) y [Hyper-V](HYPER_V_ACCEPTANCE.md) | Preparación y comprobaciones de escritorio |
| [Investigación de la papelera](RECYCLE_CAPACITY_INVESTIGATION.md) | Fallos preservados y cuarentena propuesta |
| [Capturas](images/README.md), [diagramas](requirements/diagrams/domain-model/README.md) y [archivo histórico](archive/native-recycle/README.md) | Reproducción y procedencia de los materiales compartidos |

Mantengo esta sección junto a la inglesa. Comparto las imágenes, diagramas, código archivado y evidencias originales para no duplicar ni alterar sus bytes. En las páginas históricas explico los resultados en español y enlazo el registro original con sus tablas y detalles completos.

## Licencia

Distribuyo el proyecto bajo [MIT](../../LICENSE). Conservo las licencias y atribuciones de las dependencias.
