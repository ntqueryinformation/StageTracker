# Changelog

## 1.0.0
- Initial release.
- Loot panel: full interactable census (base game through Hallowed Concepts), no teleporter-phase gating.
- Drone panel: drones by type and tier, triple drone shop, drone scrapper/combiner, upgraded broken drones included.
- Hold Tab / Always-visible display modes.
- Per-category toggles in the in-game options menu (Risk of Options).
- "Show Used Over Total" option: every counter reads like `Chest: 5/6` even when nothing is used yet (on by default).
- Multishop terminals and the Newt Altar are detected reliably (their behaviour component lives on the prefab root, not the interactable child).
- Gunner turrets are tracked only in the drone panel, where they belong.
- Unknown interactables are still listed, grouped by name, so future content is never invisible — and each one is logged once so gaps are easy to close.

## 1.0.1
- Chests are now split by kind: small chest, large chest, legendary chest, and damage/healing/utility category chests — detected from each chest's actual drop data, not name guessing.
- Crashed multishop terminals (the free "capsules") are their own category, separate from paid multishop terminals.
- Shipping request pods and Aurelionite geodes/fragments recognized by display name.
- New chests appear as individual toggles in the options menu.
- `stage_tracker_scan` now dumps each chest's drop table, item tag, and tier chances.

## 1.0.2
- Fixed: gunner/medic/missile/etc. drones were missing from the drone panel. Broken drones are matched against each DroneDef's broken-variant spawn card now, so every purchasable drone shows by type and tier — including combiner-upgraded ones. Anything the catalog doesn't cover still appears (gray, grouped by name) and is logged once.
