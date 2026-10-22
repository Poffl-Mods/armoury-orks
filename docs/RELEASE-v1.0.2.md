# Armoury: Orks! WAAAAAGH! — v1.0.2

Automatic weapon upgrades are safer. The replacement is created and delivered before the original weapon is extracted, and equipped replacements are checked against their original slot. Failed upgrades attempt cleanup and slot restoration; if cleanup itself fails, further upgrades stop for the session.

Upgrades now use the current player inventory, shared stash and companion inventories. Vendor, ground and unrelated collections are excluded. Duplicate scans and stale item references are checked explicitly, including ownership changes during inventory callbacks.

Weapon stats, abilities, level thresholds, blueprint identities and save formats are unchanged. The hidden Power Klaw generator now matches the source override ordering without changing weapon values.

Both platform packages are rebuilt. Automated package, blueprint and platform checks pass. The new recovery paths and save/load behavior still require in-game verification. Inventory operations are not atomic: a failing game or mod callback can also interrupt cleanup. If an upgrade recovery error is logged, reload the last good save rather than saving the partial state.

## Downloads and installation

- **macOS:** Install `ArmouryOrks-1.0.2-macOS.zip` with Mac ModInstaller.
- **Windows:** Install `ArmouryOrks-1.0.2-Windows.zip` with Modfinder.
- **Checksums:** `SHA256SUMS.txt`.

Requires [MicroPatches](https://github.com/microsoftenator2022/MicroPatches/releases). Update the existing installation and restart the game.
