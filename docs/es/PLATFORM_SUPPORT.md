# Compatibilidad de plataformas

[English](../PLATFORM_SUPPORT.md) · [Índice en español](README.md)

Soporto Windows x64 en la versión 0.2.0 publicada. Planifico bases comunes en 0.3.0, soporte macOS en 0.4.0 y Linux en 0.5.0, seguido de estabilización hacia 1.0. Son resultados previstos; no he publicado ni aceptado paquetes macOS/Linux. La [hoja de ruta](ROADMAP.md) define los criterios de cada entrega.

## Matriz y alcance

| Destino | Estado actual | Alcance de aceptación previsto |
| --- | --- | --- |
| Windows x64 | Publicado y aceptado en [0.2.0](RELEASE_CANDIDATE_0.2.0.md) | Conservar funciones, compatibilidad de perfiles y pruebas nativas de seguridad de archivos ante cada cambio compartido. |
| macOS Apple Silicon | Planificado; sin declaración de soporte | Destino principal de 0.4.0. En 0.3.0 concreto versiones de macOS y sistemas de archivos según las dependencias fijadas y el acceso a pruebas nativas. |
| macOS Intel | Fuera de alcance | No planifico implementación, CI ni paquetes x64. Una entrega posterior podría reconsiderarlo con aceptación propia. |
| Linux x64 | Planificado; sin declaración de soporte | Alcance inicial de 0.5.0: una versión concreta de Ubuntu LTS y GNOME. En 0.3.0 fijo versión, formato de paquete, sistema de archivos, bibliotecas y cobertura de backend/sesiones X11/Wayland. |
| Otras distribuciones, escritorios y ARM64 de Linux | Fuera del alcance estable inicial | Ampliar solo con capacidad de mantenimiento, paquetes nativos y aceptación registrada. |

Distingo cuatro niveles de evidencia: compilación, pruebas compartidas automatizadas, pruebas experimentales de escritorio/paquetes y versión estable soportada. CI en verde u otro identificador de runtime no demuestran el último nivel. Registro sistema/compilación exactos, arquitectura, sistema de archivos y comportamiento de mayúsculas, escritorio/backend, hashes y resultados para cada combinación soportada.

