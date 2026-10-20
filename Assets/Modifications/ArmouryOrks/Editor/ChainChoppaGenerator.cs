using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Kingmaker.View.Equipment;

namespace ArmouryOrks.Editor
{
    internal static partial class ChoppaGenerator
    {
        internal const string ChainChoppaWeaponGuid = "fabce84f9db74c7b8fdb024b191491be";
        internal static readonly string[] ChainChoppaGuids = Enumerable.Range(1, 6)
            .Select(tier => tier == 1 ? ChainChoppaWeaponGuid : ChainId("v" + tier)).ToArray();
        private static readonly int[] ChainDamageMin = { 8, 10, 18, 22, 27, 34 };
        private static readonly int[] ChainDamageMax = { 13, 16, 27, 33, 41, 50 };
        private static readonly int[] ChainPenetration = { 20, 20, 25, 25, 30, 35 };
        private static readonly string[] ChainNames =
        {
            "Chain Choppa", "Kustom Chain Choppa", "'Urty Chain Choppa",
            "Killy Chain Choppa", "Dead Killy Chain Choppa", "Da Loudest Choppa"
        };
        private static string ChainId(string key)
        {
            using var hash = MD5.Create();
            return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes("ArmouryOrks/ChainChoppa/" + key))
                .Select(value => value.ToString("x2")));
        }
        private const string ChainswordPrototype = "d1fcb447486c4163b178e248f9ebc929";
        private const string ChainChoppaFx = "046cf83ca27244998b0603750d4a833e";
        private const string ChainChoppaPrefabPath = Art + "/ChainChoppa.prefab";
        private const string ChainChoppaIconPath = Art + "/ChainChoppa_InventoryIcon.png";

        private static void GenerateChainChoppaArt()
        {
            string path = Art + "/ChainChoppa.fbx";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new InvalidDataException("Chain Choppa FBX is missing.");
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importAnimation = false;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.SaveAndReimport();

            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject root = new GameObject("ChainChoppa_Root");
            try
            {
                ConfigureHolsterOffsets(root.AddComponent<EquipmentOffsets>());
                AddComponentByName(root, "FxLocatorMapper", false);
                AddComponentByName(root, "ArmouryOrks.Runtime.OrkWeaponMaterialBinder", true);
                GameObject model = UnityEngine.Object.Instantiate(fbx);
                model.name = "ChainChoppa_FBX_Model";
                // This FBX is authored in metres with the grip at zero. Retain Unity's
                // imported basis/unit conversion instead of applying the older source scale.
                model.transform.SetParent(root.transform, false);
                Material material = CreateChainChoppaMaterial();
                Bounds bounds = new Bounds();
                bool first = true;
                int triangles = 0;
                foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.sharedMaterial = material;
                    if (first) { bounds = renderer.bounds; first = false; }
                    else bounds.Encapsulate(renderer.bounds);
                }
                foreach (MeshFilter filter in model.GetComponentsInChildren<MeshFilter>(true))
                    for (int submesh = 0; submesh < filter.sharedMesh.subMeshCount; submesh++)
                        triangles += (int)filter.sharedMesh.GetIndexCount(submesh) / 3;
                if (triangles != 60000 || Mathf.Abs(bounds.size.y - 1.04f) > 0.01f)
                    throw new InvalidDataException($"Chain Choppa import mismatch: {triangles} triangles, bounds {bounds}.");
                Debug.Log($"[ArmouryOrks] Chain Choppa: {triangles} triangles; bounds {bounds}; grip origin (0,0,0).");
                PrefabUtility.SaveAsPrefabAsset(root, ChainChoppaPrefabPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            AssetDatabase.SaveAssets();
        }

        private static Material CreateChainChoppaMaterial()
        {
            string basePath = Art + "/ChainChoppa_BaseColor.png";
            AssetDatabase.ImportAsset(basePath, ImportAssetOptions.ForceSynchronousImport);
            Texture2D baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(basePath);
            if (baseColor == null) throw new InvalidDataException("Chain Choppa base colour is missing.");
            Texture2D metallic = LoadPng(Art + "/ChainChoppa_Metallic.png", true);
            Texture2D roughness = LoadPng(Art + "/ChainChoppa_Roughness.png", true);
            Texture2D sourceNormal = LoadPng(Art + "/ChainChoppa_Normal_Source.png", true);
            Texture2D mask = PackOwlcatMasks(metallic, roughness,
                Art + "/ChainChoppa_Masks.asset", "ChainChoppa_Masks");
            Texture2D normal = PackUnityNormal(sourceNormal,
                Art + "/ChainChoppa_Normal.asset", "ChainChoppa_Normal");
            UnityEngine.Object.DestroyImmediate(metallic);
            UnityEngine.Object.DestroyImmediate(roughness);
            UnityEngine.Object.DestroyImmediate(sourceNormal);
            Shader shader = Shader.Find("Owlcat/Lit");
            if (shader == null) throw new InvalidOperationException("Owlcat/Lit is unavailable.");
            string path = Art + "/ChainChoppa.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = "ChainChoppa" };
                AssetDatabase.CreateAsset(material, path);
            }
            else material.shader = shader;
            SetTexture(material, baseColor, "_BaseMap", "_BaseColorMap", "_MainTex");
            SetTexture(material, mask, "_MasksMap");
            SetTexture(material, normal, "_BumpMap", "_NormalMap");
            SetFloat(material, 0.9f, "_Metallic");
            SetFloat(material, 0.62f, "_Roughness");
            SetColor(material, Color.white, "_BaseColor", "_Color", "_AdditionalAlbedoColor");
            material.DisableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_MASKSMAP");
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void GenerateChainChoppaBlueprint()
        {
            UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(ChainChoppaPrefabPath);
            if (prefab == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string prefabGuid, out long prefabId))
                throw new InvalidDataException("Chain Choppa prefab cannot be resolved.");
            UnityEngine.Object icon = PrepareInventoryIcon(ChainChoppaIconPath, "Chain Choppa");
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(icon, out string iconGuid, out long iconId))
                throw new InvalidDataException("Chain Choppa icon cannot be resolved.");
            GenerateChainChoppaAbilities();
            for (int i = 0; i < 6; i++)
            {
                int tier = i + 1;
                JObject weapon = PrepareClone(Load(ChainswordPrototype), ChainChoppaGuids[i], ChainswordPrototype);
                SetText(weapon, $"chain-choppa-v{tier}-name", $"chain-choppa-v{tier}-description", $"chain-choppa-v{tier}-flavor");
                Override(weapon, "m_Icon", UnityReference(iconGuid, iconId));
                JObject visual = (JObject)weapon["Data"]["m_VisualParameters"];
                visual["m_WeaponModel"] = UnityReference(prefabGuid, prefabId);
                visual["m_WeaponBeltModelOverride"] = null;
                visual["m_WeaponSheathModelOverride"] = null;
                AddOverride(weapon, "m_VisualParameters");
                Override(weapon, "WarhammerDamage", ChainDamageMin[i]);
                Override(weapon, "WarhammerMaxDamage", ChainDamageMax[i]);
                Override(weapon, "WarhammerPenetration", ChainPenetration[i]);
                Override(weapon, "ItemLevel", tier == 6 ? 55 : i * 10 + 9);
                Override(weapon, "m_Rarity", tier <= 2 ? "Pattern" : "Unique");
                Override(weapon, "m_Enchantments", new JArray());
                Override(weapon, "CanBeUsedInGame", true);
                Override(weapon, "IsUnlootable", false);
                Override(weapon, "IsNonRemovable", false);
                Override(weapon, "m_IsNotable", true);
                // Keep native strike/cleave, including chain sounds, FX and AOO.
                SetAbilitySlot(weapon, "Ability3", "SingleShot", ChainId($"v{tier}-chain"), ChainChoppaFx, 2);
                if (tier >= 3) SetAbilitySlot(weapon, "Ability4", "SingleShot", ChainId($"v{tier}-bite"), ChainChoppaFx, 2);
                else ClearAbilitySlot(weapon, "Ability4");
                if (tier >= 5) SetAbilitySlot(weapon, "Ability5", "Custom", ChainId($"v{tier}-red"), ChainChoppaFx, 3);
                else ClearAbilitySlot(weapon, "Ability5");
                SaveBlueprint(tier == 1 ? "ChainChoppa_Item" : $"ChainChoppa_V{tier}_Item", weapon);
            }
        }
    }
}
