"""Cache asset contracts and preservation of the accepted weapon blueprints."""
import json
import os
from pathlib import Path
import re
import unittest
import zipfile

ROOT = Path(__file__).resolve().parents[1]
MOD = ROOT / 'Assets/Modifications/ArmouryOrks'
CACHE_ID = 'f73bd67747f24b429b96c138ea280a63'

class ArmouryCacheTests(unittest.TestCase):
    def test_cache_blueprint_resolves_own_prefab(self):
        bp = json.loads((MOD / 'Blueprints/OrkArmouryCache_MapObject.jbp').read_text())
        self.assertEqual(bp['AssetId'], CACHE_ID)
        self.assertTrue(bp['Data']['$type'].endswith(', BlueprintDynamicMapObject'))
        self.assertEqual(bp['Data']['PrototypeLink'], '')
        meta = (MOD / 'Art/OrkArmouryCache.prefab.meta').read_text()
        self.assertIn('guid: ' + bp['Data']['Prefab']['guid'], meta)
        prefab = (MOD / 'Art/OrkArmouryCache.prefab').read_text()
        self.assertIn('--- !u!1 &' + str(bp['Data']['Prefab']['fileid']), prefab)
        self.assertNotIn('AstartesCustodes', prefab)
        self.assertNotIn('GuardianSpear', prefab)

    def test_cache_is_finite_persistent_interactive_and_named(self):
        prefab = (MOD / 'Art/OrkArmouryCache.prefab').read_text()
        for setting in ['UniqueId: armoury-orks-warrant-cache', 'LootContainerType: 2',
                        'DestroyWhenEmpty: 0', 'ShowOnMapWhenEmpty: 0',
                        'ShowOvertip: 1', 'ShowHighlight: 1', 'AddMapMarker: 1',
                        'm_LootTables: []']:
            self.assertIn(setting, prefab)
        self.assertIn('BoxCollider:', prefab)
        strings = json.loads((MOD / 'Localization/enGB.json').read_text())['strings']
        for name in ['Name', 'Description']:
            asset = (MOD / f'Art/OrkArmouryCache_{name}.asset').read_text()
            key = re.search(r'm_Key: (\S+)', asset).group(1)
            self.assertTrue(strings[key]['Text'])

    @unittest.skipUnless((ROOT / "Releases/ArmouryOrks-0.6.4.zip").exists(), "Historical package unavailable")
    def test_existing_weapon_blueprints_match_accepted_build(self):
        baseline = ROOT / 'Releases/ArmouryOrks-0.6.4.zip'
        with zipfile.ZipFile(baseline) as archive:
            previous = [name for name in archive.namelist() if name.startswith('Blueprints/') and name.endswith('.jbp')]
            self.assertGreater(len(previous), 100)
            for name in previous:
                self.assertEqual(json.loads(archive.read(name)),
                                 json.loads((MOD / 'Blueprints' / Path(name).name).read_text()), name)

    @unittest.skipUnless((ROOT / "Releases/ArmouryOrks-0.7.1.zip").exists(), "Historical package unavailable")
    def test_replacement_preserves_saved_cache_blueprint(self):
        with zipfile.ZipFile(ROOT / 'Releases/ArmouryOrks-0.7.1.zip') as archive:
            previous = json.loads(archive.read('Blueprints/OrkArmouryCache_MapObject.jbp'))
        current = json.loads((MOD / 'Blueprints/OrkArmouryCache_MapObject.jbp').read_text())
        self.assertEqual(previous, current)

    @unittest.skipUnless(os.getenv('ORK_MOD_ZIP'), 'Set ORK_MOD_ZIP to validate build output')
    def test_package_contains_cache_and_controller(self):
        with zipfile.ZipFile(ROOT / os.environ['ORK_MOD_ZIP']) as archive:
            bp = next(name for name in archive.namelist() if name == 'Blueprints/OrkArmouryCache_MapObject.jbp')
            self.assertEqual(json.loads(archive.read(bp))['AssetId'], CACHE_ID)
            dll = next(name for name in archive.namelist() if name == 'Assemblies/ArmouryOrks.Runtime.dll')
            self.assertIn(b'OrkArmouryCacheController', archive.read(dll))
