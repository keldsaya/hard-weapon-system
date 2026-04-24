# hard-weapon-system
A realistic, magazine-fed weapon simulation framework for Unity

> REQUIRES: uclang — the Unity C Language framework.
> This system depends on include.math_h, include.stdio_h, include.time_h, include.input_h, and custom btoi types.

---

## Overview
`hard-weapon-system` is a component-based weapon implementation focused on mechanical correctness:
* Chambered rounds
* Removable magazines with physical presence
* Fire selectors (SAFE / SEMI / AUTO)
* Ammo uncertainty 
* Tactile reload mechanics (separate magazine and chamber reloads)
> Not a "hit-scan with ammo counter" — a mechanical simulation.

---

## Components
### `g17.c.cs`
A complete Glock 17 implementation (`MonoBehaviour` + `h_weapon` interface).

**Inspector Fields:**
| Field | Description |
|-------|-------------|
| `rounds_per_minute` | Rate of fire (600 default) |
| `type` | `SEMI` / `AUTO` / `MANUAL` |
| `calliber` | Ammo type (`_9x19`, `_9x18`, `_12x70`, etc.) |
| `console_out` | Enable/disable debug logging |

**Read-only Debug Fields (Greyed out in Inspector):**
| Field | Description |
|-------|-------------|
| `chambered` | Round in chamber? (`btoi`) |
| `mag_inserted` | Magazine present? (`btoi`) |
| `ammo_in_mag` | Status string or `"?"` (unknown) |
| `selector` | Current fire mode |

---

## Controls

| Action | Input |
|--------|-------|
| Reload (new mag) | `R` |
| Tactical reload (extract mag) | `Shift + R` |
| Chamber manipulation | `Alt + R` |
| Check chamber | `Alt + T` |
| Check magazine | `Shift + T` |
| Cycle fire selector | `B` |

---

## Weapon Interface (`h_weapon`)

```csharp
public interface h_weapon {
  void shoot();
  void reload();
  void reload_chamber();
  void insert_mag(magazine_t mag);
  void insert_chamber_ammo(ammo_t ammo);
  magazine_t extract_mag();
  void set_selector(selector_mode_t mode);
  selector_mode_t get_selector();
  void check_chamber();
  void check_magazine();
}
```

---

## Data Structures

### `ammo_t`
```csharp
public struct ammo_t {
  public uid_t uid_t;      // unique identifier
  public string uid;       // getter for uid_t.id
  public int dmg;
  public int penetr_power;
}
```

### `magazine_t`
```csharp
public class magazine_t {
  public uid_t uid_t;
  public string uid;       // getter
  public int max_count;
  public List<ammo_t> ammo;
  public calliber_t calliber;
}
```

### `enum calliber_t`
- `_9x19` — 9mm Parabellum
- `_9x18` — 9mm Makarov
- `_12x70` — 12 gauge
- `_545x39` — 5.45×39mm
- `_556x45` — 5.56×45mm

### `enum selector_mode_t`
- `SAVE` — Safety (cannot fire)
- `SEMI` — Semiautomatic (one shot per click)
- `AUTO` — Full auto (hold to fire)

### `enum type_t`
- `SEMI` — Pistols, DMRs
- `AUTO` — Rifles, SMGs
- `MANUAL` — Bolt-action, pump-action

---

## Core Mechanics

### Magazine Uncertainty
After reloading, `ammo_in_mag = "?"` — you don't know how many rounds are in the new magazine until you call `check_magazine()`:

```csharp
check_magazine(); // prints: "Magazine... Almost full in 6..."
```
Result phrases: `"Empty"`, `"Full"`, `"Almost full"`, `"More than half"`, `"About half"`, `"Less than half"`, `"Almost empty"`, `"No magazine"`

### Chamber Manipulation
- `Alt + R` with round chambered → extracts round to `tmp_ammo`
- `Alt + R` with `tmp_ammo` present → manually inserts into chamber
- `Alt + T` → prints chamber status

### Fire Modes
Cycle with `B`:
```
SAVE → SEMI → AUTO → SAVE
```
AUTO mode only available if `type == type_t.AUTO`

### Shooting Logic
1. Check fire delay (`60 / rounds_per_minute`)
2. Check safety (`selector_mode_t.SAVE`)
3. Check chambered round
4. Empty chamber → `"Click..."`
5. Fire → `"Bang! {uid}"`
6. Delay applied, no automatic chambering (manual reload only)


