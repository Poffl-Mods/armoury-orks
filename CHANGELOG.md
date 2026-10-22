# Changelog

## 1.0.2 — 2026-10-22

- Deliver and verify upgraded weapons before extracting the originals; check equipment insertion and clean up failed replacements. Stop further upgrades if recovery itself fails.
- Discover current player, stash and companion inventories explicitly instead of retaining collections from global events. Exclude vendors and unrelated inventories, deduplicate scans and recheck ownership after callbacks.
- Preserve weapon stats, abilities, progression thresholds, blueprint identities and save formats.
- Match generated hidden Power Klaw override ordering to source without changing weapon values or weakening package checks.
- Add installed-game API compilation and focused upgrade failure-path coverage. Build and validate separate macOS and Windows packages; actual game/save/load testing of the new recovery paths remains pending.

## 1.0.1 — 2026-10-21

- Classified all Power Klaw tiers and the internal DA BIG SQUEEZE! weapons as Power weapons so Power Weapon Specialization applies. Power classification confirmed in-game on macOS by the user.

## 1.0.0 — 2026-10-20

- Use consistent Ork War Chest names for authoring folders, reference files, internal scene objects and source metadata. Saved blueprint and prefab identities are unchanged.

- First full release: Choppa, Big Choppa, Power Klaw and Chain Choppa, six tiers each, automatic progression and English Ork-themed abilities.
- Includes the persistent DA LOOT BOX armoury cache with the accepted 40,000-triangle Ork War Chest, updated weapon materials and the tested Chain Choppa/Crushing Assault fixes.
- Separate Windows 64-bit and macOS packages, with platform-specific Unity AssetBundle validation and SHA-256 checksums. Windows in-game verification remains pending; macOS pre-release gameplay and visuals were accepted by the user.
- Added platform installation notes and release-build entry points. Preserve Unity reference metadata in source control and keep historical-release regression checks optional for fresh clones.

## 0.7.2 — 2026-10-20

- Replaced the placeholder cache geometry with the user-provided Ork War Chest, reduced from 739,366 to 40,000 triangles (94.59% fewer). Reprojected all PBR channels, including a high-to-low normal bake, to retain surface detail without UV distortion.
- Kept 2K textures and adapted the materials to Owlcat/Lit. Scaled the closed model to 1.02 m wide, grounded its origin and fitted the loot collider to its bounds.
- Preserved cache blueprint/prefab identity, unique ID, position, interaction and saved contents; no refilling or replacement of the saved entity. Weapon visuals and balance remain as in 0.7.1.
- Added reproducible Blender preparation script, reduced editable source, previews and source hash/report. Supplied original files remain untouched. In-game appearance and interaction still require verification.

## 0.7.1 — 2026-10-20

- Increased shine on all four weapon families: roughness multiplier 0.90 → 0.62 on Choppa, Big Choppa and Chain Choppa; 0.68 → 0.50 on Power Klaw. Metallic multiplier 0.80 → 0.90 throughout.
- Preserved authored material masks, base colours, normal maps, geometry, placement, abilities, stats and the cache. Actual in-game appearance still requires visual confirmation.
- Recorded user confirmation that the 0.7.0 armoury cache works in-game.

## 0.7.0 — 2026-10-20

- Added DA LOOT BOX, an independent native saved loot container in the Warrant Chamber with one V1 weapon from each of the four Ork weapon families. Existing progression handles party-level upgrades.
- Use saved area entities for duplicate detection, keep empty containers persistent and seed contents only at first creation. Resolve all weapon blueprints before spawning.
- Added original low-poly cache geometry/materials, a collider, native loot interaction and English marker/description. Adeptus Custodes is not a dependency. Placement and persistence still require in-game verification.
- Removed temporary Chain Choppa damage-audit logging after user acceptance of 0.6.4. Retained combat fixes, save-compatible legacy IDs, tier stats and accepted weapon visuals.

## 0.6.4 — 2026-10-20

