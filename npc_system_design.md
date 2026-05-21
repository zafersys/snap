# SNAP: Neural NPC System Design & Exhaustive Use Cases

This document outlines the deeply detailed architecture of the Neural NPC System and provides an exhaustive registry of 100+ micro-use cases defining exactly how NPCs behave, react, and evolve.

---

## PART 1: SYSTEM STRUCTURE (HOW IT WORKS)

To achieve human-like, unpredictable-yet-logical behavior, the `NPCController` must be rebuilt using a decoupled pipeline. Every NPC loop passes through four distinct layers:

### 1. The Sensory Layer (Input)
NPCs are no longer omniscient. They have a physical **Vision Cone** (e.g., 120 degrees, 15m distance) and a **Hearing Radius** (e.g., 5m for footsteps, 10m for camera shutters).
*   **Vision Matrix**: Detects the Player (are they holding a camera?), other NPCs, empty benches, and structures.
*   **Audio Matrix**: Detects shutter clicks, shouting, and footsteps.

### 2. The Appraisal Layer (OCC & OCEAN)
When a stimulus is detected, the NPC appraises it using their `NPCPersonalityData` (OCEAN).
*   **OCC Engine**: Asks: "Is this event desirable?" "Is this person a friend?"
*   **Example**: The Player raises a camera (Stimulus). The NPC has high Neuroticism and low Agreeableness. The Appraisal Layer flags this as a "Threat/Privacy Violation."

### 3. The Execution Layer (Action Selection)
Instead of a rigid state machine, we use a utility-based or GOAP (Goal-Oriented Action Planning) system. 
*   If the Appraisal Layer outputs "Threat", the Execution layer calculates the highest utility action: Flee, Hide Face, or Confront.

### 4. The Memory & Reflection Layer (Storage & AI)
*   **Short-Term Memory**: The event (e.g., "Player took my photo") is logged to a `MemoryStream` with an emotional weight.
*   **Nightly AI Reflection**: During the Night Phase, the system batches these memories into an LLM/Neural Net prompt to generate permanent beliefs: *"The photographer is stalking me. I will be hostile to them tomorrow."*

---

## PART 2: EXHAUSTIVE USE CASE REGISTRY (100+)

Below are the highly granular use cases defining every possible action, interaction, and thought process of the NPCs.

### Category A: Sensory & Perception (UC-S01 to UC-S15)
*   **UC-S01**: NPC detects the player entering their 15m vision cone.
*   **UC-S02**: NPC detects the player moving quickly (running) vs slowly (sneaking) in their vision cone.
*   **UC-S03**: NPC loses line of sight of the player when the player breaks it behind a house.
*   **UC-S04**: NPC hears a camera shutter fire within a 10m radius.
*   **UC-S05**: NPC sees a camera flash pop within a 20m radius.
*   **UC-S06**: NPC detects a known friend entering their vision cone.
*   **UC-S07**: NPC detects a known rival entering their vision cone.
*   **UC-S08**: NPC identifies an empty bench within a 10m radius.
*   **UC-S09**: NPC identifies that the Newspaper Board has a new edition pinned.
*   **UC-S10**: NPC's vision is reduced during the Night phase due to darkness.
*   **UC-S11**: NPC's vision is obstructed by another NPC standing in front of them.
*   **UC-S12**: NPC notices the player standing unnaturally still for more than 5 seconds.
*   **UC-S13**: NPC hears a confrontation/argument happening nearby.
*   **UC-S14**: NPC's awareness radius increases when they are in a High Arousal emotional state (paranoia).
*   **UC-S15**: NPC's awareness radius decreases when they are in a Low Arousal emotional state (daydreaming).

