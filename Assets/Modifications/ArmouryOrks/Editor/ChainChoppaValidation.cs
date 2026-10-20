using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace ArmouryOrks.Editor
{
    internal static partial class ChoppaGenerator
    {
        // Exercise the real native modifier implementation, not a duplicated formula.
        private static void ValidateChainCombatContracts()
        {
            if (AccessTools.PropertyGetter(typeof(ContextAction), "Context") == null ||
                AccessTools.PropertyGetter(typeof(RuleCalculateDamage), "InitiatorWeaponStatsRule") == null)
                throw new InvalidOperationException("Required Chain Choppa combat context API is unavailable.");
            foreach (int value in new[] { 100, 450 })
            {
                var modifiers = new CompositeModifiersManager();
                modifiers.Add(ModifierType.PctMul, 150, ModifierDescriptor.None);
                if (modifiers.Apply(value, null) != value * 3 / 2)
                    throw new InvalidOperationException("Native 150% damage multiplier contract failed.");
            }
            var combined = new CompositeModifiersManager();
            combined.Add(ModifierType.ValAdd, 20, ModifierDescriptor.None);
            combined.Add(ModifierType.PctMul, 150, ModifierDescriptor.None);
            if (combined.Apply(100, null) != 180)
                throw new InvalidOperationException("Native weapon bonus and damage multiplier ordering changed.");
            var extra = new CompositeModifiersManager();
            extra.Add(ModifierType.PctMul, 35, ModifierDescriptor.None);
            if (extra.Apply(400, null) != 140)
                throw new InvalidOperationException("Native follow-up damage multiplier contract failed.");
            Type runtime = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType("ArmouryOrks.Runtime.ChainChoppaCombat"))
                .FirstOrDefault(type => type != null);
            var map = runtime?.GetField("HitPercent", BindingFlags.NonPublic | BindingFlags.Static)
                ?.GetValue(null) as Dictionary<string, int>;
            if (map == null || map.Count != 24) throw new InvalidOperationException("Chain combat stage map is incomplete.");
            for (int tier = 1; tier <= 6; tier++)
            {
                if (map[ChainId($"v{tier}-boost-hit")] != 150) throw new InvalidOperationException("Boost damage mapping mismatch.");
                if (map.ContainsKey(tier == 1 ? ChainChoppaWeaponGuid : ChainId("v" + tier)))
                    throw new InvalidOperationException("A normal weapon was accidentally assigned a special multiplier.");
                if (tier >= 3)
                    for (int j = 1; j <= 3; j++)
                        if (map[ChainId($"v{tier}-extra{j}-hit")] != 35) throw new InvalidOperationException("Follow-up damage mapping mismatch.");
            }
            MethodInfo defer = runtime.GetMethod("DeferCrushingAssaultEnd", BindingFlags.NonPublic | BindingFlags.Static);
            if (defer == null) throw new InvalidOperationException("Crushing Assault consumption guard is missing.");
            const string crushing = "5cf53b9f8ee5421a9fa6181f257e3d11";
            for (int tier = 1; tier <= 6; tier++)
            {
                string weapon = ChainChoppaGuids[tier - 1];
                foreach (string key in new[] { "chain", "bite", "red" })
                {
                    bool unlocked = key == "chain" || (key == "bite" ? tier >= 3 : tier >= 5);
                    bool actual = (bool)defer.Invoke(null, new object[] { crushing, ChainId($"v{tier}-{key}"), weapon });
                    if (actual != unlocked) throw new InvalidOperationException("Crushing Assault wrapper consumption mismatch.");
                }
                foreach (string key in new[] { "boost-hit", "bite-hit", "red-hit", "extra1-hit", "extra2-hit", "extra3-hit" })
                    if ((bool)defer.Invoke(null, new object[] { crushing, ChainId($"v{tier}-{key}"), weapon }))
                        throw new InvalidOperationException("Actual child attack must retain native Crushing Assault consumption.");
                foreach (object[] args in new[] {
                    new object[] { "unrelated-buff", ChainId($"v{tier}-chain"), weapon },
                    new object[] { crushing, "9dec1bdade284190b0977f5f70d26d3e", weapon },
                    new object[] { crushing, ChainId($"v{tier}-chain"), "unrelated-weapon" },
                    new object[] { crushing, null, weapon },
                    new object[] { crushing, ChainId($"v{tier}-chain"), null } })
                    if ((bool)defer.Invoke(null, args)) throw new InvalidOperationException("Crushing Assault guard exceeds Chain wrapper scope.");
            }
            Debug.Log("[ArmouryOrks] Crushing Assault contracts passed: 12 wrappers defer, actual attacks consume, unrelated facts/weapons unaffected.");
            Debug.Log("[ArmouryOrks] Chain combat contracts passed: native modifiers, API access and all 24 child attack stages.");
        }
    }
}
