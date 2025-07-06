# Technical Context: Minipoll V5

## Technology Stack

### Core Platform
- **Engine**: Unity 6000.1.8f1 (Unity 6.1)
- **Target Platforms**: Windows, Mac, Linux (StandaloneWindows64 primary)
- **Render Pipeline**: Universal Render Pipeline (URP) 17.1.0
- **Graphics API**: DirectX 12 (Windows), Metal (Mac), Vulkan/OpenGL (Linux)

### Development Environment
- **IDE**: Visual Studio Code Insiders
- **AI Assistant**: GitHub Copilot with Agent Mode
- **Version Control**: Git with GitHub integration
- **Build System**: Unity Cloud Build (future consideration)

### AI Integration Stack
- **Unity MCP Bridge**: Custom package from @justinpbarnett/unity-mcp
- **MCP Server**: Python-based server running on localhost:6400
- **Communication Protocol**: Model Context Protocol (MCP)
- **Screenshot Automation**: Playwright with Chromium, Firefox, Webkit

### Unity Packages and Dependencies

#### Core Unity Packages
```json
{
  "com.unity.render-pipelines.universal": "17.1.0",
  "com.unity.inputsystem": "1.14.0",
  "com.unity.ugui": "2.0.0",
  "com.unity.visualscripting": "1.9.8",
  "com.unity.test-framework": "1.5.1",
  "com.unity.timeline": "1.8.8"
}
```

#### AI Integration Packages
```json
{
  "com.justinpbarnett.unity-mcp": "https://github.com/justinpbarnett/unity-mcp.git?path=/UnityMcpBridge"
}
```

#### Development Tools
```json
{
  "com.unity.ide.visualstudio": "2.0.23",
  "com.unity.ide.rider": "3.0.36",
  "com.unity.multiplayer.center": "1.0.0",
  "com.unity.ai.navigation": "2.0.8"
}
```

## Development Setup

### Prerequisites
- **Unity Hub**: Latest version with Unity 6000.1.8f1 installed
- **VS Code Insiders**: With Copilot extension and Agent Mode enabled
- **Python**: 3.8+ for MCP server functionality
- **Node.js**: Latest LTS for Playwright and npm packages
- **Git**: For version control and package management

### Environment Configuration

#### VS Code Settings
```json
{
  "chat.agent.enabled": true,
  "chat.mcp.enabled": true,
  "chat.mcp.discovery.enabled": true
}
```

#### MCP Server Configuration
```json
{
  "mcpServers": {
    "unity-mcp-clean": {
      "command": "python",
      "args": [
        "C:\\Users\\Amita\\AppData\\Local\\Programs\\UnityMCP\\UnityMcpServer\\src\\server.py"
      ],
      "cwd": "C:\\Users\\Amita\\AppData\\Local\\Programs\\UnityMCP\\UnityMcpServer\\src"
    }
  }
}
```

#### Unity Project Settings
- **Color Space**: Linear
- **Graphics API**: Auto (DirectX 12 preferred on Windows)
- **Scripting Backend**: IL2CPP
- **API Compatibility Level**: .NET Standard 2.1
- **Incremental GC**: Enabled

### File Structure
```
Minipoll_V5/
├── .github/
│   ├── instructions/
│   │   └── memory-bank.instructions.md
│   └── memory-bank/
│       ├── projectbrief.md
│       ├── productContext.md
│       ├── activeContext.md
│       ├── systemPatterns.md
│       ├── techContext.md
│       ├── progress.md
│       └── tasks/
├── .vscode/
│   └── settings.json
├── Assets/
│   ├── Scenes/
│   ├── Scripts/
│   ├── Prefabs/
│   ├── Materials/
│   ├── UI/
│   └── Data/
├── Packages/
│   ├── manifest.json
│   └── packages-lock.json
├── ProjectSettings/
├── Library/ (gitignored)
├── Logs/ (gitignored)
└── Temp/ (gitignored)
```

## Technical Constraints

### Unity 6.1 Specific Considerations
- **New Features**: Leverage Unity 6.1 improvements in rendering and performance
- **Breaking Changes**: Monitor for potential issues with newer Unity version
- **Package Compatibility**: Ensure all packages support Unity 6.1
- **Documentation**: Some features may have limited documentation due to newness

### Platform Limitations
- **Windows**: Primary development and testing platform
- **Mac**: Secondary testing (requires Mac hardware for builds)
- **Linux**: Tertiary support (may require additional testing)
- **Performance**: Target minimum 60 FPS on integrated graphics