La [matriz de Avalonia](https://docs.avaloniaui.net/docs/supported-platforms) varía según versión y arquitectura, y sus backends Linux tienen requisitos distintos. La uso como referencia, no como prueba de que las operaciones de archivos de Dowlume funcionan allí. Compruebo la matriz correspondiente a las versiones fijadas en el repositorio antes de establecer requisitos mínimos.

## Hallazgos al revisar el código de 0.2.0

| Área | Implementación actual | Base necesaria |
| --- | --- | --- |
| Composición y capacidades | `App.axaml.cs` construye la integración de inicio de Windows; propiedades y mensajes de interfaz mencionan Windows. | Seleccionar adaptadores de forma centralizada y exponer capacidades reales. Evitar comprobaciones del sistema repartidas por modelos de vista o dominio. |
| Gestor de archivos | [DesktopDialogs](../../Dowlume/Dowlume.App/Services/DesktopDialogs.cs) rechaza sistemas distintos de Windows e invoca Explorer en Windows. | Mostrar/abrir de forma nativa con argumentos seguros y resultados claros si no está disponible o se deniega permiso. |
| Descargas y perfiles | [DownloadsFolder](../../Dowlume/Dowlume.Infrastructure/DownloadsFolder.cs) usa la carpeta conocida de Windows y una alternativa simple bajo el directorio personal. | Descubrimiento nativo/configurado, ubicación de perfiles y permisos explícitos; no suponer una carpeta literal `~/Downloads` para todos los usuarios. |
| Identidad de rutas | [PathSafety](../../Dowlume/Dowlume.Infrastructure/PathSafety.cs) decide la comparación de mayúsculas solo por `OperatingSystem.IsWindows()`. | Revisar comportamiento por volumen, nombres Unicode, directorios protegidos, enlaces simbólicos y duros, montajes e identidad ante colisiones antes de activar acciones de producción. |
| Modificación de archivos | [SafeFileMover](../../Dowlume/Dowlume.Infrastructure/SafeFileMover.cs) usa identificadores nativos en Windows y `File.Move` como alternativa; [DirectoryLease](../../Dowlume/Dowlume.Infrastructure/DirectoryLease.cs) solo obtiene identificadores nativos de directorios en Windows. | Definir y demostrar garantías equivalentes de no sobrescritura, identidad/carreras, metadatos y copia interrumpida por implementación. Un flujo administrado abierto no prueba protección equivalente en todos los sistemas. |
| Supervisión y disponibilidad | Existen eventos orientativos, reconciliación y comprobaciones de disponibilidad. | Pruebas nativas de eventos perdidos/duplicados, escritura activa, suspensión/reanudación, volúmenes ausentes y reinicio; límites explícitos por plataforma. |
| Ciclo de vida | Ventana, bandeja, bloqueo de perfil, cierre e inicio de sesión tienen evidencia Windows. | Menú/dock/bandeja nativos, teclado, restaurar/salir, inicio opcional y alternativa utilizable sin bandeja. Validar permisos sin exigir ejecución elevada. |
| Informes de pruebas | Varias pruebas específicas de Windows retornan anticipadamente en otros sistemas. | Separar cobertura nativa e indicar casos no aplicables para que un total correcto no implique comprobaciones nativas no ejecutadas. |
| Persistencia | Perfiles e historial contienen rutas locales. | Conservar perfiles 0.2.0 en su sistema original, validar migraciones y respaldos, y diseñar por separado la importación entre sistemas. |
| Distribución | [Build-Release.ps1](../../scripts/Build-Release.ps1) genera ZIP e instalador Windows x64. | Estructuras nativas, dependencias, firma, procedencia y hashes, aceptación de instalación, actualización y desinstalación. |

Son hallazgos y trabajo previsto, no correcciones completadas. Mantengo Windows soportado mientras los resuelvo mediante PRs pequeños. Uso muestras desechables en toda investigación nativa y conservo intactos evidencias y checkpoints históricos.

## Distribución y dependencias

Para macOS planteo un paquete de aplicación apropiado y distribución pública firmada/notarizada, con Finder y permisos documentados. Necesito acceso a las credenciales de firma correspondientes y entornos de aceptación nativa antes de cerrar esa entrega. Utilizo la [guía de notarización de Apple](https://developer.apple.com/documentation/security/notarizing-macos-software-before-distribution) como referencia. No equiparo compilación cruzada o ejecución local sin firma con un instalador público aceptado.

En Linux empiezo con un formato principal para la distribución declarada y documento las dependencias nativas aunque incluya .NET. Elijo el formato en 0.3.0 tras comprobar permisos, actualizaciones e integración; no prometo AppImage, Flatpak, Snap y paquetes nativos a la vez. X11, XWayland y Wayland nativo son combinaciones distintas de aceptación, y no doy por disponible la bandeja en todos los escritorios.

## Recuperación y aceptación

Mantengo desactivada la papelera nativa. No hago depender las primeras adaptaciones de implementar la papelera nativa ni de repetir las pruebas de capacidad retiradas de Windows. La cuarentena recuperable requiere diseño y aceptación propios; 1.0 debe incluir una implementación aceptada o documentar su exclusión.

Antes de declarar soporte, verifico las funciones documentadas en ambos idiomas y sobre los paquetes exactos: vista previa/cancelación, organización y supervisión, Conservar/Posponer/Mover, Deshacer compatible, historial, recuperación de interrupciones, reinicio, diálogos, mostrar en gestor de archivos, inicio y ciclo de vida. Cubro permisos, disco lleno, sustitución de origen, colisiones, mayúsculas/Unicode, metadatos, montajes e interrupciones entre volúmenes con muestras desechables. Red, enlaces y marcadores de nube siguen fuera de soporte salvo diseño y aceptación separados.

Registro resultados automáticos y manuales por separado, repito las regresiones de Windows afectadas y publico los archivos exactos aceptados. Solo declaro soporte después de superar las pruebas, no al planificar el hito.
