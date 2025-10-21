
# LackOfNameStuff Terraria Mod

Created by @LackOfName

## Overview
This mod adds unique weapons, tools, and progression systems to Terraria, with a focus on minimal sprites, beautiful graphics, and mechanics inspired by Undertale, Deltarune, and time-based gameplay.

## Features
- Rogue weapons with custom stealth strike effects, mobility skills, and Calamity integration
- Temporal progression system: unlock new tiers and materials as you advance
- Tiered armor sets and tools with scaling stats and visual effects
- Custom projectiles, VFX, and synergy between weapon classes
- Vanilla and Calamity recipe support for all major items
- Hammer of Justice rogue weapon featuring cursor dash, chained parries, cooldown visuals, and a combo-triggered Hammerfall ultimate

## Item Creation
See `DirectivesForItemCreation.txt` for standards and best practices when adding new items.

## Current Progress & TODOs

## Easter Egg Ideas
- Players named 'Gerson' or 'Gerson Boom' get ...
- 1/x chance for the 'gyaa ha ha' sound effect to be played on hammer of justice dash

### Materials & Progression
- [ ] Verify acquisition/progression for: Time Shard, Eternal Shard, Time Gem, Eternal Gem
- [x] Add/adjust recipes to match tier flow (see `Common/TemporalProgressionSystem.cs`)
- [x] Tooltips and localization entries (`Localization/en-US.hjson`)

### Tools
- [x] Temporal Pickaxe: tier-based pick power and tooltip reflect current tier
- [ ] Additional temporal tools (axe/hammer/drill?)
- [ ] Balance mining speed, pick power, special effects per tier

### Armor
- [x] Tiered stat scaling for set and per-helmet class variants
- [ ] Set bonus and effects scale by tier
- [ ] Sprites/variants per tier (palette or trim changes)
- [ ] Update tooltips and set bonus descriptions

### Weapons
- [x] Rogue: Krisblade, Black Shard, Little Sponge, Hammer of Justice (stealth, dash, parry kits)
- [ ] Ranged: Upgraded variants/tier scaling
- [ ] Melee: Tiered variants/scaling, on-hit procs
- [ ] Magic: Tier scaling/new staves/tomes
- [ ] Summon: Minion scaling/new summons

### Projectiles & Effects
- [x] VFX tint by tier color for missiles and bolt
- [ ] Tier scaling for projectile damage/AI
- [ ] Consistent tier color for all temporal VFX

### Crafting & Progression
- [ ] Crafting trees for upgraded/variant items gated by tier mats
- [ ] Ensure progression unlocks and `/temporaltier` testing path
- [ ] Add in-world acquisition notes for progression

### Localization & Docs
- [x] Item names, tooltips, progression messages
- [x] Localized progression texts in code
- [ ] Document crafting examples for Calamity and vanilla
- [x] Hammer of Justice keybinds, buffs, item, and projectile strings

### Testing & Balance
- [ ] Smoke test each tier with `/temporaltier [1-4]`
- [ ] DPS balance pass per class across tiers
- [ ] Multiplayer sync checks for unlocks/visuals
- [ ] Temporal Pickaxe mining feel test
- [ ] Confirm projectiles avoid Target Dummies
- [ ] Validate `/temporaltier` UX
- [ ] Armor scaling QA
- [ ] Field-test Hammer of Justice dash/parry timings (solo + multiplayer)
- [ ] Evaluate Hammer of Justice Hammerfall combo timing and damage; tweak dash interval/cooldown if needed

## Notes
- Temporal armor set bonuses scale with tier:
	- Ranged: attack speed/crit
	- Melee: attack speed/knockback
	- Magic: mana cost reduction, chance to not consume mana
	- Summon: minion damage, +1 slot at tier 3+
- Temporal missile system spawns more missiles per tier (2/3/4/5); cooldown scales with tier

## Future Plans
- Collab weapons from Undertale & Deltarune
- Upgrades to time shard weapons/tools
- Full crafting trees and unlock feedback
- Visual tier accents on armor VFX

---
For more details, see code comments and `DirectivesForItemCreation.txt`.