### MCP Bridge Constraints
- **Connection Stability**: Python server must remain active for AI integration
- **Port Availability**: Requires port 6400 to be available
- **Error Handling**: Limited error recovery if MCP connection fails
- **Platform Dependency**: Currently Windows-optimized

### Resource Constraints
- **Memory**: Target maximum 2GB RAM usage
- **Storage**: Minimal persistent data requirements
- **Network**: Local-only operation (no network requirements)
- **Graphics**: Support for DirectX 11+ equivalent on all platforms

## Dependencies Management

### Unity Package Manager
- **Registry Sources**: Unity Registry, Git repositories
- **Version Locking**: Lock critical package versions for stability
- **Update Strategy**: Conservative updates with thorough testing
- **Custom Packages**: Unity MCP Bridge as custom Git dependency

### External Dependencies
- **Python MCP Server**: Requires separate installation and maintenance
- **Playwright**: Managed through npm, requires browser installations
- **VS Code Extensions**: GitHub Copilot, Unity tools

### Version Control Strategy
- **Git LFS**: For large assets (textures, models, audio)
- **Ignore Patterns**: Standard Unity .gitignore with custom additions
- **Branch Strategy**: Main branch for stable releases, feature branches for development
- **Commit Hooks**: Automated checks for Unity scene and prefab conflicts

## Build Configuration

### Development Builds
- **Debug Symbols**: Enabled for development builds
- **Logging**: Verbose logging for MCP integration debugging
- **Assertions**: Enabled for catch development errors
- **Profiler**: Enabled for performance monitoring

### Release Builds
- **Optimization**: Script optimization enabled
- **Stripping**: Aggressive code stripping for smaller builds
- **Compression**: LZ4 compression for faster loading
- **Security**: Remove development-only features

### CI/CD Considerations
- **Unity Cloud Build**: Future consideration for automated builds
- **Testing Pipeline**: Automated testing with Unity Test Framework
- **Build Validation**: Ensure MCP integration doesn't break builds
- **Platform Testing**: Automated testing across target platforms

## Performance Considerations

### Unity Performance
- **URP Optimization**: Leverage URP's mobile-optimized rendering
- **Batching**: Minimize draw calls through proper batching
- **Texture Optimization**: Appropriate texture compression and sizing
- **Memory Management**: Object pooling for UI elements

### MCP Performance
- **Connection Pooling**: Reuse MCP connections where possible
- **Command Batching**: Group related MCP commands for efficiency
- **Error Recovery**: Quick fallback when MCP is unavailable
- **Resource Monitoring**: Track MCP server resource usage

### Screenshot Performance
- **Selective Capture**: Only capture screenshots for significant changes
- **Compression**: Optimize screenshot file sizes
- **Async Operations**: Don't block development workflow for screenshots
- **Storage Management**: Automatic cleanup of old screenshots

## Security Considerations

### Local Development
- **MCP Server**: Localhost-only binding for security
- **File Access**: Limit file system access to project directories
- **Network Isolation**: No external network requirements
- **Data Privacy**: All poll data stored locally

### Code Security
- **Input Validation**: Sanitize all user inputs
- **Error Handling**: Prevent information leakage through error messages
- **Code Obfuscation**: Consider obfuscation for release builds
- **Asset Protection**: Protect sensitive assets from extraction

## Troubleshooting Guide

### Common MCP Issues
- **Connection Refused**: Check Python server status and port availability
- **Permission Denied**: Verify Unity Hub and Editor permissions
- **Timeout Errors**: Increase MCP timeout settings if needed
- **Version Conflicts**: Ensure Python and Unity MCP Bridge compatibility

### Unity Development Issues
- **Package Resolution**: Delete Library folder and reimport if package issues occur
- **Build Failures**: Check for platform-specific requirements and settings
- **Performance Issues**: Use Unity Profiler to identify bottlenecks
- **Input System**: Verify Input System settings for target platforms

### VS Code Integration Issues
- **Copilot Errors**: Restart VS Code and verify Copilot subscription
- **MCP Discovery**: Check MCP configuration in VS Code settings
- **Extension Conflicts**: Disable conflicting extensions if issues arise
- **Performance**: Monitor VS Code memory usage with large Unity projects

This technical foundation provides a solid base for developing Minipoll V5 while leveraging cutting-edge AI integration capabilities through the Unity MCP bridge.
