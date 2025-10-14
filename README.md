A Terraria mod created by @LackOfName.
Focus on minimal sprites but beautiful graphics.
Mostly just stuff I always wanted a mod to exist for.

TODO:
-[x] Add code for krisblade.png (Items\Weapons\Rogue) and drops for ButterscotchCinnamonPie.png (Items\Consumables)
-[x] Add black shard item - features: Fast attack speed, has function similar to terrablade but for later game, projectile sprite flips vertically each attack. Rogue weapon. Deltarune reference.

Known Bugs:
TBD

Future additions:
-Collab weapons from Undertale & Deltarune.
-Upgrades to time shard weapons and tools

Future Balance Changes:
-More velocity for temporal Launcher [x]
-Various crafting changes []

## Temporal progression upgrades TODO

- Materials (new temporal mats)
	- [ ] Verify acquisition/progression for: Time Shard, Eternal Shard, Time Gem, Eternal Gem
	- [x] Add/adjust recipes (fallbacks) to match tier flow when Calamity missing (see `Common/TemporalProgressionSystem.cs`)
	- [x] Tooltips and localization entries (see `Localization/en-US.hjson`)

- Tools
	- [x] Temporal Pickaxe: tier-based pick power and tooltip reflect current tier (`Items/Tools/TemporalPickaxe.cs`)
	- [ ] Consider additional temporal tools (axe/hammer/drill?) with tier scaling
	- [ ] Balance mining speed, pick power, special effects per tier

- Armor (Temporal set)
	- [x] Implement tiered stat scaling for set and per-helmet class variants (`Items/Armour/Temporal/`)
	- [ ] Ensure set bonus and `Players/TemporalPlayer.cs` effects scale by tier
	- [ ] Sprites/variants per tier (palette or trim changes) [Art]
	- [ ] Update tooltips and set bonus descriptions

- Weapons (per class)
	- Ranged
		- [ ] Add upgraded variants or integrate tier scaling into existing temporal ranged weapons (`Items/Weapons/Ranged/`)
		- [ ] Balance damage, use time, and temporal missile synergy
	- Melee
		- [ ] Tiered variants or scaling for temporal melee (`Items/Weapons/Melee/`)
		- [ ] Consider on-hit temporal procs per tier
	- Magic
		- [ ] Tier scaling or new staves/tomes (`Items/Weapons/Magic/`)
		- [ ] Mana cost interactions with `Buffs/TemporalCasting`
	- Summon
		- [ ] Minion damage/behavior scaling and/or new summons (`Items/Weapons/Summon/`)
		- [ ] Synergy with `Buffs/TemporalMinions`

- Projectiles & Effects
	- [ ] Ensure projectile damage/AI scales with tier (`Projectiles/*`, e.g., `TemporalMissile.cs`, `TemporalBolt.cs`)
	- [x] VFX tint by tier color for missiles and bolt (orange/purple/cyan/white)
	- [ ] Sweep remaining temporal VFX (ripples, explosions, trails) for tier color consistency
	- [x] Subtle tier tint added to VCR overlay

- Crafting & progression
	- [ ] Crafting trees for each upgraded/variant item gated by tier mats
	- [ ] Ensure `Players/TemporalPlayer.SetTier` + auto-unlocks and `/temporaltier` testing path are respected
	- [ ] Balance non-Calamity drop/recipes (Time/Eternal Shard/Gem) for vanilla-only playthrough - [depreciated (Calamity only)]
	- [ ] Add in-world acquisition notes (e.g., Providence/Polterghast/DoG gating or vanilla equivalents)

- Localization & Docs
	- [x] Add/verify item names, tooltips, and progression messages (`Localization/en-US.hjson`)
	- [x] Replace hardcoded progression texts with localized strings in `Players/TemporalPlayer.cs`
	- [ ] Document crafting examples for both Calamity and vanilla fallbacks
	- [ ] README update with crafting examples once finalized

- Testing & balance
	- [ ] Smoke test each tier with `/temporaltier [1-4]` and optional `lock`
	- [ ] DPS balance pass per class across tiers
	- [ ] Multiplayer sync checks for unlocks and visuals
	- [ ] Temporal Pickaxe mining feel test across tiers (useTime curve, pick power thresholds)
	- [ ] Confirm projectiles avoid Target Dummies (already blocked for missiles/sub-missiles)
	- [ ] Validate `/temporaltier` UX: show/lock toggles and multiplayer behavior
	- [ ] Armor scaling QA: verify per-tier bonuses are “slightly better” than contemporaries without power creep

Notes:
- Temporal armor set bonuses now scale modestly with temporal tier:
  - Ranged: extra attack speed and crit chance per tier
  - Melee: extra attack speed and knockback per tier
  - Magic: extra mana cost reduction; higher chance to not consume mana as tier rises
  - Summon: extra minion damage and +1 additional slot at tier 3+
- Temporal missile system now spawns more missiles per tier (2/3/4/5) in a small arc above the player; cooldown scales with tier.

## Bigger fish candidates (next passes)

- Temporal armor tier scaling end-to-end
	- Scale set bonus intensity and class-specific effects by tier (e.g., missile cooldowns/damage, casting/mana reductions)
	- Add tier-aware tooltips to armor set pieces and set bonus description
	- Visual tier accents on armor VFX where possible

- Weapon tier paths per class
	- Define a minimal weapon per class with tier-scaling stats or variants (Ranged/Melee/Magic/Summon)
	- Ensure synergy hooks with missiles/bullet time per tier
	- Add recipes for each tier path (Calamity + vanilla fallbacks when applicable)

- Full crafting trees and unlock feedback
	- Establish clear material → tool/weapon/armor paths for tiers 1–4
	- Provide in-game guidance via localized messages when reaching new tiers
	- Optional: a simple guide item/NPC dialog for progression hints
