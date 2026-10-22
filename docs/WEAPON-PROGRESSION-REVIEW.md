# Weapon progression review — 2026-10-22

## Confirmed issues

The original controller permanently remembered every collection received through global item-added and item-removed events. This included vendors, loot and potentially collections belonging to a previously loaded save because the controller survives scene changes. Collection scanning therefore did not establish player ownership.

Replacement unequipped and extracted the original before `collection.Add(target)` created its successor. Creation/add exceptions could leave the original detached from all saved inventories. Slot insertion returned no success value, yet the controller always logged success. Inventory and body scans also overlapped; there was no explicit per-pass item deduplication. The detached old entity could be visited again through a body snapshot or inconsistent slot state. Broad reflection over members containing “Party” was not an ownership contract and omitted explicitly named remote companions.

## Verified installed API behavior

Inspected method IL from the installed game's `WH40KRT.app/Contents/Resources/Data/Managed/Code.dll` using the existing Mono.Cecil inspection utility. SHA-256: `446ebca34e768cb157e1353be60a27957288f6422b3c91a3be708e9e13036660`.

- `ItemEntity.Collection` and `HoldingSlot` are separate references. Equipping normally retains collection membership.
- `ItemsCollection.Add(BlueprintItem)` creates an entity, then delegates to `Add(ItemEntity, bool noAutoMerge)`.
- `Add(ItemEntity, true)` skips merging. Add inserts before raising item-added events; exceptions from those callbacks can occur after mutation. Other Add branches can convert/dispose special resource items and return null. The controller checks exact identity and membership rather than assuming delivery.
- `ItemsCollection.Remove(item, count = null)` extracts the full entity when count is omitted. It does not dispose it. Extract clears collection membership, updates indices/weight, and can unequip it. Removal events occur after extraction, so an exception does not establish that removal failed.
- `ItemSlot.RemoveItem(false, false)` still raises equipment events (the two arguments control automatic merge and force, not event dispatch). It clears slot/holding references but retains inventory membership. It can return false due to removal restrictions.
- `ItemSlot.InsertItem(item, false)` returns void. It checks eligibility and owner inventory compatibility and can log and return without insertion. Later equip callbacks may throw after changes. Neither this method nor collection Add/Remove is a verified transaction.
- `Player.PartyAndPets`, `PartyAndPetsDetached`, and `RemoteCompanions` are explicit rosters maintained by `UpdateCharacterLists`; detached roster entries are checked for disposal by that method.

## Changes

The controller now gathers the current player's inventory, shared stash and explicit companion inventories on every pass. It adds body items, deduplicates entity references, and rejects vendor collections, unowned equipment slots, stale references, detached originals and unusual item counts. No collection references are cached across saves.

Replacement uses a separately created entity and disables automatic merging. It verifies delivery into the original collection before unequipping anything, checks slot eligibility, verifies both slot references after insertion, and only then extracts the old item. Ownership is checked again after callbacks. An exception after successful original extraction is distinguished from one before extraction.

Before commit, cleanup removes and disposes only the known replacement and attempts to restore the original equipment slot. If insertion is refused, the original remains in its inventory. If cleanup itself throws, progression stops for the rest of the session and logs the failure, preventing repeated creation. This is deliberately not described as atomic.

No blueprint GUIDs, level thresholds, weapon data, abilities, serialized fields or dependencies changed. Unusual stacked weapon entities are left alone rather than accidentally converting multiple items into one. Existing replacement behavior still does not migrate arbitrary third-party instance modifications/enchantments to the new entity.

## Validation

`python3 -m unittest discover -s tests -v` runs the source contracts and the new compiled C# controller harness. The harness substitutes explicitly limited game APIs; it tests controller decisions and recovery, not native events or serialization. It covers equipped/shared inventory, stash, active/remote/detached companions, combat deferral, missing/already upgraded weapons, stacks, vendor/ground collections, repeated checks, creation/Add failures, exceptions after Add, rejected/throwing insertion, locked slots, removal failures and exceptions after extraction. It also moves the original to another collection from an Add callback and checks that it is not extracted from the wrong collection.

`tests/progression/compile_installed_api.py MANAGED MONO_BLEEDING_EDGE` compiles the actual controller against installed game assemblies. Full modern-language runtime compilation is performed by the existing Unity build; the older standalone Mono compiler is used only for the controller probe/harness.

## Remaining runtime risks and in-game checklist

Arbitrary failing game/mod callbacks can interrupt both an operation and its cleanup. The original is retained until replacement delivery, but if cleanup also fails, both entities or partially changed equipment can remain. The session stop is not serialized: do not save such a state; reload the last good save and inspect the logged error. Native event side effects, equipment visual refresh and save/load behavior require game testing.

