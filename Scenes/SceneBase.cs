// File: MUIBridge/Scenes/SceneBase.cs
// Purpose: Base implementation for scenes with common lifecycle handling.

using System.Windows.Forms;

namespace MUIBridge.Scenes
{
    /// <summary>
    /// Base class for scenes providing common lifecycle implementation.
    /// Wraps a Form or UserControl for WinForms integration.
    /// </summary>
    public abstract class SceneBase : IScene
    {
        private bool _disposed;

        public virtual string Id => GetType().Name;
        public virtual string Name => GetType().Name;
        public bool IsVisible { get; private set; }
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// The underlying WinForms control for this scene.
        /// </summary>
        protected Control? RootControl { get; set; }

        public virtual void Initialize()
        {
            if (IsInitialized) return;
            OnInitialize();
            IsInitialized = true;
        }

        public virtual void Show()
        {
            if (IsVisible) return;
            IsVisible = true;
            RootControl?.Show();
            OnShow();
        }

        public virtual void Hide()
        {
            if (!IsVisible) return;
            IsVisible = false;
            RootControl?.Hide();
            OnHide();
        }

        public virtual void Update(float deltaTime)
        {
            if (!IsVisible) return;
            OnUpdate(deltaTime);
        }

        public virtual void Render()
        {
            if (!IsVisible) return;
            RootControl?.Refresh();
            OnRender();
        }

        /// <summary>Override to perform initialization logic.</summary>
        protected virtual void OnInitialize() { }

        /// <summary>Override to perform show logic.</summary>
        protected virtual void OnShow() { }

        /// <summary>Override to perform hide logic.</summary>
        protected virtual void OnHide() { }

        /// <summary>Override to perform per-frame update logic.</summary>
        protected virtual void OnUpdate(float deltaTime) { }

        /// <summary>Override to perform custom render logic.</summary>
        protected virtual void OnRender() { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                RootControl?.Dispose();
            }
            _disposed = true;
        }
    }
}
