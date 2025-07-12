# Existing Scripts Inventory

## Movement & Navigation Systems
- `SimpleRandomWalker.cs` - Random movement behavior for NPCs
- `CharacterMovementSync.cs` - Movement synchronization system
- `QuickMovementTest.cs` - Testing movement functionality
- `NavMeshBuilder.cs` - Navigation mesh construction
- `NavMeshAutoBuilder.cs` - Automatic navigation mesh building

## Core Minipoll Systems
- `WorkingMinipollCore.cs` - Main Minipoll character logic
- `PenguinAI.cs` - AI behavior for penguin characters
- `PenguinDebugger.cs` - Debugging tools for penguin AI
- `MinipollAnimationController.cs` - Animation control system
- `MinipollSpawner.cs` - Spawning system for Minipoll creatures
- `MinipollEnums.cs` - Enumeration definitions

## Game Management
- `GameManager.cs` - Main game management
- `AudioManager.cs` - Audio system management

## UI Systems
- `SimpleButtonManager.cs` - Button handling
- `DirectPlayButtonHandler.cs` - Direct play functionality
- `MenuButtonsAnimationController.cs` - Menu animation control

## Tutorial System
- `TutorialManager.cs` - Tutorial flow management
- `TutorialUI.cs` - Tutorial user interface
- `TutorialHighlight.cs` - Tutorial highlighting system
- `TutorialData.cs` - Tutorial data structure

## Tools & Utilities
- `PerformanceProfiler.cs` - Performance monitoring
- `AdvancedFoodSource.cs` - Food system implementation

## CRITICAL WORKFLOW RULE
**Before creating ANY new script, ALWAYS:**
1. Search for existing functionality using semantic_search
2. Check if similar systems already exist
3. Extend or modify existing scripts rather than create new ones
4. Only create new scripts if truly no existing solution exists

## Search Strategy
When asked to implement functionality:
1. Use `semantic_search` with relevant keywords
2. Use `grep_search` to find specific patterns
3. Use `file_search` to locate scripts by name pattern
4. Read existing scripts to understand current architecture
5. Propose modifications to existing systems first
