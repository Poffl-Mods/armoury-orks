using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.View.Equipment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OwlcatModification.Editor;
using OwlcatModification.Editor.Build;
using UnityEditor;
using UnityEngine;

namespace ArmouryOrks.Editor
{
    internal static class ChoppaGenerator
    {
        // Generated assets and the distributable are built from the Unity editor menu.
        internal const string WeaponGuid = "9f1a0cab53be493b96989c9a672d2ae5";

        private const string Root = "Assets/Modifications/ArmouryOrks";
        private const string Art = Root + "/Art";
        private const string Blueprints = Root + "/Blueprints";
        private const string Localization = Root + "/Localization/enGB.json";
        private const string FbxPath = Art + "/Choppa.fbx";
        private const string PrefabPath = Art + "/Choppa.prefab";
        private const string InventoryIconPath = Art + "/Choppa_InventoryIcon.png";
        private const string BaseColorPath = Art + "/Choppa_BaseColor.png";
        private const string MetallicPath = Art + "/Choppa_Metallic.png";
        private const string NormalPath = Art + "/Choppa_Normal_Source.png";
        private const string RoughnessPath = Art + "/Choppa_Roughness.png";
        private const string MaterialPath = Art + "/Choppa.mat";
        private const string PackedMaskPath = Art + "/Choppa_MetallicSmoothness.asset";
        private const string PackedNormalPath = Art + "/Choppa_Normal.asset";

        // Vanilla one-handed primitive axe; uses the BrutalOneHanded animation style.
        private const string WeaponPrototype = "b61740707f6b477f9ed139b22a81d245";

        [MenuItem("Armoury Orks/Build 0.0.4")]
        public static void Build()
        {
            Generate();
            Modification mod = AssetDatabase.LoadAssetAtPath<Modification>(Root + "/ArmouryOrks.asset");
            if (mod == null) throw new InvalidOperationException("ArmouryOrks Modification asset was not found.");
            mod.Manifest.Version = "0.0.4";
            EditorUtility.SetDirty(mod);
            AssetDatabase.SaveAssets();
            var result = Builder.Build(mod);
            if ((int)result != 0) throw new InvalidOperationException("Build failed: " + result);
        }

        [MenuItem("Armoury Orks/Generate Choppa")]
        public static void Generate()
        {
            Directory.CreateDirectory(Art);
            Directory.CreateDirectory(Blueprints);
            Directory.CreateDirectory(Path.GetDirectoryName(Localization));
            GenerateArt();
            GenerateBlueprint();
            WriteLocalization();
            AssetDatabase.Refresh();
            Debug.Log("[ArmouryOrks] Choppa generated. ToyBox name: Choppa; Blueprint: " + WeaponGuid);
        }

