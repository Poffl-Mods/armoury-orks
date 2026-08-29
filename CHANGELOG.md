# Changelog

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
