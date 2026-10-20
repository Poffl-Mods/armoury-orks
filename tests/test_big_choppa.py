"""Big Choppa visual inheritance and mechanics regression checks."""
import json
import os
from pathlib import Path
import unittest
import zipfile

ROOT = Path(__file__).resolve().parents[1]
BLUEPRINTS = ROOT / 'Assets/Modifications/ArmouryOrks/Blueprints'
SWORD = '88863b6b0c61404b96b01c2bc648ba5e'
WEAPONS = sorted(BLUEPRINTS.glob('BigChoppa_*Item.jbp'))


class BigChoppaTests(unittest.TestCase):
    def test_all_tiers_and_special_attacks_use_explicit_sword_visuals(self):
        self.assertEqual(len(WEAPONS), 18)
        for path in WEAPONS:
            with self.subTest(weapon=path.name):
                data = json.loads(path.read_text())['Data']
                self.assertEqual(data['PrototypeLink'], SWORD)
                self.assertIn('m_VisualParameters', data['m_Overrides'])
                self.assertEqual(data['m_VisualParameters']['m_WeaponAnimationStyle'], 'BrutalTwoHanded')
                # Neither staff features nor native sword bonuses may be inherited.
                self.assertIn('Components', data['m_Overrides'])
                self.assertEqual(data['Components'], [])
                self.assertEqual(data['m_Enchantments'], [])

    @unittest.skipUnless((ROOT / 'Releases/ArmouryOrks-0.5.3.zip').exists(), 'Previous package unavailable')
    def test_existing_identities_and_mechanics_are_preserved(self):
        with zipfile.ZipFile(ROOT / 'Releases/ArmouryOrks-0.5.3.zip') as old:
            for path in WEAPONS:
                with self.subTest(weapon=path.name):
                    before = json.loads(old.read('Blueprints/' + path.name))
                    after = json.loads(path.read_text())
                    self.assertEqual(before['AssetId'], after['AssetId'])
                    for field in ('PrototypeLink', 'm_Overrides', 'm_VisualParameters'):
                        before['Data'].pop(field)
                        after['Data'].pop(field)
                    self.assertEqual(before['Data'], after['Data'])

    @unittest.skipUnless(os.environ.get('ORK_MOD_ZIP'), 'Set ORK_MOD_ZIP to validate build output')
    def test_package_uses_current_sword_blueprints(self):
        with zipfile.ZipFile(os.environ['ORK_MOD_ZIP']) as package:
            for path in WEAPONS:
                self.assertEqual(json.loads(package.read('Blueprints/' + path.name)), json.loads(path.read_text()))
