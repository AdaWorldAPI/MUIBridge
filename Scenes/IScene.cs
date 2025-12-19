// File: MUIBridge/Scenes/IScene.cs
// Purpose: Core scene contract for Unity-style navigation (ported from DUSK).

namespace MUIBridge.Scenes
{
    /// <summary>
    /// Defines the contract for a navigable scene in MUIBridge.
    /// Scenes provide Unity-style Push/Pop navigation with lifecycle management.
    /// </summary>
    public interface IScene : IDisposable
    {
        /// <summary>
        /// Unique identifier for this scene.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Display name for this scene.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets whether this scene is currently visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Gets whether this scene has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Called once when the scene is first registered.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Called when the scene becomes the active scene.
        /// </summary>
        void Show();

        /// <summary>
        /// Called when the scene is no longer the active scene.
        /// </summary>
        void Hide();

        /// <summary>
        /// Called every frame while the scene is active.
        /// </summary>
        /// <param name="deltaTime">Time since last update in seconds.</param>
        void Update(float deltaTime);

        /// <summary>
        /// Called to render the scene.
        /// </summary>
        void Render();
    }
}
