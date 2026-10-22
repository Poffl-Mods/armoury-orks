"""Power Klaw design-contract checks against generated game blueprints.

Run: python3 -m unittest discover -s tests -v
Optionally set ORK_MOD_ZIP to check a built ZIP against these source assets.
These checks do not replace in-game combat and save/load validation.
"""
import json
import os
from pathlib import Path
import re
import unittest
import zipfile

ROOT = Path(__file__).resolve().parents[1]
MOD = ROOT / 'Assets/Modifications/ArmouryOrks'
BLUEPRINTS = MOD / 'Blueprints'


def load(name):
    return json.loads((BLUEPRINTS / (name + '.jbp')).read_text())


def component(blueprint, kind):
    matches = [c for c in blueprint['Data']['Components']
               if c['$type'].split(', ')[-1] == kind]
    if len(matches) != 1:
        raise AssertionError(f'Expected one {kind}, got {len(matches)}')
    return matches[0]


def actions(blueprint):
    return component(blueprint, 'AbilityEffectRunAction')['Actions']['Actions']


class PowerKlawContract(unittest.TestCase):
    def test_six_tiers_and_unlocks(self):
        expected = [(8, 13, 30), (10, 14, 30), (18, 27, 35),
                    (22, 32, 40), (27, 41, 45), (34, 50, 50)]
        for tier, values in enumerate(expected, 1):
            with self.subTest(tier=tier):
                d = load(f'PowerKlaw_V{tier}_Item')['Data']
                self.assertEqual(tuple(d[k] for k in ('WarhammerDamage', 'WarhammerMaxDamage', 'WarhammerPenetration')), values)
                self.assertFalse(d['IsTwoHanded'])
                self.assertEqual(d['m_VisualParameters']['m_WeaponAnimationStyle'], 'Fist')
                slots = d['AbilityContainer']
                self.assertEqual(sum(s['m_Ability'] is not None for s in slots.values()), 3 + (tier >= 3) + (tier >= 5))
                for index in range(1, 6):
                    slot = slots[f'Ability{index}']
                    if slot['m_Ability']:
                        self.assertEqual(slot['AP'], 1 if index == 1 else 2)
                self.assertEqual(d['m_AttackOfOpportunityAbility'], slots['Ability1']['m_Ability'])

    def test_all_tiers_use_a_separate_shared_empty_holster_reference(self):
        holsters = []
        for tier in range(1, 7):
            bp = load(f'PowerKlaw_V{tier}_Item')
            visual = bp['Data']['m_VisualParameters']
            belt = visual['m_WeaponBeltModelOverride']
            self.assertIsNotNone(belt)
            self.assertNotEqual(belt, visual['m_WeaponModel'])
            self.assertEqual(belt, visual['m_WeaponSheathModelOverride'])
            self.assertIn('m_VisualParameters.m_WeaponBeltModelOverride', bp['Data']['m_Overrides'])
            self.assertIn('m_VisualParameters.m_WeaponSheathModelOverride', bp['Data']['m_Overrides'])
            holsters.append(belt)
        self.assertTrue(all(ref == holsters[0] for ref in holsters))

    def test_custom_inventory_icon_and_distinct_native_ability_icons(self):
        art = MOD / 'Art/PowerKlaw_InventoryIcon.png'
        png = art.read_bytes()
        self.assertEqual(png[:8], b'\x89PNG\r\n\x1a\n')
        import struct
        self.assertEqual(struct.unpack('>II', png[16:24]), (512, 512))
        self.assertEqual(png[25], 6, 'Expected an RGBA icon with transparency')
        meta = Path(str(art) + '.meta').read_text()
        guid = re.search(r'^guid: ([0-9a-f]{32})$', meta, re.M).group(1)
        for tier in range(1, 7):
            self.assertEqual(load(f'PowerKlaw_V{tier}_Item')['Data']['m_Icon'], {'guid': guid, 'fileid': 21300000})
        names = ['PowerKlaw_Krump_Ability', 'PowerKlaw_Sweep_Ability', 'PowerKlaw_GetBack_Ability',
                 'PowerKlaw_Armour_Ability', 'PowerKlaw_V5_Squeeze_Ability']
        icons = [load(name)['Data']['m_Icon']['guid'] for name in names]
        self.assertEqual(len(set(icons)), 5)
        self.assertNotIn('a6cba97367839af4e8869281de029095', icons, 'Axe placeholder remains')
        self.assertEqual(load('PowerKlaw_NoArmour_Buff')['Data']['m_Icon']['guid'], icons[3])
        self.assertEqual(load('PowerKlaw_V6_Squeeze_Ability')['Data']['m_Icon']['guid'], icons[4])

    def test_upgrade_and_animation_identity_match_all_tiers(self):
        series = [load(f'PowerKlaw_V{i}_Item')['AssetId'] for i in range(1, 7)]
        self.assertEqual(series[0], '89d3b9e7c7134f7f94c36518212b8596')
        for script, field in [('ChoppaProgressionController.cs', 'PowerKlawGuids'),
                              ('PowerKlawFistAnimations.cs', 'WeaponGuids')]:
            source = (MOD / 'Scripts' / script).read_text()
            block = re.search(r'\b' + field + r'\s*=.*?\{(.*?)\}', source, re.S).group(1)
            self.assertEqual(re.findall(r'"([0-9a-f]{32})"', block), series)
        progression = (MOD / 'Scripts/ChoppaProgressionController.cs').read_text()
        self.assertIn('TryUpgradeItemInSeries(item, targetTier, PowerKlawGuids, "Power Klaw")', progression)

    def test_push_is_on_hit_only(self):
        bp = load('PowerKlaw_GetBack_Ability')
        self.assertFalse(any('AbilityEffectMissIsHit' in c['$type'] for c in bp['Data']['Components']))
        push, = actions(bp)
        self.assertTrue(push['$type'].endswith(', ContextActionPush'))
        self.assertEqual(push['Cells']['Value'], 1)
        self.assertFalse(push['ProvokeAttackOfOpportunity'])

    def test_armour_tradeoff_hit_only_two_rounds_and_three_round_cooldown(self):
        bp = load('PowerKlaw_Armour_Ability')
        self.assertFalse(any('AbilityEffectMissIsHit' in c['$type'] for c in bp['Data']['Components']))
        effects = actions(bp)
        self.assertEqual(len(effects), 2)
        self.assertEqual({a['ToCaster'] for a in effects}, {True, False})
        for effect in effects:
            self.assertEqual(effect['m_Buff'], '!bp_' + load('PowerKlaw_NoArmour_Buff')['AssetId'])
            self.assertEqual(effect['DurationValue']['RoundsValue']['Value'], 2)
            self.assertEqual(effect['DurationValue']['BonusValue']['Value'], 0)
            self.assertFalse(effect['Permanent'])
        cd = component(bp, 'WarhammerCooldown')
        self.assertEqual(cd['CooldownInRounds'], 3)
        self.assertFalse(cd['UntilEndOfCombat'])

    def test_armour_is_capped_at_zero_without_touching_deflection(self):
        bp = load('PowerKlaw_NoArmour_Buff')
        self.assertEqual(len(bp['Data']['Components']), 1)
        armour = component(bp, 'WarhammerArmorBonus')
        self.assertTrue(armour['ForceAbsorptionMaximum'])
        self.assertEqual(armour['AbsorptionMaximumValue']['ValueType'], 'Simple')
        self.assertEqual(armour['AbsorptionMaximumValue']['Value'], 0)
        self.assertFalse(armour['BonusAbsorptionValue']['Enabled'])
        self.assertFalse(armour['BonusDeflectionValue']['Enabled'])
        for flag in ['ForceDeflectionMinimum', 'ForceDeflectionMaximum', 'ForcePctDeflectionMaximum']:
            self.assertFalse(armour[flag])
        self.assertEqual(armour['Restrictions']['Property']['Getters'], [])
        self.assertEqual(bp['Data']['Stacking'], 'Replace')
        self.assertEqual(bp['Data']['Initiative'], 'ByCaster')

    def test_squeeze_damage_cooldown_and_prone_even_on_miss(self):
        for tier in [5, 6]:
            with self.subTest(tier=tier):
                bp = load(f'PowerKlaw_V{tier}_Squeeze_Ability')
                component(bp, 'AbilityEffectMissIsHit')
                cd = component(bp, 'WarhammerCooldown')
                self.assertEqual(cd['CooldownInRounds'], 4)
                self.assertFalse(cd['UntilEndOfCombat'])
                effect, = actions(bp)
                self.assertEqual(effect['m_Buff'], '!bp_8de915da52f24d9bb925d13fb2c8cf98')
                self.assertTrue(effect['ToCaster'])
                self.assertEqual(effect['DurationValue']['RoundsValue']['Value'], 1)
                hidden = load(f'PowerKlaw_V{tier}_HiddenSqueeze_Item')
                visible = load(f'PowerKlaw_V{tier}_Item')['Data']
                self.assertEqual(component(bp, 'WarhammerOverrideAbilityWeapon')['m_Weapon'], '!bp_' + hidden['AssetId'])
                for key in ['WarhammerDamage', 'WarhammerMaxDamage']:
                    self.assertEqual(hidden['Data'][key], visible[key] * 2)
                self.assertEqual(hidden['Data']['WarhammerPenetration'], visible['WarhammerPenetration'])
                self.assertFalse(hidden['Data']['CanBeUsedInGame'])
                self.assertTrue(hidden['Data']['IsUnlootable'])
                self.assertEqual(hidden['Data']['m_DamageType'], {'Type': 'Impact'})
                self.assertEqual(hidden['Data']['m_VisualParameters']['m_WeaponAnimationStyle'], 'Fist')
                self.assertFalse(component(bp, 'WarhammerAbilityAttackDelivery')['m_AutoHit'])

    def test_all_slots_resolve_and_localization_is_present(self):
        all_bps = [json.loads(p.read_text()) for p in BLUEPRINTS.glob('*.jbp')]
        by_id = {bp['AssetId']: bp for bp in all_bps}
        self.assertEqual(len(by_id), len(all_bps), 'Duplicate blueprint ID')
        strings = json.loads((MOD / 'Localization/enGB.json').read_text())['strings']
        for tier in range(1, 7):
            for slot in load(f'PowerKlaw_V{tier}_Item')['Data']['AbilityContainer'].values():
                if slot['m_Ability']:
                    ability = by_id[slot['m_Ability'][4:]]
                    self.assertEqual(ability['Data']['ActionPointCost'], slot['AP'])
        for p in BLUEPRINTS.glob('PowerKlaw*.jbp'):
            bp = json.loads(p.read_text())
            for key in re.findall(r'"m_Key": "(power-klaw-[^"]+)"', p.read_text()):
                self.assertTrue(strings[key]['Text'])
            if any('AbilityEffectRunAction' in c['$type'] for c in bp['Data']['Components']):
                self.assertIn('Components', bp['Data']['m_Overrides'])

    @unittest.skipUnless(os.environ.get('ORK_MOD_ZIP'), 'Set ORK_MOD_ZIP to validate build output')
    def test_package_contains_current_assets_and_runtime(self):
        with zipfile.ZipFile(os.environ['ORK_MOD_ZIP']) as z:
            self.assertIsNone(z.testzip())
            self.assertEqual(json.loads(z.read('OwlcatModificationManifest.json'))['Version'], os.environ.get('ORK_MOD_VERSION', '1.0.2'))
            runtime = z.read('Assemblies/ArmouryOrks.Runtime.dll')
            self.assertGreater(len(runtime), 1000)
            for tier in range(1, 7):
                guid = load(f'PowerKlaw_V{tier}_Item')['AssetId']
                self.assertIn(guid.encode('utf-16-le'), runtime)
            for p in BLUEPRINTS.glob('PowerKlaw*.jbp'):
                self.assertEqual(json.loads(z.read('Blueprints/' + p.name)), json.loads(p.read_text()))
            localization = next(n for n in z.namelist() if n.endswith('enGB.json'))
            self.assertEqual(json.loads(z.read(localization)), json.loads((MOD / 'Localization/enGB.json').read_text()))
            self.assertTrue(any('directreference' in n.lower() for n in z.namelist()))


if __name__ == '__main__':
    unittest.main()
