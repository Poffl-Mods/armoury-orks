using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ArmouryOrks.Editor
{
    internal static partial class ChoppaGenerator
    {
        private static string ChainJam => ChainId("jam-buff");
        private static string ChainNoParry => ChainId("no-parry-buff");

        private static JObject ChainAction(string type, string key) => new JObject
        {
            ["$type"] = type, ["name"] = "$ChainChoppa$" + key
        };
        private static JObject ChainActions(params JObject[] actions) => new JObject { ["Actions"] = new JArray(actions) };
        private static JObject ChainRandom(params (int weight, JObject[] actions)[] outcomes)
        {
            JObject action = ChainAction("0cdc69d3ddfd42f28045114acc14e069, ContextActionRandomize", "random");
            action["m_Actions"] = new JArray(outcomes.Select(outcome => new JObject
            {
                ["Action"] = ChainActions(outcome.actions), ["Weight"] = outcome.weight,
                ["UseContextValue"] = false, ["ValueName"] = "Value1"
            }));
            return action;
        }
        private static JObject ChainCast(string guid)
        {
            JObject action = ChainAction("2f9cfff2340b8c344ab4fd92c2eb61f2, ContextActionCastSpell", guid);
            action["m_Spell"] = "!bp_" + guid;
            action["OverrideDC"] = false; action["DC"] = SimpleValue(0);
            action["OverrideSpellLevel"] = false; action["SpellLevel"] = SimpleValue(0);
            action["CastByTarget"] = false;
            // The native action performs a free child cast immediately, so subsequent
            // actions see its result and a post-cast mishap is rolled once per use.
            action["UseFullAbilityCastCycle"] = false;
            return action;
        }
        private static JObject ChainBuff(string guid)
        {
            JObject action = ApplyBuffAction(guid, 1, true);
            action["AsChild"] = false;
            return action;
        }
        private static JObject ChainOnHit(params JObject[] actions)
        {
            JObject action = ChainAction("65ae140109ad4f05a0ae3e0f4f226214, DodgeActions", "on-hit");
            action["ActionsOnHit"] = ChainActions(actions);
            action["DodgePenalty"] = 0;
            action["ActionsOnDodge"] = ChainActions();
            return action;
        }
        private static void ChainRun(JObject ability, params JObject[] actions)
        {
            // Always override the complete component array when modifying inherited
            // delivery/actions. Otherwise prototype delivery can survive removal.
            JObject run = CloneComponent(TwoHandedStrikeAbility, "AbilityEffectRunAction", "$AbilityEffectRunAction$chain-" + ability["AssetId"]);
            run["SavingThrowType"] = "Unknown";
            run["Actions"] = ChainActions(actions);
            JArray components = (JArray)ability["Data"]["Components"];
            foreach (JObject old in components.Children<JObject>().Where(c => c["$type"].ToString().Contains("AbilityEffectRunAction")).ToArray()) old.Remove();
            components.Add(run);
            AddOverride(ability, "Components");
        }
        private static void ChainCheckMotor(JObject ability)
        {
            JObject check = ChainAction("c53b6a74192ad9d43818c98797b7e0de, AbilityCasterHasNoFacts", "motor-check");
            check["m_Flags"] = 0;
            check["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            check["m_Overrides"] = new JArray();
            check["m_Facts"] = new JArray("!bp_" + ChainJam);
            ((JArray)ability["Data"]["Components"]).Add(check);
            AddOverride(ability, "Components");
        }
        private static JObject ChainAbility(string guid, string key, int ap)
        {
            JObject ability = PrepareClone(Load(StrikeAbilityPrototype), guid, StrikeAbilityPrototype);
            SetAbilityText(ability, "chain-" + key + "-name", "chain-" + key + "-description");
            SetAbilityIcon(ability, key == "red" ? ThunderHammerAoeIconGuid : key == "bite" ? KlawSqueezeIconGuid : RevelInSlaughterIconGuid);
            Override(ability, "ActionPointCost", ap);
            Override(ability, "CooldownRounds", 0);
            Override(ability, "VeilThicknessPointsToAdd", 0);
            Override(ability, "m_FXSettings", "!bp_" + ChainChoppaFx);
            Override(ability, "CombatStateRestriction", "NoRestriction");
            return ability;
        }
        private static JObject ChainWrapper(int tier, string key, int ap, int cooldown, bool self)
        {
            JObject ability = ChainAbility(ChainId($"v{tier}-{key}"), key, ap);
            JArray components = (JArray)ability["Data"]["Components"];
            foreach (JObject delivery in components.Children<JObject>()
                .Where(c => c["$type"].ToString().Contains("WarhammerAbilityAttackDelivery")).ToArray()) delivery.Remove();
            AddOverride(ability, "Components");
            ChainCheckMotor(ability);
            AddRoundCooldown(ability, cooldown, "$WarhammerCooldown$chain-" + key);
            if (self)
            {
                Override(ability, "Range", "Personal");
                Override(ability, "CanTargetEnemies", false);
                Override(ability, "CanTargetFriends", false);
                Override(ability, "CanTargetSelf", true);
                Override(ability, "ShouldTurnToTarget", false);
            }
            return ability;
        }
        private static string ChainHiddenWeapon(int i, string key, float multiplier)
        {
            string guid = ChainId($"v{i + 1}-{key}-weapon");
            JObject weapon = PrepareClone(Load(ChainswordPrototype), guid, ChainswordPrototype);
            Override(weapon, "Components", new JArray());
            Override(weapon, "WarhammerDamage", Mathf.Max(1, Mathf.RoundToInt(ChainDamageMin[i] * multiplier)));
            Override(weapon, "WarhammerMaxDamage", Mathf.Max(1, Mathf.RoundToInt(ChainDamageMax[i] * multiplier)));
            Override(weapon, "WarhammerPenetration", ChainPenetration[i]);
            Override(weapon, "m_Enchantments", new JArray());
            Override(weapon, "CanBeUsedInGame", false);
            Override(weapon, "IsUnlootable", true);
            weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = null;
            AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
            SaveBlueprint($"ChainChoppa_V{i + 1}_Hidden{key}_Item", weapon);
            return guid;
        }
        private static JObject ChainHit(int i, string key, float multiplier, bool area = false)
        {
            string guid = ChainId($"v{i + 1}-{key}-hit");
            ChainHiddenWeapon(i, key, multiplier); // Legacy save compatibility only.
            JObject ability;
            if (area)
            {
                ability = PrepareClone(Load(MassStunAbilityPrototype), guid, MassStunAbilityPrototype);
                // Keep only the native circular attack delivery, removing psychic FX,
                // stun and inherited actions. One-cell radius around the user.
                JObject delivery = CloneComponent(MassStunAbilityPrototype, "WarhammerAbilityAttackDelivery", "$WarhammerAbilityAttackDelivery$chain-red");
                delivery["WeaponAttack"] = "Melee";
                delivery["m_PatternSettings"]["Pattern"] = new JObject
                {
                    ["m_Type"] = "Circle", ["m_Blueprint"] = null, ["m_Radius"] = 1, ["m_Angle"] = 0
                };
                delivery["m_PatternSettings"]["Targets"] = "Any";
                delivery["m_PatternSettings"]["m_UseMeleeLos"] = true;
                delivery["m_DisableDodgeForAlly"] = false;
                Override(ability, "Components", new JArray(delivery));
                Override(ability, "CanTargetEnemies", true);
                Override(ability, "CanTargetFriends", true);
                // Cast at the user to centre the circle. The native melee pattern
                // delivery explicitly excludes the attacker from damage targets.
                Override(ability, "CanTargetSelf", true);
                Override(ability, "CanTargetPoint", true);
                Override(ability, "EffectOnAlly", "Harmful");
                Override(ability, "m_FXSettings", null);
                Override(ability, "Animation", "Directional");
                Override(ability, "m_AbilityGroups", new JArray());
            }
            else ability = ChainAbility(guid, key == "boost" ? "chain" : "bite", 0);
            // Keep legacy hidden-item IDs for saves, but use the real equipped
            // weapon via ChainChoppaCombat. Do not install a virtual weapon override.
            SetAbilityText(ability, "chain-" + (area ? "red" : key == "boost" ? "chain" : "bite") + "-name",
                "chain-" + (area ? "red" : key == "boost" ? "chain" : "bite") + "-description");
            Override(ability, "Type", "Weapon");
            Override(ability, "AbilityParamsSource", "Weapon");
            Override(ability, "ActionPointCost", 0);
            Override(ability, "VeilThicknessPointsToAdd", 0);
            Override(ability, "CombatStateRestriction", "NoRestriction");
            Override(ability, "m_FXSettings", "!bp_" + ChainChoppaFx);
            Override(ability, "CooldownRounds", 0);
            Override(ability, "Hidden", true);
            Override(ability, "HiddenInUI", true);
            return ability;
        }
        private static JObject ChainSelfDamage(int i)
        {
            JObject damage = ChainAction("bdc93cbacbdc05843a933659f15c1302, ContextActionDealDamage", "kickback");
            damage["DamageType"] = new JObject { ["Type"] = "Rending" };
            damage["UseDiceForDamage"] = false;
            damage["MinDamage"] = SimpleValue(ChainDamageMin[i]);
            damage["MaxDamage"] = SimpleValue(ChainDamageMax[i]);
            damage["BonusDamage"] = SimpleValue(0);
            damage["Value"] = new JObject { ["DiceType"] = "Zero", ["DiceCountValue"] = SimpleValue(0), ["BonusValue"] = SimpleValue(0) };
            damage["Penetration"] = damage["Value"].DeepClone();
            damage["Half"] = true;
            damage["HasRighteousFury"] = false;
            damage["DoNotUseCrModifier"] = true;
            damage["DoNotUseCasterModifiers"] = true;
            damage["m_Weapon"] = null;
            JObject caster = ChainAction("5f950c181b3157a4486fcd36b702b702, ContextActionOnContextCaster", "self");
            caster["Actions"] = ChainActions(damage);
            return caster;
        }
        private static void GenerateChainChoppaAbilities()
        {
            JObject jam = PrepareClone(Load(BerserkBuffPrototype), ChainJam, BerserkBuffPrototype);
            Override(jam, "Components", new JArray());
            Override(jam, "Stacking", "Replace");
            Override(jam, "Initiative", "ByCaster");
            SetAbilityText(jam, "chain-jam-name", "chain-jam-description");
            SetAbilityIcon(jam, ThunderHammerAoeIconGuid);
            SaveBlueprint("ChainChoppa_Jammed_Buff", jam);

            JObject noParry = PrepareClone(Load(BerserkBuffPrototype), ChainNoParry, BerserkBuffPrototype);
            JObject parry = CloneComponent(ParryModifierReference, "WarhammerParryChanceModifierDefender", "$WarhammerParryChanceModifierDefender$chain");
            parry["Restrictions"]["Property"]["Getters"] = new JArray();
            parry["m_AlwaysParry"] = false;
            parry["m_NeverParry"] = true;
            Override(noParry, "Components", new JArray(parry));
            Override(noParry, "Stacking", "Replace");
            Override(noParry, "Initiative", "ByCaster");
            SetAbilityText(noParry, "chain-no-parry-name", "chain-no-parry-description");
            SetAbilityIcon(noParry, KlawSqueezeIconGuid);
            SaveBlueprint("ChainChoppa_NoParry_Buff", noParry);

            for (int i = 0; i < 6; i++)
            {
                int tier = i + 1;
                JObject boost = ChainHit(i, "boost", 1.5f);
                SaveBlueprint($"ChainChoppa_V{tier}_Boost_Hit", boost);
                JObject chain = ChainWrapper(tier, "chain", 2, 2, false);
                ChainRun(chain, ChainRandom((20, new[] { ChainSelfDamage(i) }),
                    (80, new[] { ChainCast(boost["AssetId"].ToString()) })));
                SaveBlueprint($"ChainChoppa_V{tier}_Chain_Ability", chain);
                if (tier >= 3)
                {
                    // A finite graph of three possible follow-ups: no recursion and no
                    // extra AP charges. A miss stops the chain; each hit can jam it.
                    for (int j = 3; j >= 1; j--)
                    {
                        JObject extra = ChainHit(i, "extra" + j, .35f);
                        JObject[] next = j < 3 ? new[] { ChainCast(ChainId($"v{tier}-extra{j + 1}-hit")) } : Array.Empty<JObject>();
                        ChainRun(extra, ChainOnHit(ChainRandom((25, new[] { ChainBuff(ChainNoParry) }), (75, next))));
                        SaveBlueprint($"ChainChoppa_V{tier}_Extra{j}_Hit", extra);
                    }
                    JObject first = ChainHit(i, "bite", 1f);
                    ChainRun(first, ChainOnHit(ChainCast(ChainId($"v{tier}-extra1-hit"))));
                    SaveBlueprint($"ChainChoppa_V{tier}_Bite_Hit", first);
                    JObject bite = ChainWrapper(tier, "bite", 2, 3, false);
                    ChainRun(bite, ChainCast(first["AssetId"].ToString()));
                    SaveBlueprint($"ChainChoppa_V{tier}_Bite_Ability", bite);
                }
                if (tier >= 5)
                {
                    JObject sweep = ChainHit(i, "red", 1.5f, true);
                    SaveBlueprint($"ChainChoppa_V{tier}_Red_Hit", sweep);
                    JObject red = ChainWrapper(tier, "red", 3, 4, true);
                    ApplyBladeDancePresentation(red);
                    Override(red, "m_FXSettings", "!bp_" + ChainChoppaFx);
                    ChainRun(red, ChainCast(sweep["AssetId"].ToString()),
                        ChainRandom((1, Array.Empty<JObject>()), (1, new[] { ChainBuff(ChainJam) }),
                            (1, new[] { ChainBuff(ProneBuffGuid) })));
                    SaveBlueprint($"ChainChoppa_V{tier}_Red_Ability", red);
                }
            }
        }
    }
}
