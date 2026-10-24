#!/usr/bin/env python3
"""Build native macOS packages and restricted Linux previews; never publish a release."""
import argparse
import hashlib
import json
from pathlib import Path
import platform
import plistlib
import shutil
import subprocess
import tarfile
import tempfile
import uuid
import xml.etree.ElementTree as ET
from datetime import datetime, timezone

from package_docs import copy_user_documentation

ROOT = Path(__file__).resolve().parent.parent


def run(*args, **kwargs):
    return subprocess.run(args, check=True, cwd=ROOT, **kwargs)


def build(runtime, output, staging):
    expected = {"osx-arm64": ("Darwin", "arm64"), "linux-x64": ("Linux", "x86_64")}[runtime]
    if (platform.system(), platform.machine()) != expected:
        raise RuntimeError("Build and test on the declared native OS and architecture.")
    if output.exists():
        raise RuntimeError("Choose a new output directory; existing artifacts are never replaced.")
    project = ROOT / "src/Dowlume.App/Dowlume.App.csproj"
    version = ET.parse(project).findtext("./PropertyGroup/Version")
    source = run("git", "rev-parse", "HEAD", capture_output=True, text=True).stdout.strip()
    dirty = bool(run("git", "status", "--porcelain", capture_output=True, text=True).stdout.strip())
    output.mkdir(parents=True)
    package_name = f"Dowlume-{version}-{runtime}" + ("-preview" if runtime == "linux-x64" else "")
    # Keep the bundle outside synced workspaces whose providers can reapply Finder metadata.
    package = staging / package_name
    if runtime == "osx-arm64":
        bundle = package / "Dowlume.app/Contents"
        publish = bundle / "MacOS"
        resources = bundle / "Resources"
        resources.mkdir(parents=True)
        with (bundle / "Info.plist").open("wb") as stream:
            plistlib.dump({"CFBundleIdentifier": "com.mglezz.dowlume", "CFBundleName": "Dowlume",
                          "CFBundleExecutable": "Dowlume.App", "CFBundlePackageType": "APPL",
                          "CFBundleShortVersionString": version, "CFBundleVersion": version,
                          "LSMinimumSystemVersion": "15.0", "NSHighResolutionCapable": True,
                          "CFBundleDisplayName": "Dowlume", "CFBundleIconFile": "Dowlume.icns",
                          "CFBundleDevelopmentRegion": "en", "CFBundleLocalizations": ["en", "es"],
                          "NSDownloadsFolderUsageDescription": "Dowlume organizes completed downloads in the folder you select.",
                          "NSDocumentsFolderUsageDescription": "Dowlume accesses only the organization or destination folder you select.",
                          "NSDesktopFolderUsageDescription": "Dowlume accesses only the organization or destination folder you select."}, stream)
        descriptions = {
            "en": "Dowlume accesses the folder you select to organize or move completed downloads.",
            "es": "Dowlume accede a la carpeta que eliges para organizar o mover descargas completas."
        }
        for language, description in descriptions.items():
            localized = resources / f"{language}.lproj"
            localized.mkdir()
            (localized / "InfoPlist.strings").write_text("".join(
                f'{key} = {json.dumps(description, ensure_ascii=False)};\n'
                for key in ("NSDownloadsFolderUsageDescription", "NSDocumentsFolderUsageDescription", "NSDesktopFolderUsageDescription")
            ), encoding="utf-8")
        iconset = staging / "Dowlume.iconset"
        iconset.mkdir()
        for size in (16, 32, 128, 256, 512):
            for scale in (1, 2):
                name = f"icon_{size}x{size}" + ("@2x" if scale == 2 else "") + ".png"
                run("sips", "-s", "format", "png", "-z", str(size * scale), str(size * scale),
                    str(ROOT / "src/Dowlume.App/Assets/dowlume.ico"), "--out", str(iconset / name), capture_output=True)
        run("iconutil", "-c", "icns", str(iconset), "-o", str(resources / "Dowlume.icns"))
    else:
        publish = package
        resources = package
    run("dotnet", "publish", str(project), "-c", "Release", "-r", runtime, "--self-contained", "true",
        "-o", str(publish), "-p:PublishTrimmed=false", "-p:DebugType=None", "-p:DebugSymbols=false")
    for symbols in publish.rglob("*.pdb"):
        symbols.unlink()
    if not (publish / "es/Dowlume.App.resources.dll").is_file():
        raise RuntimeError("Spanish resources are missing.")
    executable = publish / "Dowlume.App"
    executable.chmod(executable.stat().st_mode | 0o111)
    shutil.copy2(ROOT / "LICENSE", resources / "LICENSE")
    instructions = "MACOS.txt" if runtime == "osx-arm64" else "PLATFORM-PREVIEW.txt"
    shutil.copy2(ROOT / "packaging" / instructions, resources / "START-HERE.txt")
    shutil.copy2(ROOT / "packaging/es" / instructions, resources / "START-HERE.es.txt")
    copy_user_documentation(ROOT, resources, source)
    if runtime == "osx-arm64" and not (publish / "libdowlume_native.dylib").is_file():
        raise RuntimeError("The native file-operation adapter is missing.")
    info = {"Version": version, "SourceCommit": source, "DirtyWorkingTree": dirty,
            "RuntimeIdentifier": runtime, "SelfContained": True, "BuiltAtUtc": datetime.now(timezone.utc).isoformat(),
            "Support": "macOS 15+ Apple Silicon; local same-volume APFS operations" if runtime == "osx-arm64" else "Experimental preview only; file mutations and recovery disabled", "Signing": "Ad-hoc only; no Developer ID signature or notarization" if runtime == "osx-arm64" else "Unsigned",
            "OS": platform.platform(), "Architecture": platform.machine()}
    info_name = f"BUILD-INFO.{runtime}.json"
    (output / info_name).write_text(json.dumps(info, indent=2) + "\n", encoding="utf-8")
    shutil.copy2(output / info_name, resources / "BUILD-INFO.json")
    if runtime == "osx-arm64":
        # Ad-hoc signatures are for local execution only, not Developer ID distribution/notarization.
        # Normalize Finder metadata only inside this newly created bundle; retain quarantine/provenance attributes.
        app_bundle = package / "Dowlume.app"
        for attribute in ("com.apple.FinderInfo", "com.apple.ResourceFork"):
            run("/usr/bin/xattr", "-dr", attribute, str(app_bundle))
        run("codesign", "--force", "--deep", "--sign", "-", str(app_bundle))
        run("codesign", "--verify", "--deep", "--strict", str(app_bundle))
        archive = output / f"{package_name}.zip"
        run("ditto", "-c", "-k", "--sequesterRsrc", "--keepParent", str(package / "Dowlume.app"), str(archive))
    else:
        archive = output / f"{package_name}.tar.gz"
        with tarfile.open(archive, "w:gz") as stream:
            stream.add(package, arcname=package_name)
    # Smoke-test the extracted archive, not the staging directory.
    extracted = staging / "extracted"
    extracted.mkdir()
    if runtime == "osx-arm64":
        run("ditto", "-x", "-k", str(archive), str(extracted))
        run("codesign", "--verify", "--deep", "--strict", str(extracted / "Dowlume.app"))
        tested_executable = extracted / "Dowlume.app/Contents/MacOS/Dowlume.App"
    else:
        with tarfile.open(archive) as stream:
            stream.extractall(extracted, filter="data")
        tested_executable = extracted / package_name / "Dowlume.App"
    profile = output / ("smoke-profile-" + uuid.uuid4().hex)
    command = [str(tested_executable), "--smoke-test", "--data-directory", str(profile)]
    if runtime == "linux-x64":
        command = ["xvfb-run", "-a", *command]
    run(*command, timeout=60)
    if not (profile / "dowlume.db").is_file():
        raise RuntimeError("The extracted application did not initialize its isolated profile.")
    workflow = None
    if runtime == "osx-arm64":
        workflow = json.loads((profile / "MACOS-WORKFLOW.json").read_text(encoding="utf-8"))
        if workflow.get("Passed") is not True:
            raise RuntimeError("The extracted macOS application did not pass its native workflow.")
    digest = hashlib.sha256(archive.read_bytes()).hexdigest()
    report = {"Passed": True, "ArchiveSha256": digest, "SourceCommit": source,
              "RuntimeIdentifier": runtime, "NativeWindowLaunchAndGracefulExit": True,
              "FileMutationsEnabled": runtime == "osx-arm64", "NativeWorkflow": workflow, "ManualDesktopAcceptance": False,
              "CleanMachineAcceptance": False, "Note": "Extracted-package automation on an SDK-equipped runner; manual installation, permissions and actual login need separate acceptance."}
    report_name = f"ACCEPTANCE.{runtime}.json"
    (output / report_name).write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    files = [archive, output / info_name, output / report_name]
    checksums = "".join(f"{hashlib.sha256(file.read_bytes()).hexdigest()}  {file.name}\n" for file in files)
    (output / f"SHA256SUMS.{runtime}.txt").write_text(checksums, encoding="ascii")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--runtime", required=True, choices=["osx-arm64", "linux-x64"])
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    with tempfile.TemporaryDirectory(prefix="dowlume-preview-") as temporary:
        build(args.runtime, args.output.resolve(), Path(temporary).resolve())
