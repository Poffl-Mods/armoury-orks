# Armoury: Orks

Armoury: Orks adds custom-built Ork weapons for player use in *Warhammer 40,000: Rogue Trader*.

## Version 0.1.2 — Choppa

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
- Power Klaw uses `Fist` with three original human punch variants per hand. Its runtime patch preserves the original damage/sound event tracks and applies only to the Power Klaw blueprint.
- Worn Ork-metal finish with restrained metallic reflections; material masks use the Owlcat/Lit channel layout.
- Custom inventory icon.

## Installation

Install and enable [MicroPatches](https://github.com/microsoftenator2022/MicroPatches/releases) first. Our development baseline is runtime version **1.21.1**, with **Load OwlMod BlueprintDirectReferences dependencies** enabled and experimental patches disabled. Use the runtime mod ZIP, not the editor installer, for the game.

Download `ArmouryOrks.zip` from the latest GitHub release and add the ZIP to ModFinder. Enable **Armoury: Orks! WAAAAAGH!** and completely restart the game after installing or updating the mod.

For testing or existing campaigns, add the V1 item through ToyBox by searching for **Choppa**. Blueprint ID: `9f1a0cab53be493b96989c9a672d2ae5`.

## Development

Use MicroPatches as the baseline for future development and in-game validation. Preserve the existing MicroPatches editor/build integration in the local Owlcat template; check compatibility before updating it. Verify generated bundle dependencies, weapon visuals, abilities, progression, and save/load behavior in the target game version. MicroPatches 1.21.1 disables the old ability-variants action-bar fix; do not rely on that fix.

The repository contains the mod-specific source assets, generated blueprints and generator/runtime code. The local Owlcat Rogue Trader mod template, extracted vanilla assets, Unity caches and build output are intentionally excluded.

All weapon models and textures added by Armoury: Orks are original assets made for this mod; models from other mods are not reused.