- Fixed Crushing Assault being consumed by the Chain Choppa special-ability wrapper before its scheduled weapon attack. Only that native buff and the 12 unlocked Chain Choppa wrappers are affected.
- Actual child attacks retain native buff consumption: the initial bite consumes it before later bites; the circular attack retains it across its damage targets. A self-kickback without a weapon attack does not consume it.
- Normal attacks and raw tier damage are unchanged. Added executable engine contract checks for all wrappers, child stages and unrelated buff/weapon boundaries. In-game bonus application and consumption still require verification.

## 0.6.3 — 2026-10-20

- Confirmed in-game: slots 3/4 play chainsword audio and slot 5 deals area damage. High damage from slots 1/2 remains unresolved.
- Expanded damage diagnostics to include minimum/maximum base-damage bonus sources, resulting base values and weapon slot. Previous diagnostics only captured shared percentage/flat modifiers and could not explain the discrepancy.
- Limit diagnostics independently per attack ability so normal attack previews cannot consume the entire session allowance. No weapon balance or damage changes in this diagnostic build.

## 0.6.2 — 2026-10-20

- Fixed DA BUTTON WUZ RED! being rejected with CannotTargetSelf. The inner attack accepts its caster as the circle centre; native melee pattern delivery still excludes the caster from damage targets.
- Restored native chainsword FX/sounds on slots 3–5 and the special ability blueprints.
- Child attacks now preserve the actual equipped item and hand. Removed virtual weapon overrides from live special attacks; legacy hidden-item IDs remain for save compatibility.
- Apply 150% / 100% / 35% factors through the native multiplicative damage manager, preserving normal character bonuses. Normal attacks receive no custom multiplier and raw V1–V6 damage is unchanged.
- Added native engine contract checks and bounded damage diagnostics for the reported high normal-hit damage. Live sound, area damage and final damage comparison still require an in-game retest.

## 0.6.1 — 2026-10-20

- Changed Chain Choppa ability and status names to English Ork wording: DON'T LET GO!, DA BUTTON WUZ RED!, DA MOTOR'S DED! and CHAIN'S STUCK!. CHAAAAAAAAAAIN! is unchanged.
- Updated the motor-jam description to use the English ability names. Mechanics, damage, progression and visuals are unchanged.

## 0.6.0 — 2026-10-20

- Added six Chain Choppa tiers and automatic progression at party levels 16, 26, 36, 44 and 50. Preserved the V1 identity and accepted 0.5.8 visual placement.
- Balanced base damage at 8–13 / 10–16 / 18–27 / 22–33 / 27–41 / 34–50, with 20 / 20 / 25 / 25 / 30 / 35% armour penetration.
- Added CHAAAAAAAAAAIN! at V1 (150% base damage or 20% self-kickback), NICH' LOSLASS'N! at V3 (up to three 35% follow-ups, with miss/jam termination) and DA KNOPF WAR ROT! at V5 (friendly-fire circular attack and one random motor mishap).
- Uses finite native action graphs, free child attacks, explicit cooldowns and one-round self-penalties. Standard strike and cleave remain available when the motor jams.
- In-game verification of random outcomes, attack sequencing, AP, friendly fire, buff expiry, progression and save/load remains pending.

## 0.5.8 — 2026-10-20

- Rotated Chain Choppa another 180 degrees around its blade axis to reverse the cutting-edge direction shown in the 0.5.7 in-game test.
- Retained the 60,000-triangle model, size, hand attachment, item identity and mechanics. In-game confirmation remains pending.

## 0.5.7 — 2026-10-20

- Rebuilt Chain Choppa directly from the original 912,580-triangle mesh at 60,000 triangles and rebaked the normal map.
- Moved the hand attachment approximately 6 cm toward the blade and rolled the cutting edge 90 degrees around the blade axis. Both adjustments are baked into the game mesh. Overall length remains 1.04 m.
- Regenerated the inventory icon. Item identity, native chainsword mechanics and all previous weapon blueprints are preserved.
- In-game confirmation of the adjusted grip and cutting-edge direction remains pending.

