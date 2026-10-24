"""Prepare offline user documentation without shipping development archives."""

import re
import shutil
from pathlib import Path
from urllib.parse import quote, unquote, urlsplit


# Keep the complete image provenance page with the curated user-facing images.
DOCUMENTS = (
    'README.md',
    'docs/user/guide.md',
    'docs/user/platform-support.md',
    'docs/es/README.md',
    'docs/es/user/guide.md',
    'docs/es/user/platform-support.md',
    'docs/images/README.md',
    'docs/es/images/README.md',
    'docs/images/overview.png',
    'docs/images/classification.png',
    'docs/images/review.png',
    'docs/images/activity.png',
)


def copy_user_documentation(root: Path, destination: Path, revision: str):
    """Keep bundled links local; resolve other repository links to the build revision."""
    if not re.fullmatch(r'[0-9a-f]{40}', revision):
        raise ValueError('Documentation links require a full Git commit hash.')
    root = root.resolve()
    bundled = {root / name for name in DOCUMENTS} | {root / 'LICENSE'}
    for name in DOCUMENTS:
        source = root / name
        target = destination / name
        target.parent.mkdir(parents=True, exist_ok=True)
        if source.suffix != '.md':
            shutil.copy2(source, target)
            continue

        def resolve_link(match):
            value = match[1]
            url = urlsplit(value)
            if url.scheme or url.netloc or not url.path:
                return match[0]
            linked = (source.parent / unquote(url.path)).resolve()
            if not linked.is_relative_to(root):
                raise ValueError(f'Documentation link escapes the repository: {name}: {value}')
            if not linked.exists():
                raise ValueError(f'Missing documentation link: {name}: {value}')
            if linked in bundled:
                return match[0]
            remote = f'https://github.com/mglezz/dowlume/blob/{revision}/{quote(linked.relative_to(root).as_posix())}'
            if url.fragment:
                remote += '#' + url.fragment
            return '](' + remote + ')'

        text = re.sub(r'\]\(([^\s)]+)\)', resolve_link, source.read_text(encoding='utf-8'))
        target.write_text(text, encoding='utf-8')
