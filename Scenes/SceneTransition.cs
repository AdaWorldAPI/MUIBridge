// File: MUIBridge/Scenes/SceneTransition.cs
// Purpose: Scene transition effects and configuration (ported from DUSK).

namespace MUIBridge.Scenes
{
    /// <summary>
    /// Types of scene transitions.
    /// </summary>
    public enum TransitionType
    {
        None,
        Fade,
        SlideLeft,
        SlideRight,
        SlideUp,
        SlideDown,
        CrossFade,
        CopperBars  // Classic Amiga-style effect
    }

    /// <summary>
    /// Configuration for scene transitions.
    /// </summary>
    public class TransitionConfig
    {
        /// <summary>Type of transition effect.</summary>
        public TransitionType Type { get; set; } = TransitionType.Fade;

        /// <summary>Duration of the transition in seconds.</summary>
        public float Duration { get; set; } = 0.3f;

        /// <summary>Easing function for the transition.</summary>
        public EasingType Easing { get; set; } = EasingType.EaseInOut;

        /// <summary>Creates a fade transition.</summary>
        public static TransitionConfig Fade(float duration = 0.3f) =>
            new() { Type = TransitionType.Fade, Duration = duration };

        /// <summary>Creates a slide left transition.</summary>
        public static TransitionConfig SlideLeft(float duration = 0.3f) =>
            new() { Type = TransitionType.SlideLeft, Duration = duration };

        /// <summary>Creates a slide right transition.</summary>
        public static TransitionConfig SlideRight(float duration = 0.3f) =>
            new() { Type = TransitionType.SlideRight, Duration = duration };

        /// <summary>Creates an Amiga-style copper bars transition.</summary>
        public static TransitionConfig CopperBars(float duration = 0.5f) =>
            new() { Type = TransitionType.CopperBars, Duration = duration };
    }

    /// <summary>
    /// Easing types for transitions.
    /// </summary>
    public enum EasingType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    /// <summary>
    /// Manages an active scene transition.
    /// </summary>
    internal class SceneTransition
    {
        public IScene? FromScene { get; }
        public IScene? ToScene { get; }
        public TransitionConfig Config { get; }
        public float Progress { get; private set; }
        public bool IsComplete => Progress >= 1f;

        private float _elapsed;

        public SceneTransition(IScene? from, IScene? to, TransitionConfig config)
        {
            FromScene = from;
            ToScene = to;
            Config = config;
        }

        public void Update(float deltaTime)
        {
            if (IsComplete) return;

            _elapsed += deltaTime;
            Progress = Math.Min(_elapsed / Config.Duration, 1f);
            Progress = ApplyEasing(Progress, Config.Easing);
        }

        private static float ApplyEasing(float t, EasingType easing)
        {
            return easing switch
            {
                EasingType.Linear => t,
                EasingType.EaseIn => t * t,
                EasingType.EaseOut => 1f - (1f - t) * (1f - t),
                EasingType.EaseInOut => t < 0.5f
                    ? 2f * t * t
                    : 1f - (float)Math.Pow(-2 * t + 2, 2) / 2f,
                _ => t
            };
        }
    }
}
