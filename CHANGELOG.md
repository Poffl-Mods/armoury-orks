# Changelog

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
