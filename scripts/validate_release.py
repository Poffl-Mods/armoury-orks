"""Validate a distributable's manifest, runtime and actual Unity target platform.

Requires UnityPy in the validation environment (not needed by mod users).
Usage: python3 scripts/validate_release.py Releases/ArmouryOrks-1.0.1-Windows.zip --platform windows
"""
import argparse
import hashlib
import json
from pathlib import Path
import zipfile
import UnityPy

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('package', type=Path)
parser.add_argument('--platform', choices=['windows', 'macos'], required=True)
args = parser.parse_args()
expected = {'windows': 19, 'macos': 2}[args.platform]

def serialized_targets(file):
    target = getattr(file, 'target_platform', None)
    if target is not None:
        yield int(target)
    for child in (getattr(file, 'files', None) or {}).values():
        yield from serialized_targets(child)

with zipfile.ZipFile(args.package) as archive:
    assert archive.testzip() is None, 'ZIP CRC check failed'
    manifest = json.loads(archive.read('OwlcatModificationManifest.json'))
    assert manifest['Version'] == '1.0.1', manifest
    assert manifest['UniqueName'] == 'ArmouryOrks', manifest
    assert len(archive.read('Assemblies/ArmouryOrks.Runtime.dll')) > 1000
    bundles = [name for name in archive.namelist() if name.startswith('Bundles/') and not name.endswith('/')]
    assert bundles, 'No asset bundles'
    platforms = set()
    for name in bundles:
        env = UnityPy.load(archive.read(name))
        targets = {target for file in env.files.values() for target in serialized_targets(file)}
        assert targets == {expected}, (name, targets, expected)
        platforms.update(targets)
    bp = json.loads(archive.read('Blueprints/OrkArmouryCache_MapObject.jbp'))
    assert bp['AssetId'] == 'f73bd67747f24b429b96c138ea280a63'
print(json.dumps({'package': args.package.name, 'version': manifest['Version'],
                  'platform': args.platform, 'unity_targets': sorted(platforms),
                  'bundles': len(bundles), 'sha256': hashlib.sha256(args.package.read_bytes()).hexdigest()}, indent=2))
