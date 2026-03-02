# Plan: Deep Dark Ocean Dice Combat

Build a 60-second survival game where you play as a fish using 4 dice-based attacks to hunt enemies. Each enemy type (PiercingFish→Pierce, JawFish→JawBite, EelFish→Zap, UglyFish→Poison) drops meat that powers up its corresponding attack through independent progression tracks (15/30/45 pieces → +1 dice / roll 2 dice / special effect). Features bottom-bar UI with attack selection, dice animation, combat log, and progression meters; ocean current zones; complete menu flow; and file-saved high scores.

**Key Decisions:**
- Per-attack progression (4 independent meat counters/progress bars)
- Dice roll determines meat dropped (1-6 pieces per kill)
- Different dice types per attack (inspector-configurable, e.g., Zap uses d4, JawBite uses d8)
- One-hit enemy kills with geometric attack ranges (Pierce=horizontal line, JawBite=wide cone, etc.)
- New Input System for WASD/arrows + mouse selection + ESC/R hotkeys
- Trigger-based combat (touch enemy → instant attack with current selection)

---

**Steps**

2. **Create Data Architecture**
   - `AttackData` ScriptableObject: name, `DiceType` enum field (D4/D6/D8/D12), attackRange float, `AttackShape` enum (HorizontalLine, WideCone, Circle, Bite), description
   - `AttackProgressionData` class: meatCount int, tier1/2/3Unlocked bools
   - `GameState` enum: StartMenu, Playing, Paused, GameOver
   - Create 4 AttackData assets: Pierce (horizontal line), JawBite (wide cone), Zap (circle), Poison (bite)

3. **Input System Migration**
   - Generate C# class from [InputSystem_Actions.inputactions](Assets/InputSystem_Actions.inputactions)
   - Create `InputManager` MonoBehaviour wrapping PlayerInput component
   - Modify [PlayerMovement.cs](Assets/Scripts/PlayerMovement.cs): replace `Input.GetAxisRaw` with `inputActions.Player.Move.ReadValue<Vector2>()`
   - Add handlers: Attack (mouse click on UI), Previous/Next (1/2 keys for attack cycling), Pause (ESC), Reset (R from InputActions or KeyCode)

4. **Combat System Core**
   - Create `CombatManager` singleton:
     - `AttackData currentAttack` (selected from 4 options)
     - `PerformAttack(Enemy target)`: rolls dice based on attackData.diceType + progression bonuses, spawns meat (roll result = pieces), kills enemy instantly
     - `RollDice(DiceType, int bonus, bool rollTwice)`: returns int result
     - Events: `OnAttackExecuted(AttackData, int rollResult)`, `OnEnemyKilled(EnemyType)`
   - Create `AttackVisualizer` component attached to player:
     - Gizmos/debug draw attack range shapes in Scene view
     - Runtime overlay sprite renderer showing attack zone (enable on hover/attack)
   - Update [PlayerScript.cs](Assets/Scripts/PlayerScript.cs) `OnTriggerEnter2D`: if collider has `EnemyBase` component → `CombatManager.PerformAttack(enemy)`

5. **Progression System**
   - Create `ProgressionManager` singleton:
     - `Dictionary<AttackType, AttackProgressionData>` tracking 4 separate counters
     - `AddMeat(AttackType type, int amount)` increments counter, checks 15/30/45 thresholds, unlocks tiers
     - `GetAttackBonus(AttackType)`: returns (+X dice modifier, bool rollTwice, bool hasSpecialFX)
     - Event: `OnProgressionChanged(AttackType, int meatCount, int tier)`
   - Integrate into `CombatManager.RollDice()`: apply +1 at tier1, roll twice/take higher at tier2

6. **Enemy System**
   - Create `EnemyBase` abstract class:
     - `EnemyType` enum (Piercing, Jaw, Eel, Ugly)
     - `bool isImmobilized` state with timer (for Zap attack)
     - `Die()`: triggers death VFX, disables AI, signals spawner
   - Create 4 concrete enemy classes in new scripts:
     - `EnemyPiercing`: fast linear chase using `Vector2.MoveTowards` toward player
     - `EnemyJaw`: slow circular orbit around player using angle lerp
     - `EelEnemy`: sine wave pattern perpendicular to player direction
     - `UglyEnemy`: random wander with `NavMeshAgent` equivalent or timer-based direction changes + sudden dash bursts
   - Attach appropriate script to each prefab in [Assets/Prefabs/](Assets/Prefabs)
   - Create `EnemySpawner` singleton: spawns enemies at edges of play area on intervals, tracks alive count, increases spawn rate over time

