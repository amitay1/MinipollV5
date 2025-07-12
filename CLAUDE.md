# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Minipoll V5** is a sophisticated Unity 6000.1.8f1 creature simulation game featuring AI-powered creatures called "Minipolls". The project combines advanced creature simulation with modern Unity development practices, including ML-Agents integration, performance optimization, and modular architecture.

## Key Development Commands

### Unity Operations
```bash
# Open Unity project
unity -projectPath /path/to/Minipoll_V5

# Build project (requires Unity Editor)
# File → Build Settings → Build
```

### Python Integration
```bash
# Install Python dependencies
pip install -e .

# Run MCP server (AI-Unity bridge)
python -m minipoll.mcp_server
```

### Git Operations
```bash
# Check project status
git status

# Commit changes
git add .
git commit -m "Description of changes"

# Create version tag
git tag -a v1.1.0 -m "Version description"
```

## Architecture Overview

### Scene-Based Development System
The project uses a **5-scene progressive development approach**:

1. **01_CoreCreatureScene** - Basic creature systems (needs, movement, emotions)
2. **02_SocialSystemsScene** - Multi-creature social interactions  
3. **03_WorldEconomyScene** - Economy, building, world management
4. **04_AdvancedFeaturesScene** - Genetics, reproduction, advanced AI
5. **05_IntegrationTestScene** - Full system integration

**Development Workflow**: Always start with CoreCreatureScene for basic creature work, then progress through scenes as complexity increases.

### Core System Architecture

**Central Component**: `MinipollCore.cs` - Main creature controller that orchestrates all subsystems:
- MinipollHealth (health management)
- MinipollNeedsSystem (hunger, thirst, energy, sleep)
- MinipollEmotionsSystem (emotion modeling with events)
- MinipollMemorySystem (creature memory and learning)
- MinipollMovementController (NavMesh-based movement)
- MinipollSocialRelations (relationship management)

**Manager Pattern**: Singleton managers provide global access:
- `GameManager` - Overall game state
- `MinipollManager` - Creature lifecycle management
- `ObjectPoolManager` - Performance optimization
- `UIManager` - Interface coordination

### Key Design Patterns

1. **Component-Based**: Modular MonoBehaviour components that can be mixed and matched
2. **Event-Driven**: Extensive use of C# events for loose coupling between systems
3. **Enum-Driven**: Centralized type system in `MinipollEnums.cs` covering all game concepts
4. **Performance-Optimized**: Object pooling, LOD system, and update scheduling

## Directory Structure

```
Assets/Scripts/
├── Branding/                 # Brand identity system (colors, typography)
├── Managers/                 # Core system managers (singleton pattern)
├── Minipoll/                 # Core creature systems
│   ├── AI/                   # AI planning, behavior trees, utility AI
│   ├── Controllers/          # Movement, interaction, visual controllers
│   ├── Core/                 # Core minipoll systems (needs, memory, emotions)
│   └── Social/               # Social relationships and tribe systems
├── Systems/                  # Major game systems
│   ├── Advanced/             # Genetics, research, narrative systems
│   ├── Core/                 # Core gameplay (food chain, reproduction, skills)
│   ├── Social/               # Battle, diplomacy, economy systems
│   └── World/                # Building, streaming, world management
├── UI/                       # Interface systems (HUD, menus, debug)
└── Tools/                    # Development utilities (spawner, profiler, debug)
```

## Essential Files for Understanding

- **`MinipollCore.cs`** - Main creature controller, start here for creature behavior
- **`MinipollEnums.cs`** - All type definitions (NeedType, EmotionType, AIState, etc.)
- **`ObjectPoolManager.cs`** - Performance optimization system
- **`GameManager.cs`** - Global game state management
- **`Scenes/README.md`** - Detailed scene organization and development workflow

## AI & Machine Learning Integration

### Unity ML-Agents (v2.0.1)
- Behavior training for creature AI
- ONNX model export and runtime execution
- Training environments for different creature behaviors

