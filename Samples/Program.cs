// File: MUIBridge/Samples/Program.cs
// Purpose: Demo application showing MUIBridge drop-in replacement for WinForms.
// This demonstrates how to migrate a legacy SMB application.

using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MUIBridge.Core;
using MUIBridge.Extensions;
using MUIBridge.Forms;

namespace MUIBridge.Samples
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // === Setup Dependency Injection ===
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            // Add MUIBridge with default theme
            services.AddMUIBridge(options =>
            {
                options.DefaultThemeName = "Light"; // or "Dark" or "AmigaWorkbench"
            });

            // Add prediction layer for AI-driven mood reactivity
            services.AddMUIBridgePrediction();

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // === Start the mood adapter ===
            var moodAdapter = serviceProvider.GetRequiredService<IWaveMoodAdapter>();
            moodAdapter.StartAsync().Wait();

            // === Run the main form ===
            var mainForm = serviceProvider.GetRequiredService<DemoMainForm>();
            Application.Run(mainForm);

            // Cleanup
            moodAdapter.StopAsync().Wait();
        }
    }
}