### Category B: Camera Reactions & Player Interactions (UC-C16 to UC-C40)
*   **UC-C16**: High-Extraversion NPC poses dynamically when they see the player aiming a camera.
*   **UC-C17**: High-Extraversion NPC approaches the player and initiates friendly dialogue after being photographed.
*   **UC-C18**: Low-Agreeableness NPC verbally tells the player to go away when a camera is raised.
*   **UC-C19**: High-Neuroticism NPC covers their face with their hands when a camera is raised.
*   **UC-C20**: High-Neuroticism NPC physically runs away when the player aims at them.
*   **UC-C21**: High-Conscientiousness NPC ignores the camera completely to focus on their current routine.
*   **UC-C22**: NPC becomes irritated if the player keeps the camera aimed at them for more than 3 continuous seconds.
*   **UC-C23**: NPC confronts the player aggressively if they are photographed after previously asking the player to stop.
*   **UC-C24**: NPC smiles politely if the player lowers the camera without taking a shot.
*   **UC-C25**: NPC flinches if a flash goes off in their face at close range.
*   **UC-C26**: NPC reacts with confusion if the player photographs a blank wall next to them.
*   **UC-C27**: NPC warns their nearby friends ("Watch out, the photographer is here") when the player approaches.
*   **UC-C28**: NPC alters their walking path to avoid passing directly in front of the player's active camera.
*   **UC-C29**: NPC stops walking and waits for the player to finish taking a picture of someone else.
*   **UC-C30**: NPC photobombs the player's composition if they have the "Joker" archetype.
*   **UC-C31**: NPC acts completely natural if the player aims at them from beyond their vision cone (telephoto lens stealth).
*   **UC-C32**: NPC turns their back to the player if they have a negative relationship score.
*   **UC-C33**: NPC gives the player a thumbs-up if they have a highly positive relationship score.
*   **UC-C34**: NPC demands the player delete a photo (reducing relationship heavily if ignored).
*   **UC-C35**: NPC asks the player to take a portrait of them and their friend.
*   **UC-C36**: NPC feels flattered and gains a temporary relationship boost if photographed while doing something they are proud of.
*   **UC-C37**: NPC feels humiliated and loses massive relationship points if photographed while crying or falling.
*   **UC-C38**: NPC follows the player around out of curiosity (High Openness).
*   **UC-C39**: NPC stares suspiciously at the player until they leave the immediate area.
*   **UC-C40**: NPC drops their current carried item (e.g., a book) if startled by a sudden flash.

### Category C: Emotional & Internal States (UC-E41 to UC-E55)
*   **UC-E41**: NPC transitions to "Joy" state after a positive interaction, boosting walking speed.
*   **UC-E42**: NPC transitions to "Sadness" state, slowing walking speed and slouching posture.
*   **UC-E43**: NPC transitions to "Anger" state, resulting in sharp, fast movements and red tinting.
*   **UC-E44**: NPC transitions to "Fear" state, causing them to hug walls and avoid the town square center.
*   **UC-E45**: NPC experiences a random mood swing (High Neuroticism) without external stimulus.
*   **UC-E46**: NPC's emotional state slowly decays back to their baseline over 2 in-game hours.
*   **UC-E47**: NPC's baseline emotional valence is lowered by bad weather (rain/overcast).
*   **UC-E48**: NPC's baseline emotional valence is raised by bright, sunny midday phases.
*   **UC-E49**: NPC experiences "Contagious Emotion", shifting their state to match a highly emotional group they join.
*   **UC-E50**: NPC suppresses their true emotion (High Conscientiousness) when in public.
*   **UC-E51**: NPC exhibits physical trembling when in a high-stress state.
*   **UC-E52**: NPC whistles or hums aloud when in a state of high joy and low stress.
*   **UC-E53**: NPC seeks isolation (pathfinds to empty corners) when feeling melancholic.
*   **UC-E54**: NPC seeks crowds (pathfinds to the fountain) when feeling energetic.
*   **UC-E55**: NPC becomes irritable and more likely to reject interactions when their invisible "Cortisol" level is high.

