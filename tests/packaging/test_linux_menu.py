"""Portable registration tests use disposable paths, never the user's XDG directories."""
import importlib.util
from pathlib import Path
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
SPEC = importlib.util.spec_from_file_location('linux_menu', ROOT / 'packaging/linux/register-menu.py')
menu = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(menu)


class LinuxMenuTests(unittest.TestCase):
    def test_literal_path_registration_update_and_removal(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary).resolve()
            package = root / 'spaces and $dollar'
            package.mkdir()
            (package / 'Dowlume.App').touch()
            data = root / 'data'
            entry = data / 'applications/com.mglezz.dowlume.portable.desktop'
            menu.register(package, data)
            self.assertIn('\\\\$dollar', entry.read_text())
            self.assertIn('X-Dowlume-Portable=1', entry.read_text())
            second = root / 'updated'
            second.mkdir()
            (second / 'Dowlume.App').touch()
            menu.register(second, data)
            self.assertIn(menu.quote_exec(second / 'Dowlume.App'), entry.read_text())
            menu.register(second, data, remove=True)
            self.assertFalse(entry.exists())

    def test_unrelated_entry_and_field_codes_are_refused(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary).resolve()
            entry = root / 'applications/com.mglezz.dowlume.portable.desktop'
            entry.parent.mkdir()
            original = '[Desktop Entry]\nExec=unrelated\n'
            entry.write_text(original)
            for remove in (False, True):
                with self.assertRaises(RuntimeError):
                    menu.register(root, root, remove=remove)
                self.assertEqual(original, entry.read_text())
            with self.assertRaises(ValueError):
                menu.quote_exec(root / '%f' / 'Dowlume.App')
