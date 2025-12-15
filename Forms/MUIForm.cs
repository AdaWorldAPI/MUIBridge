// File: MUIBridge/Forms/MUIForm.cs
// Purpose: WinForms-compatible Form with automatic theming.
// 100% drop-in replacement for System.Windows.Forms.Form.

using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MUIBridge.Core;

namespace MUIBridge.Forms
{
    /// <summary>
    /// Base form for MUIBridge applications with automatic theme application.
    /// Drop-in replacement for System.Windows.Forms.Form.
    /// </summary>
    public class MUIForm : Form
    {
        protected readonly ThemeManager? _themeManager;
        private readonly ILogger<MUIForm>? _logger;
        private bool _isApplyingTheme;
        private Font? _appliedFont;

        /// <summary>
        /// Creates a new MUIForm without ThemeManager (for designer compatibility).
        /// </summary>
        public MUIForm()
        {
            _logger?.LogWarning("MUIForm created without ThemeManager - theming not applied.");
            InitializeMuiForm();
        }

        /// <summary>
        /// Creates a new MUIForm with ThemeManager (recommended for DI).
        /// </summary>
        public MUIForm(ThemeManager themeManager, ILogger<MUIForm>? logger = null)
        {
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _logger = logger;

            InitializeMuiForm();
            ApplyTheme(_themeManager.CurrentTheme);

            _themeManager.ThemeChanged += ThemeManager_ThemeChanged;
            _logger?.LogDebug("MUIForm created with ThemeManager.");
        }

        private void InitializeMuiForm()
        {
            DoubleBuffered = true;
        }

        private void ThemeManager_ThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            if (_isApplyingTheme) return;

            ApplyTheme(e.NewTheme);
            _logger?.LogTrace("Theme changed to {Theme}", e.NewTheme.Name);
        }

        /// <summary>
        /// Applies a theme to this form.
        /// Override in derived forms for custom behavior.
        /// </summary>
        protected virtual void ApplyTheme(ITheme theme)
        {
            if (theme == null) return;

            _isApplyingTheme = true;
            try
            {
                BackColor = theme.BackgroundColor;
                ForeColor = theme.TextColor;

                try
                {
                    if (Font.Name != theme.DefaultFontName ||
                        Math.Abs(Font.Size - theme.DefaultFontSize) > 0.1f)
                    {
                        _appliedFont?.Dispose();
                        _appliedFont = new Font(theme.DefaultFontName, theme.DefaultFontSize);
                        Font = _appliedFont;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to apply font {Font}/{Size}",
                        theme.DefaultFontName, theme.DefaultFontSize);
                }

                _logger?.LogTrace("Theme '{Theme}' applied to form '{Form}'", theme.Name, Name);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error applying theme '{Theme}' to form '{Form}'",
                    theme.Name, Name);
            }
            finally
            {
                _isApplyingTheme = false;
            }

            Invalidate(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_themeManager != null)
                {
                    _themeManager.ThemeChanged -= ThemeManager_ThemeChanged;
                    _logger?.LogDebug("MUIForm disposed and unsubscribed from ThemeManager.");
                }

                _appliedFont?.Dispose();
                _appliedFont = null;
            }

            base.Dispose(disposing);
        }
    }
}
