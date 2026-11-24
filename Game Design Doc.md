# Game Design Doc: Underrun

**Concept:** Pathway 1 (Thrill-Seekers), Concept 2: Platformer

**Team:** Ronny Baroudi, Muzamil Humayun, Abdou Jobe, Junaid Hoda

> "The player takes on the role of an underwater explorer in search for treasure in this first-person 3D platformer. They have to use platforms and underwater creatures to navigate the depths, avoid the obstacles and hazards, and find all of the hidden treasures. How fast can they complete the levels, and how far can they get?"

**Key:** **REQUIRED** = from the concept brief. **MODIFIED** = changes the brief. **ADDITIONAL** = our own ideas. All MODIFIED and ADDITIONAL features below were approved by our project lead.

**Status:** Done = in the game now. Partial = some of it is in. Planned = still to build.

---

## 1. Basic movement
*Brief: The player can move forward, backward, rotate left/right, strafe left/right, crouch, jump, sprint. Movement suggests water resistance. No swimming.*

| # | Feature | Type | Status |
|---|---|---|---|
| 1.A | Move forward / backward, strafe left / right | REQUIRED | Done |
| 1.B | Rotate left / right | REQUIRED | Partial (the diver turns to face the way they move) |
| 1.C | Jump | REQUIRED | Done |
| 1.D | Sprint | REQUIRED | Done (Left Shift) |
| 1.E | Crouch | REQUIRED | Planned |
| 1.F | Movement suggests water resistance, no swimming | REQUIRED | Partial (floaty low-gravity jumps) |
| 1.1 | Being able to dash in the air and on the ground for a quick burst of movement | ADDITIONAL | Done |
| 1.2 | The bursts have a stamina bar that comes back with time | ADDITIONAL | Done (dash meter) |
| 1.3 | There are items that restore part of your stamina bar instantly | ADDITIONAL | Planned |

## 2. Levels
*Brief: At least 2 levels with different challenges for the player. The end of the second level includes an area where the player's oxygen tank breaks and they must use air bubbles to cross.*

| # | Feature | Type | Status |
|---|---|---|---|
| 2.A | At least 2 levels with different challenges | REQUIRED | Done (4 levels: The Shallows, Kelp Caves, The Shipwreck, The Abyss) |
| 2.B | Oxygen tank breaks and the player uses air bubbles to cross | REQUIRED | Done (at the end of Level 4, our last level) |
| 2.1 | There are different hidden objects on each level for the player to find (hidden treasure / shipwreck etc.) | ADDITIONAL | Partial (shipwreck in Level 3) |
| 2.2 | Time-based challenges, awarding different numbers of stars / medals based on how quickly the player completes the level | ADDITIONAL | Planned |
| 2.3 | Different difficulties, which vary the number and health of the enemies | ADDITIONAL | Planned |

## 3. Platforms
*Brief: The levels are made up of platforms of different types: solid, wobbly/slippery, one-time use, jump pads, moving etc.*

| # | Feature | Type | Status |
|---|---|---|---|
| 3.A | Solid, wobbly / slippery, one-time use, jump pads and moving platforms | REQUIRED | Done |
| 3.1 | Temporary platforms that disappear after they are stood on | ADDITIONAL | Done |
| 3.2 | A large shell / old breaking ship that breaks if it is stood on for too long | ADDITIONAL | Partial (crumbling platforms, ship deck in Level 3) |
| 3.3 | Other debris / friendly fish you would find in the sea act as platforms | ADDITIONAL | Planned |

## 4. Traps
*Brief: There are also many traps of different types: spiky fish, exploding fish, sticky trash that adds to the player's weight or affects vision etc.*

| # | Feature | Type | Status |
|---|---|---|---|
| 4.A | Spiky fish | REQUIRED | Done (pufferfish) |
| 4.B | Exploding fish | REQUIRED | Done |
| 4.C | Sticky trash that adds to the player's weight | REQUIRED | Done |
| 4.D | Sticky trash that affects vision | REQUIRED | Planned |
| 4.1 | Places between fights and platforming with arrows that shoot across the screen that the player must avoid | ADDITIONAL | Done |
| 4.2 | Sharks that follow us through certain themes and levels until we finish that level, like a time chase | ADDITIONAL | Planned |
| 4.3 | An old booby trap from a shipwreck which shoots an arrow at the player when triggered (movement-sensitive trap) | ADDITIONAL | Partial (arrow traps fire when the player comes into range) |

## 5. Checkpoints
*Brief: Player position is reset to the start or the last checkpoint if they fall off platforms or get harmed by traps.*

| # | Feature | Type | Status |
|---|---|---|---|
| 5.A | Reset to the start or last checkpoint after falling or being harmed | REQUIRED | Done (falling resets; traps take health) |
| 5.1 | Checkpoints are placed throughout the levels, saving progress when the player reaches certain areas | ADDITIONAL | Done |
| 5.2 | If the player falls off a platform or gets killed by an enemy or trap, they respawn at the last checkpoint they activated | ADDITIONAL | Partial (falling respawns; 0 health is game over) |
| 5.3 | Checkpoints change colour or glow when activated | ADDITIONAL | Planned |
| 5.4 | Players can restart the current level from the beginning to improve their time | ADDITIONAL | Done (Restart in the pause menu) |

