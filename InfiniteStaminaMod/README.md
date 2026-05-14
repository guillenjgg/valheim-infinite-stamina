# Infinite Stamina

A lightweight quality-of-life mod that prevents stamina depletion.

---

## Features

- Toggle infinite stamina on/off with a hotkey
- Instantly restores stamina when enabled
- Optional running-only mode
- Lightweight client-side behavior
- No server installation required

---

## Dependencies

- BepInExPack Valheim by denikson

---

## Installation

### Using a Mod Manager (Recommended)

Install with Thunderstore Mod Manager or r2modman.

### Manual Installation

1. Install BepInExPack Valheim
2. Download this mod from Thunderstore
3. Extract the contents into your Valheim game folder

The DLL should end up here:

```text
BepInEx/plugins/HexInfiniteStamina/HexInfiniteStamina.dll
```

---

## Default Hotkey

```text
F6
```

The hotkey can be changed in the config file.

---

## Configuration

Config file location:

```text
BepInEx/config/com.hex.infinitestamina.cfg
```

Example configuration:

```ini
[General]

## Enable or disable the mod
# Setting type: Boolean
# Default value: true
Enabled = true

## Toggle infinite stamina hotkey
# Setting type: KeyboardShortcut
# Default value: F6
ToggleKey = F6

## Enable infinite stamina only while running
# Setting type: Boolean
# Default value: false
EnableWhileRunning = false
```

---

## Usage

1. Launch Valheim with the mod installed
2. Press `F6` (or your configured hotkey) to toggle infinite stamina
3. A message will appear confirming the current status
4. When enabled, stamina consumption is disabled

---

## Multiplayer

This mod is intended as a client-side quality-of-life mod.

Only players with the mod installed will receive infinite stamina behavior.