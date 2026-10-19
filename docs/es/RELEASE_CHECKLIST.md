# Validación y publicación

[English](../RELEASE_CHECKLIST.md) · [Índice en español](README.md)

Para la versión actual, consulta el [registro de aceptación y publicación de 0.2.0](RELEASE_CANDIDATE_0.2.0.md). Conservo debajo el registro histórico de 0.1.0.

Mantengo este registro para los artefactos Windows x64 de **v0.1.0**, publicados el 18 de octubre de 2026 desde `94ba177`. Los seis archivos públicos fueron descargados y verificados. Las pruebas anteriores no aprueban binarios modificados, incluido el selector de idioma añadido después.

## Generar paquetes reproducibles

Instalo un SDK **10.0.4xx** e Inno Setup 6 y ejecuto desde la raíz:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Release.ps1 -RequireInstaller
```

Con `-RequireInstaller`, la ausencia del compilador detiene el proceso antes de empaquetar. Para una instalación personalizada uso `-InnoCompiler "C:\ruta\ISCC.exe"`. Solo omito el requisito cuando quiero un candidato exclusivamente portátil.

Cada ejecución crea un directorio nuevo bajo `artifacts/releases/`; `-OutputDirectory <ruta-nueva>` permite elegirlo. Rechazo directorios existentes para evitar mezclar archivos antiguos. Incluyo publicación autónoma, ZIP, instalador, kit de aceptación, notas, información de compilación y hashes. Leo la versión del proyecto. Reservo `-SkipTests` para una revisión exacta ya validada.

Elimino símbolos de depuración únicamente de la nueva carpeta de publicación. `BUILD-INFO.json` identifica commit, destino, fecha, firma y estado del árbol. Para una publicación aceptada compilo desde `main` limpio; no distribuyo un árbol sucio como versión aprobada.

El instalador no requiere elevación, crea accesos y conserva datos al desinstalar. Rechaza actualización y desinstalación mientras la aplicación está abierta. El inicio con Windows es opcional. El ZIP incluye .NET y la guía sin conexión. CI de empaquetado comprueba lanzamiento, instalación y desinstalación, pero su equipo con SDK no equivale a un Windows limpio.

## Controles completados de v0.1.0

- [x] Integré los PR iniciales conservando el historial y superé CI de compilación, pruebas, formato, seguridad y paquetes.
- [x] Volví a comprobar dependencias, analizadores y secretos del commit limpio `94ba177`.
- [x] Reconstruí desde `main` limpio, conservé hashes y documenté los paquetes sin firma.
- [x] Comprobé el candidato previo en un Windows 11 limpio y repetí aceptación de los artefactos finales en un invitado independiente sin .NET compartido.
- [x] Registré primer uso humano, inicio de sesión, bandeja, asistentes, revisión y deshacer, además de revisión independiente.
- [x] Probé movimiento y deshacer entre discos físicos NVMe/SATA e interrupción de una copia de 512 MiB, conservando origen y copia parcial.
- [x] Desactivé la papelera en interfaz, API, ejecutor y adaptador tras los fallos de capacidad, conservando todos los resultados fallidos.
- [x] Conservé máquina virtual, checkpoints, exportaciones e investigación sin repetir las pruebas nativas.
- [x] Comprobé actualización y desinstalación con conservación de archivos ficticios y perfil.
- [x] Autoricé publicación, creé `v0.1.0` en el commit aceptado y verifiqué todas las descargas públicas.

El [registro del candidato](RELEASE_CANDIDATE_0.1.0.md) vincula pruebas y hashes. No marco hardware, máquina limpia, firma ni revisión independiente como completados basándome únicamente en pruebas unitarias.

## Versiones posteriores

Repito las comprobaciones afectadas para cualquier binario nuevo. Para la localización incluyo ambos idiomas, persistencia, diálogos, bandeja, teclado, tamaño mínimo y escalado de Windows. La compilación y las pruebas sin pantalla complementan estas comprobaciones; no las sustituyen.

Conservo la etiqueta y los adjuntos aceptados de v0.1.0. No los reemplazo por reconstrucciones. Recojo incidencias sin bases de datos privadas ni archivos personales.
