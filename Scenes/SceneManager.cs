// File: MUIBridge/Scenes/SceneManager.cs
// Purpose: Unity-style scene management with Push/Pop navigation (ported from DUSK).

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace MUIBridge.Scenes
{
    /// <summary>
    /// Manages scene registration, navigation, and transitions.
    /// Provides Unity-style Push/Pop navigation with configurable transitions.
    /// Thread-safe implementation using ConcurrentDictionary.
    /// </summary>
    public class SceneManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, IScene> _scenes = new();
        private readonly ConcurrentStack<IScene> _sceneStack = new();
        private readonly ILogger<SceneManager>? _logger;
        private readonly object _transitionLock = new();

        private volatile IScene? _activeScene;
        private volatile bool _disposed;
        private SceneTransition? _activeTransition;

        /// <summary>
        /// Raised when a scene transition begins.
        /// </summary>
        public event EventHandler<SceneTransitionEventArgs>? TransitionStarted;

        /// <summary>
        /// Raised when a scene transition completes.
        /// </summary>
        public event EventHandler<SceneTransitionEventArgs>? TransitionCompleted;

        /// <summary>
        /// Gets the currently active scene.
        /// </summary>
        public IScene? ActiveScene => _activeScene;

        /// <summary>
        /// Gets all registered scenes.
        /// </summary>
        public IReadOnlyCollection<IScene> RegisteredScenes => _scenes.Values.ToArray();

        /// <summary>
        /// Gets whether a transition is currently in progress.
        /// </summary>
        public bool IsTransitioning
        {
            get { lock (_transitionLock) return _activeTransition != null && !_activeTransition.IsComplete; }
        }

        public SceneManager(ILogger<SceneManager>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Registers a scene for navigation.
        /// </summary>
        public void Register(IScene scene)
        {
            if (!_scenes.TryAdd(scene.Id, scene))
                throw new InvalidOperationException($"Scene with ID '{scene.Id}' is already registered.");

            scene.Initialize();
            _logger?.LogDebug("Registered scene: {SceneId}", scene.Id);
        }

        /// <summary>
        /// Registers a scene by type.
        /// </summary>
        public void Register<T>() where T : IScene, new()
        {
            var scene = new T();
            Register(scene);
        }

        /// <summary>
        /// Unregisters and disposes a scene.
        /// </summary>
        public void Unregister(string sceneId)
        {
            if (_scenes.TryRemove(sceneId, out var scene))
            {
                scene.Dispose();
                _logger?.LogDebug("Unregistered scene: {SceneId}", sceneId);
            }
        }

        /// <summary>
        /// Gets a scene by ID.
        /// </summary>
        public IScene? GetScene(string sceneId)
        {
            return _scenes.GetValueOrDefault(sceneId);
        }

        /// <summary>
        /// Navigates to a scene by ID, optionally with a transition.
        /// </summary>
        public void NavigateTo(string sceneId, TransitionConfig? transition = null)
        {
            if (!_scenes.TryGetValue(sceneId, out var scene))
            {
                _logger?.LogWarning("Scene not found: {SceneId}", sceneId);
                return;
            }

            NavigateTo(scene, transition);
        }

        /// <summary>
        /// Navigates to a scene, optionally with a transition.
        /// </summary>
        public void NavigateTo(IScene scene, TransitionConfig? transition = null)
        {
            lock (_transitionLock)
            {
                var previousScene = _activeScene;

                if (transition != null && transition.Type != TransitionType.None)
                {
                    _activeTransition = new SceneTransition(previousScene, scene, transition);
                    TransitionStarted?.Invoke(this, new SceneTransitionEventArgs(previousScene, scene));
                    _logger?.LogDebug("Starting transition from {From} to {To}",
                        previousScene?.Id ?? "null", scene.Id);
                }
                else
                {
                    previousScene?.Hide();
                    _activeScene = scene;
                    scene.Show();
                    _logger?.LogInformation("Navigated to scene: {SceneId}", scene.Id);
                }
            }
        }

        /// <summary>
        /// Pushes a scene onto the navigation stack and navigates to it.
        /// </summary>
        public void Push(string sceneId, TransitionConfig? transition = null)
        {
            if (!_scenes.TryGetValue(sceneId, out var scene))
            {
                _logger?.LogWarning("Scene not found for push: {SceneId}", sceneId);
                return;
            }

            if (_activeScene != null)
            {
                _sceneStack.Push(_activeScene);
            }

            NavigateTo(scene, transition);
        }

        /// <summary>
        /// Pops the current scene and navigates to the previous scene.
        /// </summary>
        public void Pop(TransitionConfig? transition = null)
        {
            if (!_sceneStack.TryPop(out var previousScene)) return;

            NavigateTo(previousScene, transition);
        }

        /// <summary>
        /// Pops all scenes and navigates to the root scene.
        /// </summary>
        public void PopToRoot(TransitionConfig? transition = null)
        {
            if (!_sceneStack.TryPop(out var rootScene)) return;

            while (_sceneStack.TryPop(out var scene))
            {
                rootScene.Hide();
                rootScene = scene;
            }

            NavigateTo(rootScene, transition);
        }

        /// <summary>
        /// Updates the active scene and any active transitions.
        /// </summary>
        public void Update(float deltaTime)
        {
            if (_activeTransition != null && !_activeTransition.IsComplete)
            {
                _activeTransition.Update(deltaTime);

                if (_activeTransition.IsComplete)
                {
                    lock (_transitionLock)
                    {
                        _activeTransition.FromScene?.Hide();
                        _activeScene = _activeTransition.ToScene;
                        _activeScene?.Show();

                        TransitionCompleted?.Invoke(this,
                            new SceneTransitionEventArgs(_activeTransition.FromScene, _activeTransition.ToScene));

                        _activeTransition = null;
                    }
                }
            }

            _activeScene?.Update(deltaTime);
        }

        /// <summary>
        /// Renders the active scene.
        /// </summary>
        public void Render()
        {
            _activeScene?.Render();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var scene in _scenes.Values)
            {
                scene.Dispose();
            }
            _scenes.Clear();

            _logger?.LogDebug("SceneManager disposed");
        }
    }

    /// <summary>
    /// Event args for scene transitions.
    /// </summary>
    public class SceneTransitionEventArgs : EventArgs
    {
        public IScene? FromScene { get; }
        public IScene? ToScene { get; }

        public SceneTransitionEventArgs(IScene? from, IScene? to)
        {
            FromScene = from;
            ToScene = to;
        }
    }
}
