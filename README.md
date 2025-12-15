# MUIBridge - Magic User Interface Bridge

> Windows Forms drop-in replacement inspired by Amiga MUI, Unity, and modern theming patterns

## Vision

MUIBridge bridges the gap between legacy Windows Forms applications and modern UI expectations, providing:
- **Amiga MUI inspiration** - The elegance of Magic User Interface
- **Unity patterns** - Modern game engine UI concepts
- **Windows Forms compatibility** - Drop-in replacement for existing apps
- **AI migration path** - Designed to help companies modernize legacy code with AI assistance

## Features

### Theme System
- Centralized `ThemeManager` for application-wide theming
- Automatic theme propagation to all MUI controls
- Light/dark mode support
- Custom theme creation

### Mood-Reactive Controls
- Visual feedback based on application state
- `WaveMoodAdapter` for smooth mood transitions
- `RubiconGate` for threshold-based state changes
- Prediction telemetry for AI-driven suggestions

### Controls
- `MUIButton` - Themed button with mood reactivity
- `MUITextBox` - Themed text input
- `MUILabel` - Themed label
- `MUIForm` - Base form with auto-theming

## Project Structure

```
MUIBridge/
├── Core/
│   ├── ThemeManager.cs           # Central theme orchestration
│   └── ThemeChangedEventArgs.cs  # Event args for theme changes
│
├── Controls/
│   ├── MUIButton.cs              # Themed button control
│   ├── MUITextBox.cs             # Themed text input
│   └── MUILabel.cs               # Themed label
│
├── Forms/
│   └── MUIForm.cs                # Base form with auto-theming
│
├── Prediction/
│   ├── Config/
│   │   └── OptiScopeConfig.cs    # Prediction configuration
│   ├── Interfaces/
│   │   ├── IWaveMoodAdapter.cs   # Mood adaptation contract
│   │   ├── IPredictionTelemetryBus.cs
│   │   ├── IOptiScopeRenderer.cs
│   │   └── IRubiconGate.cs       # State threshold contract
│   ├── Services/
│   │   ├── WaveMoodAdapter.cs    # Smooth mood transitions
│   │   ├── RubiconGate.cs        # Threshold state changes
│   │   ├── PredictionTelemetryBus.cs
│   │   └── AsciiOptiScopeRenderer.cs
│   └── Models/
│       ├── PredictionPacket.cs
│       └── UserInteractionSignal.cs
│
└── Extensions/
    └── MUIBridgeServiceCollectionExtensions.cs  # DI setup
```

## Quick Start

### Installation
```bash
dotnet add package MUIBridge
```

### Basic Usage
```csharp
using MUIBridge.Core;
using MUIBridge.Forms;
using MUIBridge.Controls;

// Setup with DI
services.AddMUIBridge();

// Create a themed form
public class MainForm : MUIForm
{
    private readonly MUIButton _submitButton;

    public MainForm(ThemeManager themeManager) : base(themeManager)
    {
        _submitButton = new MUIButton(themeManager)
        {
            Text = "Submit",
            Location = new Point(10, 10)
        };
        Controls.Add(_submitButton);
    }
}
```

### Theme Switching
```csharp
// Switch to dark theme
themeManager.SetTheme(new DarkTheme());

// All MUI controls automatically update
```

### Mood Reactivity
```csharp
// Controls react to mood intensity (0.0 - 1.0)
themeManager.SetMoodIntensity(0.7f);

// Buttons, labels, etc. visually respond
```

## Integration with AdaWorld Ecosystem

MUIBridge is part of the larger AdaWorld API ecosystem:

| Repository | Relationship |
|------------|--------------|
| **DUSK** | SMB rewrite - MUIBridge provides UI |
| **DUSK.UI** | Alias pointing to MUIBridge |
| **SIMAFPort** | C# API layer - MUIBridge for desktop clients |
| **DuskMigration** | Migration tools using MUIBridge patterns |

## Why "Magic User Interface"?

The name honors the Amiga MUI (Magic User Interface) framework, known for:
- Elegant, skinnable interfaces
- Efficient resource usage
- Developer-friendly API
- Timeless design principles

MUIBridge brings these concepts to modern .NET while maintaining Windows Forms compatibility.

## AI Migration Path

MUIBridge is designed to assist AI-powered code modernization:

1. **Pattern Recognition** - AI can identify WinForms patterns
2. **Drop-in Replacement** - MUI controls match WinForms API
3. **Gradual Migration** - Mix legacy and MUI controls
4. **Theme Unification** - Consistent look across migrated code

## Roadmap

- [ ] Complete control set (DataGrid, ListView, TreeView)
- [ ] XAML-style declarative UI option
- [ ] Animation framework
- [ ] Accessibility improvements
- [ ] Cross-platform via Avalonia/MAUI

## Related Projects

- [SIMAFPort](https://github.com/AdaWorldAPI/SIMAFPort) - Complete ecosystem overview
- [SIMAF](https://github.com/AdaWorldAPI/SIMAF) - SAP middleware
- [DUSK](https://github.com/AdaWorldAPI/DUSK) - SMB rewrite

## License

Proprietary - AdaWorld API

## Version

1.0.0