        private static void GenerateArt()
        {
            AssetDatabase.ImportAsset(FbxPath, ImportAssetOptions.ForceSynchronousImport);
            ModelImporter importer = AssetImporter.GetAtPath(FbxPath) as ModelImporter;
            if (importer == null) throw new InvalidDataException("Choppa.fbx could not be imported.");
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.SaveAndReimport();

            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            if (fbx == null) throw new InvalidDataException("Choppa FBX asset is missing after import.");
            Material material = CreateMaterial();

            GameObject root = new GameObject("Choppa_Root");
            EquipmentOffsets offsets = root.AddComponent<EquipmentOffsets>();
            ConfigureHolsterOffsets(offsets);
            AddComponentByName(root, "FxLocatorMapper", false);
            AddComponentByName(root, "ArmouryOrks.Runtime.OrkWeaponMaterialBinder", true);

            GameObject model = UnityEngine.Object.Instantiate(fbx);
            model.name = "Choppa_FBX_Model";
            model.transform.SetParent(root.transform, false);
            // Slide the grip section into the character's closed hand.
            model.transform.localPosition = new Vector3(0f, 0.34f, 0f);
            // Turn the model end-for-end so the character grips the handle, not the axe head.
            model.transform.localRotation = Quaternion.AngleAxis(270f, Vector3.up) * Quaternion.Euler(270f, 0f, 0f);
            // 80% of the initial in-game size (68).
            model.transform.localScale = Vector3.one * 54.4f;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = material;

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureHolsterOffsets(EquipmentOffsets offsets)
        {
            SerializedObject serialized = new SerializedObject(offsets);
            SerializedProperty slots = serialized.FindProperty("m_SlotOffsets");
            slots.arraySize = 12;
            for (int i = 0; i < slots.arraySize; i++)
            {
                SerializedProperty slot = slots.GetArrayElementAtIndex(i);
                slot.FindPropertyRelative("Position").vector3Value = Vector3.zero;
                slot.FindPropertyRelative("Rotation").vector3Value = Vector3.zero;
            }
            ConfigureHolsterSlot(slots, 1, new Vector3(0.04f, 0.03f, -0.15f), new Vector3(1.75f, 255.27f, 266.19f));
            ConfigureHolsterSlot(slots, 3, new Vector3(-0.01f, -0.04f, -0.11f), new Vector3(11.24f, 90.97f, 74.20f));
            ConfigureHolsterSlot(slots, 6, new Vector3(0.01f, -0.03f, -0.12f), new Vector3(358.31f, 95.50f, 90.41f));
            ConfigureHolsterSlot(slots, 8, new Vector3(-0.06f, -0.04f, -0.09f), new Vector3(0.91f, 281.02f, 276.02f));
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureHolsterSlot(SerializedProperty slots, int index, Vector3 position, Vector3 rotation)
        {
            SerializedProperty slot = slots.GetArrayElementAtIndex(index);
            slot.FindPropertyRelative("Position").vector3Value = position;
            slot.FindPropertyRelative("Rotation").vector3Value = rotation;
        }

        private static void AddComponentByName(GameObject host, string fullName, bool required)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => { try { return assembly.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(candidate => candidate.FullName == fullName || candidate.Name == fullName);
            if (type == null)
            {
                if (required) throw new InvalidOperationException(fullName + " was not found.");
                return;
            }
            host.AddComponent(type);
        }

        private static Material CreateMaterial()
        {
            AssetDatabase.ImportAsset(BaseColorPath, ImportAssetOptions.ForceSynchronousImport);
            Texture2D baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(BaseColorPath);
            if (baseColor == null) throw new InvalidDataException("Choppa base-colour texture is missing.");

            Texture2D metallic = LoadPng(MetallicPath, true);
            Texture2D roughness = LoadPng(RoughnessPath, true);
            Texture2D normalSource = LoadPng(NormalPath, true);
            Texture2D mask = PackMetallicSmoothness(metallic, roughness);
            Texture2D normal = PackUnityNormal(normalSource);

            Shader shader = Shader.Find("Owlcat/Lit");
            if (shader == null) throw new InvalidOperationException("Owlcat/Lit is unavailable.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "Choppa" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else material.shader = shader;
            SetTexture(material, baseColor, "_BaseMap", "_BaseColorMap", "_MainTex");
            SetTexture(material, mask, "_MetallicGlossMap", "_MaskMap", "_MasksMap");
            SetTexture(material, normal, "_BumpMap", "_NormalMap");
            SetFloat(material, 1f, "_Metallic");
            SetFloat(material, 1f, "_Smoothness");
            SetFloat(material, 0.42f, "_Roughness");
            SetColor(material, Color.white, "_BaseColor", "_Color", "_AdditionalAlbedoColor");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static Texture2D LoadPng(string assetPath, bool linear)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, true, linear);
            if (!texture.LoadImage(File.ReadAllBytes(assetPath), false))
                throw new InvalidDataException("Could not decode " + assetPath);
            return texture;
        }

        private static Texture2D PackMetallicSmoothness(Texture2D metallic, Texture2D roughness)
        {
            if (metallic.width != roughness.width || metallic.height != roughness.height)
                throw new InvalidDataException("Choppa metallic and roughness textures have different dimensions.");
            Color32[] metal = metallic.GetPixels32();
            Color32[] rough = roughness.GetPixels32();
            for (int i = 0; i < metal.Length; i++)
                metal[i] = new Color32(metal[i].r, 0, 0, (byte)(255 - rough[i].r));
            Texture2D packed = new Texture2D(metallic.width, metallic.height, TextureFormat.RGBA32, true, true)
                { name = "Choppa_MetallicSmoothness" };
            packed.SetPixels32(metal);
            packed.Apply(true, false);
            return ReplaceTextureAsset(packed, PackedMaskPath);
        }

        private static Texture2D PackUnityNormal(Texture2D source)
        {
            Color32[] pixels = source.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(255, pixels[i].g, 255, pixels[i].r);
            Texture2D packed = new Texture2D(source.width, source.height, TextureFormat.RGBA32, true, true)
                { name = "Choppa_Normal" };
            packed.SetPixels32(pixels);
            packed.Apply(true, false);
            return ReplaceTextureAsset(packed, PackedNormalPath);
        }

        private static Texture2D ReplaceTextureAsset(Texture2D source, string path)
        {
            Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(source, path);
                return source;
            }
            EditorUtility.CopySerialized(source, existing);
            UnityEngine.Object.DestroyImmediate(source);
            return existing;
        }

        private static void SetTexture(Material material, Texture texture, params string[] names)
        { foreach (string name in names) if (material.HasProperty(name)) material.SetTexture(name, texture); }

        private static void SetFloat(Material material, float value, params string[] names)
        { foreach (string name in names) if (material.HasProperty(name)) material.SetFloat(name, value); }

        private static void SetColor(Material material, Color value, params string[] names)
        { foreach (string name in names) if (material.HasProperty(name)) material.SetColor(name, value); }

        private static void GenerateBlueprint()
        {
            UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(PrefabPath);
            if (prefab == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string prefabGuid, out long prefabFileId))
                throw new InvalidDataException("Choppa prefab could not be resolved.");
            UnityEngine.Object icon = PrepareInventoryIcon();
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(icon, out string iconGuid, out long iconFileId))
                throw new InvalidDataException("Choppa inventory icon could not be resolved.");

            JObject weapon = PrepareClone(Load(WeaponPrototype), WeaponGuid, WeaponPrototype);
            weapon["Data"]["Components"] = new JArray();
            SetText(weapon, "choppa-name", "choppa-description", "choppa-flavor");
            weapon["Data"]["m_Icon"] = UnityReference(iconGuid, iconFileId);
            AddOverride(weapon, "m_Icon");
            weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = UnityReference(prefabGuid, prefabFileId);
            AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
            Override(weapon, "Family", "Primitive");
            Override(weapon, "Classification", "None");
            Override(weapon, "m_HoldingType", "OneHanded");
            Override(weapon, "IsTwoHanded", false);
            Override(weapon, "WarhammerDamage", 12);
            Override(weapon, "WarhammerMaxDamage", 18);
            Override(weapon, "WarhammerPenetration", 10);
            Override(weapon, "m_Enchantments", new JArray());
            Override(weapon, "m_Rarity", "Pattern");
            Override(weapon, "CanBeUsedInGame", true);
            Override(weapon, "IsUnlootable", false);
            Override(weapon, "IsNonRemovable", false);
            Override(weapon, "m_IsNotable", true);
            Override(weapon, "m_DamageType", new JObject { ["Type"] = "Rending" });
            File.WriteAllText(Path.Combine(Blueprints, "Choppa_Item.jbp"), weapon.ToString(Formatting.Indented));
        }

