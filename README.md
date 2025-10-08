A Terraria mod created by @LackOfName.
Focus on minimal sprites but beautiful graphics.
Mostly just stuff I always wanted a mod to exist for.

TODO Now:
-Bugtest temporalmissiles & temporalbolt [x]


Known Bugs:
TBD

Future additions:
-More testing + general use commands

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
	- [ ] Implement tiered stat scaling for set and per-helmet class variants (`Items/Armour/Temporal/`)
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
	- [ ] VFX tint by tier color (orange/purple/cyan/white)

- Crafting & progression
	- [ ] Crafting trees for each upgraded/variant item gated by tier mats
	- [ ] Ensure `Players/TemporalPlayer.SetTier` + auto-unlocks and `/temporaltier` testing path are respected

- Localization & Docs
	- [ ] Add/verify item names, tooltips, and progression messages (`Localization/en-US.hjson`)
	- [ ] README update with crafting examples once finalized

- Testing & balance
	- [ ] Smoke test each tier with `/temporaltier [1-4]` and optional `lock`
	- [ ] DPS balance pass per class across tiers
	- [ ] Multiplayer sync checks for unlocks and visuals
