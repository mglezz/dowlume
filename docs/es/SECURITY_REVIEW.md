# Revisión de seguridad de Windows

[English](../SECURITY_REVIEW.md) · [Índice en español](README.md)

Documento las protecciones, comprobaciones y límites de mi implementación. Desactivé la papelera nativa en la interfaz, API, ejecutor y adaptador; no distribuyo su implementación COM. Los fallos de capacidad permanecen registrados y el contenido ausente no se ha recuperado.

## Comprobaciones

Desde la raíz ejecuto:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Security.ps1
```

El script crea una carpeta nueva de informes ignorada por Git y falla ante resultados incompletos de dependencias, vulnerabilidades detectadas, avisos de analizadores o secretos. Descarga Gitleaks 8.30.1 de su publicación oficial y verifica un SHA-256 fijado. Los informes ocultan los valores de secretos; CI adjunta informes, no perfiles ni copias de fuentes privadas.

El candidato limpio `94ba177` superó comprobaciones locales y alojadas el 18 de octubre: dependencias directas/transitivas, analizadores y búsqueda de secretos en fuentes y 35 commits locales. Estos son resultados fechados, no garantías permanentes. Repito las comprobaciones antes de publicar porque cambian los avisos y reglas.

## Límites revisados

- Utilizo consultas SQL elegidas de enumeraciones y literales fijos; parámetros para datos y filtros.
- Invoco el Explorador por ruta absoluta y argumentos estructurados. Las DLL nativas se buscan en System32.
- Registro inicio automático por usuario con ruta absoluta entre comillas y desactivado inicialmente.
- Verifico identificadores de origen, destinos libres y antecesores protegidos contra renombrados.
- Serializo decisiones y compruebo colisiones, fuentes modificadas e interrupciones.
- Rechazo la papelera incluso si se ejecuta directamente un comando deshabilitado; no creo registros ni modifico archivos.

## Interpretación de la investigación

Los dos inventarios finales carecían de origen sobredimensionado y de contenido recuperable visible. Los cuatro metadatos restantes correspondían a muestras restauradas; no contenían los archivos perdidos. La eliminación nativa y la expulsión por capacidad siguen siendo las explicaciones principales, sin afirmar borrado físico irreversible.

Accedí en modo de solo lectura a una copia verificada del checkpoint mediante su clave clara existente. Eso permitió inspeccionar el sistema de archivos, pero no recuperó las muestras ausentes. Conservo la [investigación](RECYCLE_CAPACITY_INVESTIGATION.md) y sus límites.

## Aceptación y limitaciones

Registré aceptación automatizada, confirmación humana, revisión independiente y publicación de v0.1.0. Los binarios futuros necesitan nueva aceptación. No repito las pruebas nativas de capacidad ni modifico la máquina de evidencia.

Las comprobaciones no son una barrera frente a procesos hostiles con los mismos privilegios. Los hashes cubren el flujo principal. Interrumpir un proceso no simula pérdida de alimentación ni almacenamiento dañado. Las pruebas y escaneos superados no demuestran ausencia de todo defecto.
