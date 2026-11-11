# Infinite Stamina

Press F6 to toggle infinite stamina for every action. Enable activity filters if you only want it for certain activities.

## Installation

Install with a mod manager, or place `InfiniteStaminaMod.dll` in `BepInEx/plugins/HexInfiniteStamina/`.

Requires BepInExPack Valheim `5.4.2360` or newer.

## Configuration

Edit `BepInEx/config/com.hex.infinitestamina.cfg`, or use Configuration Manager. Changes apply without restarting.

```ini
[General]
DebugLogging = false
ModEnabled = true
ToggleKey = F6

[Stamina Filters]
OnlySelectedActivities = false
NoRunningStaminaDrain = false
NoSwimmingStaminaDrain = false
NoEncumberedStaminaDrain = false
NoBuildingStaminaDrain = false
NoCultivatorStaminaDrain = false
NoHoeStaminaDrain = false
NoPickaxeStaminaDrain = false
```

- `ModEnabled = false`: vanilla stamina for everything.
- `ModEnabled = true`, `OnlySelectedActivities = false`: every action is free. Filter values are ignored.
- `ModEnabled = true`, `OnlySelectedActivities = true`: only enabled Stamina Filters are free. Everything else uses vanilla stamina.
- Building includes repairs and removing pieces. Pickaxes include mining.
- Activity filters start off. Enable OnlySelectedActivities, then choose your activities.
- In filtered mode, unlisted actions such as jumping and dodging use vanilla stamina.
- Changing an active stamina setting refills stamina. The bar hides while infinite stamina applies.
- `DebugLogging` defaults to false and stays first in Configuration Manager.

Old settings migrate automatically. Saved choices are kept. New configs start with full infinite stamina, filters off, and logging off.

## Multiplayer

Client-side only. No server installation needed.

## Support & Feedback
[Discord](https://discord.gg/wU2FXD94v4)

## Source Code
[GitHub](https://github.com/guillenjgg/valheim-infinite-stamina)
