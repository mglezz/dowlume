#!/usr/bin/env python3
"""Check local documentation links, project paths, and repository hygiene."""

import hashlib
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path
from urllib.parse import unquote, urlsplit

ROOT = Path(__file__).resolve().parents[1]


def source_files():
    result = subprocess.run(
        ['git', 'ls-files', '--cached', '--others', '--exclude-standard', '-z'],
        cwd=ROOT, check=True, capture_output=True,
    )
    return sorted({ROOT / name for name in result.stdout.decode().split('\0')
                   if name and (ROOT / name).is_file()})


def anchors(text):
    result = set(re.findall(r'(?:id|name)=["\']([^"\']+)["\']', text))
    counts = {}
    fenced = False
    for line in text.splitlines():
        if line.startswith(('```', '~~~')):
            fenced = not fenced
        if fenced:
            continue
        match = re.match(r'^#{1,6}\s+(.+?)(?:\s+#+)?$', line)
        if not match:
            continue
        title = re.sub(r'\[([^]]+)\]\([^)]+\)', r'\1', match[1])
        title = re.sub(r'<[^>]+>', '', title).lower()
        slug = re.sub(r'[^\w\- ]', '', title).replace(' ', '-')
        occurrence = counts.get(slug, 0)
        counts[slug] = occurrence + 1
        result.add(f'{slug}-{occurrence}' if occurrence else slug)
    return result


def check(files):
    errors = []
    checked_links = 0
    headings = {}
    for path in files:
        relative = path.relative_to(ROOT).as_posix()
        if re.search(r'(^|/)(bin|obj|artifacts|node_modules|__pycache__)/', relative):
            errors.append(f'{relative}: generated output must not be versioned')
        if path.name in {'.DS_Store', 'Thumbs.db', '.env'} or path.suffix in {'.db', '.pfx', '.pem', '.key'}:
            errors.append(f'{relative}: private or machine-specific file must not be versioned')
        if path.suffix == '.md':
            text = path.read_text(encoding='utf-8')
            # Code blocks document syntax and historical commands, not navigation.
            text = re.sub(r'^```.*?^```[^\n]*', '', text, flags=re.M | re.S)
            targets = re.findall(r'\]\(([^\s)]+)(?:\s+"[^"]*")?\)', text)
            targets += re.findall(r'^\[[^]]+\]:\s+(\S+)', text, flags=re.M)
            targets += re.findall(r'(?:href|src)=["\']([^"\']+)["\']', text)
            for target in targets:
                url = urlsplit(target.strip('<>'))
                if url.scheme or url.netloc or target == '#':
                    continue
                destination = (path.parent / unquote(url.path)).resolve() if url.path else path
                checked_links += 1
                if not destination.is_relative_to(ROOT):
                    errors.append(f'{relative}: link escapes the repository: {target}')
                elif not destination.exists():
                    errors.append(f'{relative}: missing link target: {target}')
                elif url.fragment and destination.suffix == '.md':
                    if destination not in headings:
                        headings[destination] = anchors(destination.read_text(encoding='utf-8'))
                    if unquote(url.fragment) not in headings[destination]:
                        errors.append(f'{relative}: missing heading: {target}')
        if path.suffix in {'.csproj', '.slnx'}:
            document = ET.parse(path)
            for element in document.iter():
                reference = element.get('Include') if element.tag == 'ProjectReference' else element.get('Path') if element.tag == 'Project' else None
                if reference and not (path.parent / reference.replace('\\', '/')).is_file():
                    errors.append(f'{relative}: missing project: {reference}')
    archive = ROOT / 'docs/archive/native-recycle'
    for entry in (archive / 'SHA256SUMS.txt').read_text().splitlines():
        digest, name = entry.split(maxsplit=1)
        path = archive / name.lstrip('*')
        if not path.is_file() or hashlib.sha256(path.read_bytes()).hexdigest().lower() != digest.lower():
            errors.append(f'Historical archive checksum mismatch: {name}')
    return errors, checked_links


def main():
    files = source_files()
    errors, links = check(files)
    if errors:
        print('\n'.join(errors), file=sys.stderr)
        print(f'Repository checks failed: {len(errors)} issue(s).', file=sys.stderr)
        return 1
    print(f'Repository checks passed: {len(files)} source files, {links} local links, project references, and archive checksums.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
