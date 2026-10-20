using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace ArmouryOrks.Runtime
{
    /// <summary>Replaces older Ork weapon tiers outside combat according to party level.</summary>
    public sealed class ChoppaProgressionController : MonoBehaviour,
        ILevelUpCompleteUIHandler, IItemsCollectionHandler, IPartyCombatHandler
    {
        private static readonly string[] ChoppaGuids =
        {
            "9f1a0cab53be493b96989c9a672d2ae5", "78f71e9a52ab4f8e9a46bd2390860ec2",
            "ed79a91f997e45c6bd68e154aca4884c", "3b41ba583ab84c01bed6b8ea0fa31229",
            "833093446bd84d6ea45fb862d3a9a027", "c20a56ca164f46a480704c4b348f381f"
        };
        private static readonly string[] BigChoppaGuids =
        {
            "f842bd6192ec4f9d91eb83278470c671", "a0f30782b1454053920b101af9e10908",
            "9bd81c627e58489ba2495eeef5df8f77", "9afbdac806004903ab8c370d1c94b208",
            "1a1602ca54e04a0b8231434b100dd175", "43d65e58f04c4ea580076b00f89b7ab0"
        };

        private static readonly string[] PowerKlawGuids =
        {
            "89d3b9e7c7134f7f94c36518212b8596", "9ffb3db35be843a1a18cfcac2d90581a",
            "a7e3c400849846d1b5436136acc15319", "601279edb9554f70b608c135fc44cfbf",
            "9876491339ca4a60bd0419ceac6c80ec", "218bd6f72ed845768d0e9a52e5d2073f"
        };

        private static readonly string[] ChainChoppaGuids =
        {
            "fabce84f9db74c7b8fdb024b191491be",
            "72f312ef3be22fb1f5b0627abd773ae9",
            "843635dbc15aca212327825426b20ed2",
            "584c60c7745d002e85a66aba123b701e",
            "cf485f64f9cf98bb894f26e586387bcc",
            "32cc23dc06f570f61c025bc2888fc4f0",
        };

        private static readonly MethodInfo SubscribeGlobalMethod = typeof(EventBus).GetMethod(
            "SubscribeGlobal", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo UnsubscribeGlobalMethod = typeof(EventBus).GetMethod(
            "UnsubscribeGlobal", BindingFlags.Static | BindingFlags.NonPublic);
        private readonly HashSet<ItemsCollection> m_KnownCollections = new HashSet<ItemsCollection>();
        private float m_NextCheck;
        private bool m_ChangingItems;
        private bool m_DeferredByCombat;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void EnsureController()
        {
            if (FindFirstObjectByType<ChoppaProgressionController>() != null) return;
            GameObject host = new GameObject("ArmouryOrks_ChoppaProgression");
            DontDestroyOnLoad(host);
            host.AddComponent<ChoppaProgressionController>();
        }

        private void OnEnable() => SubscribeGlobalMethod?.Invoke(null, new object[] { this, null });
        private void OnDisable() => UnsubscribeGlobalMethod?.Invoke(null, new object[] { this, null });

        private void Update()
        {
            if (Time.unscaledTime < m_NextCheck) return;
            m_NextCheck = Time.unscaledTime + 1f;
            RememberPlayerCollections();
            TryUpgradeAll();
        }

        public void HandleLevelUpComplete(bool isChargen) => TryUpgradeAll();

        public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
        {
            if (collection != null) m_KnownCollections.Add(collection);
            if (!m_ChangingItems) TryUpgradeAll();
        }

        public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
        {
            if (collection != null) m_KnownCollections.Add(collection);
        }

        public void HandlePartyCombatStateChanged(bool inCombat)
        {
            if (!inCombat && m_DeferredByCombat) TryUpgradeAll();
        }

        private void RememberPlayerCollections()
        {
            Player player = Game.Instance?.Player;
            if (player == null) return;
            if (player.Inventory != null) m_KnownCollections.Add(player.Inventory);
            if (player.SharedStash != null) m_KnownCollections.Add(player.SharedStash);
        }

        private void TryUpgradeAll()
        {
            Player player = Game.Instance?.Player;
            if (player == null || m_ChangingItems) return;
            if (IsCombatActive(player))
            {
                m_DeferredByCombat = true;
                return;
            }

            m_DeferredByCombat = false;
            int targetTier = TierForLevel(player.PartyLevel);
            m_ChangingItems = true;
            try
            {
                foreach (ItemsCollection collection in m_KnownCollections.Where(value => value != null).ToArray())
                foreach (ItemEntity item in collection.Items.ToArray())
                    TryUpgradeItem(item, targetTier);

                foreach (BaseUnitEntity unit in EnumeratePartyUnits(player))
                {
                    if (unit.Inventory?.Collection != null) m_KnownCollections.Add(unit.Inventory.Collection);
                    PropertyInfo bodyProperty = unit.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .FirstOrDefault(property => typeof(PartUnitBody).IsAssignableFrom(property.PropertyType));
                    if (bodyProperty?.GetValue(unit) is PartUnitBody body)
                    foreach (ItemEntity item in body.Items.ToArray())
                        TryUpgradeItem(item, targetTier);
                }
            }
            finally { m_ChangingItems = false; }
        }

        private static IEnumerable<BaseUnitEntity> EnumeratePartyUnits(Player player)
        {
            var result = new HashSet<BaseUnitEntity>();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (PropertyInfo property in player.GetType().GetProperties(flags)
                .Where(property => property.Name.IndexOf("Party", StringComparison.OrdinalIgnoreCase) >= 0 && property.GetIndexParameters().Length == 0))
            {
                object value;
                try { value = property.GetValue(player); } catch { continue; }
                AddUnits(value, result);
            }
            foreach (FieldInfo field in player.GetType().GetFields(flags)
                .Where(field => field.Name.IndexOf("Party", StringComparison.OrdinalIgnoreCase) >= 0))
                AddUnits(field.GetValue(player), result);
            return result;
        }

        private static void AddUnits(object value, HashSet<BaseUnitEntity> result)
        {
            if (value is BaseUnitEntity unit) result.Add(unit);
            else if (value is IEnumerable enumerable)
            foreach (object item in enumerable)
                if (item is BaseUnitEntity partyUnit) result.Add(partyUnit);
        }

        private static bool IsCombatActive(Player player) => player.IsInCombat ||
            EnumeratePartyUnits(player).Any(unit => unit != null && unit.IsInCombat);

        private void TryUpgradeItem(ItemEntity item, int targetTier)
        {
            if (item?.Blueprint == null) return;
            string guid = item.Blueprint.AssetGuid.ToString();
            if (Array.IndexOf(ChoppaGuids, guid) >= 0) TryUpgradeItemInSeries(item, targetTier, ChoppaGuids, "Choppa");
            else if (Array.IndexOf(BigChoppaGuids, guid) >= 0) TryUpgradeItemInSeries(item, targetTier, BigChoppaGuids, "Big Choppa");
            else if (Array.IndexOf(ChainChoppaGuids, guid) >= 0) TryUpgradeItemInSeries(item, targetTier, ChainChoppaGuids, "Chain Choppa");
            else if (Array.IndexOf(PowerKlawGuids, guid) >= 0) TryUpgradeItemInSeries(item, targetTier, PowerKlawGuids, "Power Klaw");
        }

        private void TryUpgradeItemInSeries(ItemEntity item, int targetTier, string[] series, string weaponName)
        {
            int currentTier = Array.IndexOf(series, item.Blueprint.AssetGuid.ToString());
            if (currentTier < 0 || currentTier >= targetTier) return;

            BlueprintItem target = ResourcesLibrary.TryGetBlueprint(series[targetTier]) as BlueprintItem;
            if (target == null)
            {
                Debug.LogError($"[ArmouryOrks] {weaponName} tier blueprint was not found: " + series[targetTier]);
                return;
            }

            var slot = item.HoldingSlot;
            ItemsCollection collection = item.Collection ?? slot?.MaybeOwnerInventory?.Collection;
            if (collection == null) return;
            if (slot != null && !slot.RemoveItem(false, false)) return;
            if (item.Collection != null) item.Collection.Remove(item);
            ItemEntity replacement = collection.Add(target);
            if (slot != null) slot.InsertItem(replacement, false);
            Debug.Log($"[ArmouryOrks] {weaponName} upgraded from V{currentTier + 1} to V{targetTier + 1} at party level {Game.Instance.Player.PartyLevel}.");
        }

        private static int TierForLevel(int level)
        {
            if (level >= 50) return 5;
            if (level >= 44) return 4;
            if (level >= 36) return 3;
            if (level >= 26) return 2;
            if (level >= 16) return 1;
            return 0;
        }
    }
}
