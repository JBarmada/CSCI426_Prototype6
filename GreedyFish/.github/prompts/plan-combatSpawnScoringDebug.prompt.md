## Plan: Combat-Aware Spawning + Attack Effects

DRAFT: Implement a weighted-cap spawn director that creates three scenario types (vertical stacks for Bite, horizontal lines for Pierce, and elite chonky single-target events for Zap), while keeping Poison as naturally strong over time. Reuse current systems instead of replacing them: keep [GameManager](Assets/Scripts/GameManager.cs), [AttackSystem](Assets/Scripts/Combat/AttackSystem.cs), and current enemy prefabs, then extend them with configurable values and debug controls. Add status effect support for stun and poison stacking, wire scoring from kill/meat/time/wave events, and harden player hurt flow. All critical behavior is data-tunable in Inspector with optional keyboard debug and on-screen debug HUD for fast testing.

**Steps**
1. Create spawn architecture extensions in [FishSpawner](Assets/Scripts/FishSpawner.cs): weighted occupancy cap, alive tracking, scenario scheduler, and scenario-specific formation spawning (vertical, horizontal, elite single). Add tunables for max occupancy, spawn costs, scenario weights, cooldowns, and wave bonus triggers.
2. Add lightweight enemy metadata + runtime hooks in [EnemyHealth](Assets/Scripts/Combat/EnemyHealth.cs) and/or a new enemy config component to support spawnCost, isChonky, size multiplier, HP multiplier, and score values; ensure chonky uses 1.25 scale and higher HP.
3. Implement status effects: add stun/immobilize support in [EvilFishScript](Assets/Scripts/EvilFishScript.cs) (movement lock timer) and poison stacking DOT runtime in [EnemyHealth](Assets/Scripts/Combat/EnemyHealth.cs) (stack count, tick interval, duration, max stacks, periodic damage).
4. Implement Pierce and Zap gameplay behavior by extending [SpecialAttackZone](Assets/Scripts/Combat/SpecialAttackZone.cs) and attack routing in [PlayerScript](Assets/Scripts/PlayerScript.cs):  
   - Pierce: wide/short line-zone multi-hit pattern.  
   - Zap: single-target hit + stun duration application.  
   - Poison: apply stack-based DOT on hit.
5. Wire full scoring in [GameManager](Assets/Scripts/GameManager.cs) and call sites: enemy kill points, meat points, survival-time points per second, and wave clear bonuses from spawner scenario completion.
6. Validate/fix player hurt path in [EnemyDamageDealer](Assets/Scripts/Combat/EnemyDamageDealer.cs) and [PlayerHealth](Assets/Scripts/Combat/PlayerHealth.cs), including tag/layer assumptions and damage cooldown behavior so contact damage consistently works.
7. Add debug tooling: inspector flags on spawner/combat scripts, keyboard toggles in a dedicated debug script (or existing manager update loop), and a debug HUD script under [Assets/Scripts/UI](Assets/Scripts/UI) showing occupancy, scenario, active effects, DPS/score rates.
8. Update scene/prefab wiring in [SampleScene.unity](Assets/Scenes/SampleScene.unity) and relevant prefabs under [Assets/Prefabs](Assets/Prefabs) so components/fields are assigned and default values are sane for playtesting.

**Verification**
- Play mode checks:
  - Start menu: no active spawning until game starts.
  - Pause menu: spawning and time-driven scoring stop while paused.
  - Occupancy cap: never exceeds weighted max under stress.
  - Scenario behavior: visible vertical/horizontal/chonky patterns occur at configured frequencies.
  - Attack effects: Pierce hits lines, Zap stuns, Poison stacks and ticks.
  - Player hurt: collision damage reduces HP and can end run.
  - Scoring: kill/meat/time/wave increments update live and persist to high score.
- Diagnostics:
  - Use debug HUD + inspector/keyboard toggles to force each scenario/effect and verify cap accounting and timers.
- Regression:
  - Confirm Bite behavior remains unchanged except intended interactions with new spawn patterns.

**Decisions**
- Scoring includes kill points, meat points, survival-time points, and wave clear bonus.
- Cap model is weighted occupancy (chonky counts as multiple).
- Attack rules are Pierce line multi-hit, Zap single-target stun, Poison DOT stacking.
- Debug support includes inspector flags, keyboard toggles, and a debug HUD.
