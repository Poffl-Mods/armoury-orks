using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace ArmouryOrks.Runtime
{
    /// <summary>Keep Chain Choppa child attacks attached to the weapon actually held.</summary>
    public static class ChainChoppaCombat
    {
        private static readonly Dictionary<string, int> HitPercent = BuildHitPercent();
        private static readonly HashSet<string> Weapons = BuildWeapons();
        private static readonly HashSet<string> Wrappers = BuildWrappers();
        private const string CrushingAssaultBuff = "5cf53b9f8ee5421a9fa6181f257e3d11";

        private static HashSet<string> BuildWrappers()
        {
            var result = new HashSet<string>();
            for (int tier = 1; tier <= 6; tier++)
            {
                result.Add(Id($"v{tier}-chain"));
                if (tier >= 3) result.Add(Id($"v{tier}-bite"));
                if (tier >= 5) result.Add(Id($"v{tier}-red"));
            }
            return result;
        }
        private static bool DeferCrushingAssaultEnd(string fact, string ability, string weapon)
            => fact == CrushingAssaultBuff && ability != null && Wrappers.Contains(ability)
                && weapon != null && Weapons.Contains(weapon);
        private static readonly MethodInfo ContextGetter = AccessTools.PropertyGetter(typeof(ContextAction), "Context");
        private static readonly MethodInfo WeaponStatsGetter = AccessTools.PropertyGetter(typeof(RuleCalculateDamage), "InitiatorWeaponStatsRule");
        private static RuleCalculateStatsWeapon Stats(RuleCalculateDamage rule)
            => WeaponStatsGetter?.Invoke(rule, null) as RuleCalculateStatsWeapon;
        private static bool installed;
        public static bool Enabled { get; set; }

        private static string Id(string key)
        {
            using var hash = MD5.Create();
            return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes("ArmouryOrks/ChainChoppa/" + key))).Replace("-", "").ToLowerInvariant();
        }
        private static HashSet<string> BuildWeapons()
        {
            var result = new HashSet<string> { "fabce84f9db74c7b8fdb024b191491be" };
            for (int tier = 2; tier <= 6; tier++) result.Add(Id("v" + tier));
            return result;
        }
        private static Dictionary<string, int> BuildHitPercent()
        {
            var result = new Dictionary<string, int>();
            for (int tier = 1; tier <= 6; tier++)
            {
                result[Id($"v{tier}-boost-hit")] = 150;
                if (tier >= 3)
                {
                    result[Id($"v{tier}-bite-hit")] = 100;
                    for (int j = 1; j <= 3; j++) result[Id($"v{tier}-extra{j}-hit")] = 35;
                }
                if (tier >= 5) result[Id($"v{tier}-red-hit")] = 150;
            }
            return result;
        }
        public static void Initialize()
        {
            Enabled = true;
            if (installed) return;
            var harmony = new Harmony("ArmouryOrks.ChainChoppa.Combat");
            harmony.CreateClassProcessor(typeof(CastPatch)).Patch();
            harmony.CreateClassProcessor(typeof(DamagePatch)).Patch();
            harmony.CreateClassProcessor(typeof(CrushingAssaultEndPatch)).Patch();
            installed = true;
        }

        [HarmonyPatch(typeof(ContextActionCastSpell), "RunAction")]
        private static class CastPatch
        {
            private static bool Prefix(ContextActionCastSpell __instance)
            {
                string guid = __instance.Spell?.AssetGuid.ToString();
                if (!Enabled || guid == null || !HitPercent.ContainsKey(guid)) return true;
                var context = ContextGetter.Invoke(__instance, null) as MechanicsContext;
                AbilityData parent = context?.SourceAbilityContext?.Ability;
                ItemEntityWeapon weapon = parent?.Weapon;
                if (weapon == null || !Weapons.Contains(weapon.Blueprint.AssetGuid.ToString()))
                {
                    Debug.LogError("[ArmouryOrks] Chain Choppa child attack has no equipped Chain Choppa source: " + guid);
                    return false;
                }
                var child = new AbilityData(__instance.Spell, parent.Caster)
                {
                    OverrideWeapon = weapon,
                    OverrideSourceItem = parent.SourceItem ?? weapon,
                    ItemSlotIndex = parent.ItemSlotIndex,
                    ItemSlotSource = parent.ItemSlotSource,
                    FXSettingsOverride = parent.FXSettingsOverride
                };
                // Match the native immediate/free cast, preserving the actual item and
                // hand. Creating a bare AbilityData loses these in the native action.
                var rule = Rulebook.Trigger(new RulePerformAbility(child, context.MainTarget)
                {
                    IgnoreCooldown = true,
                    ForceFreeAction = true
                });
                rule.Context?.RewindActionIndex(null);
                return false;
            }
        }

        // RulePerformAbility schedules child attacks. The outer ability ends before
        // their delivery, so its melee-weapon context prematurely consumes the buff.
        // Only suppress this buff's end handler for our non-damaging wrappers. The
        // actual attack retains native consumption (one bite, or the whole AoE).
        [HarmonyPatch(typeof(AbilityLifecycleTriggerCaster), "HandleExecutionProcessEnd")]
        private static class CrushingAssaultEndPatch
        {
            private static bool Prefix(AbilityLifecycleTriggerCaster __instance, AbilityExecutionContext __0)
            {
                if (!Enabled) return true;
                AbilityData ability = __0?.Ability;
                return !DeferCrushingAssaultEnd(__instance.Fact?.Blueprint.AssetGuid.ToString(),
                    ability?.Blueprint.AssetGuid.ToString(), ability?.Weapon?.Blueprint.AssetGuid.ToString());
            }
        }

        [HarmonyPatch(typeof(RuleCalculateDamage), "OnTrigger")]
        private static class DamagePatch
        {
            private static void Prefix(RuleCalculateDamage __instance)
            {
                if (!Enabled) return;
                var stats = Stats(__instance);
                string weapon = stats?.Weapon?.Blueprint.AssetGuid.ToString();
                string ability = stats?.Ability?.Blueprint.AssetGuid.ToString();
                if (weapon == null || !Weapons.Contains(weapon) || ability == null) return;
                // A multiplicative factor uses the same real weapon and character
                // modifiers as strike/cleave. Normal attacks receive no extra factor.
                if (HitPercent.TryGetValue(ability, out int percent) && percent != 100)
                    __instance.ValueModifiers.Add(ModifierType.PctMul, percent, __instance, ModifierDescriptor.None);
            }
        }
    }
}
