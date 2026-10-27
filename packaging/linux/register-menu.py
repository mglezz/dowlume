#!/usr/bin/env python3
"""Register only this extracted portable copy; does not enable login startup."""
import argparse
import os
from pathlib import Path
import tempfile


def quote_exec(path):
    value = str(path)
    if not path.is_absolute() or any(ord(c) < 32 or c in '=%' for c in value):
        raise ValueError("The application path cannot be represented safely in a desktop entry.")
    escaped = ''.join('\\\\\\\\' if c == '\\' else '\\\\' + c if c in '\"`$' else c for c in value)
    return '"' + escaped + '"'


def reject_links(path):
    for part in (path, *path.parents):
        if part.is_symlink():
            raise RuntimeError("Symbolic links are not supported for menu registration.")


def register(package, data_home, remove=False):
    target = data_home / 'applications/com.mglezz.dowlume.portable.desktop'
    reject_links(target)
    if target.exists() and (target.stat().st_size > 65536 or
                            'X-Dowlume-Portable=1\n' not in target.read_text()):
        raise RuntimeError("The existing menu entry is not managed by Dowlume.")
    if remove:
        target.unlink(missing_ok=True)
        return
    executable = package / 'Dowlume.App'
    if not executable.is_file():
        raise RuntimeError("Run this script from an extracted Dowlume package.")
    icon = str(package / 'dowlume.png')
    if any(ord(c) < 32 for c in icon):
        raise ValueError("Invalid icon path.")
    content = ('[Desktop Entry]\nType=Application\nName=Dowlume\nExec=' + quote_exec(executable) +
               '\nIcon=' + icon.replace('\\', '\\\\') +
               '\nTerminal=false\nCategories=Utility;FileTools;\nX-Dowlume-Portable=1\n')
    target.parent.mkdir(parents=True, exist_ok=True)
    temporary = None
    try:
        with tempfile.NamedTemporaryFile(mode='w', encoding='utf-8', dir=target.parent, delete=False) as stream:
            temporary = Path(stream.name)
            stream.write(content)
            stream.flush()
            os.fsync(stream.fileno())
        temporary.chmod(0o644)
        reject_links(target)
        os.replace(temporary, target)
    finally:
        if temporary is not None:
            temporary.unlink(missing_ok=True)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--remove', action='store_true')
    args = parser.parse_args()
    configured = Path(os.environ.get('XDG_DATA_HOME', ''))
    data = configured if configured.is_absolute() else Path.home() / '.local/share'
    register(Path(__file__).resolve().parent, data, args.remove)
