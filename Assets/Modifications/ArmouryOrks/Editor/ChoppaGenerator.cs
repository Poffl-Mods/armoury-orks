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
        internal const string BigChoppaWeaponGuid = "f842bd6192ec4f9d91eb83278470c671";
        internal static readonly string[] WeaponGuids =
        {
            WeaponGuid, "78f71e9a52ab4f8e9a46bd2390860ec2", "ed79a91f997e45c6bd68e154aca4884c",
            "3b41ba583ab84c01bed6b8ea0fa31229", "833093446bd84d6ea45fb862d3a9a027",
            "c20a56ca164f46a480704c4b348f381f"
        };

        private static readonly int[] DamageMin = { 9, 11, 20, 24, 30, 38 };
        private static readonly int[] DamageMax = { 15, 16, 30, 36, 46, 55 };
        private static readonly int[] Penetration = { 15, 15, 20, 20, 25, 30 };
        private static readonly string[] TierNames =
        {
            "Choppa", "Sharp Choppa", "'Urty Choppa", "Killy Choppa",
            "Dead Killy Choppa", "Da Bestest Choppa"
        };
        private static readonly string[] TierFiles =
        {
            "Choppa_Item", "Choppa_V2_Item", "Choppa_V3_Item", "Choppa_V4_Item",
            "Choppa_V5_Item", "Choppa_V6_Item"
        };

        private const string WaaaghAbilityGuid = "558b219108fa471785260b78b0fe8923";
        private const string StrikeAbilityGuid = "6e0bea3ad51d4a2f82178732f4c83e91";
        private const string GreaterWaaaghAbilityGuid = "ceda5ff1bcfd46efa12191d0235cbc71";
        private const string SlaughterV5AbilityGuid = "2799bb77dc804d72bea1c3a119e22db1";
        private const string SlaughterV6AbilityGuid = "b9f72f07ca7e47a2a3c9732ff8ac9895";
        private const string SlaughterV5WeaponGuid = "d12cfc17490b42fea2c05188234dbc1c";
        private const string SlaughterV6WeaponGuid = "0215570a37cd46fe91f919f914203c49";
        private const string SlaughterBuffGuid = "3a432daf113644c0b8814360346874dc";

        private const string ChargeAbilityPrototype = "c78506dd0e14f7c45a599990e4e65038";
        private const string StrikeAbilityPrototype = "be721a48e3614ddbb86bf18503511c2c";
        private const string TwoHandedWeaponPrototype = "993996a4c0a24463aa400b9441d4caa8";
        private const string TwoHandedStrikeAbility = "9dec1bdade284190b0977f5f70d26d3e";
        private const string TwoHandedCleaveAbility = "163013a18e9c46419b2311454ad2b2c8";
        private const string PowerAxeFx = "5a72a93ee3bd4a049aa4ddef1e4c7f84";
        private const string MassStunAbilityPrototype = "0db7f73376e14ac6ab3a66b3d71afb90";
        private const string BerserkAbilityPrototype = "ce9c33784314498c9f9fc1f22495fa2d";
        private const string BerserkBuffPrototype = "dedd82d493484e8caf6d7aa6e243f55e";
        private const string OverrideWeaponReference = "84c32baad3f14585a32f5747d721dfc3";
        private const string ParryModifierReference = "53c19a9468d24539863989b3be9ed1f5";
        private const string DodgeModifierReference = "96e84143a45d4150b0233b5b58087fd1";
        private const string AxeStrikeIconGuid = "a6cba97367839af4e8869281de029095";
        private const string ChargeIconGuid = "4ef6dd81793f49444b6e53d7c68bdfd3";
        private const string TauntingScreamIconGuid = "1c30d7bb062c45f47b82491b6d4e401e";
        private const string RevelInSlaughterIconGuid = "7798cb69741ccc542b1286cff7272b6b";

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
        private const string BigChoppaFbxPath = Art + "/BigChoppa.fbx";
        private const string BigChoppaPrefabPath = Art + "/BigChoppa.prefab";
        private const string BigChoppaBaseColorPath = Art + "/BigChoppa_BaseColor.png";
        private const string BigChoppaMetallicPath = Art + "/BigChoppa_Metallic.png";
        private const string BigChoppaNormalPath = Art + "/BigChoppa_Normal_Source.png";
        private const string BigChoppaRoughnessPath = Art + "/BigChoppa_Roughness.png";
        private const string BigChoppaMaterialPath = Art + "/BigChoppa.mat";
        private const string BigChoppaPackedMaskPath = Art + "/BigChoppa_MetallicSmoothness.asset";
        private const string BigChoppaPackedNormalPath = Art + "/BigChoppa_Normal.asset";
        private const string BigChoppaGripMeshPath = Art + "/BigChoppa_GripAligned.asset";

        // Vanilla one-handed primitive axe; uses the BrutalOneHanded animation style.
        private const string WeaponPrototype = "b61740707f6b477f9ed139b22a81d245";

        [MenuItem("Armoury Orks/Build 0.2.11 Test")]
        public static void Build()
        {
            Generate();
            Modification mod = AssetDatabase.LoadAssetAtPath<Modification>(Root + "/ArmouryOrks.asset");
            if (mod == null) throw new InvalidOperationException("ArmouryOrks Modification asset was not found.");
            mod.Manifest.Version = "0.2.11";
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
            GenerateBigChoppaArt();
            GenerateBigChoppaBlueprint();
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
            SetFloat(material, 0.95f, "_Roughness");
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
            => PackMetallicSmoothness(metallic, roughness, PackedMaskPath, "Choppa_MetallicSmoothness");

        private static Texture2D PackMetallicSmoothness(Texture2D metallic, Texture2D roughness, string outputPath, string textureName)
        {
            if (metallic.width != roughness.width || metallic.height != roughness.height)
                throw new InvalidDataException("Choppa metallic and roughness textures have different dimensions.");
            Color32[] metal = metallic.GetPixels32();
            Color32[] rough = roughness.GetPixels32();
            for (int i = 0; i < metal.Length; i++)
                metal[i] = new Color32(metal[i].r, 0, 0, (byte)(255 - rough[i].r));
            Texture2D packed = new Texture2D(metallic.width, metallic.height, TextureFormat.RGBA32, true, true)
                { name = textureName };
            packed.SetPixels32(metal);
            packed.Apply(true, false);
            return ReplaceTextureAsset(packed, outputPath);
        }

        private static Texture2D PackUnityNormal(Texture2D source)
            => PackUnityNormal(source, PackedNormalPath, "Choppa_Normal");

        private static Texture2D PackUnityNormal(Texture2D source, string outputPath, string textureName)
        {
            Color32[] pixels = source.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(255, pixels[i].g, 255, pixels[i].r);
            Texture2D packed = new Texture2D(source.width, source.height, TextureFormat.RGBA32, true, true)
                { name = textureName };
            packed.SetPixels32(pixels);
            packed.Apply(true, false);
            return ReplaceTextureAsset(packed, outputPath);
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

            GenerateSpecialAbilities();
            for (int i = 0; i < 6; i++)
            {
                int tier = i + 1;
                JObject weapon = PrepareClone(Load(WeaponPrototype), WeaponGuids[i], WeaponPrototype);
                weapon["Data"]["Components"] = new JArray();
                SetText(weapon, $"choppa-v{tier}-name", $"choppa-v{tier}-description", $"choppa-v{tier}-flavor");
                weapon["Data"]["m_Icon"] = UnityReference(iconGuid, iconFileId);
                AddOverride(weapon, "m_Icon");
                weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = UnityReference(prefabGuid, prefabFileId);
                AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
                Override(weapon, "Family", "Primitive");
                Override(weapon, "Classification", "Sword");
                Override(weapon, "m_HoldingType", "OneHanded");
                Override(weapon, "IsTwoHanded", false);
                Override(weapon, "WarhammerDamage", DamageMin[i]);
                Override(weapon, "WarhammerMaxDamage", DamageMax[i]);
                Override(weapon, "WarhammerPenetration", Penetration[i]);
                Override(weapon, "ItemLevel", tier == 6 ? 55 : i * 10 + 9);
                Override(weapon, "m_Enchantments", new JArray());
                Override(weapon, "m_Rarity", tier <= 2 ? "Pattern" : "Unique");
                Override(weapon, "CanBeUsedInGame", true);
                Override(weapon, "IsUnlootable", false);
                Override(weapon, "IsNonRemovable", false);
                Override(weapon, "m_IsNotable", true);
                Override(weapon, "m_DamageType", new JObject { ["Type"] = "Rending" });
                SetAbilitySlot(weapon, "Ability1", "SingleShot", StrikeAbilityGuid, "612936da5a8948ccab3ac067e5d45144", 1);
                weapon["Data"]["m_AttackOfOpportunityAbility"] = "!bp_" + StrikeAbilityGuid;
                AddOverride(weapon, "m_AttackOfOpportunityAbility");
                SetAbilitySlot(weapon, "Ability3", "Custom", WaaaghAbilityGuid, null, 1);
                if (tier >= 3) SetAbilitySlot(weapon, "Ability4", "Custom", GreaterWaaaghAbilityGuid, null, 1);
                else ClearAbilitySlot(weapon, "Ability4");
                if (tier >= 5) SetAbilitySlot(weapon, "Ability5", "AOE", tier == 5 ? SlaughterV5AbilityGuid : SlaughterV6AbilityGuid, null, 2);
                else ClearAbilitySlot(weapon, "Ability5");
                File.WriteAllText(Path.Combine(Blueprints, TierFiles[i] + ".jbp"), weapon.ToString(Formatting.Indented));
            }
        }

        private static void GenerateBigChoppaArt()
        {
            AssetDatabase.ImportAsset(BigChoppaFbxPath, ImportAssetOptions.ForceSynchronousImport);
            ModelImporter importer = AssetImporter.GetAtPath(BigChoppaFbxPath) as ModelImporter;
            if (importer == null) throw new InvalidDataException("BigChoppa.fbx could not be imported.");
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.SaveAndReimport();

            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(BigChoppaFbxPath);
            if (fbx == null) throw new InvalidDataException("Big Choppa FBX is missing after import.");
            Material material = CreateBigChoppaMaterial();

            GameObject root = new GameObject("BigChoppa_Root");
            EquipmentOffsets offsets = root.AddComponent<EquipmentOffsets>();
            ConfigureBigChoppaHolsterOffsets(offsets);
            AddComponentByName(root, "FxLocatorMapper", false);
            AddComponentByName(root, "ArmouryOrks.Runtime.OrkWeaponMaterialBinder", true);

            // Castellan Axe melee poses are authored around an additional 45-degree visual root.
            // Keep that hierarchy while omitting its bolter muzzle and shot-pose controller.
            GameObject visual = new GameObject("BigChoppa_Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            GameObject model = UnityEngine.Object.Instantiate(fbx);
            model.name = "BigChoppa_FBX_Model";
            model.transform.SetParent(visual.transform, false);
            // Match the proven Castellan Axe two-handed grip and attack orientation.
            // Preserve the proven Castellan-local mesh placement. Drawn grip correction belongs
            // to EquipmentOffsets.m_MainHand, not to this nested visual transform.
            model.transform.localPosition = new Vector3(0.335f, 0.265f, 0.02f);
            model.transform.localRotation = Quaternion.Euler(0f, 0f, 225f)
                * Quaternion.AngleAxis(150f, Vector3.right);
            // The FBX is already 30% larger than the regular Choppa. Use the same Meshy-to-game
            // conversion as that weapon so the size increase is applied only once.
            model.transform.localScale = Vector3.one * 54.4f;
            // The doll-room attachment path can rebuild/reset prefab transforms. Bake the small
            // grip correction into the mesh itself, which cannot be discarded by attachment code.
            // The exaggerated 0.2.9 diagnostic established that positive root-space X/Y moves
            // the visible mesh left/down in the doll room. Apply the small inverse correction so
            // the grip moves right toward the hand and slightly upward.
            BakeBigChoppaGripAlignment(model, visual.transform.localRotation,
                new Vector3(-0.03f, -0.05f, 0f));
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = material;

            PrefabUtility.SaveAsPrefabAsset(root, BigChoppaPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureBigChoppaHolsterOffsets(EquipmentOffsets offsets)
        {
            SerializedObject serialized = new SerializedObject(offsets);
            serialized.FindProperty("m_MainHand.Position").vector3Value = Vector3.zero;
            SerializedProperty slots = serialized.FindProperty("m_SlotOffsets");
            slots.arraySize = 12;
            for (int i = 0; i < slots.arraySize; i++)
            {
                SerializedProperty slot = slots.GetArrayElementAtIndex(i);
                slot.FindPropertyRelative("Position").vector3Value = Vector3.zero;
                slot.FindPropertyRelative("Rotation").vector3Value = Vector3.zero;
            }
            ConfigureHolsterSlot(slots, 6, new Vector3(0.18f, -0.18f, -0.12f), new Vector3(90f, 0f, 0f));
            ConfigureHolsterSlot(slots, 8, new Vector3(-0.18f, -0.18f, -0.12f), new Vector3(90f, 0f, 0f));
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BakeBigChoppaGripAlignment(GameObject model, Quaternion visualRotation,
            Vector3 desiredRootSpaceOffset)
        {
            MeshFilter filter = model.GetComponentInChildren<MeshFilter>(true);
            if (filter == null || filter.sharedMesh == null)
                throw new InvalidDataException("Big Choppa mesh filter is missing.");

            // Convert the requested root-space movement back through the visual and model
            // rotations, then through the model scale, to obtain a source-mesh vertex offset.
            Vector3 localVertexOffset = Quaternion.Inverse(model.transform.localRotation)
                * (Quaternion.Inverse(visualRotation) * desiredRootSpaceOffset);
            localVertexOffset /= model.transform.localScale.x;

            Mesh generated = UnityEngine.Object.Instantiate(filter.sharedMesh);
            generated.name = "BigChoppa_GripAligned";
            Vector3[] vertices = generated.vertices;
            for (int i = 0; i < vertices.Length; i++) vertices[i] += localVertexOffset;
            generated.vertices = vertices;
            generated.RecalculateBounds();

            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(BigChoppaGripMeshPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(generated, BigChoppaGripMeshPath);
                existing = generated;
            }
            else
            {
                EditorUtility.CopySerialized(generated, existing);
                UnityEngine.Object.DestroyImmediate(generated);
                EditorUtility.SetDirty(existing);
            }
            filter.sharedMesh = existing;
        }

        private static Material CreateBigChoppaMaterial()
        {
            AssetDatabase.ImportAsset(BigChoppaBaseColorPath, ImportAssetOptions.ForceSynchronousImport);
            Texture2D baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(BigChoppaBaseColorPath);
            if (baseColor == null) throw new InvalidDataException("Big Choppa base-colour texture is missing.");
            Texture2D metallic = LoadPng(BigChoppaMetallicPath, true);
            Texture2D roughness = LoadPng(BigChoppaRoughnessPath, true);
            Texture2D normalSource = LoadPng(BigChoppaNormalPath, true);
            Texture2D mask = PackMetallicSmoothness(metallic, roughness, BigChoppaPackedMaskPath, "BigChoppa_MetallicSmoothness");
            Texture2D normal = PackUnityNormal(normalSource, BigChoppaPackedNormalPath, "BigChoppa_Normal");

            Shader shader = Shader.Find("Owlcat/Lit");
            if (shader == null) throw new InvalidOperationException("Owlcat/Lit is unavailable.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(BigChoppaMaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "BigChoppa" };
                AssetDatabase.CreateAsset(material, BigChoppaMaterialPath);
            }
            else material.shader = shader;
            SetTexture(material, baseColor, "_BaseMap", "_BaseColorMap", "_MainTex");
            SetTexture(material, mask, "_MetallicGlossMap", "_MaskMap", "_MasksMap");
            SetTexture(material, normal, "_BumpMap", "_NormalMap");
            SetFloat(material, 1f, "_Metallic");
            SetFloat(material, 1f, "_Smoothness");
            SetFloat(material, 0.95f, "_Roughness");
            SetColor(material, Color.white, "_BaseColor", "_Color", "_AdditionalAlbedoColor");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static void GenerateBigChoppaBlueprint()
        {
            UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(BigChoppaPrefabPath);
            if (prefab == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string prefabGuid, out long prefabFileId))
                throw new InvalidDataException("Big Choppa prefab could not be resolved.");

            JObject weapon = PrepareClone(Load(TwoHandedWeaponPrototype), BigChoppaWeaponGuid, TwoHandedWeaponPrototype);
            weapon["Data"]["Components"] = new JArray();
            SetText(weapon, "big-choppa-v1-name", "big-choppa-v1-description", "big-choppa-v1-flavor");
            weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = UnityReference(prefabGuid, prefabFileId);
            AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
            weapon["Data"]["m_VisualParameters"]["m_WeaponAnimationStyle"] = "TwoHandedHammer";
            AddOverride(weapon, "m_VisualParameters.m_WeaponAnimationStyle");
            Override(weapon, "Family", "Primitive");
            Override(weapon, "Classification", "Sword");
            Override(weapon, "m_HoldingType", "TwoHanded");
            Override(weapon, "IsTwoHanded", true);
            Override(weapon, "WarhammerDamage", 14);
            Override(weapon, "WarhammerMaxDamage", 20);
            Override(weapon, "WarhammerPenetration", 25);
            Override(weapon, "ItemLevel", 9);
            Override(weapon, "m_Enchantments", new JArray());
            Override(weapon, "m_Rarity", "Pattern");
            Override(weapon, "CanBeUsedInGame", true);
            Override(weapon, "IsUnlootable", false);
            Override(weapon, "IsNonRemovable", false);
            Override(weapon, "m_IsNotable", true);
            Override(weapon, "m_DamageType", new JObject { ["Type"] = "Rending" });
            SetAbilitySlot(weapon, "Ability1", "SingleShot", TwoHandedStrikeAbility, PowerAxeFx, 1);
            SetAbilitySlot(weapon, "Ability2", "AOE", TwoHandedCleaveAbility, PowerAxeFx, 2);
            ClearAbilitySlot(weapon, "Ability3");
            ClearAbilitySlot(weapon, "Ability4");
            ClearAbilitySlot(weapon, "Ability5");
            weapon["Data"]["m_AttackOfOpportunityAbility"] = "!bp_" + TwoHandedStrikeAbility;
            AddOverride(weapon, "m_AttackOfOpportunityAbility");
            SaveBlueprint("BigChoppa_V1_Item", weapon);
        }

        private static void GenerateSpecialAbilities()
        {
            JObject strike = PrepareClone(Load(StrikeAbilityPrototype), StrikeAbilityGuid, StrikeAbilityPrototype);
            SetAbilityText(strike, "choppa-strike-name", "choppa-strike-description");
            SetAbilityIcon(strike, AxeStrikeIconGuid);
            SaveBlueprint("Choppa_Strike_Ability", strike);

            JObject charge = PrepareClone(Load(ChargeAbilityPrototype), WaaaghAbilityGuid, ChargeAbilityPrototype);
            SetAbilityText(charge, "choppa-waaagh-name", "choppa-waaagh-description");
            SetAbilityIcon(charge, ChargeIconGuid);
            charge["Data"]["ActionPointCost"] = 1;
            AddOverride(charge, "ActionPointCost");
            SaveBlueprint("Choppa_Waaagh_Ability", charge);

            JObject roar = PrepareClone(Load(MassStunAbilityPrototype), GreaterWaaaghAbilityGuid, MassStunAbilityPrototype);
            SetAbilityText(roar, "choppa-greater-waaagh-name", "choppa-greater-waaagh-description");
            SetAbilityIcon(roar, TauntingScreamIconGuid);
            JArray roarActions = (JArray)roar["Data"]["Components"].Children<JObject>()
                .First(c => c["$type"]?.ToString().Contains("AbilityEffectRunAction") == true)["Actions"]["Actions"];
            foreach (JToken damage in roarActions.Where(a => a["$type"]?.ToString().Contains("ContextActionDealDamage") == true).ToArray())
                damage.Remove();
            roar["Data"]["AbilityParamsSource"] = "None";
            roar["Data"]["VeilThicknessPointsToAdd"] = 0;
            roar["Data"]["ActionPointCost"] = 1;
            roar["Data"]["CooldownRounds"] = 0;
            roar["Data"]["CombatStateRestriction"] = "InCombatOnly";
            AddUntilCombatEndCooldown(roar, "$WarhammerCooldown$choppa-greater-waaagh");
            foreach (string property in new[] { "AbilityParamsSource", "VeilThicknessPointsToAdd", "ActionPointCost", "CooldownRounds", "CombatStateRestriction" })
                AddOverride(roar, property);
            SaveBlueprint("Choppa_GreaterWaaagh_Ability", roar);

            GenerateSlaughterBuff();
            GenerateSlaughterAbility(4, SlaughterV5AbilityGuid, SlaughterV5WeaponGuid);
            GenerateSlaughterAbility(5, SlaughterV6AbilityGuid, SlaughterV6WeaponGuid);
        }

        private static void GenerateSlaughterBuff()
        {
            JObject buff = PrepareClone(Load(BerserkBuffPrototype), SlaughterBuffGuid, BerserkBuffPrototype);
            JArray components = new JArray();
            JObject ws = buff["Data"]["Components"].Children<JObject>()
                .First(c => c["$type"]?.ToString().Contains("AddContextStatBonus") == true && c["Stat"]?.ToString() == "WarhammerWeaponSkill");
            ws = (JObject)ws.DeepClone();
            ws["name"] = "$AddContextStatBonus$choppa-slaughter-ws";
            ws["Value"]["ValueType"] = "Simple";
            ws["Value"]["Value"] = 10;
            ws["Multiplier"] = 1;
            components.Add(ws);
            JObject damage = (JObject)buff["Data"]["Components"].Children<JObject>()
                .First(c => c["$type"]?.ToString().Contains("WarhammerDamageModifierInitiator") == true).DeepClone();
            damage["name"] = "$WarhammerDamageModifierInitiator$choppa-slaughter";
            damage["MinimumDamage"]["Enabled"] = false;
            damage["MaximumDamage"]["Enabled"] = false;
            damage["PercentDamageModifier"]["Enabled"] = true;
            damage["PercentDamageModifier"]["ValueType"] = "Simple";
            damage["PercentDamageModifier"]["Value"] = 20;
            components.Add(damage);
            JObject parry = CloneComponent(ParryModifierReference, "WarhammerParryChanceModifierDefender", "$WarhammerParryChanceModifierDefender$choppa-slaughter");
            parry["Restrictions"]["Property"]["Getters"] = new JArray();
            parry["ParryChance"]["Value"] = -20;
            components.Add(parry);
            JObject dodge = CloneComponent(DodgeModifierReference, "WarhammerDodgeChanceModifierDefender", "$WarhammerDodgeChanceModifierDefender$choppa-slaughter");
            dodge["Restrictions"]["Property"]["Getters"] = new JArray();
            dodge["DodgeChance"]["Value"] = -20;
            dodge["PercentDodgeModifier"] = false;
            components.Add(dodge);
            buff["Data"]["Components"] = components;
            buff["Data"]["Stacking"] = "Replace";
            SetAbilityText(buff, "choppa-slaughter-buff-name", "choppa-slaughter-buff-description");
            AddOverride(buff, "Components");
            AddOverride(buff, "Stacking");
            SaveBlueprint("Choppa_Slaughter_Buff", buff);
        }

        private static void GenerateSlaughterAbility(int i, string abilityGuid, string hiddenWeaponGuid)
        {
            int tier = i + 1;
            JObject hidden = PrepareClone(Load(WeaponPrototype), hiddenWeaponGuid, WeaponPrototype);
            hidden["Data"]["Components"] = new JArray();
            hidden["Data"]["m_VisualParameters"]["m_WeaponModel"] = null;
            AddOverride(hidden, "m_VisualParameters.m_WeaponModel");
            Override(hidden, "WarhammerDamage", Mathf.RoundToInt(DamageMin[i] * 1.5f));
            Override(hidden, "WarhammerMaxDamage", Mathf.RoundToInt(DamageMax[i] * 1.5f));
            Override(hidden, "WarhammerPenetration", Penetration[i]);
            Override(hidden, "CanBeUsedInGame", false);
            Override(hidden, "IsUnlootable", true);
            SaveBlueprint($"Choppa_V{tier}_HiddenSlaughter_Item", hidden);

            JObject ability = PrepareClone(Load(MassStunAbilityPrototype), abilityGuid, MassStunAbilityPrototype);
            SetAbilityText(ability, "choppa-slaughter-name", $"choppa-v{tier}-slaughter-description");
            SetAbilityIcon(ability, RevelInSlaughterIconGuid);
            JObject run = ability["Data"]["Components"].Children<JObject>()
                .First(c => c["$type"]?.ToString().Contains("AbilityEffectRunAction") == true);
            run["Actions"]["Actions"] = new JArray(new JObject
            {
                ["$type"] = "5d13a597de91e4746b804f8233518523, ContextActionApplyBuff",
                ["name"] = "$ContextActionApplyBuff$choppa-slaughter",
                ["m_Buff"] = "!bp_" + SlaughterBuffGuid,
                ["BuffEndCondition"] = "CombatEnd",
                ["Permanent"] = false,
                ["DurationValue"] = DurationRounds(2),
                ["ToCaster"] = true,
                ["AsChild"] = true,
                ["SameDuration"] = false,
                ["AddFactSource"] = false
            });
            JObject overrideTemplate = (JObject)Load(OverrideWeaponReference)["Data"]["Components"].Children<JObject>()
                .First(c => c["$type"]?.ToString().Contains("WarhammerOverrideAbilityWeapon") == true).DeepClone();
            overrideTemplate["name"] = $"$WarhammerOverrideAbilityWeapon$choppa-v{tier}-slaughter";
            overrideTemplate["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            overrideTemplate["m_Weapon"] = "!bp_" + hiddenWeaponGuid;
            overrideTemplate["m_ForceShowWeaponDamageInUi"] = true;
            ((JArray)ability["Data"]["Components"]).Add(overrideTemplate);
            AddOverride(ability, overrideTemplate["name"].ToString());
            ability["Data"]["Type"] = "Weapon";
            ability["Data"]["AbilityParamsSource"] = "Weapon";
            ability["Data"]["ActionPointCost"] = 2;
            ability["Data"]["VeilThicknessPointsToAdd"] = 0;
            ability["Data"]["CombatStateRestriction"] = "InCombatOnly";
            AddUntilCombatEndCooldown(ability, $"$WarhammerCooldown$choppa-v{tier}-slaughter");
            foreach (string property in new[] { "Type", "AbilityParamsSource", "ActionPointCost", "VeilThicknessPointsToAdd", "CombatStateRestriction" })
                AddOverride(ability, property);
            SaveBlueprint($"Choppa_V{tier}_Slaughter_Ability", ability);
        }

        private static void AddUntilCombatEndCooldown(JObject ability, string name)
        {
            JObject cooldown = (JObject)Load(BerserkAbilityPrototype)["Data"]["Components"].Children<JObject>()
                .First(c => c["$type"]?.ToString().Contains("WarhammerCooldown") == true).DeepClone();
            cooldown["name"] = name;
            cooldown["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            cooldown["CooldownInRounds"] = 1;
            cooldown["UntilEndOfCombat"] = true;
            ((JArray)ability["Data"]["Components"]).Insert(0, cooldown);
            AddOverride(ability, name);
        }

        private static JObject DurationRounds(int rounds) => new JObject
        {
            ["BonusValue"] = SimpleValue(0),
            ["RoundsValue"] = SimpleValue(rounds)
        };

        private static JObject SimpleValue(int value) => new JObject
        {
            ["ValueType"] = "Simple", ["Value"] = value, ["ValueRank"] = "Default",
            ["ValueShared"] = "Damage", ["Property"] = "None", ["m_CustomProperty"] = null,
            ["PropertyName"] = "Value1"
        };

        private static void SetAbilityText(JObject root, string name, string description)
        {
            root["Data"]["m_DisplayName"] = Localized(name);
            root["Data"]["m_Description"] = Localized(description);
            AddOverride(root, "m_DisplayName");
            AddOverride(root, "m_Description");
        }

        private static void SetAbilityIcon(JObject root, string iconGuid)
        {
            root["Data"]["m_Icon"] = UnityReference(iconGuid, 21300000L);
            AddOverride(root, "m_Icon");
        }

        private static JObject CloneComponent(string blueprintId, string typeName, string name)
        {
            JObject component = (JObject)Load(blueprintId)["Data"]["Components"].Children<JObject>()
                .First(item => item["$type"]?.ToString().Contains(typeName) == true).DeepClone();
            component["name"] = name;
            component["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            component["m_Overrides"] = new JArray();
            return component;
        }

        private static void SaveBlueprint(string name, JObject root) =>
            File.WriteAllText(Path.Combine(Blueprints, name + ".jbp"), root.ToString(Formatting.Indented));

        private static void SetAbilitySlot(JObject weapon, string slotName, string type, string ability, string fx, int ap)
        {
            JObject slot = (JObject)weapon["Data"]["AbilityContainer"][slotName];
            slot["Type"] = type;
            slot["Mode"] = "Default";
            slot["m_Ability"] = "!bp_" + ability;
            slot["m_FXSettings"] = fx == null ? null : "!bp_" + fx;
            slot["OnHitOverrideType"] = "None";
            slot["m_OnHitActions"] = null;
            slot["AP"] = ap;
            foreach (string property in new[] { "Type", "Mode", "m_Ability", "m_FXSettings", "OnHitOverrideType", "m_OnHitActions", "AP" })
                AddOverride(weapon, $"WeaponAbilities.{slotName}.{property}");
        }

        private static void ClearAbilitySlot(JObject weapon, string slotName)
        {
            JObject slot = (JObject)weapon["Data"]["AbilityContainer"][slotName];
            slot["Type"] = "None";
            slot["Mode"] = "Default";
            slot["m_Ability"] = null;
            slot["m_FXSettings"] = null;
            slot["OnHitOverrideType"] = "None";
            slot["m_OnHitActions"] = null;
            slot["AP"] = 0;
            foreach (string property in new[] { "Type", "Mode", "m_Ability", "m_FXSettings", "OnHitOverrideType", "m_OnHitActions", "AP" })
                AddOverride(weapon, $"WeaponAbilities.{slotName}.{property}");
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
            string[] levelRanges = { "1-15", "16-25", "26-35", "36-43", "44-49", "50-55" };
            string[] descriptions =
            {
                "A heavy Ork blade hammered together from scavenged metal. It is crude, badly balanced and remarkably effective when swung with sufficient enthusiasm.",
                "Someone has taken the unusual step of sharpening this Choppa. The irregular edge now catches armour, flesh and anything else unfortunate enough to stand nearby.",
                "This scarred weapon has earned a reputation for leaving survivors in considerable discomfort. Orks consider this a sign of quality workmanship.",
                "Layer upon layer of dried blood, crude repairs and replacement teeth record the Choppa's long service. Its owners stopped counting victories when they ran out of fingers.",
                "A brutally oversized edge reinforced with whatever metal its owner could steal. Among Orks, calling a weapon 'dead killy' is both high praise and a warning to keep well clear of it.",
                "Every Ork claims to own the biggest and best Choppa. The bearer of this weapon has settled the argument repeatedly and with considerable finality."
            };
            string[] flavors =
            {
                "'It chops. Dat makes it a choppa.'", "'Dis one's sharp. Sharp is better.'",
                "'If 'e's still shoutin', hit 'im again.'", "'Sharp is good. Killy is better.'",
                "'One swing. Lotsa bits.'", "'Da best Choppa belongs to da Ork still standin'.'"
            };
            JObject strings = new JObject();
            for (int i = 0; i < 6; i++)
            {
                int tier = i + 1;
                string unlocked = tier >= 5 ? "\n• SLAUGHTER" : tier >= 3 ? "\n• WAAAAAAAAAAGH!" : "";
                strings[$"choppa-v{tier}-name"] = Entry(TierNames[i]);
                strings[$"choppa-v{tier}-description"] = Entry($"{descriptions[i]}\n\n• Levels: {levelRanges[i]}\n• Damage: {DamageMin[i]}-{DamageMax[i]}\n• Armour penetration: {Penetration[i]}%\n• WAAAGH!{unlocked}");
                strings[$"choppa-v{tier}-flavor"] = Entry(flavors[i]);
            }
            strings["choppa-waaagh-name"] = Entry("WAAAGH!");
            strings["choppa-waaagh-description"] = Entry("Charge an enemy and strike with the equipped Choppa. Cost: 1 AP.");
            strings["choppa-strike-name"] = Entry("Strike");
            strings["choppa-strike-description"] = Entry("Make a single melee attack with the equipped Choppa. Cost: 1 AP.");
            strings["choppa-greater-waaagh-name"] = Entry("WAAAAAAAAAAGH!");
            strings["choppa-greater-waaagh-description"] = Entry("Release a deafening war cry. Enemies in the area must pass a Toughness resistance test or become stunned for 1 round. Cost: 1 AP. Usable once per combat.");
            strings["choppa-slaughter-name"] = Entry("SLAUGHTER");
            strings["choppa-v5-slaughter-description"] = Entry("Strike all enemies around the target area for 150% weapon damage, then enter a slaughter-frenzy for 2 rounds. Cost: 2 AP. Usable once per combat.");
            strings["choppa-v6-slaughter-description"] = Entry("Strike all enemies around the target area for 150% weapon damage, then enter a slaughter-frenzy for 2 rounds. Cost: 2 AP. Usable once per combat.");
            strings["choppa-slaughter-buff-name"] = Entry("Slaughter-frenzy");
            strings["choppa-slaughter-buff-description"] = Entry("+20% damage and +10 Weapon Skill, but -20% parry and dodge chance for 2 rounds.");
            strings["big-choppa-v1-name"] = Entry("Big Choppa");
            strings["big-choppa-v1-description"] = Entry("A two-handed Ork blade built for bigger swings, louder impacts and considerably less subtlety.\n\n• Test version V1\n• Damage: 14-20\n• Armour penetration: 25%\n• Strike\n• Cleave");
            strings["big-choppa-v1-flavor"] = Entry("'If ya need two hands, it'z twice as choppy.'");
            JObject document = new JObject
            {
                ["strings"] = strings
            };
            File.WriteAllText(Localization, document.ToString(Formatting.Indented));
        }

        private static JObject Entry(string text) => new JObject { ["Offset"] = 0, ["Text"] = text };
    }
}
