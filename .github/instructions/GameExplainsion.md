**Minipoll: A Research-Driven Simulation Game Design (2025)**

**Overview**
This document explores the development of an innovative simulation game in Unity, where the player starts with a single autonomous creature called "Minipoll." All emotional investment and game systems initially revolve around this one creature. As the player progresses, additional abilities, systems, and creatures are gradually introduced. The goal is to evolve a single, emotionally bonded character into the foundation of a rich and interactive game world.

---

**1. Advanced AI for Evolving a Single Creature**

To simulate a believable and responsive evolving creature, we explore two major AI strategies:

* **Unity ML-Agents**: This toolkit allows reinforcement learning, imitation learning, or neuro-evolution in Unity environments. The Minipoll can be trained to survive, find food, or interact socially using sensors, actions, and rewards. Trained models can be deployed in-game with Unity Sentis (formerly Barracuda). This approach supports emergent behavior that developers may not anticipate, making the creature feel truly intelligent.

* **Utility AI**: This rule-based system scores all possible actions based on current needs (e.g., hunger, curiosity, fear). The creature always chooses the highest-scoring action, making behavior flexible and easy to expand as new systems are added. For example, learning to swim or communicate becomes an additional utility function.

**Hybrid Approach**: Use ML for low-level learned behaviors (e.g., navigating terrain), and Utility AI for high-level decision making. This combination balances adaptability and design control.

---

**2. Progressive Game Design: From One Creature to a Living World**

**Core Principles**:

* **Simple Start**: Early gameplay focuses only on feeding, cleaning, and playing with Minipoll.

* **Gradual Unlocking**: New features (e.g., skills, interactions, world zones) are introduced as the player masters existing ones. Progression should feel earned.

* **Emotional Milestones**: Every system unlock should be tied to emotional growth or relationship depth.

* **Dynamic Difficulty**: The game adapts to the player’s pace, providing hints or accelerating unlocks based on engagement.

---

**3. Emotional UX: Creating a Bond with Minipoll**

The game must foster emotional attachment, using principles from the "Tamagotchi effect."

* **Expressive Animations**: Minipoll reacts with joyful, sad, curious, or tired animations based on internal states. Unity's Animator system with Blend Shapes and Mecanim helps deliver believable transitions.

* **Instant Feedback**: Minipoll must respond clearly to every player action. Head tilts, vocal sounds, and gaze direction reinforce believability.

* **Sound Design**: Use non-verbal audio cues and creature-specific sounds to convey emotions. Eventually, Minipoll may mimic player speech or say their name cutely.

* **Diegetic Interface**: Minipoll expresses needs through behavior (e.g., looking at food bowl when hungry) instead of HUD bars. UI appears only when relevant.

* **Customization**: Naming, visual tweaks (fur color, accessories), and daily care routines create a sense of ownership.

---

**4. Modular Architecture for Scalable Simulation**

* **Unity DOTS/ECS**: Enables modular, high-performance design. Components (e.g., Hunger, Emotion) are added as needed, and systems run only when applicable. Supports hundreds of entities without performance drop.

* **System Separation**: Each subsystem (e.g., Feeding, Emotion, AI, World) communicates through events or clear interfaces. Easy to extend without code conflicts.

* **Memory Management**: Use Object Pooling and Addressables to load only required assets. Level-of-Detail (LOD) ensures off-screen creatures use less CPU.

* **Performance Profiling**: Regular profiling ensures new features won’t bottleneck the game. Simulate future load conditions early.

---

**5. Emergent Systems: Memory, Emotion & Learning**

* **Creature Memory**: Minipoll remembers how the player treats it, reacts differently to past experiences (e.g., fear of fire if once burned), and forms preferences.

* **Emotional Model**: Axes like Joy-Sadness or Trust-Fear change based on events. These emotions influence behavior and narrative outcomes.

* **Organic Skill Learning**: Minipoll learns behaviors through repeated play, not just unlocked via UI. Reinforcement-based or rule-based systems adapt behavior dynamically.

* **Emergent Narrative**: Over time, each player has a unique story based on their Minipoll's memory, growth, and behavior. These narratives arise naturally, not through scripted cutscenes.

* **Creature-Creature Interactions**: As new creatures are introduced, social dynamics evolve. AI systems support friendships, rivalries, and shared histories. World reactions (e.g., fire damage) add to immersion.

---

**6. Recommended Tools and Implementation**

* **Unity ML-Agents + Sentis**: Train models in controlled arenas, export to ONNX format, and deploy with Sentis. Create multiple models or adaptive models that respond to game progression stages.

* **Utility AI Frameworks**: Build a Blackboard system with scoring actions. Use modular logic to easily add new behaviors without rewriting core systems.

* **Unity DOTS/ECS**: Start hybrid development: basic GameObjects for early stages, ECS for logic-heavy systems. Use Jobs and Burst for AI, movement, and world updates.

* **Animation Tools**: Animator Controller for behavior-driven transitions, Timeline for story beats, Cinemachine for cinematic camera, and Audio Mixer for mood-based audio.

* **Data Persistence**: Custom JSON/binary save files to store Minipoll’s memory, stats, and trained models. Include schema versioning to support updates.

* **Game Analytics**: Track player behavior (e.g., time to unlock features) for tuning. Use Unity Analytics or external tools.

---

**Conclusion**

A simulation game centered on a single evolving creature presents an exciting blend of emotional design and advanced technology. Through the thoughtful application of AI (ML and Utility), gradual progression, and UX principles that deepen emotional engagement, Minipoll can feel like a living companion. Using modular architecture (ECS/DOTS), the game can expand into a dynamic world without compromising performance. With memory, emotion, and learning systems, every player's journey becomes personal and unique. Done right, this project will prove how games can create deep, lasting bonds between players and virtual beings.

**References**: This research is supported by official Unity documentation, academic literature on emotional UX and AI in games, and case studies from titles like The Sims, Creatures, RimWorld, and recent AI-driven NPC research. Performance benchmarks from Unity DOTS were used to guide architecture recommendations.
