"""Chain Choppa balance, bounded random action graphs and upgrade regression checks."""
import hashlib
import json
import os
from pathlib import Path
import unittest
import zipfile

ROOT = Path(__file__).resolve().parents[1]
MOD = ROOT / 'Assets/Modifications/ArmouryOrks'
BLUEPRINTS = MOD / 'Blueprints'
CHAIN_GUID = 'fabce84f9db74c7b8fdb024b191491be'
MIN = [8, 10, 18, 22, 27, 34]
MAX = [13, 16, 27, 33, 41, 50]
PEN = [20, 20, 25, 25, 30, 35]


def uid(key):
    return hashlib.md5(('ArmouryOrks/ChainChoppa/' + key).encode()).hexdigest()


def read(name):
    return json.loads((BLUEPRINTS / (name + '.jbp')).read_text())


def nodes(value, kind):
    result = []
    if isinstance(value, dict):
        if value.get('$type', '').split(', ')[-1] == kind:
            result.append(value)
        for item in value.values():
            result.extend(nodes(item, kind))
    elif isinstance(value, list):
        for item in value:
            result.extend(nodes(item, kind))
    return result


def item(tier):
    return read('ChainChoppa_Item' if tier == 1 else f'ChainChoppa_V{tier}_Item')


class ChainChoppaTests(unittest.TestCase):
    def test_six_tiers_balance_and_unlocks(self):
        for i in range(6):
            tier = i + 1
            bp = item(tier)
            data = bp['Data']
            self.assertEqual(bp['AssetId'], CHAIN_GUID if tier == 1 else uid(f'v{tier}'))
            self.assertEqual([data[k] for k in ('WarhammerDamage', 'WarhammerMaxDamage', 'WarhammerPenetration')], [MIN[i], MAX[i], PEN[i]])
            choppa = read('Choppa_Item' if tier == 1 else f'Choppa_V{tier}_Item')['Data']
            self.assertLessEqual(data['WarhammerDamage'], choppa['WarhammerDamage'])
            self.assertLessEqual(data['WarhammerMaxDamage'], choppa['WarhammerMaxDamage'])
            self.assertGreater(data['WarhammerPenetration'], choppa['WarhammerPenetration'])
            self.assertEqual(data['Family'], 'Chain')
            self.assertEqual(data['m_HoldingType'], 'OneHanded')
            self.assertFalse(data['IsTwoHanded'])
            abilities = data['AbilityContainer']
            self.assertEqual(abilities['Ability3']['m_Ability'], '!bp_' + uid(f'v{tier}-chain'))
            self.assertEqual(abilities['Ability3']['AP'], 2)
            self.assertEqual(abilities['Ability4']['m_Ability'], '!bp_' + uid(f'v{tier}-bite') if tier >= 3 else None)
            self.assertEqual(abilities['Ability5']['m_Ability'], '!bp_' + uid(f'v{tier}-red') if tier >= 5 else None)
            if tier >= 5:
                self.assertEqual(abilities['Ability5']['AP'], 3)
            # The accepted standard attacks and chainsword audio remain inherited.
            self.assertEqual(abilities['Ability1']['AP'], 1)
            self.assertEqual(abilities['Ability2']['AP'], 2)
            self.assertEqual(data['m_VisualParameters']['m_WeaponAnimationStyle'], 'BrutalOneHanded')
            self.assertEqual(data['m_VisualParameters']['m_SoundTypeSwitch']['Value'], 'Chain')

    def test_kickback_excludes_enemy_attack_and_scales_with_tier(self):
        for i in range(6):
            tier = i + 1
            ability = read(f'ChainChoppa_V{tier}_Chain_Ability')
            self.assertFalse(nodes(ability, 'WarhammerAbilityAttackDelivery'))
            roll = nodes(ability, 'ContextActionRandomize')
            self.assertEqual(len(roll), 1)
            failure, success = roll[0]['m_Actions']
            self.assertEqual([failure['Weight'], success['Weight']], [20, 80])
            self.assertFalse(nodes(failure, 'ContextActionCastSpell'))
            caster = nodes(failure, 'ContextActionOnContextCaster')
            self.assertEqual(len(caster), 1)
            damage = nodes(caster, 'ContextActionDealDamage')[0]
            self.assertTrue(damage['Half'])
            self.assertEqual(damage['MinDamage']['Value'], MIN[i])
            self.assertEqual(damage['MaxDamage']['Value'], MAX[i])
            self.assertTrue(damage['DoNotUseCasterModifiers'])
            self.assertFalse(damage['HasRighteousFury'])
            self.assertEqual(nodes(success, 'ContextActionCastSpell')[0]['m_Spell'], '!bp_' + uid(f'v{tier}-boost-hit'))
            hidden = read(f'ChainChoppa_V{tier}_Hiddenboost_Item')['Data']
            self.assertEqual(hidden['WarhammerDamage'], round(MIN[i] * 1.5))
            self.assertEqual(hidden['WarhammerMaxDamage'], round(MAX[i] * 1.5))

    def test_followups_are_bounded_hit_only_and_free(self):
        for tier in range(3, 7):
            initial = read(f'ChainChoppa_V{tier}_Bite_Hit')
            on_hit = nodes(initial, 'DodgeActions')[0]
            self.assertEqual(on_hit['ActionsOnDodge']['Actions'], [])
            self.assertEqual(nodes(on_hit['ActionsOnHit'], 'ContextActionCastSpell')[0]['m_Spell'], '!bp_' + uid(f'v{tier}-extra1-hit'))
            for j in range(1, 4):
                followup = read(f'ChainChoppa_V{tier}_Extra{j}_Hit')
                self.assertEqual(followup['Data']['ActionPointCost'], 0)
                on_hit = nodes(followup, 'DodgeActions')[0]
                self.assertEqual(on_hit['ActionsOnDodge']['Actions'], [])
                jam, onward = nodes(on_hit['ActionsOnHit'], 'ContextActionRandomize')[0]['m_Actions']
                self.assertEqual([jam['Weight'], onward['Weight']], [25, 75])
                self.assertEqual(nodes(jam, 'ContextActionApplyBuff')[0]['m_Buff'], '!bp_' + uid('no-parry-buff'))
                self.assertFalse(nodes(jam, 'ContextActionCastSpell'))
                casts = nodes(onward, 'ContextActionCastSpell')
                self.assertEqual([a['m_Spell'] for a in casts], ['!bp_' + uid(f'v{tier}-extra{j + 1}-hit')] if j < 3 else [])
                hidden = read(f'ChainChoppa_V{tier}_Hiddenextra{j}_Item')['Data']
                self.assertEqual(hidden['WarhammerDamage'], round(MIN[tier - 1] * .35))
                self.assertEqual(hidden['WarhammerMaxDamage'], round(MAX[tier - 1] * .35))
            parry = nodes(read('ChainChoppa_NoParry_Buff'), 'WarhammerParryChanceModifierDefender')[0]
            self.assertTrue(parry['m_NeverParry'])

    def test_red_button_friendly_fire_and_one_mishap_per_cast(self):
        for tier in (5, 6):
            wrapper = read(f'ChainChoppa_V{tier}_Red_Ability')
            self.assertFalse(nodes(wrapper, 'WarhammerAbilityAttackDelivery'))
            self.assertTrue(wrapper['Data']['CanTargetSelf'])
            run = nodes(wrapper, 'AbilityEffectRunAction')[0]['Actions']['Actions']
            self.assertEqual(run[0]['m_Spell'], '!bp_' + uid(f'v{tier}-red-hit'))
            outcomes = run[1]['m_Actions']
            self.assertEqual([o['Weight'] for o in outcomes], [1, 1, 1])
            self.assertEqual(outcomes[0]['Action']['Actions'], [])
            self.assertEqual(nodes(outcomes[1], 'ContextActionApplyBuff')[0]['m_Buff'], '!bp_' + uid('jam-buff'))
            self.assertEqual(nodes(outcomes[2], 'ContextActionApplyBuff')[0]['m_Buff'], '!bp_8de915da52f24d9bb925d13fb2c8cf98')
            area = read(f'ChainChoppa_V{tier}_Red_Hit')
            delivery = nodes(area, 'WarhammerAbilityAttackDelivery')[0]
            self.assertEqual(delivery['m_PatternSettings']['Targets'], 'Any')
            self.assertEqual(delivery['m_PatternSettings']['Pattern']['m_Radius'], 1)
            self.assertTrue(area['Data']['CanTargetFriends'])
            self.assertTrue(area['Data']['CanTargetSelf'])
            self.assertEqual(delivery['WeaponAttack'], 'Melee')  # native melee patterns exclude the attacker
            self.assertFalse(nodes(area, 'ContextActionRandomize'))

    def test_motor_restrictions_and_cooldowns(self):
        for tier in range(1, 7):
            for key, file, ap, cooldown in [('chain', 'Chain', 2, 2), ('bite', 'Bite', 2, 3), ('red', 'Red', 3, 4)]:
                if key == 'bite' and tier < 3 or key == 'red' and tier < 5:
                    continue
                ability = read(f'ChainChoppa_V{tier}_{file}_Ability')
                self.assertEqual(ability['Data']['ActionPointCost'], ap)
                self.assertEqual(nodes(ability, 'WarhammerCooldown')[0]['CooldownInRounds'], cooldown)
                self.assertFalse(nodes(ability, 'WarhammerCooldown')[0]['UntilEndOfCombat'])
                self.assertEqual(nodes(ability, 'AbilityCasterHasNoFacts')[0]['m_Facts'], ['!bp_' + uid('jam-buff')])
                for buff in nodes(ability, 'ContextActionApplyBuff'):
                    self.assertEqual(buff['DurationValue']['RoundsValue']['Value'], 1)
                    self.assertTrue(buff['ToCaster'])
                    self.assertFalse(buff['AsChild'])
            # Jam never blocks either of the two native normal attacks.
            self.assertEqual(item(tier)['Data']['AbilityContainer']['Ability1']['m_Ability'], '!bp_9dec1bdade284190b0977f5f70d26d3e')

    def test_specials_have_chain_sound_and_no_virtual_weapon_override(self):
        fx = '!bp_046cf83ca27244998b0603750d4a833e'
        for path in BLUEPRINTS.glob('ChainChoppa*_Ability.jbp'):
            self.assertEqual(read(path.stem)['Data']['m_FXSettings'], fx, path.name)
        for path in BLUEPRINTS.glob('ChainChoppa*_Hit.jbp'):
            bp = read(path.stem)
            self.assertFalse(nodes(bp, 'WarhammerOverrideAbilityWeapon'), path.name)
            self.assertEqual(bp['Data']['m_FXSettings'], fx, path.name)
        for tier in range(1, 7):
            slots = item(tier)['Data']['AbilityContainer']
            for slot in ['Ability3'] + (['Ability4'] if tier >= 3 else []) + (['Ability5'] if tier >= 5 else []):
                self.assertEqual(slots[slot]['m_FXSettings'], fx)

    def test_runtime_preserves_equipped_item_and_only_scales_special_hits(self):
        runtime = (MOD / 'Scripts/ChainChoppaCombat.cs').read_text()
        self.assertIn('OverrideWeapon = weapon', runtime)
        self.assertIn('OverrideSourceItem = parent.SourceItem ?? weapon', runtime)
        self.assertIn('ItemSlotIndex = parent.ItemSlotIndex', runtime)
        self.assertIn('ItemSlotSource = parent.ItemSlotSource', runtime)
        self.assertIn('HitPercent.TryGetValue(ability', runtime)
        self.assertIn('ModifierType.PctMul, percent', runtime)
        self.assertIn('ForceFreeAction = true', runtime)
        self.assertIn('ChainChoppaCombat.Initialize()', (MOD / 'Scripts/ArmouryOrksRuntime.cs').read_text())

    def test_progression_ids_localization_and_references(self):
        progression = (MOD / 'Scripts/ChoppaProgressionController.cs').read_text()
        self.assertIn('TryUpgradeItemInSeries(item, targetTier, ChainChoppaGuids, "Chain Choppa")', progression)
        for tier in range(1, 7):
            self.assertIn(item(tier)['AssetId'], progression)
        for level in [16, 26, 36, 44, 50]:
            self.assertIn(f'level >= {level}', progression)
        strings = json.loads((MOD / 'Localization/enGB.json').read_text())['strings']
        all_bp = {read(p.stem)['AssetId']: read(p.stem) for p in BLUEPRINTS.glob('*.jbp')}
        chain = [read(p.stem) for p in BLUEPRINTS.glob('ChainChoppa*.jbp')]
        for bp in chain:
            for text in ['m_DisplayName', 'm_Description', 'm_FlavorText']:
                key = bp['Data'].get(text, {}).get('m_Key')
                if key:
                    self.assertTrue(strings[key]['Text'])
            for cast in nodes(bp, 'ContextActionCastSpell'):
                guid = cast['m_Spell'].removeprefix('!bp_')
                self.assertIn(guid, all_bp)
                self.assertFalse(cast['UseFullAbilityCastCycle'])

    @unittest.skipUnless((ROOT / 'Releases/ArmouryOrks-0.5.8.zip').exists(), 'Previous package unavailable')
    def test_other_weapon_blueprints_and_accepted_visuals_are_preserved(self):
        with zipfile.ZipFile(ROOT / 'Releases/ArmouryOrks-0.5.8.zip') as previous:
            for name in previous.namelist():
                if name.startswith('Blueprints/') and name.endswith('.jbp') and 'ChainChoppa' not in name:
                    self.assertEqual(json.loads(previous.read(name)), json.loads((BLUEPRINTS / Path(name).name).read_text()), name)
            old = json.loads(previous.read('Blueprints/ChainChoppa_Item.jbp'))['Data']['m_VisualParameters']
            for tier in range(1, 7):
                self.assertEqual(item(tier)['Data']['m_VisualParameters'], old)

    @unittest.skipUnless(os.environ.get('ORK_MOD_ZIP'), 'Set ORK_MOD_ZIP to validate the built package')
    def test_package_contains_all_tiers_and_skills(self):
        with zipfile.ZipFile(os.environ['ORK_MOD_ZIP']) as package:
            for path in BLUEPRINTS.glob('ChainChoppa*.jbp'):
                self.assertEqual(json.loads(package.read('Blueprints/' + path.name)), json.loads(path.read_text()), path.name)
