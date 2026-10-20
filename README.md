# Armoury: Orks

Armoury: Orks adds custom-built Ork weapons for player use in *Warhammer 40,000: Rogue Trader*.

## Version 1.0.0

Four Ork weapon families — **Choppa**, **Big Choppa**, **Power Klaw** and **Chain Choppa** — each with six automatically progressing tiers and distinctive attacks. Collect them from **DA LOOT BOX** in the Warrant Chamber aboard your voidship. No ToyBox required.

Download the package matching your operating system from [GitHub Releases](https://github.com/Poffl-Mods/armoury-orks/releases/tag/v1.0.0):

| Package | Platform | Validation |
| --- | --- | --- |
| `ArmouryOrks-1.0.0-Windows.zip` | Windows 64-bit | Built and package-validated; Windows play-test pending |
| `ArmouryOrks-1.0.0-macOS.zip` | macOS | Built and package-validated; the pre-release gameplay, weapon visuals and chest were accepted in-game on macOS |

The packages contain platform-specific Unity AssetBundles. Use only one package; do not mix the Windows and macOS bundles. GitHub's automatic source-code archives are not installable mod packages.

The placeholder cache model is replaced by the user's Meshy Ork War Chest, reduced from 739,366 to 40,000 triangles (94.59% fewer). Colour, metallic, roughness and tangent-space normal detail are reprojected from the original high-resolution mesh onto the reduced model, retaining 2K textures. The closed static model is approximately 1.02 m wide, 0.70 m deep and 0.58 m high; its collider follows the imported bounds.

The cache blueprint, prefab identity, root unique ID, location, loot interaction and finite saved contents are retained, so existing caches use the new appearance after restarting/loading without being reseeded. The user confirmed the new appearance in-game on macOS. Exhaustive save/load and combat edge cases remain additional checks.

Choppa, Big Choppa and Chain Choppa use a roughness multiplier of 0.62 (previously 0.90), and Power Klaw uses 0.50 (previously 0.68). All four weapons use a metallic multiplier of 0.90 (previously 0.80). Original surface masks, colours and normal maps are retained, so wear and non-metal regions stay distinct. Final appearance depends on in-game lighting.

Find **DA LOOT BOX** in the **Warrant Chamber** aboard the voidship, near the Custodes armoury cache when both mods are installed. It contains one Choppa, Big Choppa, Power Klaw and Chain Choppa. The chest works independently of Adeptus Custodes and other armoury mods; its model is generated from our Ork chest concept with Meshy and its materials are adapted to Owlcat/Lit.

The cache is a native, persistent loot container: it also appears in existing campaigns when entering/loading the Warrant Chamber, and does not refill after looting, saving/loading or leaving and returning. The four weapons use the existing automatic party-level progression. ToyBox is optional.

The Chain Choppa's visuals, placement, chainsword sounds, special attacks and Crushing Assault interaction were accepted in the user's 0.6.4 in-game test. Temporary per-hit damage audit output is removed; combat fixes and saved blueprint identities are retained. Broader edge-case checks are listed below. The user confirmed the 0.7.0 chest works in-game; exhaustive empty-state persistence and save/load checks are not separately documented.

Chain Choppa now has six automatically upgrading tiers, using the same party-level thresholds as Choppa. Existing copies retain their V1 identity and upgrade outside combat, including equipped weapons and inventory/stash copies. The accepted 60,000-triangle model, size, grip and cutting-edge direction from 0.5.8 are preserved.

| Variant | Levels | Base damage | Armour penetration | New special ability |
| --- | ---: | ---: | ---: | --- |
| Chain Choppa | 1–15 | 8–13 | 20% | CHAAAAAAAAAAIN! |
| Kustom Chain Choppa | 16–25 | 10–16 | 20% | — |
| 'Urty Chain Choppa | 26–35 | 18–27 | 25% | DON'T LET GO! |
| Killy Chain Choppa | 36–43 | 22–33 | 25% | — |
| Dead Killy Chain Choppa | 44–49 | 27–41 | 30% | DA BUTTON WUZ RED! |
| Da Loudest Choppa | 50–55 | 34–50 | 35% | — |

Normal strike (1 AP) and native chainsword cleave (2 AP) remain available at all tiers.

- **CHAAAAAAAAAAIN!** — 150% weapon damage with normal character modifiers, with a 20% kickback roll before the attack. Kickback replaces the enemy attack with half the tier's base damage to the wielder; no offensive damage bonuses or critical hits, and the wielder's defences apply. Costs 2 AP; cooldown 2 rounds from use, including kickback/misses.
- **DON'T LET GO!** — normal attack, followed on a hit by up to three additional attacks at 35% weapon damage each, including normal character modifiers. A miss ends the sequence. After each successful follow-up, a 25% jam roll either ends the sequence and prevents parrying for 1 round or allows the next follow-up. Costs 2 AP total; cooldown 3 rounds from use.
- **DA BUTTON WUZ RED!** — 150% weapon damage with normal character modifiers in a one-cell circle around the user, including allies and excluding the user. After the attack, one equally weighted roll produces no mishap, a jammed motor for 1 round, or self-knockdown for 1 round. A jammed motor blocks all three Chain Choppa specials while normal strike/cleave remain available. Costs 3 AP; cooldown 4 rounds from use.

Base damage is deliberately below Choppa to account for higher penetration and follow-up attacks. Special percentages multiply weapon damage with the normal character modifiers before target defences. Raw tier damage is not final hit damage: attribute bonuses, talents, buffs and target defences still apply. Compare attacks with the same target and buff state. Acquire **Chain Choppa** from DA LOOT BOX, or through ToyBox, blueprint ID `fabce84f9db74c7b8fdb024b191491be`. The chain remains static geometry.

Additional in-game edge-case validation: verify kickback replaces rather than adds to the enemy attack; follow-up hits/misses, AP usage, one-round penalty expiry, friendly fire, one mishap per red-button use, cooldowns, both hands, all six upgrades and save/load.

## Power Klaw progression

The Power Klaw now has six automatically upgrading tiers and five attacks, unlocked at V1, V3 and V5.

| Variant | Levels | Damage | Armour penetration | New abilities |
| --- | ---: | ---: | ---: | --- |
| Power Klaw | 1–15 | 8–13 | 30% | KRUMP!, SWEEP!, GET BACK! |
| Kustom Power Klaw | 16–25 | 10–14 | 30% | — |
| 'Urty Power Klaw | 26–35 | 18–27 | 35% | ARMOUR? WOT ARMOUR?! |
| Killy Power Klaw | 36–43 | 22–32 | 40% | — |
| Dead Killy Power Klaw | 44–49 | 27–41 | 45% | DA BIG SQUEEZE! |
| Da Boss's Klaw | 50–55 | 34–50 | 50% | — |

- **KRUMP!** — normal single-target attack, 1 AP.
- **SWEEP!** — normal damage across the three tiles in front of the wielder, 2 AP.
- **GET BACK!** — normal single-target attack; on a hit, pushes the target one cell away without provoking attacks of opportunity, 2 AP.
- **ARMOUR? WOT ARMOUR?!** — normal single-target attack; on a hit, both target and wielder lose all armour absorption against all attackers for two rounds. Deflection, dodge and parry are unchanged. Costs 2 AP, with a three-round cooldown from use, including misses. A miss applies neither armour effect.
- **DA BIG SQUEEZE!** — a single-target attack with doubled base weapon damage; after the attempt the wielder is knocked prone for one round, including on a miss. Costs 2 AP, with a four-round cooldown from use. Does not automatically knock down the target.

Existing Power Klaws retain their V1 blueprint identity and join the same automatic upgrade system as Choppa and Big Choppa. Upgrades happen outside combat. All tiers retain the accepted model, fit, materials and original human fist animation mapping in either hand. A separate empty holster/sheath model hides the Klaw while sheathed; drawing the weapon restores the normal model. Power Klaw V1 ToyBox blueprint ID: `89d3b9e7c7134f7f94c36518212b8596`.

Runtime acceptance for 0.5.3 remains pending: verify hit/miss effects, cooldown timing, two-round expiry and armour restoration (including armour above 100%), main/off-hand attacks, level transitions, and save/load. A successful build alone does not confirm these in-game behaviours.

## Choppa

The first completed weapon is the **Choppa**, a custom-modelled one-handed Ork axe with six variants that automatically upgrade as the party levels.

| Variant | Levels | Damage | Armour penetration | New ability |
| --- | ---: | ---: | ---: | --- |
| Choppa | 1–15 | 9–15 | 15% | Strike, Cleave, WAAAGH! |
| Sharp Choppa | 16–25 | 11–16 | 15% | — |
| 'Urty Choppa | 26–35 | 20–30 | 20% | WAAAAAAAAAAGH! |
| Killy Choppa | 36–43 | 24–36 | 20% | — |
| Dead Killy Choppa | 44–49 | 30–46 | 25% | SLAUGHTER |
| Da Bestest Choppa | 50–55 | 38–55 | 30% | — |

Only the V1 weapon needs to be acquired. Copies carried by party members and stored in the inventory automatically advance to the appropriate variant when the party reaches a new level range.

## Abilities

- **Strike** — a standard 1 AP single-target melee attack.
- **Cleave** — a 2 AP area melee attack.
- **WAAAGH!** — a 1 AP charge followed by a Choppa strike; available from V1.
- **WAAAAAAAAAAGH!** — a deafening area war cry. Enemies must pass a Toughness resistance test or become stunned for one round. Usable once per combat and available from V3.
- **SLAUGHTER** — strikes enemies in the target area for 150% weapon damage, then grants a two-round slaughter-frenzy: +20% damage and +10 Weapon Skill at the cost of −20% parry and dodge. Costs 2 AP, is usable once per combat and becomes available from V5.

The special abilities use dedicated base-game icons: Charge, Taunting Scream and Revel in Slaughter. Strike uses a conventional axe-strike icon.

## Visuals

- Original Choppa model and textures created specifically for this mod.
- Choppa uses the `BrutalOneHanded` animation style.
- Power Klaw uses `Fist` with three original human punch variants per hand. Its runtime patch preserves the original damage/sound event tracks and applies only to the Power Klaw blueprints across all six tiers.
- Worn Ork-metal finish with restrained metallic reflections; material masks use the Owlcat/Lit channel layout.
- Custom inventory icons rendered from our own weapon models, including the Power Klaw.
- Distinct native game ability icons for the Klaw: claw strike, sweeping attack, knockback, armour removal and crushing; the armour debuff shares its ability icon.

## Installation

Install and enable [MicroPatches](https://github.com/microsoftenator2022/MicroPatches/releases) first. Our development baseline is runtime version **1.21.1**, with **Load OwlMod BlueprintDirectReferences dependencies** enabled and experimental patches disabled. Use the runtime mod ZIP, not the editor installer, for the game.

On **macOS**, install `ArmouryOrks-1.0.0-macOS.zip` through Mac-ModInstaller to retain its backup and activation behaviour. Enable **Armoury: Orks! WAAAAAGH!** and completely restart the game.

On **Windows**, install `ArmouryOrks-1.0.0-Windows.zip` as an **Owlcat modification**, not as a Unity Mod Manager mod. With the game closed, extract the ZIP contents into `%USERPROFILE%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\Modifications\ArmouryOrks`. The manifest must be directly at `ArmouryOrks\OwlcatModificationManifest.json`, without an extra nested folder. Add `ArmouryOrks` to the `EnabledModifications` array in `OwlcatModificationManagerSettings.json` in the game's data directory, preserving all other settings and enabled mods. A compatible Owlcat mod manager can do the installation/activation instead. Completely restart the game after updating.

MicroPatches is installed separately using its runtime installation instructions. Keep **Load OwlMod BlueprintDirectReferences dependencies** enabled and experimental patches disabled. Adeptus Custodes, other armoury mods and ToyBox are not dependencies. Back up saves before installing or updating mods. Avoid removing this mod from saves that contain its items.

For testing, optionally add the V1 item through ToyBox by searching for **Choppa**. Blueprint ID: `9f1a0cab53be493b96989c9a672d2ae5`.

## Development

Use MicroPatches as the baseline for future development and in-game validation. Preserve the existing MicroPatches editor/build integration in the local Owlcat template; check compatibility before updating it. Verify generated bundle dependencies, weapon visuals, abilities, progression, and save/load behavior in the target game version. MicroPatches 1.21.1 disables the old ability-variants action-bar fix; do not rely on that fix.

The repository contains the mod-specific source assets, generated blueprints and generator/runtime code. The local Owlcat Rogue Trader mod template, extracted vanilla assets, Unity caches and build output are intentionally excluded.

All weapon models and textures added by Armoury: Orks are original assets made for this mod; models from other mods are not reused.

The Power Klaw inventory image can be reproduced with `blender --background --python scripts/render_power_klaw_icon.py`. It renders the original FBX and textures to a transparent 512×512 PNG; ability icons reference native game sprites.

## Automated validation

Run `python3 -m unittest discover -s tests -v` to check the generated Power Klaw tiers, ability slots, hit/miss effects, armour cap, cooldowns, localization, and runtime tier identifiers. Set `ORK_MOD_ZIP=Releases/ArmouryOrks-1.0.0-macOS.zip` to also validate the built package. These checks complement the in-game validation described above.

The Chain Choppa game FBX and inventory icon can be reproduced from `SourceAssets/ChainChoppa/ChainChoppa_60k.blend` with `blender --background --python scripts/prepare_chain_choppa.py`.

The Meshy chest can be rebuilt with `blender --background --python scripts/prepare_ork_chest.py -- /path/to/Meshy_folder`. The reduced editable source, comparison previews, original texture copies and reduction report are in `SourceAssets/ArmouryCache/Meshy/`. The original supplied FBX is not modified.

## Building release packages

Use Unity 6000.0.64f1 with the existing Owlcat/MicroPatches editor integration and the required macOS/Windows build support modules. `ChoppaGenerator.Build` requires the macOS target (`-buildTarget osx`); `ChoppaGenerator.BuildWindows` requires Windows 64-bit (`-buildTarget win64`). Both generate version 1.0.0 using the official builder. Copy each resulting `Build/ArmouryOrks.zip` to its platform-labelled release filename before starting the next build.

Validate each package with the Python test suite and `scripts/validate_release.py --platform windows` or `--platform macos` (requires UnityPy in the validation environment). The latter inspects the actual serialized Unity target in every bundle. Preserve checked-in prefab/material files and `.meta` files; large packed textures/meshes remain generated build inputs. Historical-release comparisons are skipped when their old ZIPs are absent.
