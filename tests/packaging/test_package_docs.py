"""Check the shipped documentation boundary and navigation."""

import re
import sys
import tempfile
import unittest
from pathlib import Path
from urllib.parse import urlsplit

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'scripts'))
from package_docs import copy_user_documentation  # noqa: E402


class PackageDocumentationTests(unittest.TestCase):
    def test_offline_guides_resolve_links_and_exclude_historical_sources(self):
        revision = 'a' * 40
        with tempfile.TemporaryDirectory() as temporary:
            output = Path(temporary)
            (output / 'LICENSE').write_bytes((ROOT / 'LICENSE').read_bytes())
            copy_user_documentation(ROOT, output, revision)
            self.assertTrue((output / 'docs/user/guide.md').is_file())
            self.assertTrue((output / 'docs/es/user/guide.md').is_file())
            self.assertFalse((output / 'docs/archive').exists())
            self.assertFalse((output / 'docs/es/archive').exists())
            self.assertFalse((output / 'docs/development').exists())
            self.assertFalse(list(output.rglob('*.cs.txt')))
            for document in output.rglob('*.md'):
                for target in re.findall(r'\]\(([^\s)]+)\)', document.read_text(encoding='utf-8')):
                    url = urlsplit(target)
                    if not url.scheme and url.path:
                        self.assertTrue((document.parent / url.path).exists(), f'{document}: {target}')
            readme = (output / 'README.md').read_text(encoding='utf-8')
            self.assertIn('(docs/user/guide.md)', readme)
            self.assertIn(f'/blob/{revision}/docs/development/roadmap.md', readme)
            self.assertIn('(docs/es/README.md)', readme)

    def test_rejects_non_revision_links(self):
        with tempfile.TemporaryDirectory() as temporary:
            with self.assertRaises(ValueError):
                copy_user_documentation(ROOT, Path(temporary), 'main')


if __name__ == '__main__':
    unittest.main()
