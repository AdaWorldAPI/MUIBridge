// File: MUIBridge/Extensions/MUIBridgeServiceCollectionExtensions.cs
// Purpose: DI registration extensions for easy MUIBridge integration.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MUIBridge.Core;
using MUIBridge.Themes;
using MUIBridge.Prediction.Interfaces;
using MUIBridge.Prediction.Services;

namespace MUIBridge.Extensions
{
    /// <summary>
    /// Extension methods for registering MUIBridge services with DI container.
    /// </summary>
    public static class MUIBridgeServiceCollectionExtensions
    {
        /// <summary>
        /// Adds MUIBridge core services (themes and ThemeManager).
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">Optional configuration callback.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMUIBridge(
            this IServiceCollection services,
            Action<MUIBridgeOptions>? configureOptions = null)
        {
            var options = new MUIBridgeOptions();
            configureOptions?.Invoke(options);

            // Register built-in themes
            services.AddSingleton<ITheme, DefaultLightTheme>();
            services.AddSingleton<ITheme, DefaultDarkTheme>();
            services.AddSingleton<ITheme, AmigaWorkbenchTheme>();

            // Register ThemeManager with correct constructor signature
            services.AddSingleton<ThemeManager>(sp =>
            {
                var themes = sp.GetServices<ITheme>();
                var logger = sp.GetService<ILogger<ThemeManager>>();
                return new ThemeManager(themes, options.DefaultThemeName, logger);
            });

            return services;
        }

        /// <summary>
        /// Adds the prediction/mood layer services.
        /// Call this after AddMUIBridge() if you want AI-driven mood reactivity.
        /// </summary>
        public static IServiceCollection AddMUIBridgePrediction(this IServiceCollection services)
        {
            // Telemetry bus (singleton for app-wide event distribution)
            services.TryAddSingleton<IPredictionTelemetryBus, PredictionTelemetryBus>();

            // Wave mood adapter (bridges predictions to theme mood)
            services.TryAddSingleton<IWaveMoodAdapter, WaveMoodAdapter>();

            // Optional: Register as hosted service for auto start/stop
            // services.AddHostedService<WaveMoodAdapterHostedService>();

            return services;
        }

        /// <summary>
        /// Adds a custom theme implementation.
        /// </summary>
        /// <typeparam name="TTheme">Theme type implementing ITheme.</typeparam>
        public static IServiceCollection AddMUIBridgeTheme<TTheme>(this IServiceCollection services)
            where TTheme : class, ITheme
        {
            services.AddSingleton<ITheme, TTheme>();
            return services;
        }
    }

    /// <summary>
    /// Configuration options for MUIBridge initialization.
    /// </summary>
    public class MUIBridgeOptions
    {
        /// <summary>
        /// Name of the default theme to activate on startup.
        /// Defaults to "Light".
        /// </summary>
        public string DefaultThemeName { get; set; } = "Light";

        /// <summary>
        /// Whether to automatically start the mood adapter.
        /// </summary>
        public bool AutoStartMoodAdapter { get; set; } = true;

        /// <summary>
        /// Minimum mood change threshold before triggering UI updates (0.0 to 1.0).
        /// Higher values reduce UI flicker but decrease responsiveness.
        /// </summary>
        public float MoodChangeThreshold { get; set; } = 0.01f;
    }
}
