using System;
using System.IO;
using Kingmaker.Localization;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace ArmouryOrks.Editor
{
    internal static partial class ChoppaGenerator
    {
        private const string OrkCacheGuid = "f73bd67747f24b429b96c138ea280a63";
        private const string OrkCachePrefabPath = Art + "/OrkArmouryCache.prefab";

        private static void GenerateArmouryCache()
        {
            GameObject root = new GameObject("OrkArmouryCache");
            try
            {
                var view = root.AddComponent<DynamicMapObjectView>();
                var viewData = new SerializedObject(view);
                viewData.FindProperty("UniqueId").stringValue = "armoury-orks-warrant-cache";
                viewData.FindProperty("m_IsInGame").boolValue = true;
                viewData.ApplyModifiedPropertiesWithoutUndo();
                var loot = root.AddComponent<InteractionLoot>();
                var data = new SerializedObject(loot);
                SerializedProperty settings = data.FindProperty("Settings");
                settings.FindPropertyRelative("NotInCombat").boolValue = true;
                settings.FindPropertyRelative("ShowOvertip").boolValue = true;
                settings.FindPropertyRelative("ShowHighlight").boolValue = true;
                settings.FindPropertyRelative("ProximityRadius").intValue = 2;
                settings.FindPropertyRelative("Type").intValue = 1;
                settings.FindPropertyRelative("LootContainerType").intValue = (int)LootContainerType.Chest;
                settings.FindPropertyRelative("DestroyWhenEmpty").boolValue = false;
                settings.FindPropertyRelative("AddMapMarker").boolValue = true;
                settings.FindPropertyRelative("ShowOnMapWhenEmpty").boolValue = false;
                settings.FindPropertyRelative("MapMarkerName").objectReferenceValue = CacheString("Name", "ork-cache-name");
                settings.FindPropertyRelative("Description").objectReferenceValue = CacheString("Description", "ork-cache-description");
                data.ApplyModifiedPropertiesWithoutUndo();
                var collider = root.AddComponent<BoxCollider>();
                AddComponentByName(root, "ArmouryOrks.Runtime.OrkWeaponMaterialBinder", true);
                AddReducedChestModel(root, collider);
                PrefabUtility.SaveAsPrefabAsset(root, OrkCachePrefabPath);
                AssetDatabase.SaveAssets();
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(OrkCachePrefabPath);
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string guid, out long id))
                    throw new InvalidDataException("Ork cache prefab reference cannot be resolved.");
                SaveBlueprint("OrkArmouryCache_MapObject", new JObject
                {
                    ["AssetId"] = OrkCacheGuid,
                    ["Data"] = new JObject
                    {
                        ["$type"] = "b7d695389bff6604ca37f190a61f05b5, BlueprintDynamicMapObject",
                        ["PrototypeLink"] = "", ["m_Overrides"] = new JArray(), ["Components"] = new JArray(),
                        ["Author"] = "Poffl", ["Comment"] = "Persistent Ork armoury cache in the Warrant Chamber.",
                        ["m_DisplayName"] = Localized("ork-cache-name"),
                        ["m_Description"] = Localized("ork-cache-description"), ["m_Icon"] = null,
                        ["Prefab"] = UnityReference(guid,id)
                    },
                    ["Meta"] = new JObject { ["ShadowDeleted"] = false }
                });
                Debug.Log("[ArmouryOrks] Native Ork cache prefab generated: saved map entity, loot interaction, collider, independent geometry/materials.");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static SharedStringAsset CacheString(string suffix, string key)
        {
            string path = Art + "/OrkArmouryCache_" + suffix + ".asset";
            var text = AssetDatabase.LoadAssetAtPath<SharedStringAsset>(path);
            if (text == null)
            {
                text = ScriptableObject.CreateInstance<SharedStringAsset>();
                AssetDatabase.CreateAsset(text,path);
            }
            var data = new SerializedObject(text);
            data.FindProperty("String").FindPropertyRelative("m_Key").stringValue = key;
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(text);
            return text;
        }
        private static void AddReducedChestModel(GameObject root, BoxCollider collider)
        {
            const string modelPath = Art + "/OrkChest.fbx";
            AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null) throw new InvalidDataException("Reduced Meshy chest FBX is missing.");
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.SaveAndReimport();
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            GameObject model = UnityEngine.Object.Instantiate(source);
            model.name = "OrkChest_Meshy_40k";
            model.transform.SetParent(root.transform, false);
            // Blender's -Y front imports as Unity +Z; retain the old cache's -Z front.
            model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f) * model.transform.localRotation;
            Material material = CreateOrkChestMaterial();
            Bounds bounds = new Bounds();
            bool first = true;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                renderer.sharedMaterial = material;
                if (first) { bounds = renderer.bounds; first = false; }
                else bounds.Encapsulate(renderer.bounds);
            }
            int triangles = 0;
            foreach (MeshFilter filter in model.GetComponentsInChildren<MeshFilter>(true))
                for (int submesh = 0; submesh < filter.sharedMesh.subMeshCount; submesh++)
                    triangles += (int)filter.sharedMesh.GetIndexCount(submesh) / 3;
            if (first || Mathf.Abs(triangles - 40000) > 2 || Mathf.Abs(bounds.size.x - 1.02f) > .01f ||
                Mathf.Abs(bounds.min.y) > .01f || bounds.size.y < .5f || bounds.size.y > .7f)
                throw new InvalidDataException($"Ork chest import mismatch: {triangles} triangles, bounds {bounds}.");
            collider.center = bounds.center;
            collider.size = bounds.size + new Vector3(.02f, .02f, .02f);
            Debug.Log($"[ArmouryOrks] Reduced Meshy chest: {triangles} triangles, bounds {bounds}; native saved cache identity retained.");
        }

        private static Material CreateOrkChestMaterial()
        {
            const string path = Art + "/OrkChest.mat";
            const string basePath = Art + "/OrkChest_BaseColor.png";
            AssetDatabase.ImportAsset(basePath, ImportAssetOptions.ForceSynchronousImport);
            var baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(basePath);
            if (baseColor == null) throw new InvalidDataException("Ork chest base colour is missing.");
            Texture2D metallic = LoadPng(Art + "/OrkChest_Metallic.png", true);
            Texture2D roughness = LoadPng(Art + "/OrkChest_Roughness.png", true);
            Texture2D normalSource = LoadPng(Art + "/OrkChest_Normal_Source.png", true);
            Texture2D mask = PackOwlcatMasks(metallic, roughness, Art + "/OrkChest_Masks.asset", "OrkChest_Masks");
            Texture2D normal = PackUnityNormal(normalSource, Art + "/OrkChest_Normal.asset", "OrkChest_Normal");
            UnityEngine.Object.DestroyImmediate(metallic);
            UnityEngine.Object.DestroyImmediate(roughness);
            UnityEngine.Object.DestroyImmediate(normalSource);
            Shader shader = Shader.Find("Owlcat/Lit");
            if (shader == null) throw new InvalidOperationException("Ork chest shader unavailable.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = "OrkChest" };
                AssetDatabase.CreateAsset(material, path);
            }
            else material.shader = shader;
            SetTexture(material, baseColor, "_BaseMap", "_BaseColorMap", "_MainTex");
            SetTexture(material, mask, "_MasksMap");
            SetTexture(material, normal, "_BumpMap", "_NormalMap");
            SetFloat(material, .9f, "_Metallic");
            SetFloat(material, .7f, "_Roughness");
            SetColor(material, Color.white, "_BaseColor", "_Color", "_AdditionalAlbedoColor");
            material.DisableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_MASKSMAP");
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