        private static UnityEngine.Object PrepareInventoryIcon()
        {
            AssetDatabase.ImportAsset(InventoryIconPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(InventoryIconPath) as TextureImporter;
            if (importer == null) throw new InvalidDataException("Choppa inventory icon importer was not found.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(InventoryIconPath).FirstOrDefault(asset => asset is Sprite)
                ?? AssetDatabase.LoadMainAssetAtPath(InventoryIconPath);
        }

        private static JObject Load(string id)
        {
            BlueprintJsonWrapper wrapper = BlueprintsDatabase.LoadWrapperById(id);
            if (wrapper == null) throw new InvalidDataException("Blueprint not found: " + id);
            using var writer = new StringWriter();
            Json.Serializer.Serialize(writer, wrapper);
            return JObject.Parse(writer.ToString());
        }

        private static JObject PrepareClone(JObject root, string id, string prototype)
        {
            root["AssetId"] = id;
            root["Data"]["PrototypeLink"] = prototype;
            root["Data"]["m_Overrides"] = new JArray();
            foreach (JObject component in root["Data"]["Components"].Children<JObject>())
            {
                component["PrototypeLink"] = new JObject
                {
                    ["guid"] = prototype,
                    ["name"] = component["name"]?.ToString() ?? ""
                };
                component["m_Overrides"] = new JArray();
            }
            return root;
        }

        private static void SetText(JObject root, string name, string description, string flavor)
        {
            root["Data"]["m_DisplayName"] = Localized(name);
            root["Data"]["m_Description"] = Localized(description);
            root["Data"]["m_FlavorText"] = Localized(flavor);
            AddOverride(root, "m_DisplayName");
            AddOverride(root, "m_Description");
            AddOverride(root, "m_FlavorText");
        }

        private static JObject Localized(string key) => new JObject
        {
            ["m_Key"] = key,
            ["m_OwnerString"] = "",
            ["m_OwnerPropertyPath"] = "",
            ["m_JsonPath"] = "",
            ["Shared"] = null
        };

        private static JObject UnityReference(string guid, long fileId) =>
            new JObject { ["guid"] = guid, ["fileid"] = fileId };

        private static void Override(JObject root, string property, JToken value)
        {
            root["Data"][property] = value;
            AddOverride(root, property);
        }

        private static void AddOverride(JObject root, string property)
        {
            JArray overrides = (JArray)root["Data"]["m_Overrides"];
            if (!overrides.Values<string>().Contains(property)) overrides.Add(property);
        }

        private static void WriteLocalization()
        {
            JObject document = new JObject
            {
                ["strings"] = new JObject
                {
                    ["choppa-name"] = Entry("Choppa"),
                    ["choppa-description"] = Entry("A brutally effective Ork cleaver assembled from scavenged metal, spikes and enthusiasm."),
                    ["choppa-flavor"] = Entry("If it is heavy, sharp and makes a satisfying noise, it is a proper choppa.")
                }
            };
            File.WriteAllText(Localization, document.ToString(Formatting.Indented));
        }

        private static JObject Entry(string text) => new JObject { ["Offset"] = 0, ["Text"] = text };
    }
}
