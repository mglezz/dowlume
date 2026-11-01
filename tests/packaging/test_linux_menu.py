"""Portable registration tests use disposable paths, never the user's XDG directories."""
import importlib.util
import os
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

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
            updated = entry.read_bytes()
            with self.assertRaises(RuntimeError):
                menu.register(package, data, remove=True)
            self.assertEqual(updated, entry.read_bytes())
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

    def test_modified_entries_are_preserved(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary).resolve()
            (root / 'Dowlume.App').touch()
            menu.register(root, root)
            entry = root / 'applications/com.mglezz.dowlume.portable.desktop'
            valid = entry.read_text(encoding='utf-8')
            for modified in (valid + 'Exec=other\n', valid + 'Unknown=value\n',
                             valid.replace('Name=Dowlume', 'Name=Other'),
                             '[Other]\n' + valid, '# X-Dowlume-Portable=1\n'):
                entry.write_text(modified, encoding='utf-8')
                original = entry.read_bytes()
                for remove in (False, True):
                    with self.assertRaises(RuntimeError):
                        menu.register(root, root, remove=remove)
                    self.assertEqual(original, entry.read_bytes())

    def test_concurrent_edit_is_preserved_and_temporary_file_is_removed(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary).resolve()
            (root / 'Dowlume.App').touch()
            menu.register(root, root)
            entry = root / 'applications/com.mglezz.dowlume.portable.desktop'
            modified = entry.read_text(encoding='utf-8').replace('Name=Dowlume', 'Name=Edited')
            with patch.object(menu.os, 'fsync', side_effect=lambda _: entry.write_text(modified, encoding='utf-8')):
                with self.assertRaises(RuntimeError):
                    menu.register(root, root)
            self.assertEqual(modified, entry.read_text(encoding='utf-8'))
            self.assertEqual([entry], list(entry.parent.iterdir()))

    @unittest.skipUnless(os.name == 'posix', 'POSIX symbolic-link fixture')
    def test_linked_executable_cannot_replace_an_existing_registration(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary).resolve()
            (root / 'Dowlume.App').touch()
            menu.register(root, root)
            entry = root / 'applications/com.mglezz.dowlume.portable.desktop'
            original = entry.read_bytes()
            linked = root / 'linked'
            linked.mkdir()
            (linked / 'Dowlume.App').symlink_to(root / 'Dowlume.App')
            with self.assertRaises(RuntimeError):
                menu.register(linked, root)
            self.assertEqual(original, entry.read_bytes())
