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
    internal static partial class ChoppaGenerator
    {
        // Generated assets and the distributable are built from the Unity editor menu.
        internal const string WeaponGuid = "9f1a0cab53be493b96989c9a672d2ae5";
        internal const string BigChoppaWeaponGuid = "f842bd6192ec4f9d91eb83278470c671";
        internal const string PowerKlawWeaponGuid = "89d3b9e7c7134f7f94c36518212b8596";
        internal static readonly string[] PowerKlawWeaponGuids =
        {
            PowerKlawWeaponGuid, "9ffb3db35be843a1a18cfcac2d90581a", "a7e3c400849846d1b5436136acc15319",
            "601279edb9554f70b608c135fc44cfbf", "9876491339ca4a60bd0419ceac6c80ec", "218bd6f72ed845768d0e9a52e5d2073f"
        };
        private static readonly int[] PowerKlawDamageMin = { 8, 10, 18, 22, 27, 34 };
        private static readonly int[] PowerKlawDamageMax = { 13, 14, 27, 32, 41, 50 };
        private static readonly int[] PowerKlawPenetration = { 30, 30, 35, 40, 45, 50 };
        private static readonly string[] PowerKlawTierNames =
        {
            "Power Klaw", "Kustom Power Klaw", "'Urty Power Klaw", "Killy Power Klaw",
            "Dead Killy Power Klaw", "Da Boss's Klaw"
        };
        private const string KlawKrumpGuid = "b155ee1863864b3d9ad0adcf8584e591";
        private const string KlawSweepGuid = "8f6642c9cd7a42a7946075203f3e2381";
        private const string KlawPushGuid = "639cde5c2c32442cbac5d472cc790e38";
        private const string KlawArmourGuid = "80d6acffc85b4680871298d80ea04890";
        private const string KlawArmourBuffGuid = "1e0c0c689b65427bb64cdf309431c382";
        private static readonly string[] KlawSqueezeGuids = { "329fa2e560e0447ebe02806909995cba", "8739cc6d623343278c5816ef358005c0" };
        private static readonly string[] KlawSqueezeWeaponGuids = { "e1e917e6daca460eb481b143a8440255", "8eb87e57eeb4458787344cd9a5b35a17" };
        private const string ArmourLimitReference = "e21dbeb3b9baacf4383912beeb5a62ef";
        private const string MissEffectReference = "7b1047c7df534302ad2d77b76129ea40";
        internal static readonly string[] BigChoppaWeaponGuids =
        {
            BigChoppaWeaponGuid, "a0f30782b1454053920b101af9e10908", "9bd81c627e58489ba2495eeef5df8f77",
            "9afbdac806004903ab8c370d1c94b208", "1a1602ca54e04a0b8231434b100dd175",
            "43d65e58f04c4ea580076b00f89b7ab0"
        };
        internal static readonly string[] WeaponGuids =
        {
            WeaponGuid, "78f71e9a52ab4f8e9a46bd2390860ec2", "ed79a91f997e45c6bd68e154aca4884c",
            "3b41ba583ab84c01bed6b8ea0fa31229", "833093446bd84d6ea45fb862d3a9a027",
            "c20a56ca164f46a480704c4b348f381f"
        };

        private static readonly int[] DamageMin = { 9, 11, 20, 24, 30, 38 };
        private static readonly int[] DamageMax = { 15, 16, 30, 36, 46, 55 };
        private static readonly int[] Penetration = { 15, 15, 20, 20, 25, 30 };
        private static readonly int[] BigChoppaDamageMin = { 12, 15, 21, 32, 37, 43 };
        private static readonly int[] BigChoppaDamageMax = { 21, 26, 30, 45, 56, 64 };
        private static readonly int[] BigChoppaPenetration = { 25, 25, 30, 30, 35, 40 };
        private static readonly string[] BigChoppaTierNames =
        {
            "Big Choppa", "Sharp Big Choppa", "'Urty Big Choppa", "Killy Big Choppa",
            "Dead Killy Big Choppa", "Da Biggest Choppa"
        };
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
        private static readonly string[] BigSwingAbilityGuids = { "6d20484cfdd24639b3a4951149f1fea8", "1bad043576274d3fb0187c787eb0eee6", "154305446437460b8d31f202521f5757", "0e044cb7a80e432db788361634b3066d", "723f6f5c82244969a3e009da10f410a0", "32f7dbda3fa847b992a69727333957f7" };
        private static readonly string[] BigSwingWeaponGuids = { "c79ea45641464ad093370da42f175bfc", "59b2ed4cd3884ccc9f6f938a2ba69c78", "88503c0505db44f8bc4ad4bf4cb6f6bb", "15fc19946fca478bb15513ce39c32ec4", "73d3f06e2ea649f59d602f0cf930df5d", "8407ada61d1c4dec97a52eb69c2ebf49" };
        private static readonly string[] SpinAbilityGuids = { "7ee4bbd643044e59908f224be2baa96d", "15ddf57cb0ba4bcbaa3753efcf8d1bec", "dd3501f5664d4e70940a9a5cbc0578d9", "f71e42ee79d540d1963b1482ffa9cf8b" };
        private static readonly string[] SpinWeaponGuids = { "ce501e085b0e437287acead7980e6fac", "0c7d74a0525e4097b75b165d276227e0", "43d1061a73a54450bb4d17193260b902", "c41bfc44ac6146d4b7fed9957ad28c62" };
        private static readonly string[] BonkAbilityGuids = { "08173427b67049ac911ac656ac540fab", "6806e82fc6694c278698ca390cad4a51" };
        private static readonly string[] BonkWeaponGuids = { "878bbff9e8d14518a1775fe789121c10", "9a4a3ed73f09454fb355e9d5161a1f52" };
        private const string BigSwingBuffGuid = "fb51718d81b04fe5954a5286fb4cad21";
        private const string SpinBuffGuid = "2a3b8fe767eb4aa8b255e9018263a3bc";
        private const string ProneBuffGuid = "8de915da52f24d9bb925d13fb2c8cf98";

        private const string ChargeAbilityPrototype = "c78506dd0e14f7c45a599990e4e65038";
        private const string StrikeAbilityPrototype = "be721a48e3614ddbb86bf18503511c2c";
        private const string TwoHandedWeaponPrototype = "993996a4c0a24463aa400b9441d4caa8";
        private const string GreatSwordWeaponPrototype = "88863b6b0c61404b96b01c2bc648ba5e";
        private const string TwoHandedStrikeAbility = "9dec1bdade284190b0977f5f70d26d3e";
        private const string TwoHandedCleaveAbility = "163013a18e9c46419b2311454ad2b2c8";
        private const string OmnissiahAxeCleaveAbility = "5e35e29dfa8e4133a9d10b3070d20c0b";
        private const string ReaperBladeDanceAbility = "e955823f54d24088ae1fdefe88d3684d";
        private const string NormalCleaveAbility = "03b6dee35fc0486da9d3fe7092afdf09";
        private const string BigChoppaCleaveGuid = "a7412fd693214091aa88c7beb83ac108";
        private const string PowerAxeFx = "5a72a93ee3bd4a049aa4ddef1e4c7f84";
        private const string MassStunAbilityPrototype = "0db7f73376e14ac6ab3a66b3d71afb90";
        private const string BerserkAbilityPrototype = "ce9c33784314498c9f9fc1f22495fa2d";
        private const string BerserkBuffPrototype = "dedd82d493484e8caf6d7aa6e243f55e";
        private const string OverrideWeaponReference = "84c32baad3f14585a32f5747d721dfc3";
        private const string ParryModifierReference = "53c19a9468d24539863989b3be9ed1f5";
        private const string DodgeModifierReference = "96e84143a45d4150b0233b5b58087fd1";
        // Native game silhouettes, matching the ability icon style used by both Choppas.
        private const string KlawStrikeIconGuid = "42b9acabc21013f4c93e7c7c959a2fa4"; // ClawAttack
        private const string KlawPushIconGuid = "2ec6ed3e7bae16342ab00d87bd0a8628"; // Kick / knockback
        private const string KlawArmourIconGuid = "885f114a74919494fbd08131f00fa886"; // Study_Defenses
        private const string KlawSqueezeIconGuid = "ec5c592ea58e791489d9814cd3004bd6"; // MastiffClaws
        private const string AxeStrikeIconGuid = "a6cba97367839af4e8869281de029095";
        private const string ChargeIconGuid = "4ef6dd81793f49444b6e53d7c68bdfd3";
        private const string TauntingScreamIconGuid = "1c30d7bb062c45f47b82491b6d4e401e";
        private const string RevelInSlaughterIconGuid = "7798cb69741ccc542b1286cff7272b6b";
        private const string ColossalCleaveIconGuid = "45cfe4b0e97ada046a0752ab1af86bbb";
        private const string BladeDanceIconGuid = "470be17a32276504c90338b2c6d409b5";
        private const string ThunderHammerAoeIconGuid = "ee0078845606d034198fb202bd59852f";

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
        private const string BigChoppaInventoryIconPath = Art + "/BigChoppa_InventoryIcon.png";
        private const string BigChoppaPrefabPath = Art + "/BigChoppa.prefab";
        private const string BigChoppaBaseColorPath = Art + "/BigChoppa_BaseColor.png";
        private const string BigChoppaMetallicPath = Art + "/BigChoppa_Metallic.png";
        private const string BigChoppaNormalPath = Art + "/BigChoppa_Normal_Source.png";
        private const string BigChoppaRoughnessPath = Art + "/BigChoppa_Roughness.png";
        private const string BigChoppaMaterialPath = Art + "/BigChoppa.mat";
        private const string BigChoppaPackedMaskPath = Art + "/BigChoppa_MetallicSmoothness.asset";
        private const string BigChoppaPackedNormalPath = Art + "/BigChoppa_Normal.asset";
        private const string BigChoppaGripMeshPath = Art + "/BigChoppa_GripAligned.asset";
        private const string PowerKlawFbxPath = Art + "/PowerKlaw.fbx";
        private const string PowerKlawPrefabPath = Art + "/PowerKlaw.prefab";
        private const string PowerKlawHolsteredPrefabPath = Art + "/PowerKlaw_Holstered.prefab";
        private const string PowerKlawInventoryIconPath = Art + "/PowerKlaw_InventoryIcon.png";
        private const string PowerKlawBaseColorPath = Art + "/PowerKlaw_BaseColor.png";
        private const string PowerKlawMetallicPath = Art + "/PowerKlaw_Metallic.png";
        private const string PowerKlawNormalPath = Art + "/PowerKlaw_Normal_Source.png";
        private const string PowerKlawRoughnessPath = Art + "/PowerKlaw_Roughness.png";
        private const string PowerKlawMaterialPath = Art + "/PowerKlaw.mat";
        private const string PowerKlawPackedMaskPath = Art + "/PowerKlaw_MetallicSmoothness.asset";
        private const string PowerKlawPackedNormalPath = Art + "/PowerKlaw_Normal.asset";
        private const string PowerKlawGripMeshPath = Art + "/PowerKlaw_GripAligned.asset";

        // Vanilla one-handed primitive axe; uses the BrutalOneHanded animation style.
        private const string WeaponPrototype = "b61740707f6b477f9ed139b22a81d245";

        [MenuItem("Armoury Orks/Build 1.0.0 macOS")]
        public static void Build() => BuildForPlatform(BuildTarget.StandaloneOSX);

        [MenuItem("Armoury Orks/Build 1.0.0 Windows")]
        public static void BuildWindows() => BuildForPlatform(BuildTarget.StandaloneWindows64);

        private static void BuildForPlatform(BuildTarget target)
        {
            if (EditorUserBuildSettings.activeBuildTarget != target)
                throw new InvalidOperationException($"Expected build platform {target}; start Unity with -buildTarget {(target == BuildTarget.StandaloneOSX ? "osx" : "win64")}.");
            Generate();
            ValidateChainCombatContracts();
            Modification mod = AssetDatabase.LoadAssetAtPath<Modification>(Root + "/ArmouryOrks.asset");
            if (mod == null) throw new InvalidOperationException("ArmouryOrks Modification asset was not found.");
            mod.Manifest.Version = "1.0.0";
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
            GeneratePowerKlawArt();
            GeneratePowerKlawBlueprint();
            GenerateChainChoppaArt();
            GenerateChainChoppaBlueprint();
            GenerateArmouryCache();
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
            Texture2D mask = PackOwlcatMasks(metallic, roughness);
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
            SetTexture(material, mask, "_MasksMap");
            SetTexture(material, normal, "_BumpMap", "_NormalMap");
            SetFloat(material, 0.9f, "_Metallic");
            SetFloat(material, 0.62f, "_Roughness");
            SetColor(material, Color.white, "_BaseColor", "_Color", "_AdditionalAlbedoColor");
            material.DisableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_MASKSMAP");
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

        private static Texture2D PackOwlcatMasks(Texture2D metallic, Texture2D roughness)
            => PackOwlcatMasks(metallic, roughness, PackedMaskPath, "Choppa_MetallicSmoothness");

        private static Texture2D PackOwlcatMasks(Texture2D metallic, Texture2D roughness, string outputPath, string textureName)
        {
            if (metallic.width != roughness.width || metallic.height != roughness.height)
                throw new InvalidDataException("Choppa metallic and roughness textures have different dimensions.");
            Color32[] metal = metallic.GetPixels32();
            Color32[] rough = roughness.GetPixels32();
            // Owlcat/Lit: R = roughness, G = emission, B = metallic, A = thickness.
            // Preserve the authored metal/non-metal regions and surface wear.
            for (int i = 0; i < metal.Length; i++)
                metal[i] = new Color32(rough[i].r, 0, metal[i].r, 0);
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
            AddComponentByName(root, "ArmouryOrks.Runtime.BigChoppaHolsterController", true);

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
                new Vector3(-0.03f, -0.085f, 0f));
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
            // Back slots are independent from m_MainHand and therefore do not affect attacks.
            // The Castellan Axe values (±0.18, -0.18, -0.12) are authored for a Custodes-sized
            // body. Pull the Big Choppa closer to a human back. Blade-plane roll is applied to
            // the mesh child by BigChoppaHolsterController, not in this rig-space rotation.
            ConfigureHolsterSlot(slots, 6, new Vector3(0.10f, -0.10f, -0.03f), new Vector3(90f, 0f, 0f));
            ConfigureHolsterSlot(slots, 8, new Vector3(-0.10f, -0.10f, -0.03f), new Vector3(90f, 0f, 0f));
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
            // Source-local X follows the shaft. Scale around the hand socket rather
            // than the FBX origin, so lengthening the weapon does not move its grip.
            Matrix4x4 meshToRoot = Matrix4x4.Rotate(visualRotation)
                * Matrix4x4.TRS(model.transform.localPosition, model.transform.localRotation,
                    model.transform.localScale);
            Vector3 grip = meshToRoot.inverse.MultiplyPoint3x4(Vector3.zero);
            Vector3 enlargement = new Vector3(1.25f, 1.10f, 1.10f);
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = grip + Vector3.Scale(vertices[i] + localVertexOffset - grip, enlargement);
            // Non-uniform scaling requires inverse-transpose normals and transformed tangents.
            Vector3[] normals = generated.normals;
            for (int i = 0; i < normals.Length; i++)
                normals[i] = new Vector3(normals[i].x / enlargement.x,
                    normals[i].y / enlargement.y, normals[i].z / enlargement.z).normalized;
            generated.normals = normals;
            Vector4[] tangents = generated.tangents;
            for (int i = 0; i < tangents.Length; i++)
            {
                Vector3 tangent = Vector3.Scale(new Vector3(tangents[i].x, tangents[i].y,
                    tangents[i].z), enlargement).normalized;
                tangents[i] = new Vector4(tangent.x, tangent.y, tangent.z, tangents[i].w);
            }
            generated.tangents = tangents;
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
            Texture2D mask = PackOwlcatMasks(metallic, roughness, BigChoppaPackedMaskPath, "BigChoppa_MetallicSmoothness");
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
            SetTexture(material, mask, "_MasksMap");
            SetTexture(material, normal, "_BumpMap", "_NormalMap");
            SetFloat(material, 0.9f, "_Metallic");
            SetFloat(material, 0.62f, "_Roughness");
            SetColor(material, Color.white, "_BaseColor", "_Color", "_AdditionalAlbedoColor");
            material.DisableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_MASKSMAP");
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static void GeneratePowerKlawArt()
        {
            AssetDatabase.ImportAsset(PowerKlawFbxPath, ImportAssetOptions.ForceSynchronousImport);
            ModelImporter importer = AssetImporter.GetAtPath(PowerKlawFbxPath) as ModelImporter;
            if (importer == null) throw new InvalidDataException("Power Klaw FBX could not be imported.");
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.SaveAndReimport();

            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(PowerKlawFbxPath);
            if (fbx == null) throw new InvalidDataException("Power Klaw FBX is missing after import.");
            Material material = CreatePowerKlawMaterial();
            GameObject root = new GameObject("PowerKlaw_Root");
            EquipmentOffsets offsets = root.AddComponent<EquipmentOffsets>();
            ConfigurePowerKlawOffsets(offsets);
            AddComponentByName(root, "FxLocatorMapper", false);
            AddComponentByName(root, "ArmouryOrks.Runtime.OrkWeaponMaterialBinder", true);
            AddComponentByName(root, "ArmouryOrks.Runtime.PowerKlawVisibilityController", true);

            GameObject visual = new GameObject("PowerKlaw_Visual");
            visual.transform.SetParent(root.transform, false);
            GameObject model = UnityEngine.Object.Instantiate(fbx);
            model.name = "PowerKlaw_FBX_Model";
            model.transform.SetParent(visual.transform, false);
            // Meshy authored the claw around its geometric centre. Move the rear quarter to the
            // hand socket and keep a deliberately oversized, but still human-rig-compatible scale.
            // Pull the cuff over the hand and forearm. The source pivot sits in the centre of
            // the complete claw, so the first visual test otherwise leaves it hanging below.
            model.transform.localPosition = new Vector3(0.08f, 0.16f, 0.12f);
            // Keep the accepted fore/aft pitch. Lean around the model-local cross axis,
            // not the socket axis, so the cuff turns towards the arm.
            model.transform.localRotation = Quaternion.Euler(270f, 0f, 0f)
                * Quaternion.AngleAxis(-60f, Vector3.right)
                * Quaternion.AngleAxis(20f, Vector3.forward);
            model.transform.localScale = Vector3.one * 85f;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = material;

            PrefabUtility.SaveAsPrefabAsset(root, PowerKlawPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);

            // The engine selects BeltModel whenever IsInHand is false. An empty
            // prefab avoids relying on skeleton-specific holster socket names.
            GameObject holstered = new GameObject("PowerKlaw_Holstered");
            PrefabUtility.SaveAsPrefabAsset(holstered, PowerKlawHolsteredPrefabPath);
            UnityEngine.Object.DestroyImmediate(holstered);
            AssetDatabase.SaveAssets();
        }

        private static void ConfigurePowerKlawOffsets(EquipmentOffsets offsets)
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
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Material CreatePowerKlawMaterial()
        {
            AssetDatabase.ImportAsset(PowerKlawBaseColorPath, ImportAssetOptions.ForceSynchronousImport);
            Texture2D baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(PowerKlawBaseColorPath);
            if (baseColor == null) throw new InvalidDataException("Power Klaw base-colour texture is missing.");
            Texture2D metallic = LoadPng(PowerKlawMetallicPath, true);
            Texture2D roughness = LoadPng(PowerKlawRoughnessPath, true);
            Texture2D normalSource = LoadPng(PowerKlawNormalPath, true);
            Texture2D mask = PackOwlcatMasks(metallic, roughness, PowerKlawPackedMaskPath, "PowerKlaw_MetallicSmoothness");
            Texture2D normal = PackUnityNormal(normalSource, PowerKlawPackedNormalPath, "PowerKlaw_Normal");
            Shader shader = Shader.Find("Owlcat/Lit");
            if (shader == null) throw new InvalidOperationException("Owlcat/Lit is unavailable.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(PowerKlawMaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "PowerKlaw" };
                AssetDatabase.CreateAsset(material, PowerKlawMaterialPath);
            }
            else material.shader = shader;
            SetTexture(material, baseColor, "_BaseMap", "_BaseColorMap", "_MainTex");
            SetTexture(material, mask, "_MasksMap");
            SetTexture(material, normal, "_BumpMap", "_NormalMap");
            SetFloat(material, 0.9f, "_Metallic");
            SetFloat(material, 0.50f, "_Roughness");
            SetColor(material, Color.white, "_BaseColor", "_Color", "_AdditionalAlbedoColor");
            material.DisableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_MASKSMAP");
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static void GeneratePowerKlawBlueprint()
        {
            UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(PowerKlawPrefabPath);
            if (prefab == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string prefabGuid, out long prefabFileId))
                throw new InvalidDataException("Power Klaw prefab could not be resolved.");
            GeneratePowerKlawTiers(prefabGuid, prefabFileId);
        }

        private static void GeneratePowerKlawTiers(string prefabGuid, long prefabFileId)
        {
            UnityEngine.Object holstered = AssetDatabase.LoadMainAssetAtPath(PowerKlawHolsteredPrefabPath);
            if (holstered == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(holstered, out string holsteredGuid, out long holsteredFileId))
                throw new InvalidDataException("Power Klaw holstered prefab could not be resolved.");
            if (((GameObject)holstered).GetComponentsInChildren<Renderer>(true).Length != 0)
                throw new InvalidDataException("Power Klaw holstered prefab must contain no renderers.");
            UnityEngine.Object icon = PrepareInventoryIcon(PowerKlawInventoryIconPath, "Power Klaw");
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(icon, out string iconGuid, out long iconFileId))
                throw new InvalidDataException("Power Klaw inventory icon could not be resolved.");
            GeneratePowerKlawAbilities(iconGuid, iconFileId);
            for (int i = 0; i < PowerKlawWeaponGuids.Length; i++)
            {
                int tier = i + 1;
                JObject weapon = PrepareClone(Load(WeaponPrototype), PowerKlawWeaponGuids[i], WeaponPrototype);
                Override(weapon, "Components", new JArray());
                SetText(weapon, $"power-klaw-v{tier}-name", $"power-klaw-v{tier}-description", $"power-klaw-v{tier}-flavor");
                Override(weapon, "m_Icon", UnityReference(iconGuid, iconFileId));
                weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = UnityReference(prefabGuid, prefabFileId);
                AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
                weapon["Data"]["m_VisualParameters"]["m_WeaponBeltModelOverride"] = UnityReference(holsteredGuid, holsteredFileId);
                weapon["Data"]["m_VisualParameters"]["m_WeaponSheathModelOverride"] = UnityReference(holsteredGuid, holsteredFileId);
                AddOverride(weapon, "m_VisualParameters.m_WeaponBeltModelOverride");
                AddOverride(weapon, "m_VisualParameters.m_WeaponSheathModelOverride");
                weapon["Data"]["m_VisualParameters"]["m_WeaponAnimationStyle"] = "Fist";
                AddOverride(weapon, "m_VisualParameters.m_WeaponAnimationStyle");
                Override(weapon, "Family", "Primitive");
                Override(weapon, "Classification", "None");
                Override(weapon, "m_HoldingType", "OneHanded");
                Override(weapon, "IsTwoHanded", false);
                Override(weapon, "WarhammerDamage", PowerKlawDamageMin[i]);
                Override(weapon, "WarhammerMaxDamage", PowerKlawDamageMax[i]);
                Override(weapon, "WarhammerPenetration", PowerKlawPenetration[i]);
                Override(weapon, "ItemLevel", tier == 6 ? 55 : i * 10 + 9);
                Override(weapon, "m_Enchantments", new JArray());
                Override(weapon, "m_Rarity", tier <= 2 ? "Pattern" : "Unique");
                Override(weapon, "CanBeUsedInGame", true);
                Override(weapon, "IsUnlootable", false);
                Override(weapon, "IsNonRemovable", false);
                Override(weapon, "m_IsNotable", true);
                Override(weapon, "m_DamageType", new JObject { ["Type"] = "Impact" });
                SetAbilitySlot(weapon, "Ability1", "SingleShot", KlawKrumpGuid, null, 1);
                SetAbilitySlot(weapon, "Ability2", "AOE", KlawSweepGuid, PowerAxeFx, 2);
                SetAbilitySlot(weapon, "Ability3", "SingleShot", KlawPushGuid, null, 2);
                if (tier >= 3) SetAbilitySlot(weapon, "Ability4", "SingleShot", KlawArmourGuid, null, 2);
                else ClearAbilitySlot(weapon, "Ability4");
                if (tier >= 5) SetAbilitySlot(weapon, "Ability5", "SingleShot", KlawSqueezeGuids[i - 4], null, 2);
                else ClearAbilitySlot(weapon, "Ability5");
                Override(weapon, "m_AttackOfOpportunityAbility", "!bp_" + KlawKrumpGuid);
                SaveBlueprint($"PowerKlaw_V{tier}_Item", weapon);
            }
        }

        private static void GeneratePowerKlawAbilities(string iconGuid, long iconFileId)
        {
            JObject krump = CreatePowerKlawAttack(KlawKrumpGuid, "krump", 1);
            SaveBlueprint("PowerKlaw_Krump_Ability", krump);

            JObject sweep = PrepareClone(Load(NormalCleaveAbility), KlawSweepGuid, NormalCleaveAbility);
            SetAbilityText(sweep, "power-klaw-sweep-name", "power-klaw-sweep-description");
            SetAbilityIcon(sweep, ColossalCleaveIconGuid);
            Override(sweep, "ActionPointCost", 2);
            Override(sweep, "CombatStateRestriction", "NoRestriction");
            SaveBlueprint("PowerKlaw_Sweep_Ability", sweep);

            JObject push = CreatePowerKlawAttack(KlawPushGuid, "push", 2, PushAction(1));
            SaveBlueprint("PowerKlaw_GetBack_Ability", push);

            // Cap armour absorption at zero rather than subtracting 100 points.
            // This handles armour above 100% and leaves deflection untouched.
            JObject buff = PrepareClone(Load(BerserkBuffPrototype), KlawArmourBuffGuid, BerserkBuffPrototype);
            JObject armour = CloneComponent(ArmourLimitReference, "WarhammerArmorBonus", "$WarhammerArmorBonus$power-klaw");
            armour["ForceDeflectionMaximum"] = false;
            armour["ForcePctDeflectionMaximum"] = false;
            armour["ForceAbsorptionMaximum"] = true;
            armour["AbsorptionMaximumValue"] = SimpleValue(0);
            Override(buff, "Components", new JArray(armour));
            Override(buff, "Stacking", "Replace");
            Override(buff, "Initiative", "ByCaster");
            SetAbilityText(buff, "power-klaw-armour-buff-name", "power-klaw-armour-buff-description");
            SetAbilityIcon(buff, KlawArmourIconGuid);
            SaveBlueprint("PowerKlaw_NoArmour_Buff", buff);

            JObject strip = CreatePowerKlawAttack(KlawArmourGuid, "armour", 2,
                ApplyBuffAction(KlawArmourBuffGuid, 2, false), ApplyBuffAction(KlawArmourBuffGuid, 2, true));
            AddRoundCooldown(strip, 3, "$WarhammerCooldown$power-klaw-armour");
            SaveBlueprint("PowerKlaw_Armour_Ability", strip);

            for (int i = 4; i < 6; i++)
            {
                string weaponGuid = KlawSqueezeWeaponGuids[i - 4];
                JObject hidden = PrepareClone(Load(WeaponPrototype), weaponGuid, WeaponPrototype);
                Override(hidden, "Components", new JArray());
                Override(hidden, "m_Icon", UnityReference(iconGuid, iconFileId));
                hidden["Data"]["m_VisualParameters"]["m_WeaponModel"] = null;
                hidden["Data"]["m_VisualParameters"]["m_WeaponAnimationStyle"] = "Fist";
                AddOverride(hidden, "m_VisualParameters.m_WeaponModel");
                AddOverride(hidden, "m_VisualParameters.m_WeaponAnimationStyle");
                Override(hidden, "Classification", "None");
                Override(hidden, "WarhammerDamage", PowerKlawDamageMin[i] * 2);
                Override(hidden, "WarhammerMaxDamage", PowerKlawDamageMax[i] * 2);
                Override(hidden, "WarhammerPenetration", PowerKlawPenetration[i]);
                Override(hidden, "m_DamageType", new JObject { ["Type"] = "Impact" });
                Override(hidden, "CanBeUsedInGame", false);
                Override(hidden, "IsUnlootable", true);
                SaveBlueprint($"PowerKlaw_V{i + 1}_HiddenSqueeze_Item", hidden);

                JObject squeeze = CreatePowerKlawAttack(KlawSqueezeGuids[i - 4], "squeeze", 2,
                    ApplyBuffAction(ProneBuffGuid, 1, true));
                ConfigureBigChoppaAttack(squeeze, weaponGuid, "power-klaw-squeeze-name", "power-klaw-squeeze-description", 2);
                SetAbilityIcon(squeeze, KlawSqueezeIconGuid);
                // Delivery normally runs effects only on a hit. This marker allows
                // the self-knockdown after a miss too; it does not make damage hit.
                JObject miss = CloneComponent(MissEffectReference, "AbilityEffectMissIsHit", "$AbilityEffectMissIsHit$power-klaw-squeeze");
                ((JArray)squeeze["Data"]["Components"]).Add(miss);
                AddOverride(squeeze, "Components");
                AddRoundCooldown(squeeze, 4, "$WarhammerCooldown$power-klaw-squeeze");
                SaveBlueprint($"PowerKlaw_V{i + 1}_Squeeze_Ability", squeeze);
            }
        }

        private static JObject CreatePowerKlawAttack(string guid, string key, int ap, params JObject[] onHitActions)
        {
            JObject ability = PrepareClone(Load(StrikeAbilityPrototype), guid, StrikeAbilityPrototype);
            SetAbilityText(ability, $"power-klaw-{key}-name", $"power-klaw-{key}-description");
            string icon = key == "krump" ? KlawStrikeIconGuid
                : key == "push" ? KlawPushIconGuid
                : key == "armour" ? KlawArmourIconGuid : KlawSqueezeIconGuid;
            SetAbilityIcon(ability, icon);
            Override(ability, "ActionPointCost", ap);
            Override(ability, "CooldownRounds", 0);
            Override(ability, "CombatStateRestriction", "NoRestriction");
            if (onHitActions.Length > 0)
            {
                JObject run = CloneComponent(TwoHandedStrikeAbility, "AbilityEffectRunAction", "$AbilityEffectRunAction$power-klaw-" + key);
                run["Actions"]["Actions"] = new JArray(onHitActions);
                ((JArray)ability["Data"]["Components"]).Add(run);
                AddOverride(ability, "Components");
            }
            return ability;
        }

        private static JObject PrepareBigChoppaClone(string guid)
        {
            // Retain the existing mechanics, but inherit the native two-handed sword handling.
            // Override the complete visual object so the staff prototype cannot supply
            // the effective animation style through nested visual inheritance.
            JObject weapon = PrepareClone(Load(TwoHandedWeaponPrototype), guid, GreatSwordWeaponPrototype);
            JObject sword = Load(GreatSwordWeaponPrototype);
            foreach (JProperty property in ((JObject)weapon["Data"]).Properties())
            {
                if (property.Name == "PrototypeLink" || property.Name == "m_Overrides"
                    || property.Name == "Components" || property.Name == "m_VisualParameters") continue;
                if (!JToken.DeepEquals(property.Value, sword["Data"][property.Name]))
                    AddOverride(weapon, property.Name);
            }
            Override(weapon, "Components", new JArray());
            Override(weapon, "m_VisualParameters", sword["Data"]["m_VisualParameters"].DeepClone());
            return weapon;
        }

        private static void GenerateBigChoppaBlueprint()
        {
            UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(BigChoppaPrefabPath);
            if (prefab == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string prefabGuid, out long prefabFileId))
                throw new InvalidDataException("Big Choppa prefab could not be resolved.");
            UnityEngine.Object icon = PrepareInventoryIcon(BigChoppaInventoryIconPath, "Big Choppa");
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(icon, out string iconGuid, out long iconFileId))
                throw new InvalidDataException("Big Choppa inventory icon could not be resolved.");

            GenerateBigChoppaAbilities();
            for (int i = 0; i < 6; i++)
            {
                int tier = i + 1;
                JObject weapon = PrepareBigChoppaClone(BigChoppaWeaponGuids[i]);
                weapon["Data"]["Components"] = new JArray();
                SetText(weapon, $"big-choppa-v{tier}-name", $"big-choppa-v{tier}-description", $"big-choppa-v{tier}-flavor");
                weapon["Data"]["m_Icon"] = UnityReference(iconGuid, iconFileId);
                AddOverride(weapon, "m_Icon");
                weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = UnityReference(prefabGuid, prefabFileId);
                AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
                weapon["Data"]["m_VisualParameters"]["m_WeaponAnimationStyle"] = "BrutalTwoHanded";
                AddOverride(weapon, "m_VisualParameters.m_WeaponAnimationStyle");
                Override(weapon, "Family", "Primitive");
                Override(weapon, "Classification", "None");
                Override(weapon, "m_HoldingType", "TwoHanded");
                Override(weapon, "IsTwoHanded", true);
                Override(weapon, "WarhammerDamage", BigChoppaDamageMin[i]);
                Override(weapon, "WarhammerMaxDamage", BigChoppaDamageMax[i]);
                Override(weapon, "WarhammerPenetration", BigChoppaPenetration[i]);
                Override(weapon, "ItemLevel", tier == 6 ? 55 : i * 10 + 9);
                Override(weapon, "m_Enchantments", new JArray());
                Override(weapon, "m_Rarity", tier <= 2 ? "Pattern" : "Unique");
                Override(weapon, "CanBeUsedInGame", true);
                Override(weapon, "IsUnlootable", false);
                Override(weapon, "IsNonRemovable", false);
                Override(weapon, "m_IsNotable", true);
                Override(weapon, "m_DamageType", new JObject { ["Type"] = "Rending" });
                SetAbilitySlot(weapon, "Ability1", "SingleShot", TwoHandedStrikeAbility, PowerAxeFx, 1);
                SetAbilitySlot(weapon, "Ability2", "AOE", BigChoppaCleaveGuid, PowerAxeFx, 2);
                SetAbilitySlot(weapon, "Ability3", "AOE", BigSwingAbilityGuids[i], PowerAxeFx, 2);
                if (tier >= 3) SetAbilitySlot(weapon, "Ability4", "AOE", SpinAbilityGuids[i - 2], PowerAxeFx, 2);
                else ClearAbilitySlot(weapon, "Ability4");
                if (tier >= 5) SetAbilitySlot(weapon, "Ability5", "SingleShot", BonkAbilityGuids[i - 4], PowerAxeFx, 2);
                else ClearAbilitySlot(weapon, "Ability5");
                weapon["Data"]["m_AttackOfOpportunityAbility"] = "!bp_" + TwoHandedStrikeAbility;
                AddOverride(weapon, "m_AttackOfOpportunityAbility");
                SaveBlueprint($"BigChoppa_V{tier}_Item", weapon);
            }
        }

        private static void GenerateBigChoppaAbilities()
        {
            JObject cleave = PrepareClone(Load(NormalCleaveAbility), BigChoppaCleaveGuid, NormalCleaveAbility);
            SetAbilityText(cleave, "big-choppa-cleave-name", "big-choppa-cleave-description");
            SetAbilityIcon(cleave, AxeStrikeIconGuid);
            Override(cleave, "CombatStateRestriction", "NoRestriction");
            SaveBlueprint("BigChoppa_Cleave_Ability", cleave);
            GenerateBigChoppaPenaltyBuff(BigSwingBuffGuid, "big-choppa-big-swing-buff", false);
            GenerateBigChoppaPenaltyBuff(SpinBuffGuid, "big-choppa-spin-buff", true);
            for (int i = 0; i < 6; i++)
            {
                GenerateHiddenBigChoppa(i, BigSwingWeaponGuids[i], 1.1f, $"BigChoppa_V{i + 1}_HiddenBigSwing_Item");
                JObject swing = PrepareClone(Load(OmnissiahAxeCleaveAbility), BigSwingAbilityGuids[i], OmnissiahAxeCleaveAbility);
                ConfigureBigChoppaAttack(swing, BigSwingWeaponGuids[i], "big-choppa-big-swing-name", "big-choppa-big-swing-description", 2);
                SetAbilityIcon(swing, ColossalCleaveIconGuid);
                ReplaceOnHitActions(swing, PushAction(1), ApplyBuffAction(BigSwingBuffGuid, 1, true));
                SaveBlueprint($"BigChoppa_V{i + 1}_BigSwing_Ability", swing);

                if (i >= 2)
                {
                    GenerateHiddenBigChoppa(i, SpinWeaponGuids[i - 2], 1f, $"BigChoppa_V{i + 1}_HiddenSpin_Item");
                    JObject spin = PrepareClone(Load(MassStunAbilityPrototype), SpinAbilityGuids[i - 2], MassStunAbilityPrototype);
                    ConfigureBigChoppaAttack(spin, SpinWeaponGuids[i - 2], "big-choppa-spin-name", "big-choppa-spin-description", 2);
                    SetAbilityIcon(spin, BladeDanceIconGuid);
                    ApplyBladeDancePresentation(spin);
                    ReplaceRunActions(spin, PushAction(1), ApplyBuffAction(SpinBuffGuid, 1, true));
                    AddRoundCooldown(spin, 3, $"$WarhammerCooldown$big-choppa-v{i + 1}-spin");
                    SaveBlueprint($"BigChoppa_V{i + 1}_Spin_Ability", spin);
                }

                if (i >= 4)
                {
                    GenerateHiddenBigChoppa(i, BonkWeaponGuids[i - 4], 3f, $"BigChoppa_V{i + 1}_HiddenBonk_Item");
                    JObject bonk = PrepareClone(Load(TwoHandedStrikeAbility), BonkAbilityGuids[i - 4], TwoHandedStrikeAbility);
                    ConfigureBigChoppaAttack(bonk, BonkWeaponGuids[i - 4], "big-choppa-bonk-name", "big-choppa-bonk-description", 2);
                    SetAbilityIcon(bonk, ThunderHammerAoeIconGuid);
                    ReplaceOnHitActions(bonk, ApplyBuffAction(ProneBuffGuid, 1, true));
                    AddUntilCombatEndCooldown(bonk, $"$WarhammerCooldown$big-choppa-v{i + 1}-bonk");
                    SaveBlueprint($"BigChoppa_V{i + 1}_Bonk_Ability", bonk);
                }
            }
        }

        private static void GenerateHiddenBigChoppa(int i, string guid, float multiplier, string file)
        {
            JObject hidden = PrepareBigChoppaClone(guid);
            hidden["Data"]["Components"] = new JArray();
            hidden["Data"]["m_VisualParameters"]["m_WeaponModel"] = null;
            AddOverride(hidden, "m_VisualParameters.m_WeaponModel");
            hidden["Data"]["m_VisualParameters"]["m_WeaponAnimationStyle"] = "BrutalTwoHanded";
            AddOverride(hidden, "m_VisualParameters.m_WeaponAnimationStyle");
            Override(hidden, "Classification", "None");
            Override(hidden, "WarhammerDamage", Mathf.RoundToInt(BigChoppaDamageMin[i] * multiplier));
            Override(hidden, "WarhammerMaxDamage", Mathf.RoundToInt(BigChoppaDamageMax[i] * multiplier));
            Override(hidden, "WarhammerPenetration", BigChoppaPenetration[i]);
            Override(hidden, "CanBeUsedInGame", false);
            Override(hidden, "IsUnlootable", true);
            SaveBlueprint(file, hidden);
        }

        private static void ConfigureBigChoppaAttack(JObject ability, string weaponGuid, string name, string description, int ap)
        {
            SetAbilityText(ability, name, description);
            SetAbilityIcon(ability, RevelInSlaughterIconGuid);
            JObject weaponOverride = (JObject)Load(OverrideWeaponReference)["Data"]["Components"].Children<JObject>()
                .First(c => c["$type"]?.ToString().Contains("WarhammerOverrideAbilityWeapon") == true).DeepClone();
            weaponOverride["name"] = "$WarhammerOverrideAbilityWeapon$" + weaponGuid;
            weaponOverride["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            weaponOverride["m_Weapon"] = "!bp_" + weaponGuid;
            weaponOverride["m_ForceShowWeaponDamageInUi"] = true;
            ((JArray)ability["Data"]["Components"]).Add(weaponOverride);
            AddOverride(ability, weaponOverride["name"].ToString());
            Override(ability, "Type", "Weapon");
            Override(ability, "AbilityParamsSource", "Weapon");
            Override(ability, "ActionPointCost", ap);
            Override(ability, "VeilThicknessPointsToAdd", 0);
            Override(ability, "CombatStateRestriction", "NoRestriction");
        }

        private static void ApplyBladeDancePresentation(JObject ability)
        {
            JObject bladeDance = Load(ReaperBladeDanceAbility);
            Override(ability, "Animation", bladeDance["Data"]["Animation"].DeepClone());
            Override(ability, "m_FXSettings", bladeDance["Data"]["m_FXSettings"].DeepClone());
            Override(ability, "ShouldTurnToTarget", false);
            Override(ability, "SyncRotationAndAttack", false);
        }

        private static void ReplaceRunActions(JObject ability, params JObject[] actions)
        {
            JObject run = ability["Data"]["Components"].Children<JObject>()
                .First(c => c["$type"]?.ToString().Contains("AbilityEffectRunAction") == true);
            run["Actions"]["Actions"] = new JArray(actions);
        }

        private static void ReplaceOnHitActions(JObject ability, params JObject[] actions)
        {
            JObject run = ability["Data"]["Components"].Children<JObject>()
                .First(c => c["$type"]?.ToString().Contains("AbilityEffectRunAction") == true);
            JObject dodge = run["Actions"]["Actions"].Children<JObject>()
                .FirstOrDefault(a => a["$type"]?.ToString().Contains("DodgeActions") == true);
            if (dodge == null) run["Actions"]["Actions"] = new JArray(actions);
            else dodge["ActionsOnHit"]["Actions"] = new JArray(actions);
        }

        private static JObject PushAction(int cells) => new JObject
        {
            ["$type"] = "d5debd59683c7064fa9393bd52c9a624, ContextActionPush",
            ["name"] = "$ContextActionPush$big-choppa",
            ["Cells"] = SimpleValue(cells),
            ["ProvokeAttackOfOpportunity"] = false,
            ["m_UseFactOwnerAsCaster"] = false,
            ["m_PushBack"] = false,
            ["m_PushPerpendicular"] = false
        };

        private static JObject ApplyBuffAction(string buffGuid, int rounds, bool toCaster) => new JObject
        {
            ["$type"] = "5d13a597de91e4746b804f8233518523, ContextActionApplyBuff",
            ["name"] = "$ContextActionApplyBuff$big-choppa-" + buffGuid,
            ["m_Buff"] = "!bp_" + buffGuid,
            ["BuffEndCondition"] = "CombatEnd",
            ["Permanent"] = false,
            ["DurationValue"] = DurationRounds(rounds),
            ["ToCaster"] = toCaster,
            ["AsChild"] = true,
            ["SameDuration"] = false,
            ["AddFactSource"] = false
        };

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
            roar["Data"]["CombatStateRestriction"] = "NoRestriction";
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
            ability["Data"]["CombatStateRestriction"] = "NoRestriction";
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

        private static void AddRoundCooldown(JObject ability, int rounds, string name)
        {
            JObject cooldown = (JObject)Load(BerserkAbilityPrototype)["Data"]["Components"].Children<JObject>()
                .First(c => c["$type"]?.ToString().Contains("WarhammerCooldown") == true).DeepClone();
            cooldown["name"] = name;
            cooldown["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            cooldown["CooldownInRounds"] = rounds;
            cooldown["UntilEndOfCombat"] = false;
            ((JArray)ability["Data"]["Components"]).Insert(0, cooldown);
            AddOverride(ability, name);
        }

        private static void GenerateBigChoppaPenaltyBuff(string guid, string localizationPrefix, bool includeWeaponSkill)
        {
            JObject source = Load(BerserkBuffPrototype);
            JObject buff = PrepareClone(source, guid, BerserkBuffPrototype);
            JArray components = new JArray();
            if (includeWeaponSkill)
            {
                JObject ws = (JObject)source["Data"]["Components"].Children<JObject>()
                    .First(c => c["$type"]?.ToString().Contains("AddContextStatBonus") == true && c["Stat"]?.ToString() == "WarhammerWeaponSkill").DeepClone();
                ws["name"] = "$AddContextStatBonus$" + localizationPrefix;
                ws["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
                ws["Value"]["ValueType"] = "Simple";
                ws["Value"]["Value"] = -20;
                ws["Multiplier"] = 1;
                components.Add(ws);
            }
            JObject parry = CloneComponent(ParryModifierReference, "WarhammerParryChanceModifierDefender", "$WarhammerParryChanceModifierDefender$" + localizationPrefix);
            parry["Restrictions"]["Property"]["Getters"] = new JArray();
            parry["ParryChance"]["Value"] = includeWeaponSkill ? 0 : -10;
            if (!includeWeaponSkill) components.Add(parry);
            JObject dodge = CloneComponent(DodgeModifierReference, "WarhammerDodgeChanceModifierDefender", "$WarhammerDodgeChanceModifierDefender$" + localizationPrefix);
            dodge["Restrictions"]["Property"]["Getters"] = new JArray();
            dodge["DodgeChance"]["Value"] = includeWeaponSkill ? -20 : -10;
            dodge["PercentDodgeModifier"] = false;
            components.Add(dodge);
            buff["Data"]["Components"] = components;
            buff["Data"]["Stacking"] = "Replace";
            SetAbilityText(buff, localizationPrefix + "-name", localizationPrefix + "-description");
            AddOverride(buff, "Components");
            AddOverride(buff, "Stacking");
            SaveBlueprint(includeWeaponSkill ? "BigChoppa_Spin_Penalty_Buff" : "BigChoppa_BigSwing_Penalty_Buff", buff);
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
            => PrepareInventoryIcon(InventoryIconPath, "Choppa");

        private static UnityEngine.Object PrepareInventoryIcon(string path, string weaponName)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidDataException(weaponName + " inventory icon importer was not found.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(asset => asset is Sprite)
                ?? AssetDatabase.LoadMainAssetAtPath(path);
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
                strings[$"choppa-v{tier}-name"] = Entry(TierNames[i]);
                strings[$"choppa-v{tier}-description"] = Entry(descriptions[i]);
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
            string[] bigChoppaFlavor =
            {
                "'If ya need two hands, it'z twice as choppy.'", "'It got bigger. Dat means it got better.'",
                "'Now dat is a proper Choppa.'", "'Stand back. Further. Bit further.'",
                "'Too big? Dat ain't a fing.'", "'Dis one ends arguments. An' floors Orks.'"
            };
            string[] bigChoppaDescriptions =
            {
                "A two-handed Ork blade assembled from enough scavenged metal to armour a small vehicle. Its owner chose instead to attach it all to one handle.",
                "Someone has sharpened this already excessive weapon. The work is uneven, enthusiastic and capable of removing several important things at once.",
                "Repeated impacts have bent the blade into a shape its maker insists was intentional. Orks judge the weapon by the size of the dents it leaves behind.",
                "This Big Choppa has accumulated extra plates, teeth and weight with every repair. Subtlety was discarded early in the process to make room for more metal.",
                "A dead killy monument to poor judgement, built too large for sensible warriors and therefore exactly large enough for an ambitious Ork.",
                "The final answer to the ancient Ork question of how large a Choppa ought to be: larger than its wielder, louder than nearby gunfire and preferably still accelerating."
            };
            for (int i = 0; i < 6; i++)
            {
                int tier = i + 1;
                strings[$"big-choppa-v{tier}-name"] = Entry(BigChoppaTierNames[i]);
                strings[$"big-choppa-v{tier}-description"] = Entry(bigChoppaDescriptions[i]);
                strings[$"big-choppa-v{tier}-flavor"] = Entry(bigChoppaFlavor[i]);
            }
            strings["big-choppa-big-swing-name"] = Entry("BIG SWING!");
            strings["big-choppa-big-swing-description"] = Entry("Sweep a broad arc for 110% weapon damage and push every target back 1 cell. The wielder suffers -10% parry and dodge for 1 round. Cost: 2 AP.");
            strings["big-choppa-spin-name"] = Entry("SPIN TA WIN!");
            strings["big-choppa-spin-description"] = Entry("Spin wildly, striking all nearby enemies for 100% weapon damage and pushing them back 1 cell. The wielder suffers -20 Weapon Skill and dodge for 1 round. Cost: 2 AP. Cooldown: 3 rounds.");
            strings["big-choppa-bonk-name"] = Entry("DA BIGGEST BONK!");
            strings["big-choppa-bonk-description"] = Entry("Make one spectacularly ill-advised attack for 300% weapon damage. This attack can critically hit. After the attack, the wielder is knocked prone for 1 round. Cost: 2 AP. Usable once per combat.");
            strings["big-choppa-big-swing-buff-name"] = Entry("Overextended");
            strings["big-choppa-big-swing-buff-description"] = Entry("-10% parry and dodge chance for 1 round.");
            strings["big-choppa-spin-buff-name"] = Entry("Dizzy");
            strings["big-choppa-spin-buff-description"] = Entry("-20 Weapon Skill and dodge chance for 1 round.");
            strings["big-choppa-cleave-name"] = Entry("Cleave");
            strings["big-choppa-cleave-description"] = Entry("Swing the Big Choppa through the three tiles directly in front of the wielder. Cost: 2 AP.");
            string[] klawFlavor =
            {
                "'Grab it. Squeeze it. If it stops movin', squeeze da next one.'",
                "'Da extra piston makes it kustom. Da extra noise makes it better.'",
                "'Panzer? Dat just means squeeze harder.'",
                "'If da klaw fits, krump it.'",
                "'If you ain't fallin' over, you ain't squeezin' hard enough.'",
                "'Only da boss gets to press ALL da buttons.'"
            };
            for (int i = 0; i < PowerKlawWeaponGuids.Length; i++)
            {
                int tier = i + 1;
                strings[$"power-klaw-v{tier}-name"] = Entry(PowerKlawTierNames[i]);
                strings[$"power-klaw-v{tier}-description"] = Entry("A roaring mechanical claw built from pistons, scavenged armour plate and the Ork conviction that anything worth grabbing is worth crushing.");
                strings[$"power-klaw-v{tier}-flavor"] = Entry(klawFlavor[i]);
            }
            strings["power-klaw-krump-name"] = Entry("KRUMP!");
            strings["power-klaw-krump-description"] = Entry("Strike one enemy with the Power Klaw for normal weapon damage. Cost: 1 AP.");
            strings["power-klaw-sweep-name"] = Entry("SWEEP!");
            strings["power-klaw-sweep-description"] = Entry("Sweep the Power Klaw through the three tiles directly in front of the wielder for normal weapon damage. Cost: 2 AP.");
            strings["power-klaw-push-name"] = Entry("GET BACK!");
            strings["power-klaw-push-description"] = Entry("Strike one enemy for normal weapon damage. On a hit, push the target 1 cell away without provoking attacks of opportunity. Cost: 2 AP.");
            strings["power-klaw-armour-name"] = Entry("ARMOUR? WOT ARMOUR?!");
            strings["power-klaw-armour-description"] = Entry("Strike one enemy for normal weapon damage. On a hit, both the target and the wielder lose all armour absorption for 2 rounds against all attackers. Deflection, dodge and parry are unchanged. On a miss, neither loses armour. Cost: 2 AP. Cooldown: 3 rounds from use, including on a miss.");
            strings["power-klaw-armour-buff-name"] = Entry("WOT ARMOUR?!");
            strings["power-klaw-armour-buff-description"] = Entry("Armour absorption is capped at 0 for 2 rounds. Deflection, dodge and parry are unchanged.");
            strings["power-klaw-squeeze-name"] = Entry("DA BIG SQUEEZE!");
            strings["power-klaw-squeeze-description"] = Entry("Strike one enemy for 200% weapon damage. After the attack, the wielder is knocked prone for 1 round, even if the attack misses. Cost: 2 AP. Cooldown: 4 rounds from use. Does not automatically knock down the target.");
            WriteChainLocalization(strings);
            strings["ork-cache-name"] = Entry("DA LOOT BOX");
            strings["ork-cache-description"] = Entry("A battered cache of Ork weapons. Four ways to make a proper mess. No refunds.");
            JObject document = new JObject
            {
                ["strings"] = strings
            };
            File.WriteAllText(Localization, document.ToString(Formatting.Indented));
        }

        private static JObject Entry(string text) => new JObject { ["Offset"] = 0, ["Text"] = text };
    }
}
