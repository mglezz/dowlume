# Aceptación de Dowlume 0.4.0

[English](../RELEASE_CANDIDATE_0.4.0.md) · [Español](#)

Implemento 0.4.0 para macOS Apple Silicon y conservo Windows. Mantengo 0.3.0 como estable hasta aceptar los binarios nuevos. La firma está aplazada y no bloquea el hito funcional. Distingo implementación, automatización y comprobaciones pendientes del paquete exacto.

## Alcance

Dirijo esta versión a macOS 15+ ARM64 y carpetas APFS locales ordinarias del mismo volumen. Incluyo vista previa, organización, supervisión, revisión, Conservar, Posponer, Mover, Deshacer elegible, recuperación, historial, ajustes, inglés/español, Finder, menús nativos e inicio opcional. Rechazo operaciones entre volúmenes, nube, red y enlaces; Linux sigue restringido y la papelera desactivada. Consulta [notas](RELEASE_NOTES_0.4.0.md) y [garantías](FILE_OPERATION_CONTRACT.md).

## Evidencia automatizada

Ejecuto batería Release completa, formato, analizadores/dependencias, matriz nativa en dos imágenes APFS nuevas y flujo del paquete extraído. Los informes identifican revisión, estado del árbol, sistema y hashes. No uso paquetes de desarrollo con cambios sin confirmar como candidatos.

Las pruebas locales en macOS 26.6.2 ARM64, SDK 10.0.401, superaron el flujo admitido y ambas políticas APFS, incluido volumen desechable de 256 MiB lleno. Verifico conservación ante carreras/colisiones, atributos extendidos/permisos/fechas, rechazo de enlaces duros, disponibilidad nativa, permisos denegados, supervisión/cierre, recuperación con base reabierta y rechazo entre volúmenes antes del diario. CI aporta evidencia separada de macOS 15, Windows y Linux. Conservo intactas versiones históricas y pruebas retiradas de papelera.

El script macOS firma ad hoc, extrae el ZIP, vuelve a verificarlo y prueba su ejecutable real con un perfil desechable nuevo obligatorio. El informe cubre pestañas en ambos idiomas, persistencia, ciclo nativo y Deshacer, base reabierta y cierre del supervisor. No afirma instalación manual, inicio real de sesión ni máquina limpia.

## Lista del paquete exacto

Antes de publicar estable registro el hash del candidato y estos resultados sobre ese mismo archivo:

- macOS: extraer e instalar `.app` completa, revisar avisos Gatekeeper/permisos con la firma ad hoc real y verificar acceso ordinario/denegado con muestras desechables.
- macOS: actualizar un perfil cerrado de vista previa 0.3.0 conservando ajustes, idioma e historial; verificar reinicio, selector/Finder, Comando-coma, Ocultar/reabrir desde Dock, rechazo de segunda instancia, cancelar confirmaciones y Salir supervisando.
- macOS: activar inicio automático desde la app instalada, cerrar/iniciar sesión, confirmar apertura pausada, desactivarlo y verificar retirada del registro propio. Desinstalar conserva perfil y archivos.
- Windows: repetir actualización desde 0.3.0, idiomas tras reiniciar, organización/revisión/Deshacer, bandeja y cierre con el paquete nuevo. Separo automatización alojada y confirmación manual.

No traslado la confirmación de 0.3.0 a los binarios 0.4.0. Publico un candidato identificado mientras faltan estas comprobaciones y promociono únicamente los artefactos aceptados, sin recompilarlos.
