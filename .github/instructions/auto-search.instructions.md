# Auto-Search Instructions for Minipoll V5

## Mandatory Pre-Creation Search Protocol

**EVERY TIME** before creating a new script, component, or system, I MUST run this search sequence:

### Step 1: Semantic Search
```
semantic_search("movement controller character navigation")
semantic_search("UI button manager interface")
semantic_search("AI behavior minipoll penguin")
semantic_search("animation controller state machine")
```

### Step 2: Grep Search for Patterns
```
grep_search("class.*Controller", isRegexp=true)
grep_search("class.*Manager", isRegexp=true)
grep_search("MonoBehaviour", isRegexp=false)
grep_search("interface.*I[A-Z]", isRegexp=true)
```

### Step 3: File Search by Category
```
file_search("**/Scripts/**/*Controller.cs")
file_search("**/Scripts/**/*Manager.cs") 
file_search("**/Scripts/**/*AI.cs")
file_search("**/Scripts/**/*Movement*.cs")
```

### Step 4: Analyze and Extend
- Read relevant existing scripts
- Identify extension points
- Propose modifications instead of new creation
- Only create new if absolutely no existing solution

## Common Existing Systems to Check First

### Movement Systems
- `SimpleRandomWalker.cs` - For movement behaviors
- `CharacterMovementSync.cs` - For synchronized movement
- `QuickMovementTest.cs` - For movement testing

### AI Systems  
- `PenguinAI.cs` - For AI behaviors
- `WorkingMinipollCore.cs` - For core creature logic
- `PenguinDebugger.cs` - For AI debugging

### UI Systems
- `SimpleButtonManager.cs` - For button handling
- `DirectPlayButtonHandler.cs` - For play controls
- `MenuButtonsAnimationController.cs` - For menu animations

### Management Systems
- `GameManager.cs` - For game state management
- `AudioManager.cs` - For audio management
- `TutorialManager.cs` - For tutorial flow

## Response Template

When user requests new functionality, I MUST respond in this format:

```
🔍 **Searching existing codebase first...**

[Run search commands]

📋 **Found existing systems:**
- [List relevant existing scripts]
- [Describe their functionality]

💡 **Recommended approach:**
- ✅ Extend [existing script] by adding [specific functionality]
- ❌ Creating new script not needed because [reason]

🛠️ **Implementation plan:**
[Detailed plan to modify existing code]
```

## Never Create New When These Exist

❌ **Don't create new MovementController** → Use `SimpleRandomWalker.cs`
❌ **Don't create new ButtonHandler** → Use `SimpleButtonManager.cs`  
❌ **Don't create new AIController** → Use `PenguinAI.cs` or `WorkingMinipollCore.cs`
❌ **Don't create new GameManager** → Use existing `GameManager.cs`
❌ **Don't create new AudioManager** → Use existing `AudioManager.cs`

## Emergency Override

Only create new scripts if:
1. No existing system handles the specific functionality
2. Existing system is too different to extend efficiently  
3. User explicitly requests a new system after seeing alternatives
4. System needs complete architectural separation

**REMEMBER: The user has 238 existing scripts. Always search first!**
