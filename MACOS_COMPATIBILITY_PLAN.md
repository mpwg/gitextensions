# GitExtensions macOS Compatibility Plan

This document outlines a comprehensive plan to make GitExtensions runnable on macOS by identifying and addressing Windows-specific components.

## Executive Summary

GitExtensions is currently a Windows-only application with several Windows-specific dependencies that need to be addressed for macOS compatibility:

1. **Windows Shell Extensions** - Native Windows Explorer integration
2. **Visual Studio Integration** - COM-based Visual Studio interaction  
3. **Windows Taskbar Features** - Jump lists and thumbnail toolbars
4. **Windows-specific APIs** - Direct Win32 API calls
5. **Native Components** - C++ components for SSH and shell integration
6. **Platform Detection Logic** - Currently limited Windows/Unix detection

## Detailed Analysis

### 1. Native Windows Components (Complete Removal Required)

#### A. GitExtensionsShellEx (src/native/GitExtensionsShellEx/)
- **Purpose**: Windows Explorer context menu integration
- **Impact**: Right-click Git operations in Windows Explorer
- **macOS Alternative**: None - Finder doesn't support similar shell extensions
- **Action**: Remove entirely, document loss of functionality

#### B. GitExtSshAskPass (src/native/GitExtSshAskPass/)  
- **Purpose**: Windows-specific SSH password dialog
- **Impact**: SSH authentication prompts
- **macOS Alternative**: Use standard SSH agent or terminal prompts
- **Action**: Replace with cross-platform SSH handling

### 2. Windows-Specific Integrations (Disable/Remove)

#### A. Visual Studio Integration (src/app/GitUI/VisualStudioIntegration.cs)
**Current Windows-specific code:**
```csharp
[DllImport("user32.dll")]
[DllImport("ole32.dll")]
string vswhere = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
```
- **Action**: Wrap in platform detection and disable on macOS
- **Alternative**: Could integrate with VS Code on macOS in the future

#### B. Windows Taskbar Features
**Files:**
- `src/app/GitUI/WindowsJumpListManager.cs` - Windows taskbar jump lists
- `src/app/GitUI/TaskbarProgress.cs` - Taskbar progress indicators

**Action**: Disable on macOS with platform detection

#### C. Windows Shell Operations (src/app/GitCommands/Git/OsShellUtil.cs)
**Current Windows-specific code:**
```csharp
new Executable("rundll32.exe").Start("shell32.dll,OpenAs_RunDLL " + filePath);
new Executable("explorer.exe").Start(...);
```
- **Action**: Implement macOS equivalents using `open` command

### 3. Platform Detection Enhancement

#### Current State (src/app/GitCommands/Utils/EnvUtils.cs)
```csharp
public static bool RunningOnWindows()
public static bool RunningOnMacOSX() 
public static bool RunningOnUnix()
```

#### Required Enhancements
- Add more granular macOS-specific detection
- Add methods for macOS version detection
- Update all Windows-specific code paths to check platform

### 4. Windows API Dependencies

#### Win32 API Calls in GitExtUtils
**Files requiring attention:**
- `src/app/GitExtUtils/GitUI/Interops/User32/` - Windows User32 APIs
- `src/app/GitExtUtils/GitUI/Interops/ComCtl32/` - Windows Common Controls
- `src/app/GitExtUtils/GitUI/Win32ApiUtil.cs`

**Action**: Wrap in platform detection and provide no-op alternatives on macOS

### 5. UI and Theming Considerations

#### DPI Scaling (src/app/GitExtUtils/GitUI/ControlDpiExtensions.cs)
- Currently Windows-focused
- macOS has different high-DPI handling
- **Action**: Implement macOS-appropriate scaling

#### Theming (src/app/GitExtUtils/GitUI/Theming/)
- Some Windows-specific visual style fixes
- **Action**: Test and adjust for macOS appearance

## Implementation Plan

### Phase 1: Platform Detection Foundation
1. **Enhance EnvUtils.cs**
   ```csharp
   public static bool RunningOnMacOS() 
   public static MacOSVersion GetMacOSVersion()
   public static bool SupportsNativeMenus() // for future native menu integration
   ```

2. **Update Program.cs entry point**
   ```csharp
   if (EnvUtils.RunningOnWindows())
   {
       WebBrowserEmulationMode.SetBrowserFeatureControl();
       FormFixHome.CheckHomePath();
   }
   ```

