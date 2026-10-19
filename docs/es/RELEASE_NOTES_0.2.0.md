# Dowlume 0.2.0 — Inglés y español

[English](https://github.com/mglezz/dowlume/blob/main/docs/RELEASE_NOTES_0.2.0.md) · [Español](#)

En 0.2.0 añado un selector de idioma sin sustituir el inglés. Pausa la supervisión, abre **Settings > Language** y elige **English** o **Español**. Guardo la selección inmediatamente y la restauro al reiniciar. Los perfiles existentes empiezan en inglés.

Traduzco la interfaz, las confirmaciones, el menú de la bandeja y las descripciones de actividad. Cambiar de idioma no renombra carpetas de categorías, mueve archivos ni reescribe registros históricos. Windows proporciona el selector nativo de carpetas, cuyo idioma depende del sistema operativo. Conservo los mensajes históricos no reconocidos tal como se registraron.

Incluyo guías rápidas sin conexión, notas de versión y documentación del repositorio en inglés y español. También compruebo la actualización desde el instalador publicado de 0.1.0, el cambio de idioma y su persistencia en la aplicación empaquetada, y la presencia de recursos españoles en ambas distribuciones.

## Instalación o actualización

- `Dowlume-0.2.0-win-x64-setup.exe`: instalador de Windows x64 por usuario. Cierra Dowlume antes de actualizar desde 0.1.0; conservo el perfil y los archivos existentes.
- `Dowlume-0.2.0-win-x64.zip`: extrae el archivo completo en una carpeta nueva y ejecuta `Dowlume.App.exe`. Conserva juntos todos los archivos, incluida la carpeta `es`.
- `Dowlume-0.2.0-windows-acceptance.zip`: comprobaciones automáticas del paquete y procedimiento de aceptación manual en Windows.
- `BUILD-INFO.json` y `SHA256SUMS.txt`: revisión del código, procedencia de la compilación y sumas de comprobación.

Incluyo .NET en ambas distribuciones. Estos paquetes no están firmados; descárgalos desde la página de releases de este repositorio y compara sus valores SHA-256. No incluyo paquetes para macOS ni Linux.

## Limitaciones

Mantengo desactivada la papelera nativa tras los fallos anteriores de capacidad en Windows. Conservar, Posponer, Mover a y Deshacer en operaciones compatibles siguen disponibles; la cuarentena recuperable queda para más adelante. Siempre inicio la supervisión en pausa. No admito carpetas de red, enlaces, junctions ni marcadores de nube.

Empieza con copias de archivos desechables y conserva respaldos. La desinstalación mantiene tus archivos y el perfil local. Consulta la [guía de uso](https://github.com/mglezz/dowlume/blob/main/docs/es/USER_GUIDE.md) y el [procedimiento de aceptación en Windows](https://github.com/mglezz/dowlume/blob/main/docs/es/WINDOWS_ACCEPTANCE.md). Registro la aceptación de cada compilación concreta por separado; la aceptación de 0.1.0 no certifica 0.2.0.
