# System Patterns: Minipoll V5

## Architecture Overview

### High-Level System Design
```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   VS Code AI    │───▶│   Unity MCP      │───▶│   Unity Editor  │
│   Assistant     │    │   Bridge         │    │   & Runtime     │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Playwright    │    │   Python MCP     │    │   Unity Scene   │
│   Screenshot    │    │   Server         │    │   GameObjects   │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### Core System Components

#### 1. AI-Unity Bridge Layer
- **Unity MCP Server**: Python-based bridge running on port 6400
- **VS Code Integration**: Copilot Agent with MCP configuration
- **Command Translation**: AI requests → MCP commands → Unity operations

#### 2. Unity Application Layer
- **Scene Management**: Single primary scene with dynamic content loading
- **UI System**: Canvas-based interface using Unity's uGUI
- **Data Management**: ScriptableObject templates + runtime JSON persistence
- **Input Handling**: Unity Input System for cross-platform compatibility

#### 3. Documentation Layer
- **Screenshot Automation**: Playwright capturing development progress
- **Memory Bank**: Structured documentation system
- **Version Control**: Git with Unity-optimized configuration

## Design Patterns

### 1. Command Pattern (MCP Integration)
```csharp
public interface IUnityCommand
{
    CommandResult Execute();
    bool CanExecute();
    void Undo();
}

public class CreateGameObjectCommand : IUnityCommand
{
    private string objectName;
    private Vector3 position;
    private GameObject createdObject;
    
    public CommandResult Execute()
    {
        createdObject = new GameObject(objectName);
        createdObject.transform.position = position;
        return CommandResult.Success(createdObject);
    }
}
```

### 2. Observer Pattern (UI Updates)
```csharp
public class PollManager : MonoBehaviour
{
    public static event System.Action<PollData> OnPollUpdated;
    public static event System.Action<VoteData> OnVoteReceived;
    
    public void UpdatePoll(PollData poll)
    {
        // Update logic
        OnPollUpdated?.Invoke(poll);
    }
}

public class PollUI : MonoBehaviour
{
    void OnEnable()
    {
        PollManager.OnPollUpdated += HandlePollUpdate;
    }
    
    void OnDisable()
    {
        PollManager.OnPollUpdated -= HandlePollUpdate;
    }
}
```

### 3. Factory Pattern (UI Creation)
```csharp
public class UIFactory : MonoBehaviour
{
    [SerializeField] private PollButtonPrefab pollButtonPrefab;
    [SerializeField] private VoteOptionPrefab voteOptionPrefab;
    
    public PollButton CreatePollButton(PollData data, Transform parent)
    {
        var button = Instantiate(pollButtonPrefab, parent);
        button.Initialize(data);
        return button;
    }
}
```

### 4. Singleton Pattern (Managers)
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
```

## Component Relationships

### Core Components Hierarchy
```
GameManager (Singleton)
├── PollManager (Data Management)
├── UIManager (Interface Control)
├── InputManager (User Interaction)
├── SceneManager (Scene Transitions)
└── DebugManager (Development Tools)

PollManager
├── PollDataContainer (ScriptableObject)
├── ActivePollsTracker
└── VotingSystem

UIManager
├── MainMenuCanvas
├── PollCreationCanvas
├── VotingCanvas
└── ResultsCanvas
```

### Data Flow Architecture
```
User Input → InputManager → PollManager → Data Update → UI Refresh
     ↓              ↓            ↓            ↓           ↓
Screenshot ← UI Change ← Event System ← Observer ← Data Binding
```

## Key Technical Decisions

### 1. Scene Architecture
- **Single Scene Approach**: Main scene with canvas switching for different states
- **Additive Loading**: Optional scenes for complex polls or mini-games
- **State Management**: Centralized state machine for application flow

### 2. Data Architecture
- **ScriptableObjects**: Poll templates and configuration data
- **JSON Persistence**: Runtime poll data and user preferences
- **Memory Management**: Object pooling for frequently created UI elements

### 3. Input System Integration
- **Action Maps**: Separate action maps for different application states
- **Multi-Platform**: Unified input handling across desktop and potential mobile
- **Accessibility**: Keyboard navigation and screen reader support

