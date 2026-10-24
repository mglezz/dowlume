# Preparar una máquina virtual de aceptación

[English](../../development/hyper-v-acceptance.md) · [Índice en español](../README.md)

Utilizo Hyper-V como apoyo a la [aceptación de Windows](windows-acceptance.md). Preparar una máquina no completa ninguna prueba dentro del invitado. Mantengo una copia independiente y conservo intacta la máquina de evidencia y sus checkpoints.

## Crear y preparar el invitado

Desde una cuenta autorizada para administrar Hyper-V compruebo `Get-VM`. Elijo un directorio nuevo en NTFS con al menos 100 GiB libres:

```powershell
Get-VM
New-Item -ItemType Directory -Path E:\Dowlume.ContainmentAcceptance\Media
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\New-WindowsAcceptanceVm.ps1 `
    -Path E:\Dowlume.ContainmentAcceptance\Windows11Vm -Name 'Dowlume-Windows11-Containment'
```

El script rechaza nombres o directorios existentes. Prepara generación 2, Secure Boot para Windows, TPM virtual, dos CPU, 4–6 GiB de memoria dinámica y disco dinámico de 96 GiB. Usa Default Switch, habilita Guest Service Interface y deja desactivados arranque y checkpoints automáticos. No cambia la red del anfitrión ni la papelera.

Descargo una ISO x64 oficial desde [Microsoft](https://www.microsoft.com/software-download/windows11) y compruebo el SHA-256 publicado para esa edición e idioma. No utilizo espejos no oficiales. Mantengo el invitado apagado al adjuntar el medio y documento origen, nombre, hash y fecha en `ISO-VERIFICATION.json`.

Completo la instalación desde VMConnect sobre el disco vacío del invitado, sin redirigir discos personales. Respeto requisitos y licencia. Instalo actualizaciones y creo una cuenta estándar `DowlumeTester`; no publico su contraseña. No instalo SDK, Visual Studio ni .NET moderno compartido. El .NET Framework incluido en Windows es normal.

Con el invitado apagado, retiro la ISO, selecciono el disco del sistema como primer arranque y creo el checkpoint limpio:

```powershell
$vm = Get-VM -Name 'Dowlume-Windows11-Containment'
if ($vm.State -ne 'Off') { throw 'Shut down the guest before taking the clean baseline.' }
Get-VMDvdDrive -VM $vm | Set-VMDvdDrive -Path $null
Set-VMFirmware -VM $vm -FirstBootDevice (Get-VMHardDiskDrive -VM $vm)
Checkpoint-VM -VM $vm -SnapshotName 'Clean Windows - before Dowlume acceptance'
Start-VM -VM $vm
```

## Transferir y ejecutar

Preparo únicamente paquetes exactos, hashes, información de compilación y kit extraído. Incluyo el instalador anterior con su hash para comprobar una actualización real. No copio mi perfil ni archivos personales.

Utilizo `Copy-VMFile` desde el anfitrión hacia un directorio nuevo y único del invitado, sin montar discos del anfitrión ni abrir recursos de red. Si el servicio no está listo, espero al arranque y repito con otro destino.

Dentro del invitado uso la cuenta estándar y una consola sin elevar. Verifico todos los hashes, mantengo el escritorio desbloqueado y ejecuto el script de paquetes con `-RequireNoSharedDotNet` y, cuando corresponda, `-PreviousInstaller`. PowerShell Direct por sí solo no sustituye un escritorio interactivo para bandeja y automatización.

Conservo informes y muestras al fallar y completo también las pruebas humanas. Mantengo los [bloques completos de preparación y transferencia](../../development/hyper-v-acceptance.md) junto a su contexto original en inglés para no mantener variantes divergentes de comandos operativos.

## Registro histórico

El 16 de octubre preparé `Dowlume-Windows11-Acceptance`; la generación automática del enlace de Microsoft falló y descargué la ISO mediante navegador. `Windows11_Client_x64_en-gb_26300_9457.iso`, de 9.061.271.552 bytes, coincidió con el hash oficial `7E3F373BD3C2321B5D5125DFCF718DFBF1A0ABC50A267118669951890DF98A5D`.

Después se completó Windows 11 Pro 26300 y la aceptación de paquetes del candidato `cf42326`. Las pruebas nativas posteriores de capacidad fallaron. Por ello conservo esa máquina y sus tres checkpoints como evidencia, sin restaurarlos ni repetir pruebas.

El 18 de octubre utilicé una copia independiente del checkpoint limpio para aceptar los artefactos finales `94ba177`. La [ficha del candidato](../releases/acceptance/0.1.0.md) registra el resultado. Las menciones históricas a pruebas o publicación pendientes describen el momento original; v0.1.0 ya está publicada.
