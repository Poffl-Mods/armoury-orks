"""Compile and run controller failure-path tests using explicit API test doubles."""
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[1]
MONO = Path('/Applications/Unity/Hub/Editor/6000.0.64f1/Unity.app/Contents/MonoBleedingEdge/bin')

class ProgressionTests(unittest.TestCase):
    @unittest.skipUnless((MONO / 'mcs').exists() or shutil.which('mcs'), 'Mono compiler required')
    def test_upgrade_failure_paths(self):
        compiler = str(MONO / 'mcs') if (MONO / 'mcs').exists() else shutil.which('mcs')
        runtime = str(MONO / 'mono') if (MONO / 'mono').exists() else shutil.which('mono')
        with tempfile.TemporaryDirectory() as folder:
            output = str(Path(folder) / 'ProgressionTests.exe')
            subprocess.run([compiler, '-out:' + output,
                str(ROOT / 'tests/progression/ProgressionHarness.cs'),
                str(ROOT / 'Assets/Modifications/ArmouryOrks/Scripts/ChoppaProgressionController.cs')], check=True)
            subprocess.run([runtime, output], check=True)
