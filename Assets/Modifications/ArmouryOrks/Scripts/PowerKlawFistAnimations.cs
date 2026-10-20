using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Kingmaker.BundlesLoading;
using Kingmaker.Items;
using Kingmaker.View.Animation;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;
using HandAttack = Kingmaker.Visual.Animation.Kingmaker.Actions.WarhammerUnitAnimationActionHandAttack;

namespace ArmouryOrks.Runtime
{
    // Supply the missing human Fist mapping only for our weapon. Keep the original
    // wrappers (including Act/damage and sound tracks), rather than cloning clips.
    public static class PowerKlawFistAnimations
    {
        private static readonly HashSet<string> WeaponGuids = new HashSet<string>
        {
            "89d3b9e7c7134f7f94c36518212b8596",
            "9ffb3db35be843a1a18cfcac2d90581a",
            "a7e3c400849846d1b5436136acc15319",
            "601279edb9554f70b608c135fc44cfbf",
            "9876491339ca4a60bd0419ceac6c80ec",
            "218bd6f72ed845768d0e9a52e5d2073f"
        };
        private static HandAttack.ClipSettings[] mainHand, offHand;
        private static bool installed, reportedFailure;
        public static bool Enabled { get; set; }

        public static void Initialize()
        {
            Enabled = true;
            if (installed) return;
            var harmony = new Harmony("ArmouryOrks.PowerKlaw.FistAnimations");
            harmony.CreateClassProcessor(typeof(ClipPatch)).Patch();
            harmony.CreateClassProcessor(typeof(CountPatch)).Patch();
            installed = true;
        }

        private static bool Applies(HandAttack action, UnitAnimationActionHandle handle)
        {
            if (!Enabled || handle == null || handle.AttackWeaponStyle != WeaponAnimationStyle.Fist)
                return false;
            var owner = handle.Unit?.EntityData as PartUnitBody.IOwner;
            var hand = action.IsMainHand ? owner?.Body.PrimaryHand : owner?.Body.SecondaryHand;
            string guid = hand?.MaybeWeapon?.Blueprint.AssetGuid.ToString();
            return guid != null && WeaponGuids.Contains(guid);
        }

        private static HandAttack.ClipSettings[] Clips(bool isMainHand)
        {
            if (mainHand != null && offHand != null) return isMainHand ? mainHand : offHand;
            try
            {
                // Request through Owlcat's service so its reference count keeps the bundle alive.
                var bundle = BundlesLoadService.Instance.RequestBundle("humananimation.animations");
                if (bundle == null) throw new InvalidOperationException("Human animation bundle unavailable.");
                var wrappers = bundle.LoadAllAssets<AnimationClipWrapper>();
                mainHand = Resolve(wrappers, "MHA");
                offHand = Resolve(wrappers, "OHA");
                Debug.Log("[ArmouryOrks] Power Klaw: original human fist attacks loaded (3 main-hand, 3 off-hand).");
                return isMainHand ? mainHand : offHand;
            }
            catch (Exception e)
            {
                mainHand = offHand = null;
                if (!reportedFailure) Debug.LogError("[ArmouryOrks] Cannot load Power Klaw fist animations: " + e);
                reportedFailure = true;
                return null;
            }
        }

        private static HandAttack.ClipSettings[] Resolve(AnimationClipWrapper[] wrappers, string hand)
        {
            return Enumerable.Range(1, 3).Select(i =>
            {
                var name = "1H_FreeHand_" + hand + "_Attack_v" + i;
                var wrapper = wrappers.FirstOrDefault(w => w.name == name);
                if (wrapper == null || wrapper.AnimationClip == null || !wrapper.EventTracks.Any())
                    throw new InvalidOperationException("Missing fist clip/event track: " + name);
                return new HandAttack.ClipSettings { ClipWrapper = wrapper };
            }).ToArray();
        }

        [HarmonyPatch]
        private static class ClipPatch
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                foreach (var name in new[] { "GetAttackVariant", "GetReadyVariant", "GetUnreadyVariant" })
                    yield return AccessTools.Method(typeof(HandAttack), name);
            }
            private static bool Prefix(HandAttack __instance, UnitAnimationActionHandle handle,
                MethodBase __originalMethod, object[] __args, ref HandAttack.ClipSettings __result)
            {
                if (!Applies(__instance, handle)) return true;
                var clips = Clips(__instance.IsMainHand);
                if (clips == null) return true;
                // Each original punch includes its wind-up and recovery. Extra ready/unready
                // clips would repeat the attack and its Act event.
                __result = __originalMethod.Name == "GetAttackVariant"
                    ? clips[((int)__args[1] < 0 ? UnityEngine.Random.Range(0, clips.Length) : (int)__args[1]) % clips.Length]
                    : null;
                return false;
            }
        }

        [HarmonyPatch(typeof(HandAttack), "GetVariantsCount")]
        private static class CountPatch
        {
            private static bool Prefix(HandAttack __instance, UnitAnimationActionHandle handle, ref int __result)
            {
                if (!Applies(__instance, handle)) return true;
                var clips = Clips(__instance.IsMainHand);
                if (clips == null) return true;
                __result = clips.Length;
                return false;
            }
        }
    }
}
