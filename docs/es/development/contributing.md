# Contribuir a Dowlume

[English](../../../CONTRIBUTING.md) · [Índice en español](../README.md)

El código, los comentarios, las ramas, los commits, los pull requests y los títulos y textos de las publicaciones se escriben en inglés. La documentación principal usa inglés y enlaza las traducciones bajo `docs/es/`. La referencia técnica describe el sistema directamente, sin narración del autor. Conservo atribuciones, licencias y evidencias originales; distingo los resultados registrados de las pruebas ejecutadas en cada revisión.

Prefiero cambios acotados, nombres del dominio, métodos cohesionados, registros inmutables, dependencias por constructor y errores explícitos. Mantengo las decisiones de negocio fuera de las vistas. Antes de modificar operaciones de archivos, reviso [Arquitectura](../architecture/overview.md).

## Idiomas

Mantengo las mismas claves y argumentos de formato en `src/Dowlume.App/Localization/Strings.resx` y `Strings.es.resx`. El inglés es el valor inicial y la alternativa para códigos no compatibles. Traduzco la presentación sin modificar rutas, nombres de carpeta, valores de enumeración ni registros históricos.

Cuando cambio el comportamiento, actualizo ambos idiomas de la documentación. Enlazo las imágenes, diagramas, scripts y evidencias históricas compartidos sin duplicarlos ni alterar sus bytes.

## Validación

Para código, ejecuto la compilación y las pruebas de [Preparación del entorno](getting-started.md), además de:

```powershell
python3 scripts/check_repository.py
python3 -m unittest discover -s tests/packaging
dotnet format Dowlume.slnx --verify-no-changes --no-restore
dotnet list Dowlume.slnx package --vulnerable --include-transitive
```

Las pruebas de seguridad sobre archivos usan ubicaciones desechables. No repito las antiguas pruebas de capacidad de la papelera ni modifico la máquina virtual que conserva sus evidencias. Una futura cuarentena requiere diseño y aceptación propios.

Para documentación compruebo enlaces locales, imágenes y `git diff --check`. Utilizo el [generador de capturas](../images/README.md) con datos ficticios. Las capturas de pruebas pueden incluir rutas temporales personales; las inspecciono antes de compartirlas.

## Git

Creo una rama temporal desde `main`, hago commits Conventional Commits centrados en cada cambio y abro un PR. Integro tras revisar el diff y superar CI. No reescribo el historial compartido. En [Ramas](branches.md) documento los comandos.

Añado pruebas significativas cuando cambia la seguridad de las operaciones. No marco controles de publicación como completados sin la evidencia correspondiente.
