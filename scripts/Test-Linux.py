#!/usr/bin/env python3
"""Run native tests on a newly created ext4 loop image. Requires sudo; never uses a real disk."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import subprocess
import tarfile
import tempfile
import uuid

ROOT = Path(__file__).resolve().parent.parent


def run(*args, **kwargs):
    return subprocess.run(args, cwd=ROOT, check=True, **kwargs)


def verify(output, archive=None):
    if platform.system() != 'Linux' or platform.machine() != 'x86_64' or os.geteuid() == 0:
        raise RuntimeError('Run as an ordinary Linux x64 user with sudo available, not as root.')
    if output.exists():
        raise RuntimeError('Choose a new evidence directory.')
    output.mkdir(parents=True)
    with tempfile.TemporaryDirectory(prefix='dowlume-ext4-') as temporary:
        root = Path(temporary)
        disk = root / 'disposable.img'
        mount = root / 'mount'
        mount.mkdir()
        with disk.open('xb') as stream:
            stream.truncate(512 * 1024 * 1024)
        volume_uuid = str(uuid.uuid4())
        run('mkfs.ext4', '-q', '-F', '-U', volume_uuid, str(disk))
        device = None
        mounted = False
        created_alias = False
        alias = Path('/dev/disk/by-uuid') / volume_uuid
        try:
            device = run('sudo', 'losetup', '--find', '--show', str(disk), capture_output=True, text=True).stdout.strip()
            run('sudo', 'mount', '-t', 'ext4', '-o', 'nosuid,nodev', device, str(mount))
            mounted = True
            run('sudo', 'chown', f'{os.getuid()}:{os.getgid()}', str(mount))
            # Minimal CI images may not run udev. Bind only our freshly generated UUID
            # to the loop device created above; never replace an existing device alias.
            run('sudo', 'mkdir', '-p', str(alias.parent))
            if not alias.exists() and not alias.is_symlink():
                run('sudo', 'ln', '-s', device, str(alias))
                created_alias = True
            if alias.resolve() != Path(device).resolve():
                raise RuntimeError('Unexpected filesystem UUID mapping.')
            test_tmp = mount / 'tmp'
            test_tmp.mkdir()
            env = dict(os.environ, TMPDIR=str(test_tmp), TMP=str(test_tmp), TEMP=str(test_tmp))
            run('dotnet', 'test', 'Dowlume.slnx', '-c', 'Release', '--no-build',
                '--logger', 'trx', '--results-directory', str(output), env=env)
            workflow = None
            digest = None
            if archive:
                digest = hashlib.sha256(archive.read_bytes()).hexdigest()
                extracted = root / 'extracted'
                extracted.mkdir()
                with tarfile.open(archive) as stream:
                    stream.extractall(extracted, filter='data')
                executables = list(extracted.glob('*/Dowlume.App'))
                if len(executables) != 1:
                    raise RuntimeError('Expected exactly one packaged executable.')
                profile = mount / 'smoke-profile'
                run('xvfb-run', '-a', str(executables[0]), '--smoke-test', '--data-directory', str(profile),
                    env=env, timeout=120)
                workflow = json.loads((profile / 'LINUX-WORKFLOW.json').read_text())
                if workflow.get('Passed') is not True:
                    raise RuntimeError('Packaged workflow did not pass.')
            report = {'Passed': True, 'OS': platform.platform(), 'Filesystem': 'ext4',
                      'FilesystemUuid': volume_uuid, 'ArchiveSha256': digest, 'NativeWorkflow': workflow,
                      'ManualGnomeX11': False, 'ManualGnomeWayland': False,
                      'InstallUpgradeUninstall': False, 'ActualLogin': False, 'PowerLoss': False}
            (output / 'LINUX-ACCEPTANCE.json').write_text(json.dumps(report, indent=2) + '\n')
        finally:
            if mounted:
                run('sudo', 'umount', str(mount))
            if created_alias:
                run('sudo', 'rm', '--', str(alias))
            if device:
                run('sudo', 'losetup', '--detach', device)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', required=True, type=Path)
    parser.add_argument('--archive', type=Path)
    args = parser.parse_args()
    verify(args.output.resolve(), args.archive.resolve() if args.archive else None)
