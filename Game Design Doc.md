# Game Design Doc: Underrun

**Concept:** Pathway 1 (Thrill-Seekers), Game Concept #2: Platformer
**Genre:** Action, Platformer (3D)
**Team:** Ronny Baroudi, Muzamil Humayun, Abdou Jobe, Junaid Hoda

---

## Summary

The player is an underwater explorer searching for treasure in a 3D platformer. They use platforms and underwater creatures to navigate the depths, avoid obstacles and hazards, and find all of the hidden treasure. How fast can they complete the levels, and how far can they get?

---

## 1. Planned Features

**Key:** **REQUIRED** = from the concept brief. **MODIFIED** = changes a feature from the brief (approved by our project lead). **ADDITIONAL** = our own ideas.

**Status:** Done = in the game now. Partial = some of it is in. Planned = still to build.

### 1. Player movement
| # | Feature | Type | Status |
|---|---|---|---|
| 1.1 | Move forward and backward | REQUIRED | Done |
| 1.2 | Strafe left and right | REQUIRED | Done |
| 1.3 | Rotate left and right | REQUIRED | Partial (diver turns to face the way they move) |
| 1.4 | Jump | REQUIRED | Done |
| 1.5 | Crouch | REQUIRED | Planned |
| 1.6 | Sprint | REQUIRED | Done (Left Shift) |
| 1.7 | Water-resistance feel: floaty jumps with low gravity. No swimming. | REQUIRED | Partial |
| 1.8 | Dash: a quick burst of movement, on the ground and in the air | ADDITIONAL | Done |
| 1.9 | Dash meter that refills over time | ADDITIONAL | Done |
| 1.10 | Pickups that instantly refill the dash meter | ADDITIONAL | Planned |

### 2. Levels
| # | Feature | Type | Status |
|---|---|---|---|
| 2.1 | Level 1 (The Shallows) with its own set of challenges | REQUIRED | Done |
| 2.2 | Further levels with different challenges: Level 2 (Kelp Caves), Level 3 (The Shipwreck), Level 4 (The Abyss) | REQUIRED | Done |
| 2.3 | The oxygen tank breaks and the player uses air bubbles to cross. Oxygen meter drains, bubbles refill it, running out resets to the last checkpoint. | REQUIRED | Done (end of Level 4) |
| 2.4 | Moving from one level to the next (Levels 1 to 3 flow into each other with no loading screen) | REQUIRED | Done |
| 2.5 | Hidden objects to find on each level, e.g. a shipwreck | ADDITIONAL | Partial (shipwreck in Level 3) |
| 2.6 | Time challenges: stars / medals based on completion time | ADDITIONAL | Planned |
| 2.7 | Difficulty settings that change the number and health of enemies | ADDITIONAL | Planned |

### 3. Platforms
| # | Feature | Type | Status |
|---|---|---|---|
| 3.1 | Solid platforms | REQUIRED | Done |
| 3.2 | Wobbly / slippery platforms | REQUIRED | Done |
| 3.3 | One-time-use platforms that crumble after being stood on | REQUIRED | Done |
| 3.4 | Jump pads | REQUIRED | Done |
| 3.5 | Moving platforms that carry the player | REQUIRED | Done |

### 4. Traps and hazards
| # | Feature | Type | Status |
|---|---|---|---|
| 4.1 | Spiky fish that harm the player on contact (pufferfish) | REQUIRED | Done |
| 4.2 | Exploding fish | REQUIRED | Done |
| 4.3 | Sticky trash that adds weight: slower movement, lower jumps | REQUIRED | Done |
| 4.4 | Sticky trash that affects vision: blurred or darkened screen for a few seconds | REQUIRED | Planned |
| 4.5 | Arrow traps that shoot across sections | ADDITIONAL | Done |
| 4.6 | Shark chase sections | ADDITIONAL | Planned |

### 5. Checkpoints
| # | Feature | Type | Status |
|---|---|---|---|
| 5.1 | Detect when the player falls off platforms | REQUIRED | Done |
| 5.2 | Detect when the player is harmed by a trap | REQUIRED | Done (health bar, game over at 0) |
| 5.3 | Checkpoints that save the player's position when reached | REQUIRED | Done |
| 5.4 | Reset the player to the last checkpoint, or the level start if none reached | REQUIRED | Done |