### Phase 2: Disable Windows-Specific Features
1. **Visual Studio Integration**
   - Modify `VisualStudioIntegration.cs` to return early on non-Windows
   - Update menu items to hide VS-related options on macOS

2. **Windows Shell Extensions**
   - Remove native projects from macOS builds
   - Update build scripts to exclude native components on macOS

3. **Taskbar Features**
   - Wrap `WindowsJumpListManager` and `TaskbarProgress` with platform checks
   - Provide no-op implementations for macOS

### Phase 3: Implement macOS Alternatives
1. **File Operations**
   ```csharp
   // In OsShellUtil.cs
   public static void OpenAs(string filePath)
   {
       if (EnvUtils.RunningOnMacOS())
       {
           new Executable("open").Start($"-a 'Choose Application' '{filePath}'");
       }
       else
       {
           // existing Windows implementation
       }
   }
   ```

2. **File Explorer Integration**
   ```csharp
   public static void SelectPathInFileExplorer(string filePath)
   {
       if (EnvUtils.RunningOnMacOS())
       {
           new Executable("open").Start($"-R '{filePath}'");
       }
       else
       {
           OpenWithFileExplorer($"/select, {filePath.Quote()}", quote: false);
       }
   }
   ```

### Phase 4: Build and Distribution
1. **Update Build System**
   - Modify `.csproj` files to exclude Windows-specific references on macOS
   - Update CI/CD to build macOS packages
   - Create macOS application bundle

2. **Dependencies**
   - Ensure all NuGet packages support macOS
   - Replace Windows-specific packages with cross-platform alternatives

## Testing Strategy

### Functional Testing
1. **Core Git Operations**: Clone, commit, push, pull, branch management
2. **UI Components**: All forms and dialogs render correctly on macOS
3. **File Operations**: File viewing, editing, and external tool integration
4. **Settings Management**: Configuration persistence and migration

### Platform-Specific Testing
1. **macOS Integration**: File associations, dock integration
2. **Performance**: Memory usage and startup time on macOS
3. **Accessibility**: VoiceOver and other macOS accessibility features

## Risk Assessment

### High Risk
- **UI Framework Compatibility**: WinForms behavior differences on macOS
- **File System Differences**: Case sensitivity, path separators
- **External Dependencies**: Git installation paths, merge tools

### Medium Risk  
- **Performance**: Potential slowdowns due to cross-platform overhead
- **User Experience**: Loss of native Windows integrations

### Low Risk
- **Core Git Functionality**: Should work identically across platforms
- **Settings and Configuration**: Already platform-agnostic

## Timeline Estimate

- **Phase 1** (Platform Detection): 1-2 weeks
- **Phase 2** (Disable Windows Features): 2-3 weeks  
- **Phase 3** (macOS Alternatives): 3-4 weeks
- **Phase 4** (Build/Distribution): 2-3 weeks
- **Testing and Polish**: 3-4 weeks

**Total Estimated Time**: 11-16 weeks

## Success Criteria

1. ✅ GitExtensions launches successfully on macOS
2. ✅ All core Git operations function correctly
3. ✅ UI is fully functional and follows macOS conventions
4. ✅ No Windows-specific errors or crashes
5. ✅ Performance is acceptable (within 20% of Windows performance)
6. ✅ Documentation updated for macOS users

## Known Limitations on macOS

After implementation, the following Windows-specific features will not be available:

1. **Windows Explorer Integration** - No right-click context menus in Finder
2. **Visual Studio Integration** - No direct integration with Visual Studio
3. **Windows Taskbar Features** - No jump lists or taskbar progress
4. **Shell Extension Features** - No native file system integration

These limitations should be clearly documented for macOS users, with suggested workflows using the main application interface.

## Future Enhancements

Once basic macOS compatibility is achieved, consider:

1. **Finder Integration** - Explore Finder extensions or Quick Actions
2. **VS Code Integration** - Direct integration with VS Code on macOS
3. **macOS Menu Bar** - Native macOS application menu
4. **Touch Bar Support** - MacBook Pro Touch Bar integration
5. **Dock Integration** - Dock menu items and badge notifications

## Conclusion

Making GitExtensions compatible with macOS is achievable with a systematic approach to identifying and addressing Windows-specific dependencies. The core Git functionality and UI should work well on macOS, though some Windows-specific integrations will be lost. This trade-off is acceptable to make the application accessible to macOS developers.