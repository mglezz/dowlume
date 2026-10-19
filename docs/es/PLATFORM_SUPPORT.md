# Compatibilidad de plataformas

[English](../PLATFORM_SUPPORT.md) · [Índice en español](README.md)

Distribuyo y valido Dowlume para **Windows x64**. Mantengo macOS y Linux como trabajo futuro, sin paquetes ni aceptación nativa para esos sistemas. Avalonia y la separación por componentes facilitan futuras adaptaciones, pero compilar no demuestra seguridad al operar con archivos.

| Área | Implementación actual | Trabajo para otra plataforma |
| --- | --- | --- |
| Papelera | Adaptador que siempre rechaza; función desactivada. | Diseño y aceptación de almacenamiento recuperable. |
| Gestor de archivos | Selección en el Explorador de Windows. | Integración nativa con argumentos seguros. |
| Descargas | Carpeta conocida de Windows y alternativa bajo el directorio personal. | Descubrimiento de carpetas configuradas en cada sistema. |
| Archivos | Validación de rutas, identificadores y movimientos comprobados en Windows. | Mayúsculas/minúsculas, enlaces, montajes, bloqueos, colisiones y recuperación entre volúmenes. |
| Escritorio | Ventana, bandeja, bloqueo de perfil y cierre. | Disponibilidad y comportamiento nativo en cada escritorio. |
| Distribución | ZIP Windows x64 e instalador Inno Setup. | Paquetes, permisos, actualización, desinstalación y firma propios. |

## Orden de aceptación para una adaptación

Primero implementaría los adaptadores y revisaría las suposiciones de rutas y archivos. Después añadiría CI nativa y pruebas aisladas de dominio, persistencia, supervisión, recuperación e interfaz. Finalmente comprobaría escritorio real, interrupciones y paquetes por sistema y arquitectura. Solo actualizaría la tabla de soporte con evidencia disponible.

Completé los [controles de Windows v0.1.0](RELEASE_CHECKLIST.md). Cualquier binario posterior o adaptación necesita sus propias comprobaciones. Mantengo las posibilidades futuras en la [hoja de ruta](ROADMAP.md).
