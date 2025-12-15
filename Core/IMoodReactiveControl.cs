// File: MUIBridge/Core/IMoodReactiveControl.cs
// Purpose: Contract for controls that react visually to prediction mood changes.
// Inspired by Amiga MUI's dynamic theming capabilities.

namespace MUIBridge.Core
{
    /// <summary>
    /// Interface for MUIBridge controls that react visually to prediction mood.
    /// Implementing controls will update their appearance based on AI prediction confidence.
    /// </summary>
    public interface IMoodReactiveControl
    {
        /// <summary>
        /// Updates the control's visual appearance based on mood intensity.
        /// Called automatically when ThemeManager.PredictionMoodChanged fires.
        /// </summary>
        /// <param name="moodIntensity">Mood intensity from 0.0 (neutral) to 1.0 (maximum).</param>
        /// <param name="activeTheme">The currently active theme for color calculations.</param>
        void UpdateMoodVisuals(float moodIntensity, ITheme activeTheme);
    }
}
