# Archivo histórico de papelera nativa

[English](../../../archive/native-recycle/README.md) · [Índice en español](../../README.md)

Conservo los bytes originales anteriores a la contención del 17 de octubre de 2026 como archivos `.txt`, fuera de proyectos y paquetes. No los renombro, compilo ni ejecuto para repetir los fallos. El manifiesto [SHA256SUMS.txt](../../../archive/native-recycle/SHA256SUMS.txt) identifica los originales.

Proceden del árbol limpio con base `2dcc06967adb7cf815d9378fc8845c2c8045bb7e`. Los atributos de Git desactivan conversión de finales de línea para conservar sus hashes.

- [WindowsRecycleBin.cs.txt](../../../archive/native-recycle/WindowsRecycleBin.cs.txt): adaptador COM original.
- [RecycleOnlyProgressSink.cs.txt](../../../archive/native-recycle/RecycleOnlyProgressSink.cs.txt): callbacks originales.
- [Test-WindowsRecycleAcceptance.ps1.txt](../../../archive/native-recycle/Test-WindowsRecycleAcceptance.ps1.txt): arnés de políticas y capacidad retirado.
- [RecycleProbe.cs.txt](../../../archive/native-recycle/RecycleProbe.cs.txt): worker y momentos de registro.

La [investigación](../recycle-capacity-investigation.md) relaciona candidatos, hashes, ejecuciones, checkpoints y hallazgos. Este archivo de fuentes no reemplaza binarios, muestras o informes originales. Los puntos de entrada actuales y el adaptador rechazan la ejecución.
