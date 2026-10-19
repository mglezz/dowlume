# Contribuir a Dowlume

[English](../../CONTRIBUTING.md) · [Índice en español](README.md)

Mantengo el código, los comentarios, las ramas y los mensajes de commit en inglés. Escribo las decisiones e investigaciones del proyecto en primera persona singular y mantengo documentación e interfaz en inglés y español. Conservo atribuciones, licencias y evidencias originales; distingo los resultados registrados de las pruebas ejecutadas en cada revisión.

Prefiero cambios acotados, nombres del dominio, métodos cohesionados, registros inmutables, dependencias por constructor y errores explícitos. Mantengo las decisiones de negocio fuera de las vistas. Antes de modificar operaciones de archivos, reviso [Arquitectura](ARCHITECTURE.md).

## Idiomas

Mantengo las mismas claves y argumentos de formato en `Dowlume.App/Localization/Strings.resx` y `Strings.es.resx`. El inglés es el valor inicial y la alternativa para códigos no compatibles. Traduzco la presentación sin modificar rutas, nombres de carpeta, valores de enumeración ni registros históricos.

Cuando cambio el comportamiento, actualizo ambos idiomas de la documentación. Enlazo las imágenes, diagramas, scripts y evidencias históricas compartidos sin duplicarlos ni alterar sus bytes.

## Validación

Para código, ejecuto la compilación y las pruebas de [Preparación del entorno](GETTING_STARTED.md), además de:

```powershell
dotnet format Dowlume/Dowlume.slnx --verify-no-changes --no-restore
dotnet list Dowlume/Dowlume.slnx package --vulnerable --include-transitive
```

Las pruebas de seguridad sobre archivos usan ubicaciones desechables. No repito las antiguas pruebas de capacidad de la papelera ni modifico la máquina virtual que conserva sus evidencias. Una futura cuarentena requiere diseño y aceptación propios.

Para documentación compruebo enlaces locales, imágenes y `git diff --check`. Utilizo el [generador de capturas](images/README.md) con datos ficticios. Las capturas de pruebas pueden incluir rutas temporales personales; las inspecciono antes de compartirlas.

## Git

Creo una rama temporal desde `main`, hago commits Conventional Commits centrados en cada cambio y abro un PR. Integro tras revisar el diff y superar CI. No reescribo el historial compartido. En [Ramas](BRANCHES.md) documento los comandos.

Añado pruebas significativas cuando cambia la seguridad de las operaciones. No marco controles de publicación como completados sin la evidencia correspondiente.