### Custom AI Systems
- **MinipollBrain** - Central AI decision-making
- **MinipollAIPlanningSystem** - Goal-oriented action planning (GOAP)
- **UtilityAIManager** - Utility-based decision scoring
- **MinipollBehaviorTreeSystem** - Behavior tree implementation

### MCP Integration
- Unity MCP Bridge enables AI assistant interaction with Unity Editor
- Python bridge server (`python -m minipoll.mcp_server`)

## Performance Optimization

### Object Pooling
Use `ObjectPoolManager` for frequently created objects:
```csharp
// Get pooled object
GameObject obj = ObjectPoolManager.Instance.GetPooledObject("ParticleEffect");

// Return to pool when done
ObjectPoolManager.Instance.ReturnToPool("ParticleEffect", obj);
```

### Update Scheduling
Use `UpdateScheduler` for expensive operations:
```csharp
UpdateScheduler.Instance.ScheduleUpdate(this, UpdateFrequency.LowFrequency);
```

### Level of Detail (LOD)
LOD system automatically manages off-screen creature complexity.

## Common Development Tasks

### Adding New Creature Behaviors
1. Extend `MinipollBehaviorTreeSystem`
2. Add new AIState to `MinipollEnums.cs`
3. Implement behavior in creature's Update loop
4. Test in appropriate scene (01_CoreCreatureScene for basic behaviors)

### Adding New Needs System
1. Add to `NeedType` enum in `MinipollEnums.cs`
2. Implement in `MinipollNeedsSystem.cs`
3. Add UI representation in needs display system
4. Test with critical/satisfied events

### Adding New Social Interactions
1. Extend `InteractionType` enum
2. Implement in `MinipollSocialRelations.cs`
3. Update relationship calculation systems
4. Test in 02_SocialSystemsScene

### Brand System Usage
Use the branding system for consistent visual identity:
```csharp
// Style UI elements
MinipollBrandManager.StyleButton(playButton, ButtonStyle.Primary);
MinipollBrandManager.StyleText(titleText, TypographyStyle.H1);

// Access brand colors
image.color = MinipollBrandManager.Colors.MinipollBlue;
```

## Technical Specifications

### Unity Version & Dependencies
- **Unity 6000.1.8f1** (LTS)
- **Universal Render Pipeline 17.1.0**
- **ML-Agents 2.0.1**
- **Cinemachine 3.1.4**
- **AI Navigation 2.0.8**
- **Input System 1.14.0**

### Code Style Guidelines
- Hungarian notation and descriptive naming
- Bilingual comments (English/Hebrew) are common
- Event-driven architecture with comprehensive error handling
- Heavy use of Unity's component system

### Scene Navigation
Use keyboard shortcuts during development:
- **Tab**: Navigation menu
- **1-5**: Jump directly to scenes
- **←→**: Previous/next scene

## Common Issues & Solutions

### NavMesh + Animation Synchronization
Follow the guide in `Scripts/Documentation/NavMesh-Animation-Guide.md`:
- Disable Root Motion in Animator
- Use NavMeshAgent.velocity.magnitude for animation speed
- Update Animator parameters in Update()

### Performance Issues
1. Check object pooling usage in `ObjectPoolManager`
2. Verify LOD system is active for creatures
3. Use Update Scheduler for expensive operations
4. Monitor performance with built-in profiler tools

### Event System Debugging
Most systems communicate via events. Check:
- Event subscriptions in Start/OnEnable methods
- Event unsubscriptions in OnDestroy/OnDisable
- Event null checks before invoking

## Extension Points

The architecture supports easy extension in these areas:
- **New AI Behaviors**: Extend behavior tree system
- **New Creature Types**: Inherit from MinipollCore
- **New World Systems**: Add to Systems directory with appropriate namespace
- **New UI Interfaces**: Follow brand system guidelines
- **New Performance Optimizations**: Integrate with existing ObjectPoolManager and UpdateScheduler

This modular architecture ensures that future development can build incrementally on existing systems while maintaining performance and code quality standards.