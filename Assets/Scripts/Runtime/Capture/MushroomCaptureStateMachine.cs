using System;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// Deterministic capture state machine with no Unity lifecycle dependency.
    /// </summary>
    public sealed class MushroomCaptureStateMachine
    {
        public readonly struct Configuration
        {
            public Configuration(
                float alertDistance,
                float captureDistance,
                float fleeDelaySeconds,
                float calmDistance)
            {
                if (alertDistance <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(alertDistance));
                }

                if (captureDistance <= 0f || captureDistance > alertDistance)
                {
                    throw new ArgumentOutOfRangeException(nameof(captureDistance));
                }

                if (fleeDelaySeconds <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(fleeDelaySeconds));
                }

                if (calmDistance <= alertDistance)
                {
                    throw new ArgumentOutOfRangeException(nameof(calmDistance));
                }

                AlertDistance = alertDistance;
                CaptureDistance = captureDistance;
                FleeDelaySeconds = fleeDelaySeconds;
                CalmDistance = calmDistance;
            }

            public float AlertDistance { get; }
            public float CaptureDistance { get; }
            public float FleeDelaySeconds { get; }
            public float CalmDistance { get; }

            public static Configuration Default => new Configuration(
                alertDistance: 3.5f,
                captureDistance: 1.25f,
                fleeDelaySeconds: 1.5f,
                calmDistance: 6f);
        }

        private readonly Configuration configuration;
        private float alertElapsedSeconds;

        public MushroomCaptureStateMachine(Configuration configuration)
        {
            this.configuration = configuration;
            CurrentState = MushroomCaptureState.Idle;
        }

        public MushroomCaptureState CurrentState { get; private set; }

        /// <summary>
        /// Raised after a successful transition. The arguments are previous and next state.
        /// </summary>
        public event Action<MushroomCaptureState, MushroomCaptureState> StateChanged;

        /// <summary>
        /// Raised once when the mushroom is captured. Inventory systems can subscribe here.
        /// </summary>
        public event Action Captured;

        /// <summary>
        /// Advances perception-driven transitions using the current observer distance.
        /// </summary>
        public bool Tick(float observerDistance, float deltaTime)
        {
            ValidateDistance(observerDistance);

            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            switch (CurrentState)
            {
                case MushroomCaptureState.Idle:
                    alertElapsedSeconds = 0f;
                    return observerDistance <= configuration.AlertDistance && SetState(MushroomCaptureState.Alert);

                case MushroomCaptureState.Alert:
                    if (observerDistance > configuration.AlertDistance)
                    {
                        alertElapsedSeconds = 0f;
                        return SetState(MushroomCaptureState.Idle);
                    }

                    alertElapsedSeconds += deltaTime;
                    return alertElapsedSeconds >= configuration.FleeDelaySeconds && SetState(MushroomCaptureState.Flee);

                case MushroomCaptureState.Flee:
                    return observerDistance >= configuration.CalmDistance && SetState(MushroomCaptureState.Idle);

                case MushroomCaptureState.Captured:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Attempts a capture. The observer must be close and the mushroom must be alert.
        /// </summary>
        public bool TryCapture(float observerDistance)
        {
            ValidateDistance(observerDistance);

            if (CurrentState != MushroomCaptureState.Alert || observerDistance > configuration.CaptureDistance)
            {
                return false;
            }

            var changed = SetState(MushroomCaptureState.Captured);
            if (changed)
            {
                Captured?.Invoke();
            }

            return changed;
        }

        /// <summary>
        /// Returns the prototype to its initial state for another demonstration run.
        /// </summary>
        public bool Reset()
        {
            alertElapsedSeconds = 0f;
            return SetState(MushroomCaptureState.Idle);
        }

        private bool SetState(MushroomCaptureState nextState)
        {
            if (CurrentState == nextState)
            {
                return false;
            }

            var previousState = CurrentState;
            CurrentState = nextState;
            StateChanged?.Invoke(previousState, nextState);
            return true;
        }

        private static void ValidateDistance(float distance)
        {
            if (distance < 0f || float.IsNaN(distance) || float.IsInfinity(distance))
            {
                throw new ArgumentOutOfRangeException(nameof(distance));
            }
        }
    }
}
