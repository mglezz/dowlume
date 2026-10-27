#!/usr/bin/env python3
"""Prepare Linux x64 .deb and portable candidates; never publish or imply acceptance."""
import argparse
import hashlib
import json
from pathlib import Path
import platform
import shutil
import subprocess
import tarfile
import tempfile
import xml.etree.ElementTree as ET
from datetime import datetime, timezone

from package_docs import copy_user_documentation

ROOT = Path(__file__).resolve().parent.parent
DEPENDENCIES = ('libc6 (>= 2.39), libgcc-s1, libstdc++6, libssl3t64, libicu74, zlib1g, '
                'libfontconfig1, libx11-6, libice6, libsm6, libgl1, libegl1, '
                'libxrandr2, libxi6, libxcursor1, libxrender1, libxext6, libxfixes3, '
                'libdbus-1-3, xdg-utils')


def run(*args, **kwargs):
    return subprocess.run(args, check=True, cwd=ROOT, **kwargs)


def build(output):
    if platform.system() != 'Linux' or platform.machine() != 'x86_64':
        raise RuntimeError('Build on Ubuntu 24.04 LTS x64.')
    os_release = platform.freedesktop_os_release()
    if os_release.get('ID') != 'ubuntu' or os_release.get('VERSION_ID') != '24.04':
        raise RuntimeError('The official baseline must be built on Ubuntu 24.04 LTS.')
    if output.exists():
        raise RuntimeError('Choose a new output directory; existing artifacts are never replaced.')
    version = ET.parse(ROOT / 'src/Dowlume.App/Dowlume.App.csproj').findtext('./PropertyGroup/Version')
    revision = run('git', 'rev-parse', 'HEAD', capture_output=True, text=True).stdout.strip()
    dirty = bool(run('git', 'status', '--porcelain', capture_output=True, text=True).stdout.strip())
    output.mkdir(parents=True)
    with tempfile.TemporaryDirectory(prefix='dowlume-linux-') as temporary:
        staging = Path(temporary)
        package = staging / f'Dowlume-{version}-linux-x64'
        run('dotnet', 'publish', 'src/Dowlume.App/Dowlume.App.csproj', '-c', 'Release', '-r', 'linux-x64',
            '--self-contained', 'true', '-o', str(package), '-p:PublishTrimmed=false',
            '-p:DebugType=None', '-p:DebugSymbols=false')
        for symbols in package.rglob('*.pdb'):
            symbols.unlink()
        (package / 'Dowlume.App').chmod(0o755)
        if not (package / 'es/Dowlume.App.resources.dll').is_file():
            raise RuntimeError('Spanish resources are missing.')
        shutil.copy2(ROOT / 'src/Dowlume.App/Assets/dowlume.png', package / 'dowlume.png')
        shutil.copy2(ROOT / 'packaging/linux/register-menu.py', package / 'register-menu.py')
        shutil.copy2(ROOT / 'packaging/LINUX.txt', package / 'START-HERE.txt')
        shutil.copy2(ROOT / 'packaging/es/LINUX.txt', package / 'START-HERE.es.txt')
        shutil.copy2(ROOT / 'LICENSE', package / 'LICENSE')
        copy_user_documentation(ROOT, package, revision)
        info = {'Version': version, 'SourceCommit': revision, 'DirtyWorkingTree': dirty,
                'RuntimeIdentifier': 'linux-x64', 'SelfContained': True,
                'BuiltAtUtc': datetime.now(timezone.utc).isoformat(), 'OS': platform.platform(),
                'Target': 'Ubuntu 24.04 LTS x64, GNOME, local ext4, same mount',
                'NativeAcceptance': 'Pending', 'StablePublicationApproved': False,
                'OtherDistributions': 'Portable glibc x64 candidate; Fedora and Arch unverified'}
        info_text = json.dumps(info, indent=2) + '\n'
        (package / 'BUILD-INFO.json').write_text(info_text, encoding='utf-8')
        archive = output / (package.name + '.tar.gz')
        with tarfile.open(archive, 'w:gz') as stream:
            stream.add(package, arcname=package.name)
        deb_root = staging / 'deb'
        app = deb_root / 'opt/dowlume'
        app.parent.mkdir(parents=True)
        shutil.copytree(package, app)
        for source, relative in [
            (ROOT / 'packaging/linux/com.mglezz.dowlume.desktop', 'usr/share/applications/com.mglezz.dowlume.desktop'),
            (package / 'dowlume.png', 'usr/share/icons/hicolor/64x64/apps/com.mglezz.dowlume.png'),
            (ROOT / 'LICENSE', 'usr/share/doc/dowlume/copyright')]:
            target = deb_root / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, target)
        control = deb_root / 'DEBIAN'
        control.mkdir()
        for name in ('preinst', 'prerm'):
            shutil.copy2(ROOT / 'packaging/linux' / name, control / name)
            (control / name).chmod(0o755)
        size = sum(path.stat().st_size for path in deb_root.rglob('*') if path.is_file()) // 1024
        (control / 'control').write_text(
            f'Package: dowlume\nVersion: {version}\nArchitecture: amd64\nSection: utils\nPriority: optional\n'
            f'Maintainer: Dowlume contributors <noreply@github.com>\nInstalled-Size: {size}\n'
            f'Depends: {DEPENDENCIES}\nHomepage: https://github.com/mglezz/dowlume\n'
            'Description: Local download organizer with review and recovery\n'
            ' Self-contained desktop application. Linux native acceptance pending.\n', encoding='utf-8')
        deb = output / f'dowlume_{version}_amd64.deb'
        run('desktop-file-validate', str(deb_root / 'usr/share/applications/com.mglezz.dowlume.desktop'))
        run('dpkg-deb', '--root-owner-group', '--build', str(deb_root), str(deb))
        (output / 'BUILD-INFO.linux-x64.json').write_text(info_text, encoding='utf-8')
        artifacts = [archive, deb, output / 'BUILD-INFO.linux-x64.json']
        (output / 'SHA256SUMS.linux-x64.txt').write_text(''.join(
            f'{hashlib.sha256(path.read_bytes()).hexdigest()}  {path.name}\n' for path in artifacts), encoding='ascii')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    build(args.output.resolve())
