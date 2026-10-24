# Diagramas del modelo de dominio

[English](../../../../requirements/diagrams/domain-model/README.md) · [Modelo en español](../../domain-model.md)

Mantengo una única colección de diez vistas UML en inglés, compartida por ambas documentaciones. Explico su vocabulario y reglas en el modelo español. Las fuentes `.puml` y `style.iuml` están en `docs/requirements/diagrams/domain-model/src/`; los SVG derivados, en `svg/`. Edito las fuentes y regenero, sin modificar SVG a mano.

## Reproducir

La cadena validada del registro original usa PlantUML **1.2026.8**, Eclipse Temurin Java **21.0.12.1** y Windows x64. Los diagramas correspondientes utilizan Smetana embebido, sin Graphviz separado. Las fuentes piden Arial y fondo blanco; otros entornos pueden cambiar el diseño.

Conservo el JAR fuera del contenido versionado. El SHA-256 registrado es:

```text
5e1ecfa8ecd32c90b03bbf3b1eb6f020943f98ab0fcf4032be31a0002ee2c462
```

Desde la raíz:

```powershell
Get-FileHash artifacts/tools/domain-model/plantuml.jar -Algorithm SHA256
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Render-DomainModel.ps1 -PlantUmlJar artifacts/tools/domain-model/plantuml.jar -PreviewPng
```

Si Java no está en `PATH`, uso `-JavaPath "C:\ruta\java.exe"`. El script comprueba sintaxis, errores y XML SVG sin scripts ni HTML. No envía diagramas a un servicio externo. Las vistas PNG quedan ignoradas bajo `artifacts/domain-model-preview/`.

## Revisar

Inspecciono cada imagen a tamaño completo y al ancho de lectura: etiquetas, flechas, multiplicidades y recortes. Contrasto escenarios y estados con las reglas, compruebo enlaces y `git diff --check`, y guardo fuentes e imágenes correspondientes juntas. La sintaxis válida no demuestra corrección conceptual.

El proceso no ejecuta Dowlume ni modifica sus datos. Registro las comprobaciones en [Validación](../../../archive/validation.md) y mantengo [las referencias del renderizador](../../../../requirements/diagrams/domain-model/README.md) junto a las fuentes originales.