### Category D: Social Dynamics & Gossip (UC-D56 to UC-D75)
*   **UC-D56**: Two friends meet and trigger a hugging gesture animation.
*   **UC-D57**: Two friends meet and stop to have a 30-second conversation before parting.
*   **UC-D58**: NPC waves at a friend passing by from a distance.
*   **UC-D59**: Two rivals cross paths and actively alter their vectors to maximize distance from each other.
*   **UC-D60**: Rival NPCs glare at each other as they pass.
*   **UC-D61**: NPC shares gossip with a friend about the player ("The photographer was rude today").
*   **UC-D62**: NPC shares gossip about a published news story with a friend.
*   **UC-D63**: A group of 3+ NPCs forms a "Clique" at the fountain, preventing outsiders from joining the circle.
*   **UC-D64**: NPC attempts to join a conversation but is ignored (lowering their relationship with the group).
*   **UC-D65**: NPC attempts to join a conversation and is welcomed (increasing their relationship with the group).
*   **UC-D66**: NPC breaks up an argument between two other citizens (High Agreeableness).
*   **UC-D67**: NPC instigates an argument with a rival (Low Agreeableness).
*   **UC-D68**: NPC consoles a sad friend by sitting next to them on a bench.
*   **UC-D69**: NPC introduces two of their friends who don't know each other.
*   **UC-D70**: NPC mimics the behavior of their clique leader (e.g., if the leader hates the player, they do too).
*   **UC-D71**: NPC develops a "Crush" standing with another NPC, causing them to follow them subtly.
*   **UC-D72**: NPC gets rejected by their crush, sending them into a deep depressive state.
*   **UC-D73**: NPC spreads a rumor about another NPC based on a published Scandal.
*   **UC-D74**: NPC defends their friend from a rumor, solidifying their bond.
*   **UC-D75**: NPC leaves a social group abruptly because they are overwhelmed (Low Extraversion).

### Category E: Daily Routines & World Interaction (UC-R76 to UC-R90)
*   **UC-R76**: NPC leaves their home exactly at the start of the Morning phase.
*   **UC-R77**: NPC pathfinds to the Newspaper Board immediately upon leaving home.
*   **UC-R78**: NPC reads the Newspaper Board for exactly 15 seconds.
*   **UC-R79**: NPC occupies a specific bench because it is their preferred "Habit" spot.
*   **UC-R80**: NPC becomes visibly annoyed if their habitual bench is taken by someone else.
*   **UC-R81**: NPC reads a book while sitting on a bench (prop interaction).
*   **UC-R82**: NPC feeds imaginary pigeons near the fountain during Midday.
*   **UC-R83**: NPC takes a brisk exercise walk around the outer perimeter of the square.
*   **UC-R84**: NPC stops to admire a tree or architectural element (High Openness).
*   **UC-R85**: NPC aborts their current routine immediately when the Night phase triggers.
*   **UC-R86**: NPC walks back to their specific home spawner coordinate.
*   **UC-R87**: NPC avoids a specific street or path because they had a bad memory there recently.
*   **UC-R88**: NPC pauses at an intersection to look left and right before crossing.
*   **UC-R89**: NPC changes their daily schedule entirely if they are in a depressed state (e.g., skips the board).
*   **UC-R90**: NPC stands near the edge of the map, simulating staring out into the ocean/distance.

### Category F: Publishing Consequences & Memory (UC-P91 to UC-P105)
*   **UC-P91**: NPC reads a "Flattery" article about themselves and their vanity/happiness spikes.
*   **UC-P92**: NPC reads a "Scandal" article about themselves and their shame/anger spikes.
*   **UC-P93**: NPC reads a "Scandal" about their rival and their joy/schadenfreude spikes.
*   **UC-P94**: NPC reads a "Disaster" article and their fear/anxiety spikes.
*   **UC-P95**: NPC's relationship with the Player drops to -100 after a Scandal is published about them.
*   **UC-P96**: NPC's relationship with the Player increases to +50 after a Flattery article is published about them.
*   **UC-P97**: NPC's friends collectively lower their relationship with the Player by -30 due to a friend's Scandal.
*   **UC-P98**: NPC logs a "Photographed" event into their Short-Term Memory Stream with a timestamp.
*   **UC-P99**: NPC logs a "Spoke to Player" event into their Short-Term Memory.
*   **UC-P100**: During Night phase, AI synthesizes 5 "Photographed" memories into a permanent belief: "The Player is obsessed with me."
*   **UC-P101**: During Night phase, AI synthesizes 3 positive interactions into a permanent belief: "The Player is a respectful artist."
*   **UC-P102**: NPC uses a permanent belief to override their baseline OCEAN response to the Player.
*   **UC-P103**: NPC forgets a minor annoyance (like a single flash) after 3 in-game days.
*   **UC-P104**: NPC never forgets a Scandal article published about them (permanent database entry).
*   **UC-P105**: NPC's personality slowly shifts (e.g., Neuroticism increases) if they are consistently harassed by the Player over multiple days.

---
*This document defines the absolute target specification for the SNAP Neural NPC System. These 105 use cases will guide the implementation of the Sensory, Appraisal, and Execution matrices.*