### 6. Treasure and progression
| # | Feature | Type | Status |
|---|---|---|---|
| 6.1 | Treasure pieces scattered around each level | REQUIRED | Done |
| 6.2 | Collect treasure by touching it | REQUIRED | Done |
| 6.3 | On-screen treasure counter, e.g. "Treasure 3 / 8" | REQUIRED | Done |
| 6.4 | Progress to the next level when the treasure is collected | REQUIRED | Partial (goal treasure must be collected) |
| 6.5 | Win when the last level is finished | REQUIRED | Done |
| 6.6 | Optional bonus treasure in hard-to-reach places | REQUIRED | Partial |
| 6.7 | Bonus point system and score display | ADDITIONAL | Planned |

### 7. Creatures with AI
| # | Feature | Type | Status |
|---|---|---|---|
| 7.1 | Helpful creatures that carry the player between platforms | REQUIRED | Planned |
| 7.2 | Helpful creatures that bring the player treasure | REQUIRED | Planned |
| 7.3 | Harmful creatures that attack the player (pufferfish, jellyfish, charger fish, spitter fish) | REQUIRED | Done |
| 7.4 | Stealth: larger creatures detect the player within a view range | REQUIRED | Partial (charger fish spot the player in range and charge) |
| 7.5 | Dodging the attacks of larger creatures | REQUIRED | Done (charges and bubble shots can be dodged or dashed through) |

### 8. Evolving levels
| # | Feature | Type | Status |
|---|---|---|---|
| 8.1 | Track player performance (deaths, time taken) | REQUIRED | Planned |
| 8.2 | Levels get harder or easier based on performance | REQUIRED | Planned |
| 8.3 | Harder: more trapped platforms and a wider mix of platform types | ADDITIONAL | Planned |

### 9. Boss fights and weapons (ADDITIONAL, needs approval)
| # | Feature | Type | Status |
|---|---|---|---|
| 9.1 | Find basic weapons in the levels and keep them for later | ADDITIONAL | Planned |
| 9.2 | Bosses with different abilities and increasing difficulty | ADDITIONAL | Planned |
| 9.3 | Boss fights keep the platforming: dodge attacks and fight back | ADDITIONAL | Planned |

### Changes to required features
| # | Change | Type | Status |
|---|---|---|---|
| C-1 | Third-person follow camera instead of first-person. Makes platforming easier to judge. | MODIFIED (approved) | Done |
| C-2 | Harmful characters | MODIFIED (approved) | Done |

---

## 2. Playtesting Changes

| Date | Feedback | What we changed | Commit |
|---|---|---|---|
| 17 Nov 2026 | Dash was the most frustrating part | Dash goes further, recharges faster, air dashes go straight across, early presses still count | ff0dcfe |
| 17 Nov 2026 | Favourite part was the jump pads | Bubble burst, bubble trail and camera shake on launch | ff0dcfe |
| 17 Nov 2026 | Movement felt limited | Sprint, jump buffering, short hops, diver turns to face movement | ff0dcfe |
| 17 Nov 2026 | Wanted a character and more visual design | Diver visor, flippers and breathing bubbles, underwater fog | ff0dcfe |

---

## 3. Scenes

- **MainMenu**: title screen.
- **Main**: Levels 1, 2 and 3, played in one continuous run.
- **Level4**: Level 4 (The Abyss), loaded after Level 3 is complete.

---

## 4. Working Rules

- Each person builds and tests in their own scene. Finished pieces become prefabs and are added to **Main**.
- Only one person edits **Main** at a time. Say so in the group chat before and after.
- Pull before starting work, push when finished.
- Tag a GitHub release for each version we playtest.

---

## 5. Change Log

| Date | Change |
|---|---|
| 13 Oct 2026 | First draft: feature list, additional features, camera change request, core features |
| Oct 2026 | Camera change (C-1) and harmful characters (C-2) approved for the prototype |
| 23 Nov 2026 | Added feature status, Levels 2 to 4, playtesting changes and scene list |
