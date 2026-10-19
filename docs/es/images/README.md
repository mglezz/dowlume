# Capturas de la documentación

[English](../../images/README.md) · [Índice en español](../README.md)

Conservo las capturas inglesas de v0.1.0 en `docs/images/` como material compartido. Son anteriores al selector de idioma y las identifico como tales. Utilizan `MainWindow` real de Avalonia con datos ficticios preparados y renderizado sin pantalla en Windows.

| Archivo | Contenido |
| --- | --- |
| `overview.png` | Seis archivos, recuentos de categoría y dos pendientes de revisión. |
| `classification.png` | Destinos propuestos para seis descargas antes de organizar. |
| `review.png` | Comprimido e instalador pendientes, con papelera desactivada y explicación. |
| `activity.png` | Búsqueda, filtros, páginas y rutas seleccionadas. |

No presento estas imágenes como prueba de operaciones nativas superadas.

## Regenerar

Desde la raíz, en Windows y con el SDK requerido:

```powershell
dotnet run --project scripts/DocumentationScreenshots -c Release
```

Inspecciono las imágenes de `artifacts/documentation-screenshots/` antes de copiar capturas aprobadas a la documentación. El generador también produce variantes compactas de Actividad y Revisión para revisar el tamaño mínimo. Los archivos de pruebas y paquetes permanecen ignorados.

El generador utiliza vistas, modelos y reglas reales con un perfil SQLite temporal aislado. Los nombres y `C:\Dowlume-Demo\Downloads` son ficticios. No crea ni supervisa esa carpeta, ni ejecuta organización, movimiento o papelera. Elimina solo su perfil temporal al finalizar. Registro fecha y revisión en [Validación](../VALIDATION.md).