7. **Meat Drop System**
   - Create `MeatPiece` prefab: small sprite, CircleCollider2D trigger, Rigidbody2D with gravity off
   - `MeatPickup` script:
     - `AttackType meatType` field (set when spawned based on enemy killed)
     - `OnTriggerEnter2D` with "Player" tag: heal player 1-5 HP, add 1 to score, call `ProgressionManager.AddMeat(meatType, 1)`, destroy self
     - Add slight random velocity for scatter effect
   - Spawn in `CombatManager.PerformAttack()` after kill: instantiate N pieces (N = dice roll result) in circle around dead enemy position
   - Color-tint meat sprite based on meatType for visual clarity

8. **Attack Special Effects (Tier 3)**
   - `PierceSpecialFX`: In `CombatManager.PerformAttack()` when tier3 unlocked, spawn 3 delayed projectile GameObjects (LineRenderers) fanning out from player, launch after 0.3s, hit enemies in path
   - `JawBiteSpecialFX`: Change attackRange to 360° circle instead of forward cone (modify shape enum check)
   - `ZapSpecialFX`: After hitting primary target, raycast to find 2 nearest enemies within chainRange, apply Zap immobilize to them recursively
   - `PoisonSpecialFX`: Instantiate poison cloud prefab (particle system + trigger collider) at dead enemy position, lasts 3s, damages/immobilizes enemies entering
   - Implement as conditional branches in `CombatManager.PerformAttack()` checking `ProgressionManager.GetAttackBonus(currentAttack.type).hasSpecialFX`

9. **Bottom UI Bar**
   - Create Canvas (Screen Space Overlay) with UI panel anchored to bottom
   - **Attack Buttons** (4 instances of prefab):
     - Image icon, Text name label, 3-section horizontal progress bar (Image sliced sprite)
     - Button component with `OnClick` → `CombatManager.SelectAttack(index)`
     - Highlight border/glow when selected
     - Subscribe to `ProgressionManager.OnProgressionChanged` to update fill amounts (0-15, 15-30, 30-45 ranges fill sections 1, 2, 3)
   - **Dice Display Panel**:
     - Image showing dice sprite, RotateAnimation coroutine for roll effect
     - Text "Rolling..." → "Result: X" fade-in
     - Subscribe to `CombatManager.OnAttackExecuted`
   - **Combat Log Panel**:
     - ScrollRect with vertical layout, last 8 entries
     - Text prefab: "Pierce rolled 5 (+1) = 6"
     - Subscribe to `CombatManager.OnAttackExecuted`, instantiate text entries
   - **Top Status Bar**:
     - Text timer (MM:SS format counting down from 01:00)
     - Text score ("Score: 47")
     - Health bar Slider (max=100)
   - Create `UIManager` singleton subscribing to all events, updating UI elements

10. **Ocean Currents**
    - Create `CurrentZone` prefab:
      - BoxCollider2D trigger
      - `float speedModifier` field (0.5 for red, 1.5 for green)
      - ParticleSystem with bubble sprites, direction matching current flow
    - Create `PipeEmitter` prefab: sprite renderer (red/green variants), spawns `CurrentZone` in front
    - Place 6-10 pipes in [SampleScene.unity](Assets/Scenes/SampleScene.unity) at random angles
    - Modify [PlayerMovement.cs](Assets/Scripts/PlayerMovement.cs):
      - Add `currentSpeedMod = 1f` field
      - `OnTriggerStay2D`: if CurrentZone, set `currentSpeedMod = zone.speedModifier`
      - `OnTriggerExit2D`: reset to 1f
      - Apply in movement: `velocity = moveInput * moveSpeed * currentSpeedMod`

11. **Game Flow Manager**
    - Create `GameManager` singleton (DontDestroyOnLoad):
      - `GameState state`, `float gameTimer = 60f`, `int currentScore`
      - `Update()`: if Playing, decrement timer; if timer ≤ 0 or player health ≤ 0, call `EndGame()`
      - `StartGame()`: load SampleScene, reset timer/score, set state to Playing
      - `PauseGame()` / `ResumeGame()`: toggle Time.timeScale 0/1, toggle pause UI
      - `EndGame(bool survived)`: set state to GameOver, show end menu, save score
      - Listen to InputManager for ESC (pause toggle) and R (reload scene)
    - Hook timer/score updates to `UIManager` via events