### 4. UI Framework Choice
- **Unity uGUI**: Proven reliability and extensive documentation
- **Responsive Design**: Anchor-based layouts for multiple resolutions
- **Animation System**: Unity Animator for state-driven UI transitions

## Performance Patterns

### 1. Object Pooling
```csharp
public class UIObjectPool : MonoBehaviour
{
    private Queue<VoteButton> voteButtonPool = new Queue<VoteButton>();
    
    public VoteButton GetVoteButton()
    {
        if (voteButtonPool.Count > 0)
            return voteButtonPool.Dequeue();
        
        return Instantiate(voteButtonPrefab);
    }
    
    public void ReturnVoteButton(VoteButton button)
    {
        button.Reset();
        voteButtonPool.Enqueue(button);
    }
}
```

### 2. Efficient UI Updates
```csharp
public class PollResultsDisplay : MonoBehaviour
{
    private Coroutine updateCoroutine;
    
    public void StartUpdating()
    {
        if (updateCoroutine != null)
            StopCoroutine(updateCoroutine);
        updateCoroutine = StartCoroutine(UpdateLoop());
    }
    
    private IEnumerator UpdateLoop()
    {
        while (gameObject.activeInHierarchy)
        {
            RefreshDisplay();
            yield return new WaitForSeconds(0.1f); // 10 FPS updates
        }
    }
}
```

## Error Handling Patterns

### 1. Graceful Degradation
```csharp
public class PollLoader : MonoBehaviour
{
    public async Task<PollData> LoadPollAsync(string pollId)
    {
        try
        {
            return await LoadFromFile(pollId);
        }
        catch (FileNotFoundException)
        {
            Debug.LogWarning($"Poll {pollId} not found, creating default");
            return CreateDefaultPoll();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load poll {pollId}: {ex.Message}");
            return null;
        }
    }
}
```

### 2. MCP Bridge Error Handling
```csharp
public class MCPBridgeManager : MonoBehaviour
{
    private bool isConnected = false;
    
    public void CheckConnection()
    {
        try
        {
            // Test MCP connection
            isConnected = TestMCPConnection();
        }
        catch (Exception ex)
        {
            Debug.LogError($"MCP Bridge connection failed: {ex.Message}");
            isConnected = false;
            ShowConnectionError();
        }
    }
}
```

## Testing Patterns

### 1. Unit Testing Structure
```csharp
[TestFixture]
public class PollManagerTests
{
    private PollManager pollManager;
    
    [SetUp]
    public void Setup()
    {
        var go = new GameObject();
        pollManager = go.AddComponent<PollManager>();
    }
    
    [Test]
    public void CreatePoll_ValidData_ReturnsPoll()
    {
        var pollData = new PollData("Test Poll", new[] {"Option A", "Option B"});
        var result = pollManager.CreatePoll(pollData);
        Assert.IsNotNull(result);
    }
}
```

### 2. Integration Testing with MCP
```csharp
[TestFixture]
public class MCPIntegrationTests
{
    [Test]
    public async Task CreateGameObject_ThroughMCP_ObjectExists()
    {
        // Test MCP bridge functionality
        var objectName = "TestObject";
        await MCPBridge.CreateGameObjectAsync(objectName);
        
        var created = GameObject.Find(objectName);
        Assert.IsNotNull(created);
    }
}
```

## Security Patterns

### 1. Input Validation
```csharp
public class PollValidator
{
    public static bool ValidatePollData(PollData data)
    {
        if (string.IsNullOrEmpty(data.title) || data.title.Length > 100)
            return false;
            
        if (data.options == null || data.options.Length < 2)
            return false;
            
        return data.options.All(opt => !string.IsNullOrEmpty(opt) && opt.Length <= 50);
    }
}
```

### 2. Safe File Operations
```csharp
public class SafeFileHandler
{
    public static bool TrySaveData<T>(T data, string filename)
    {
        try
        {
            var json = JsonUtility.ToJson(data, true);
            var path = Path.Combine(Application.persistentDataPath, filename);
            File.WriteAllText(path, json);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save {filename}: {ex.Message}");
            return false;
        }
    }
}
```

These system patterns provide a robust foundation for the Minipoll V5 application, ensuring maintainable, testable, and scalable code while leveraging the unique capabilities of the Unity MCP bridge integration.
