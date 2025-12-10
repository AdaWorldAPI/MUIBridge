// File: MUIBridge/Core/ThemeManager.cs
// Purpose: Central manager for themes and prediction mood state.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace MUIBridge.Core
{
    /// <summary>
    /// Manages available themes, the active theme, and prediction mood state.
    /// Central orchestrator for MUIBridge theming and mood reactivity.
    /// </summary>
    public class ThemeManager
    {
        private readonly IReadOnlyDictionary<string, ITheme> _availableThemes;
        private readonly ILogger<ThemeManager>? _logger;
        private ITheme _currentTheme;

        /// <summary>
        /// Raised when the active theme changes.
        /// </summary>
        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        /// <summary>
        /// Raised when the prediction mood intensity changes.
        /// </summary>
        public event EventHandler<float>? PredictionMoodChanged;

        /// <summary>
        /// Gets the currently active theme.
        /// </summary>
        public ITheme CurrentTheme => _currentTheme;

        /// <summary>
        /// Gets the names of all available themes.
        /// </summary>
        public IEnumerable<string> AvailableThemeNames => _availableThemes.Keys;

        /// <summary>
        /// Gets the current prediction mood intensity (0.0 to 1.0).
        /// </summary>
        public float CurrentPredictionMoodIntensity => _currentTheme.PredictionMoodIntensity;

        /// <summary>
        /// Initializes a new ThemeManager instance.
        /// </summary>
        /// <param name="themes">Collection of available themes.</param>
        /// <param name="defaultThemeName">Name of the default theme to activate.</param>
        /// <param name="logger">Optional logger instance.</param>
        public ThemeManager(
            IEnumerable<ITheme> themes,
            string defaultThemeName,
            ILogger<ThemeManager>? logger = null)
        {
            _logger = logger;

            if (themes == null)
                throw new ArgumentNullException(nameof(themes));

            try
            {
                _availableThemes = themes.ToDictionary(
                    t => t.Name,
                    t => t,
                    StringComparer.OrdinalIgnoreCase);

                _logger?.LogDebug("Registered {Count} themes: {Names}",
                    _availableThemes.Count,
                    string.Join(", ", _availableThemes.Keys));
            }
            catch (ArgumentException ex)
            {
                _logger?.LogError(ex, "Duplicate theme names detected.");
                throw new ArgumentException("Duplicate theme names detected.", nameof(themes), ex);
            }

            if (_availableThemes.Count == 0)
            {
                _logger?.LogError("No themes provided.");
                throw new ArgumentException("At least one theme must be provided.", nameof(themes));
            }

            // Set default theme
            if (string.IsNullOrWhiteSpace(defaultThemeName) ||
                !_availableThemes.TryGetValue(defaultThemeName, out _currentTheme!))
            {
                _currentTheme = _availableThemes.First().Value;
                _logger?.LogWarning("Default theme '{Default}' not found. Using '{Fallback}'.",
                    defaultThemeName, _currentTheme.Name);
            }
            else
            {
                _logger?.LogInformation("ThemeManager initialized with default theme '{Theme}'.",
                    _currentTheme.Name);
            }

            _currentTheme.PredictionMoodIntensity = 0f;
        }

        /// <summary>
        /// Sets the active theme by name.
        /// </summary>
        /// <param name="themeName">Name of the theme to activate.</param>
        /// <returns>True if theme was found and set, false otherwise.</returns>
        public bool SetTheme(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
            {
                _logger?.LogWarning("SetTheme called with empty theme name.");
                return false;
            }

            if (_availableThemes.TryGetValue(themeName, out ITheme? newTheme))
            {
                if (newTheme.Name.Equals(_currentTheme.Name, StringComparison.OrdinalIgnoreCase))
                {
                    _logger?.LogDebug("Theme '{Theme}' is already active.", themeName);
                    return true;
                }

                ITheme previousTheme = _currentTheme;
                _currentTheme = newTheme;
                _currentTheme.PredictionMoodIntensity = 0f; // Reset mood on theme change

                _logger?.LogInformation("Theme changed from '{Previous}' to '{New}'.",
                    previousTheme.Name, _currentTheme.Name);

                OnThemeChanged(new ThemeChangedEventArgs(previousTheme, _currentTheme));
                OnPredictionMoodChanged(_currentTheme.PredictionMoodIntensity);

                return true;
            }

            _logger?.LogWarning("Theme '{Theme}' not found.", themeName);
            return false;
        }

        /// <summary>
        /// Sets the prediction mood intensity on the current theme.
        /// </summary>
        /// <param name="moodIntensity">Intensity value (0.0 to 1.0).</param>
        public void SetPredictionMood(float moodIntensity)
        {
            float clamped = Math.Clamp(moodIntensity, 0f, 1f);

            if (Math.Abs(_currentTheme.PredictionMoodIntensity - clamped) > 0.001f)
            {
                _logger?.LogTrace("Setting mood intensity to {Mood:F2}", clamped);
                _currentTheme.PredictionMoodIntensity = clamped;
                OnPredictionMoodChanged(clamped);
            }
        }

        /// <summary>
        /// Gets a theme by name.
        /// </summary>
        public ITheme? GetThemeByName(string themeName)
        {
            _availableThemes.TryGetValue(themeName, out ITheme? theme);
            return theme;
        }

        protected virtual void OnThemeChanged(ThemeChangedEventArgs e)
        {
            ThemeChanged?.Invoke(this, e);
        }

        protected virtual void OnPredictionMoodChanged(float newMoodIntensity)
        {
            PredictionMoodChanged?.Invoke(this, newMoodIntensity);
        }
    }

    /// <summary>
    /// Event arguments for theme changes.
    /// </summary>
    public class ThemeChangedEventArgs : EventArgs
    {
        public ITheme PreviousTheme { get; }
        public ITheme NewTheme { get; }

        public ThemeChangedEventArgs(ITheme previousTheme, ITheme newTheme)
        {
            PreviousTheme = previousTheme;
            NewTheme = newTheme;
        }
    }
}