## 0.5.6 — 2026-10-20

- Added the original Chain Choppa as an independent one-handed chainsword, using the reduced 25,000-triangle model and rebaked normal map.
- Added a grip-centred FBX, Owlcat material masks, custom inventory icon and localized item text.
- Inherits native chainsword attacks, restrictions, sounds and 7–9 damage / 10% armour penetration; no custom abilities or progression yet.
- Preserved all existing weapon blueprints, including the larger two-handed Big Choppa from 0.5.5. In-game visual and combat verification remains pending.

## 0.5.5 — 2026-10-20

- Lengthened the Big Choppa by 25% along its shaft and enlarged its width/depth by 10%, around the existing hand socket. Corrected normals and tangents for the non-uniform scaling.
- Switched all six tiers and twelve hidden attack weapons to native Great Sword inheritance and BrutalTwoHanded handling, keeping the custom axe model and existing mechanics.
- In-game verification of grip, idle, attacks and back placement remains pending.

## 0.5.4 — 2026-10-20

- Rebased all six Big Choppa tiers and their twelve hidden attack weapons on the native Omnissiah Axe, with an explicit complete visual override for axe handling instead of inherited staff visuals.
- Preserved existing weapon identities, mechanics, progression and custom models. Empty component overrides prevent inheriting native weapon bonuses.
- In-game confirmation of the drawn idle pose, attacks and sheathe/draw remains pending.

## 0.5.3 — 2026-10-19

- Added a transparent 512×512 Power Klaw inventory icon rendered from our original model and textures, matching the model-render style of Choppa and Big Choppa across all six tiers.
- Replaced placeholder axe/hammer symbols with distinct native game ability icons for claw strike, sweeping attack, knockback, armour removal and crushing. The armour debuff now shares the armour-removal icon.
- Added a reproducible Blender render script for the custom inventory artwork.

## 0.5.2 — 2026-10-19

- Removed redundant level, damage, armour penetration and ability bullet lists from all 18 weapon tier descriptions. Preserved descriptive prose, flavour text and ability descriptions.

## 0.5.1 — 2026-10-19

- Fixed Power Klaw remaining visible at the hip while sheathed. All six tiers now use an empty belt/sheath prefab through the engine's native model overrides instead of relying on holster socket-name detection.
- Preserved the drawn model, fit, materials, animations, progression and abilities.
- Added build-time validation that the holstered prefab contains no renderers. In-game sheathe/draw confirmation remains pending.

## 0.5.0 — 2026-10-19

- Added Power Klaw V1–V6 progression using the same level thresholds as Choppa and Big Choppa, preserving the original V1 blueprint identity.
- Reduced Power Klaw damage to 8–13 / 10–14 / 18–27 / 22–32 / 27–41 / 34–50, with 30 / 30 / 35 / 40 / 45 / 50% armour penetration.
- Added KRUMP! (1 AP), SWEEP! (2 AP), and GET BACK! (2 AP, one-cell push on hit) from V1.
- Added ARMOUR? WOT ARMOUR?! from V3: normal attack, 2 AP, three-round cooldown from use. On a hit, target and wielder have armour absorption capped at zero for two rounds. Deflection, dodge and parry remain unchanged. A miss consumes cooldown but applies neither debuff.
- Added DA BIG SQUEEZE! from V5: doubled base weapon damage, 2 AP, four-round cooldown from use. Knocks the wielder prone for one round after hit or miss.
- Extended the weapon-scoped main/off-hand fist animation mapping to all six tiers and added names, descriptions and flavour text.
- In-game validation of the new progression, abilities, cooldowns, durations and save/load remains pending.

## 0.4.15 — 2026-10-11

- Power Klaw uses original human main-hand/off-hand fist attack clips, including their original damage and sound event tracks, through a weapon-scoped runtime patch.
- Preserved the accepted model orientation, fit and materials. Both attack abilities were confirmed working in-game by the user. Claws remain rigid by design.