## 6. Treasure
*Brief: Treasure pieces are scattered around the level. The player progresses to the next level if they collect all, and they win if they finish the last level. Optional treasure for bonus points appears in places hard to reach.*

| # | Feature | Type | Status |
|---|---|---|---|
| 6.A | Treasure scattered around the level, progress after collecting it, win after the last level | REQUIRED | Done |
| 6.1 | Treasure pieces are scattered throughout levels, some hidden in harder to reach areas to encourage exploration | ADDITIONAL | Done |
| 6.2 | A counter on screen shows how many treasure pieces have been collected and how many remain | ADDITIONAL | Done |
| 6.3 | Collecting all required treasure unlocks the exit to the next level; finishing the final level shows a victory screen | ADDITIONAL | Partial (goal treasure must be collected; victory screen done) |
| 6.4 | Optional treasure collected for bonus points, encouraging harder routes | ADDITIONAL | Planned |
| 6.5 | Bonus point system | ADDITIONAL | Planned |

## 7. Creatures
*Brief: There are swimming creatures of different types controlled by AI. Some help the player move between platforms, or bring them treasure. Some hurt the player instead and add to the dangers the players must avoid. Stealth is occasionally required to avoid detection and dodge the attacks of the larger creatures.*

| # | Feature | Type | Status |
|---|---|---|---|
| 7.A | Creatures that help the player move between platforms or bring them treasure | REQUIRED | Planned |
| 7.B | Creatures that hurt the player | REQUIRED | Done (pufferfish, jellyfish, charger fish, spitter fish, exploding fish) |
| 7.C | Stealth to avoid detection and dodge attacks of larger creatures | REQUIRED | Partial (charger fish spot the player in range and charge) |
| 7.1 | Friendlier sea creatures such as turtles / dolphins help the player by letting them stand on the turtle's shell or ride the dolphin | ADDITIONAL | Planned |
| 7.2 | Sharks chase the player, and getting caught is instant death | ADDITIONAL | Planned |
| 7.3 | Other creatures such as pufferfish / eels or other dangerous sea creatures deal damage to the player | ADDITIONAL | Done |
| 7.4 | Stealth-based interactions | ADDITIONAL | Planned |

## 8. Evolving levels
*Brief: The game gets harder or easier depending on player performance.*

| # | Feature | Type | Status |
|---|---|---|---|
| 8.A | The game gets harder or easier depending on performance | REQUIRED | Planned |
| 8.1 | Boss fights become harder: more damage and less time before they defeat you | ADDITIONAL | Planned |
| 8.2 | Platforms become more trapped, with a wider mix of platform types | ADDITIONAL | Planned |

## 9. Approved changes and additions

| # | Feature | Type | Status |
|---|---|---|---|
| 9.A | 3D third-person camera instead of first person | MODIFIED | Done |
| 9.B | Harmful characters can be defeated: dashing into an enemy destroys it | MODIFIED | Done |
| 9.1 | Fighting bosses with weapons while still keeping the platforming: find basic weapons through the levels that you can keep for later | ADDITIONAL | Planned |
| 9.2 | Weapons have unique features such as different firing speeds and damage | ADDITIONAL | Planned |
| 9.3 | Bosses are spread around the levels with different abilities and increasing difficulty | ADDITIONAL | Planned |
| 9.4 | Boss fights have a platforming element: dodge projectiles and use movement, weapons and the environment to fight back | ADDITIONAL | Planned |

---

## Playtesting Changes

| Date | Feedback | What we changed | Commit |
|---|---|---|---|
| 17 Nov 2026 | Dash was the most frustrating part | Dash goes further, recharges faster, air dashes go straight across, early presses still count | ff0dcfe |
| 17 Nov 2026 | Favourite part was the jump pads | Bubble burst, bubble trail and camera shake on launch | ff0dcfe |
| 17 Nov 2026 | Movement felt limited | Sprint, jump buffering, short hops, diver turns to face movement | ff0dcfe |
| 17 Nov 2026 | Wanted a character and more visual design | Diver visor, flippers and breathing bubbles, underwater fog | ff0dcfe |

## Scenes

- **MainMenu**: title screen.
- **Main**: Levels 1, 2 and 3, played in one continuous run.
- **Level4**: Level 4 (The Abyss), loaded after Level 3 is complete.

## Change Log

| Date | Change |
|---|---|
| 13 Oct 2026 | First draft from our design notes |
| Oct 2026 | Camera change and harmful characters approved for the prototype |
| 24 Nov 2026 | Rebuilt from our group design doc, with every feature labelled and its status; added playtesting changes and scenes |
