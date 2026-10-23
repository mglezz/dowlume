#!/usr/bin/env python3
"""Run native read and move contracts on two newly created, disposable APFS images."""

import argparse
import json
import os
from pathlib import Path
import platform
import plistlib
import shutil
import subprocess
import tempfile
import xml.etree.ElementTree as ET


ROOT = Path(__file__).resolve().parents[1]


def capture(*args):
    return subprocess.check_output(args, cwd=ROOT)


def run_volume(filesystem, sensitive, output):
    # Resolve /var -> /private/var before tests exercise the no-symlink policy.
    staging = Path(tempfile.mkdtemp(prefix="dowlume-apfs-")).resolve()
    mount = staging / "volume"
    mount.mkdir()
    attached = False
    try:
        image = staging / "fixture.sparseimage"
        subprocess.run([
            "/usr/bin/hdiutil", "create", "-size", "256m", "-type", "SPARSE",
            "-fs", filesystem, "-volname", "DowlumeReadTests", "-nospotlight", str(image),
        ], check=True, cwd=ROOT)
        subprocess.run([
            "/usr/bin/hdiutil", "attach", str(image), "-nobrowse", "-owners", "on",
            "-mountpoint", str(mount),
        ], check=True, cwd=ROOT)
        attached = True
        info = plistlib.loads(capture("/usr/sbin/diskutil", "info", "-plist", str(mount)))
        if info.get("FilesystemType") != "apfs":
            raise RuntimeError("The disposable volume is not APFS.")
        environment = os.environ.copy()
        environment.update(TMPDIR=str(mount), TMP=str(mount), TEMP=str(mount),
                           DOWLUME_TEST_CASE_SENSITIVE=str(sensitive).lower(),
                           DOWLUME_TEST_SECOND_VOLUME=str(output), DOWLUME_TEST_OWNED_VOLUME=str(mount))
        report_name = "apfs-sensitive" if sensitive else "apfs-insensitive"
        subprocess.run([
            "dotnet", "test", "Dowlume/Dowlume.Tests/Dowlume.Tests.csproj", "-c", "Release",
            "--no-build", "--no-restore", "--filter", "Category=MacNativeRead|Category=MacNativeMove",
            "--logger", f"trx;LogFileName={report_name}.trx", "--results-directory", str(output),
        ], check=True, cwd=ROOT, env=environment, timeout=180)
        counters = ET.parse(output / f"{report_name}.trx").find(
            ".//{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}Counters")
        if counters is None or int(counters.attrib["total"]) < 25 or counters.attrib["passed"] != counters.attrib["total"]:
            raise RuntimeError("All native filesystem tests must execute and pass; skipped tests are not acceptance.")
        return {
            "filesystem": info["FilesystemType"], "caseSensitive": sensitive,
            "testsPassed": int(counters.attrib["passed"]), "trx": f"{report_name}.trx",
        }
    finally:
        # Never force-detach or remove a mounted tree. A detach failure preserves
        # this script's image and mount for inspection and fails the run.
        if attached:
            subprocess.run(["/usr/bin/hdiutil", "detach", str(mount)], check=True, cwd=ROOT)
        if os.path.ismount(mount):
            raise RuntimeError(f"Unexpected mounted fixture preserved at {staging}")
        shutil.rmtree(staging)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", required=True, type=Path, help="New directory for TRX files and the report")
    args = parser.parse_args()
    if platform.system() != "Darwin" or platform.machine() != "arm64" or int(platform.mac_ver()[0].split('.')[0]) < 15:
        raise SystemExit("Run on macOS 15+ Apple Silicon after building the Release tests.")
    if os.geteuid() == 0:
        raise SystemExit("Run as an ordinary user so permission-denial tests are meaningful.")
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=False)
    report = {
        "sourceCommit": capture("git", "rev-parse", "HEAD").decode().strip(),
        "dirtyWorkingTree": bool(capture("git", "status", "--porcelain").strip()),
        "macOS": platform.mac_ver()[0], "architecture": platform.machine(),
        "sdk": capture("dotnet", "--version").decode().strip(),
        "scope": "Native same-volume APFS read/move contracts; desktop and exact-package acceptance are separate.",
        "passed": False, "volumes": [],
    }
    try:
        for filesystem, sensitive in (("APFS", False), ("Case-sensitive APFS", True)):
            report["volumes"].append(run_volume(filesystem, sensitive, output))
        report["passed"] = True
    finally:
        (output / "ACCEPTANCE.mac-file-reads.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