12. **Menu Systems**
    - Create **StartMenu scene**:
      - UI Canvas: Title "GREEDY FISH", Start Button (`GameManager.StartGame()`), Quit Button (`Application.Quit()`)
      - Text box listing controls (WASD/Arrows, Mouse, ESC, R)
      - High score display (TextMeshPro list, reads from `ScoreManager.GetTopScores()`)
    - Create **Pause Panel** (deactivated by default in SampleScene):
      - Semi-transparent background, "PAUSED" text
      - Resume Button (`GameManager.ResumeGame()`), Restart Button (`SceneManager.LoadScene("SampleScene")`), Quit Button (load StartMenu)
    - Create **End Panel** (deactivated by default):
      - Dynamic text: "SURVIVED!" if timer ended, "DIED!" if health depleted
      - Text showing final score
      - Retry Button (reload SampleScene), Quit Button (load StartMenu)
    - Create `MenuManager` handling panel activation/deactivation based on `GameManager.state`

13. **Score Persistence**
    - Create `ScoreManager` singleton:
      - `[Serializable] HighScoreEntry` class (int score, string date)
      - `List<HighScoreEntry> highScores`
      - `SaveScore(int score)`: add new entry, sort descending, trim to top 10, write `JsonUtility.ToJson(new Wrapper(highScores))` to `Application.persistentDataPath + "/highscores.json"`
      - `LoadScores()`: read JSON file, deserialize to list (call in Awake)
      - `GetTopScores(int count = 5)` returns list for UI display
    - Call `SaveScore()` in `GameManager.EndGame()`

14. **Audio Scaffolding**
    - Create `AudioManager` singleton:
      - SerializeField AudioClips: attackSounds[4], diceRoll, meatPickup, enemyDeath, ambientMusic
      - AudioSource components: musicSource (loop), sfxSource (one-shot)
      - `PlaySFX(AudioClip)`, `PlayMusic(AudioClip)` methods
    - Subscribe to events in Start(): `CombatManager.OnAttackExecuted` → play dice roll SFX, etc.
    - Leave clip assignment empty initially (drag in inspector after sound design)

15. **Tuning Parameters**
    - Add `[Header("Movement")]` and `[Range]` attributes to [PlayerMovement.cs](Assets/Scripts/PlayerMovement.cs): baseSpeed (3-10), acceleration (5-20), currentModifierStrength (0-2)
    - Add to enemy scripts: chaseSpeed, turnSpeed, detectionRange, dashCooldown (with tooltips)
    - Add to `CombatManager`: attackCooldown, baseAttackRanges for each type
    - Add to `MeatPickup`: healAmount (1-10), driftSpeed, despawnTime
    - Add to `EnemySpawner`: initialSpawnInterval (2-5s), finalSpawnInterval (0.5-1s), maxEnemies (10-30), spawnBounds (Rect)
    - Create empty GameObject "TuningParameters" in scene with these scripts attached for designer access

16. **Scene Assembly**
    - [SampleScene.unity](Assets/Scenes/SampleScene.unity):
      - Instantiate Player prefab at origin
      - Place 8 PipeEmitter prefabs around perimeter (4 red, 4 green, rotated various angles)
      - Add parallax background layers using sprites from [Assets/Underwater Diving/Sprites/](Assets/Underwater Diving/Sprites) and `ParallaxBackground` script
      - Create "_Managers" empty with GameManager, CombatManager, ProgressionManager, EnemySpawner, UIManager, AudioManager, MenuManager scripts
      - Set Main Camera to follow Player using `CameraController` from asset pack
      - Configure lighting/post-processing for dark ocean atmosphere
    - Create StartMenu scene with start UI
    - Update Build Settings: StartMenu (index 0), SampleScene (index 1)

---

**Verification**

1. Launch from StartMenu → see control instructions and top 5 high scores
2. Click Start → SampleScene loads with 60s timer visible, score at 0
3. Use WASD/arrows to move, verify current zones change speed with bubble VFX
4. Click attack buttons at bottom or press 1/2 to cycle selections (highlight changes)
5. Collide with PiercingFish → dice animation plays, shows roll result in log, enemy disappears, 1-6 meat pieces spawn
6. Collect meat → health increases, score increments, corresponding attack progress bar fills
7. Collect 15 Pierce meat → first section of Pierce progress bar full, next attack shows "+1" modifier in log
8. Collect 30 → second section full, log shows "Rolling 2 dice: 5, 3 → 5"
9. Collect 45 → third section full, Pierce attack spawns 3 projectile swords
10. Press ESC → pause menu appears, game timer stops
11. Press R → scene reloads from beginning
12. Let timer reach 0:00 → end menu shows "SURVIVED!" with final score
13. Take enough damage to reach 0 health → end menu shows "DIED!" with score
14. Check file at `C:\Users\creat\AppData\LocalLow\<CompanyName>\GreedyFish\highscores.json` for saved scores (use `Debug.Log(Application.persistentDataPath)` to verify path)
