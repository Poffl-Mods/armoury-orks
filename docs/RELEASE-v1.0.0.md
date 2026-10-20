# Armoury: Orks! WAAAAAGH! — v1.0.0

Four brutal Ork weapon families for Warhammer 40,000: Rogue Trader:

- **Choppa** — charges, war cries and slaughter.
- **Big Choppa** — heavy swings, spinning attacks and a proper bonk.
- **Power Klaw** — crushing blows, knockback and armour-breaking attacks.
- **Chain Choppa** — CHAAAAAAAAAAIN!, chained bites and an unpredictable red button.

Each family has six tiers that automatically advance with party level. Find one of each in **DA LOOT BOX**, a persistent Ork armoury chest in the **Warrant Chamber** aboard the voidship. Existing campaigns are supported; the chest appears when the area is loaded and is not intentionally restocked after looting.

## Downloads

- **Windows:** `ArmouryOrks-1.0.0-Windows.zip`
- **macOS:** `ArmouryOrks-1.0.0-macOS.zip`
- **Checksums:** `SHA256SUMS.txt`

Use the package matching your operating system. Do not mix the asset bundles. GitHub's automatic source-code archives are not installable mod packages.

## Requirements and installation

Requires the official **MicroPatches runtime**; the tested baseline is **1.21.1**. Enable **Load OwlMod BlueprintDirectReferences dependencies** and leave experimental patches disabled. The editor installer is for developers, not for game installation.

Install Armoury: Orks as an **Owlcat modification**. On macOS, use Mac-ModInstaller. On Windows, extract the Windows ZIP into `%USERPROFILE%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\Modifications\ArmouryOrks` and enable `ArmouryOrks` in `OwlcatModificationManagerSettings.json`, preserving other settings, or use a compatible Owlcat mod manager. The manifest must be directly inside the ArmouryOrks folder. Restart the game completely after installation or updates.

Adeptus Custodes, ToyBox and other armoury mods are not required. Back up saves before installing or updating mods.

## Validation and limitations

Both platform packages are checked for manifest version, ZIP integrity, blueprints, runtime assembly and the actual Unity target of every AssetBundle. macOS pre-release weapon gameplay, visuals, sounds and chest were accepted in-game by the author. **The Windows build has not yet been play-tested on Windows.** Exhaustive combat/buff/save-load edge cases are not claimed as verified.

The chainsword chain and chest lid are static geometry. Runtime progression retains the mod's existing item identities. Avoid removing the mod from saves that contain its items.

Full details and installation guidance: [README](https://github.com/Poffl-Mods/armoury-orks#readme).