1. In a disposable save, put lower-tier copies of all four weapon families in shared inventory and stash, and equip the player and active/remote companions, including inactive weapon sets. Record counts and slots.
2. Cross each threshold (16, 26, 36, 44, 50), including a multi-tier jump. Confirm one successor per original, matching owner/slot, expected stats/abilities, and unchanged higher-tier items. Wait several polling intervals and transfer/equip items to check there is no duplication.
3. Leave lower-tier copies with a vendor and on the ground. Open/trade/loot unrelated items: those copies must remain unchanged until acquired into an eligible inventory.
4. Trigger progression in combat: replacement must wait until combat ends. Test locked equipment; the original must remain and later upgrade after unlocking.
5. Save after a successful upgrade, reload, change area, dismiss/recruit companions and switch directly between two saves. Confirm counts/slots, absence of old-save mutations, working abilities and no repeat replacements.
6. With a development-only fault injector, make entity creation/Add fail, reject InsertItem, and throw from item/equipment callbacks before and after mutation. Confirm the original remains accessible, failed replacements are cleaned up when APIs allow, and recovery failure logs once and prevents further upgrades. Do not save a deliberately corrupted state.

## Initial validation results (superseded by package follow-up below)

- Installed-assembly compile probe: passed for the final controller.
- Source/test-double suite: 29 tests, 25 passed and 4 package checks skipped when no ZIP is supplied. The compiled controller harness covers 19 scenarios.
- Existing macOS Unity build (`ArmouryOrks.Editor.ChoppaGenerator.Build`): exit 0, version 1.0.1 ZIP generated. Verified that the ZIP's runtime DLL contains the final callback-movement guard. Initial staging sources were stale; synced current repository assets into the existing build workspace before the final build. No packages or build settings were upgraded. No mod was installed or published.
- With the final ZIP supplied: 28/29 tests pass. The existing Power Klaw package test fails on ordering alone in `Data.m_Overrides` for `PowerKlaw_V5_HiddenSqueeze_Item.jbp` and `PowerKlaw_V6_HiddenSqueeze_Item.jbp`; recursively compared all Power Klaw blueprint JSON and found no other differences. Left those unrelated assets/tests unchanged.
- `scripts/validate_release.py` could not run because the available Python environment lacks UnityPy; no dependency installation was performed.
- Unity reports assembly-hashing diagnostics involving the existing `0Harmony.dll`, and Mono internal-call/shutdown warnings, but proceeds through compilation and packaging and exits successfully. Full build log: `/tmp/orks-progression-build-final.log`. These diagnostics were not addressed by changing dependencies.
- Windows packaging and actual game/save/load execution were not performed. This build is validation output, not a verified release.

## Package follow-up — 2026-10-22

The Power Klaw mismatch was fixed in the generator rather than relaxing the package test. In the two hidden DA BIG SQUEEZE! weapon clones, `Override(hidden, "Family", "Power")` now runs immediately after cloning, matching the checked-in override-list order. This moves one existing assignment; all blueprint values, components, abilities, tier thresholds and runtime behavior remain unchanged. All 15 generated Power Klaw blueprints now match the source JSON exactly, including array order.

UnityPy 1.25.4 was installed only in `/tmp/orks-package-validation` to run the existing release validator. No project dependencies or Unity packages were changed. New packages are kept under `Releases/weapon-progression-review/` so previously built release files remain intact.

Follow-up validation completed successfully:

| Check | macOS | Windows |
| --- | --- | --- |
| Existing Unity build method | Exit 0 | Exit 0 |
| Full suite with package supplied | 29/29 passed, no skips | 29/29 passed, no skips |
| Existing release validator | Passed; Unity target 2 | Passed; Unity target 19 |
| All packaged blueprint JSON vs source | 131/131 exact matches | 131/131 exact matches |
| Final progression controller included | Verified | Verified |

The installed-game API compile probe and whitespace/diff checks also passed. Build logs: `/tmp/orks-package-macos.log` and `/tmp/orks-package-windows.log`. Existing Unity assembly-hashing, shader and Mono shutdown diagnostics remain; neither build required dependency changes and both exited successfully.

Artifacts (version preserved at 1.0.1):

- `Releases/weapon-progression-review/ArmouryOrks-1.0.1-macOS.zip`, SHA-256 `3c80afa20221e9107a12c1ca3c8a624ecf7d7c0b7c7d655d95d362ce8d2b9752`.
- `Releases/weapon-progression-review/ArmouryOrks-1.0.1-Windows.zip`, SHA-256 `de49387f41a93e649365480c331cfe161364b561c772cac5e19817ad2cc33fae`.
- `Releases/weapon-progression-review/SHA256SUMS.txt`.

No installation, publication or actual game/save/load test was performed. The runtime limitations and in-game checklist above still apply.

## Release 1.0.2 — 2026-10-22

Rebuilt both platforms with manifest version 1.0.2 for publication. Both Unity build methods exited 0. Each package passed all 29 tests with no skips, the release validator confirmed macOS target 2 / Windows target 19, and all 131 packaged blueprints exactly matched source JSON. Verified inclusion of the final progression-controller guard. The installed-game API compile probe passed. Runtime and save/load limitations above remain applicable.

- macOS: `ArmouryOrks-1.0.2-macOS.zip`, SHA-256 `e613594f21ec59c36bb14b567823348eba8ece8b221640413263b21333d5bd98`.
- Windows: `ArmouryOrks-1.0.2-Windows.zip`, SHA-256 `93e80746ffe27dbd3ad94fa1feb3b619c699fd0d4ef549d3619f9a86d20a7f90`.
- Build logs: `/tmp/orks-release-1.0.2-macos.log` and `/tmp/orks-release-1.0.2-windows.log`.