## 0.4.14 — 2026-10-11

- Replaced the Power Klaw Fist animation style with BrutalOneHanded. The human main-hand attack animation set reports no animation for Fist; the replacement uses the existing heavy one-handed melee set.
- Preserved the accepted claw prefab orientation and updated the weapon description. In-game attack verification remains pending.


## 0.4.13 — 2026-10-11

- Kept the accepted Power Klaw fore/aft pitch and moved the 20-degree lateral adjustment from socket space to model-local space. In-game fit verification remains pending.


## 0.4.12 — 2026-10-11

- Reverted the Power Klaw forward pitch adjustment from 0.4.11 and applied a separate 20-degree lateral lean in weapon-socket space. In-game fit verification remains pending.


## 0.4.11 — 2026-10-11

- Tilted the Power Klaw visual by 25 degrees relative to the previous test pose to better align the cuff opening with the forearm. In-game fit verification remains pending.


## 0.4.10 — 2026-10-11

- Corrected Choppa, Big Choppa and Power Klaw material masks for Owlcat/Lit: roughness in red, metallic in blue, with emission and thickness disabled.
- Restored metallic reflections in the authored metal regions using a restrained metallic multiplier of 0.8.
- Slightly reduced roughness: Choppa and Big Choppa from 0.95 to 0.90; Power Klaw from 0.71 to 0.68.


## 0.1.2 — 2026-08-29

First completed Choppa release.

### Weapon and visuals

- Added an original, custom-modelled one-handed Ork Choppa with custom textures and inventory artwork.
- Configured the weapon to use the `BrutalOneHanded` animation style.
- Increased material roughness to `0.95` for a worn, matte Ork-metal appearance.
- Classified the Choppa as a sword mechanically so it receives the intended melee ability set while retaining axe handling and visuals.

### Automatic V1–V6 progression

- Added six level-scaled variants covering party levels 1–55.
- Added automatic upgrading for equipped, carried and inventory-stored copies.
- Added the following variants and statistics:
  - V1 **Choppa** (levels 1–15): 9–15 damage, 15% armour penetration.
  - V2 **Sharp Choppa** (levels 16–25): 11–16 damage, 15% armour penetration.
  - V3 **'Urty Choppa** (levels 26–35): 20–30 damage, 20% armour penetration.
  - V4 **Killy Choppa** (levels 36–43): 24–36 damage, 20% armour penetration.
  - V5 **Dead Killy Choppa** (levels 44–49): 30–46 damage, 25% armour penetration.
  - V6 **Da Bestest Choppa** (levels 50–55): 38–55 damage, 30% armour penetration.
- Added individual Ork-style names, descriptions and flavour text for every variant.

### Abilities

- Added **Strike**, a 1 AP single-target melee attack, to all variants.
- Added **Cleave**, a 2 AP area melee attack, to all variants.
- Added **WAAAGH!** from V1: a 1 AP charge followed by a Choppa strike.
- Added **WAAAAAAAAAAGH!** from V3: a once-per-combat area war cry that forces enemies to pass a Toughness resistance test or become stunned for one round.
- Added **SLAUGHTER** from V5: a once-per-combat, 2 AP area attack dealing 150% weapon damage and applying a two-round slaughter-frenzy.
- Slaughter-frenzy grants +20% damage and +10 Weapon Skill while reducing parry and dodge chance by 20%.
- Added once-per-combat cooldown handling for WAAAAAAAAAAGH! and SLAUGHTER.

### Icons and fixes

- Added the Charge icon to WAAAGH!.
- Added the Taunting Scream icon to WAAAAAAAAAAGH!.
- Added the Revel in Slaughter icon to SLAUGHTER.
- Replaced Strike's chain-axe artwork with a conventional axe-strike icon.
- Added a dedicated Strike name and description shared by all V1–V6 variants.
