# Infinite Stamina

Press F6 to toggle the mod. Choose which activities use no stamina.

## Installation

Install with a mod manager, or place `InfiniteStaminaMod.dll` in `BepInEx/plugins/HexInfiniteStamina/`.

Requires BepInExPack Valheim `5.4.2360` or newer.

## Configuration

Edit `BepInEx/config/com.hex.infinitestamina.cfg`, or use Configuration Manager. Changes apply without restarting.

```ini
[General]
DebugLogging = true
ModEnabled = true
ToggleKey = F6
NoRunningStaminaDrain = true
NoSwimmingStaminaDrain = true
NoEncumberedStaminaDrain = true
NoBuildingStaminaDrain = true
NoCultivatorStaminaDrain = true
NoHoeStaminaDrain = true
NoPickaxeStaminaDrain = true
```

- `ModEnabled` is the master switch. Turn it off for normal stamina use.
- While enabled, each activity follows its own setting: true means free stamina; false means normal cost.
- Building includes repairs and removing pieces. Pickaxes include mining.
- Swimming has its own NoSwimmingStaminaDrain setting. Actions without a setting, such as jumping and dodging, behave like vanilla.
- Changing an active stamina setting refills stamina. The bar hides while infinite stamina applies.
- `DebugLogging` controls mod logs and stays first in Configuration Manager.

Old config keys migrate automatically. Existing choices are kept.

## Multiplayer

Client-side only. No server installation needed.

## Support & Source

## Support & Feedback
[Discord](https://discord.gg/wU2FXD94v4)

## Source Code
[GitHub](https://github.com/guillenjgg/valheim-infinite-stamina)
