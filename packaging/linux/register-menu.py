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


def read_owned(target):
    reject_links(target)
    if not target.exists():
        return None
    if not target.is_file() or target.stat().st_size > 65536:
        raise RuntimeError("The existing menu entry is not managed by Dowlume.")
    values = {}
    group = False
    for line in target.read_text(encoding='utf-8').splitlines():
        if not line or line.startswith('#'):
            continue
        if line == '[Desktop Entry]' and not group:
            group = True
            continue
        key, separator, value = line.partition('=')
        if not group or not separator or not key or key in values:
            raise RuntimeError("The existing menu entry is not managed by Dowlume.")
        values[key] = value
    expected = {'Type': 'Application', 'Name': 'Dowlume', 'Terminal': 'false',
                'Categories': 'Utility;FileTools;', 'X-Dowlume-Portable': '1'}
    if (set(values) != set(expected) | {'Exec', 'Icon'} or
            any(values.get(key) != value for key, value in expected.items())):
        raise RuntimeError("The existing menu entry is not managed by Dowlume.")
    return values


def register(package, data_home, remove=False):
    target = data_home / 'applications/com.mglezz.dowlume.portable.desktop'
    original = read_owned(target)
    executable = package / 'Dowlume.App'
    command = quote_exec(executable)
    if remove:
        if original is not None and original['Exec'] != command:
            raise RuntimeError("The menu entry belongs to another portable copy of Dowlume.")
        target.unlink(missing_ok=True)
        return
    reject_links(executable)
    if not executable.is_file():
        raise RuntimeError("Run this script from an extracted Dowlume package.")
    icon = str(package / 'dowlume.png')
    if any(ord(c) < 32 for c in icon):
        raise ValueError("Invalid icon path.")
    content = ('[Desktop Entry]\nType=Application\nName=Dowlume\nExec=' + command +
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
        if read_owned(target) != original:
            raise RuntimeError("The menu entry changed during registration; retry after inspecting it.")
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
