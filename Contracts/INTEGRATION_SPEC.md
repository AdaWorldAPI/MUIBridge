# MUIBridge Integration Specification

**Version:** 1.0.0
**Last Updated:** 2024
**Target:** .NET 8.0+ WinForms Applications

---

## Quick Start

### 1. Add Reference
```csharp
// Add MUIBridge project reference or NuGet package
<PackageReference Include="MUIBridge" Version="1.0.0" />
```

### 2. Register Services
```csharp
var services = new ServiceCollection();
services.AddMUIBridge(options => options.DefaultTheme = "Light");
services.AddMUIBridgePrediction(); // Optional: AI mood features
var provider = services.BuildServiceProvider();
```

### 3. Use Controls
```csharp
// Old WinForms
Button btn = new Button();

// New MUIBridge (with DI)
var btn = new MUIButton(provider.GetRequiredService<ThemeManager>());
```

---

## DTOs Reference

### ThemeDto
Used for defining and loading themes.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `name` | string | ✓ | Unique identifier |
| `primaryColor` | string | ✓ | Main brand color (#RRGGBB) |
| `secondaryColor` | string | ✓ | Supporting color |
| `backgroundColor` | string | ✓ | Form/control background |
| `textColor` | string | ✓ | Default text color |
| `borderColor` | string | ✓ | Border color |
| `accentColor` | string | ✓ | Highlight/mood color |
| `fontFamily` | string | | Font name (default: "Segoe UI") |
| `fontSize` | float | | Font size in points (default: 9) |
| `cornerRadius` | float | | Corner rounding (default: 4) |
| `borderThickness` | float | | Border width (default: 1) |
| `moodBlendFactor` | float | | Max mood blend (default: 0.6) |

**Example JSON:**
```json
{
  "name": "Corporate",
  "primaryColor": "#1E88E5",
  "secondaryColor": "#E3F2FD",
  "backgroundColor": "#FFFFFF",
  "textColor": "#212121",
  "borderColor": "#BDBDBD",
  "accentColor": "#FF5722",
  "fontFamily": "Segoe UI",
  "fontSize": 9,
  "cornerRadius": 2,
  "borderThickness": 1
}
```

### PredictionDto
Used for sending predictions to drive mood.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `confidence` | float | ✓ | **Primary mood driver** (0.0-1.0) |
| `predictedValue` | string | ✓ | What was predicted |
| `sourceModel` | string | | Model identifier |
| `targetContext` | string | | Where prediction applies |
| `moodLabel` | string | | Label (Neutral/Engaged/Alert/Optimal) |
| `correlationId` | string | | For tracing |

**Example JSON:**
```json
{
  "confidence": 0.85,
  "predictedValue": "user_will_complete_purchase",
  "sourceModel": "UserBehaviorModel_v2",
  "targetContext": "checkout_form",
  "moodLabel": "Optimal"
}
```

### UserSignalDto
Used for capturing user interactions.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `actionType` | string | ✓ | Click/KeyPress/Focus/etc. |
| `elementId` | string | ✓ | Control identifier |
| `elementType` | string | ✓ | Button/TextBox/etc. |
| `sessionId` | string | | Session tracking |
| `durationMs` | long? | | For timed actions |
| `formContext` | string | | Current form name |

**Standard Action Types:**
- `Click`, `DoubleClick`
- `KeyPress`, `KeyDown`, `KeyUp`
- `Focus`, `Blur`
- `Scroll`, `Hover`
- `Select`, `Deselect`
- `Submit`, `Cancel`
- `Navigate`

---

## Service Interfaces

### IThemeService
Control themes programmatically.

```csharp
public interface IThemeService
{
    string CurrentThemeName { get; }
    ThemeStateDto GetCurrentState();
    IReadOnlyList<string> GetAvailableThemes();
    bool SetTheme(string themeName);
    bool RegisterTheme(ThemeDto theme);
    event EventHandler<ThemeChangedEventDto>? ThemeChanged;
}
```

**Usage:**
```csharp
// Inject or resolve
var themeService = provider.GetRequiredService<IThemeService>();

// Switch theme
themeService.SetTheme("Dark");

// Listen to changes
themeService.ThemeChanged += (s, e) =>
    Console.WriteLine($"Theme: {e.PreviousTheme} → {e.NewTheme}");
```

### IMoodService
Control mood/prediction reactivity.

```csharp
public interface IMoodService
{
    float CurrentMoodIntensity { get; }
    MoodStateDto GetCurrentMood();
    void SetMoodIntensity(float intensity);
    void ResetMood();
    Task ApplyPredictionAsync(PredictionDto prediction);
    event EventHandler<MoodChangedEventDto>? MoodChanged;
}
```

**Usage:**
```csharp
var moodService = provider.GetRequiredService<IMoodService>();

// Set mood directly
moodService.SetMoodIntensity(0.75f);

// Or apply prediction
await moodService.ApplyPredictionAsync(new PredictionDto
{
    Confidence = 0.9f,
    PredictedValue = "high_engagement"
});
```

### IPredictionService
Send/receive predictions.

```csharp
public interface IPredictionService
{
    Task PublishSignalAsync(UserSignalDto signal);
    Task PublishPredictionAsync(PredictionDto prediction);
    Task<PredictionDto?> GetPredictionAsync(UserSignalDto signal);
    event Func<PredictionDto, Task>? PredictionReceived;
}
```

---

## Events

### Theme Changed
```csharp
themeManager.ThemeChanged += (sender, args) =>
{
    // args.PreviousTheme - old theme name
    // args.NewTheme - new theme name
    // args.Timestamp - when changed
};
```

### Mood Changed
```csharp
moodService.MoodChanged += (sender, args) =>
{
    // args.PreviousIntensity - old value
    // args.NewIntensity - new value (0.0-1.0)
    // args.Label - "Neutral", "Engaged", etc.
    // args.Delta - change amount
};
```

### Prediction Received
```csharp
predictionService.PredictionReceived += async (prediction) =>
{
    // prediction.Confidence - confidence score
    // prediction.PredictedValue - what was predicted
    // prediction.SourceModel - model name
};
```

---

## Configuration

### appsettings.json Example
```json
{
  "MUIBridge": {
    "defaultTheme": "Light",
    "followSystemTheme": false,
    "prediction": {
      "enabled": true,
      "autoStart": true,
      "moodChangeThreshold": 0.01,
      "moodSmoothingFactor": 0.2,
      "moodDecayRate": 0.05
    },
    "features": {
      "moodReactivity": true,
      "themeSwitching": true,
      "focusAnimations": true
    }
  }
}
```

### Load Configuration
```csharp
var config = new MUIBridgeConfigDto
{
    DefaultTheme = "Dark",
    Prediction = new PredictionConfigDto
    {
        Enabled = true,
        MoodChangeThreshold = 0.02f
    }
};

services.AddMUIBridge(config);
```

---

## Integration Patterns

### Pattern 1: Direct DI Integration
Best for new applications.

```csharp
// Program.cs
services.AddMUIBridge();
services.AddMUIBridgePrediction();
services.AddTransient<MainForm>();

// MainForm.cs
public class MainForm : MUIForm
{
    public MainForm(ThemeManager tm) : base(tm) { }
}
```

### Pattern 2: Legacy Migration
For existing WinForms apps.

```csharp
// Create static accessor
public static class MUIBridgeHost
{
    public static ThemeManager ThemeManager { get; private set; }

    public static void Initialize()
    {
        var services = new ServiceCollection();
        services.AddMUIBridge();
        var provider = services.BuildServiceProvider();
        ThemeManager = provider.GetRequiredService<ThemeManager>();
    }
}

// In existing forms
var btn = new MUIButton(MUIBridgeHost.ThemeManager);
```

### Pattern 3: External ML Service
Integrate with external prediction API.

```csharp
// Configure external service
services.AddMUIBridge(options =>
{
    options.Prediction.PredictionServiceUrl = "https://ml.example.com/predict";
    options.Prediction.PredictionApiKey = "your-api-key";
});

// Or publish predictions from your service
httpClient.PostAsJsonAsync("/api/muibridge/prediction", new
{
    confidence = 0.87,
    predictedValue = "will_convert",
    sourceModel = "ConversionModel"
});
```

### Pattern 4: Webhook Integration
Forward events to external systems.

```csharp
public class WebhookPublisher : IMUIBridgeEventPublisher
{
    public async Task PublishMoodChangedAsync(MoodChangedEventDto args)
    {
        await httpClient.PostAsJsonAsync(webhookUrl, new WebhookPayloadDto
        {
            EventType = "mood.changed",
            Payload = args
        });
    }
}
```

---

## Control Mapping

| WinForms | MUIBridge | Features |
|----------|-----------|----------|
| `Button` | `MUIButton` | Mood pulse, rounded corners |
| `Label` | `MUILabel` | Mood text color |
| `TextBox` | `MUITextBox` | Focus border, mood border |
| `CheckBox` | `MUICheckBox` | Material checkmark, mood color |
| `ComboBox` | `MUIComboBox` | Custom dropdown, focus color |
| `Panel` | `MUIPanel` | Mood background, borders |
| `DataGridView` | `MUIDataGridView` | Themed headers, mood selection |
| `ProgressBar` | `MUIProgressBar` | Thin material style, mood gradient |
| `Form` | `MUIForm` | Auto-theming, double buffered |

---

## REST API Endpoints

If exposing MUIBridge via REST API:

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/muibridge/state` | GET | Get current state |
| `/api/muibridge/theme` | GET | Get current theme |
| `/api/muibridge/theme` | POST | Set theme |
| `/api/muibridge/themes` | GET | List all themes |
| `/api/muibridge/mood` | GET | Get current mood |
| `/api/muibridge/mood` | POST | Set mood |
| `/api/muibridge/prediction` | POST | Publish prediction |
| `/api/muibridge/signal` | POST | Publish signal |

---

## Error Codes

| Code | Description |
|------|-------------|
| `THEME_NOT_FOUND` | Theme name doesn't exist |
| `INVALID_MOOD_VALUE` | Mood value outside 0.0-1.0 |
| `PREDICTION_UNAVAILABLE` | Prediction service down |
| `CONFIG_ERROR` | Configuration invalid |
| `VALIDATION_ERROR` | Request validation failed |

---

## Version History

| Version | Changes |
|---------|---------|
| 1.0.0 | Initial release with core controls and prediction layer |

---

## Support

- GitHub Issues: https://github.com/your-org/MUIBridge/issues
- Documentation: See `/Samples` folder for working examples
