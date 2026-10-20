using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace ArmouryOrks.Runtime
{
    /// <summary>One persistent, finite armoury cache per campaign, using native saved map entities.</summary>
    public sealed class OrkArmouryCacheController : MonoBehaviour
    {
        private const string TargetAreaGuid = "8a2d1ed55f694366b2d512e122bd19a7";
        public const string CacheBlueprintGuid = "f73bd67747f24b429b96c138ea280a63";
        private static readonly Vector3 CachePosition = new Vector3(118.604f, 2.419f, -203.103f);
        private static readonly Quaternion CacheRotation = Quaternion.Euler(0f, 35f, 0f);
        private static readonly string[] TierOneItemGuids =
        {
            "9f1a0cab53be493b96989c9a672d2ae5", // Choppa
            "f842bd6192ec4f9d91eb83278470c671", // Big Choppa
            "89d3b9e7c7134f7f94c36518212b8596", // Power Klaw
            "fabce84f9db74c7b8fdb024b191491be"  // Chain Choppa
        };
        public static bool Enabled { get; set; } = true;
        private float nextCheck;
        private bool reportedFailure;

        public static void EnsureController()
        {
            if (FindFirstObjectByType<OrkArmouryCacheController>() != null) return;
            var host = new GameObject("ArmouryOrks_ArmouryCacheController");
            DontDestroyOnLoad(host);
            host.AddComponent<OrkArmouryCacheController>();
        }

        private void Update()
        {
            if (!Enabled || Time.unscaledTime < nextCheck) return;
            nextCheck = Time.unscaledTime + 1f;
            try { TrySpawnCache(); }
            catch (Exception exception)
            {
                if (!reportedFailure) Debug.LogError("[ArmouryOrks] Cannot prepare armoury cache: " + exception);
                reportedFailure = true;
                nextCheck = Time.unscaledTime + 10f;
            }
        }

        private static void TrySpawnCache()
        {
            Game game = Game.Instance;
            AreaPersistentState area = game?.LoadedAreaState;
            if (game?.Player == null || game.Player.IsInCombat || area == null ||
                !string.Equals(game.CurrentlyLoadedArea?.AssetGuid.ToString(), TargetAreaGuid, StringComparison.OrdinalIgnoreCase)) return;
            // Check saved entities, not just visible objects: an emptied or unloaded
            // cache must never be replaced/refilled after loading or area transitions.
            if (area.AllEntityData.OfType<DynamicMapObjectView.EntityData>()
                .Any(entity => entity.Blueprint?.AssetGuid.ToString() == CacheBlueprintGuid)) return;
            SceneEntitiesState scene = area.GetAllSceneStates().FirstOrDefault(state => state != null && state.IsSceneLoaded &&
                state.SceneName.StartsWith("VoidshipOfficersDeck", StringComparison.OrdinalIgnoreCase));
            if (scene == null) return;
            var blueprint = ResourcesLibrary.TryGetBlueprint<BlueprintDynamicMapObject>(CacheBlueprintGuid);
            var items = TierOneItemGuids.Select(guid => ResourcesLibrary.TryGetBlueprint<BlueprintItem>(guid)).ToArray();
            // Resolve everything before creating a saved entity, avoiding a partially
            // filled cache when blueprint dependencies are still loading.
            if (blueprint == null || items.Any(item => item == null)) return;
            var entityData = game.EntitySpawner.SpawnMapObject(blueprint, CachePosition, CacheRotation, scene);
            var loot = entityData?.GetOptional<InteractionLootPart>();
            if (loot == null) throw new InvalidOperationException("Ork cache has no native InteractionLootPart.");
            foreach (BlueprintItem item in items) loot.Loot.Add(item);
            Debug.Log("[ArmouryOrks] Persistent Ork armoury cache spawned with four weapons in the Warrant Chamber.");
        }
    }
}
