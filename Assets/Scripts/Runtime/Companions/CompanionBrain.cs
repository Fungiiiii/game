using System;
using System.Collections.Generic;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Companions
{
    /// <summary>What a companion is busy with.</summary>
    public enum CompanionActivity
    {
        /// <summary>Walking after the player, or waiting close to them.</summary>
        Following,

        /// <summary>Walking to something to soak up.</summary>
        Seeking,

        /// <summary>Soaking it up, standing on it.</summary>
        Soaking,
    }

    /// <summary>How a companion behaves, from its definition.</summary>
    public readonly struct CompanionTuning
    {
        public CompanionTuning(
            float followDistance,
            float speed,
            float catchUpDistance,
            float seekRadius,
            float soakSeconds,
            float arrivalDistance)
        {
            if (followDistance < 0f || speed <= 0f || catchUpDistance <= followDistance
                || seekRadius < 0f || soakSeconds < 0f || arrivalDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(followDistance), "Invalid companion tuning.");
            }

            FollowDistance = followDistance;
            Speed = speed;
            CatchUpDistance = catchUpDistance;
            SeekRadius = seekRadius;
            SoakSeconds = soakSeconds;
            ArrivalDistance = arrivalDistance;
        }

        /// <summary>It stops this far from the player, in metres.</summary>
        public float FollowDistance { get; }

        /// <summary>Its walking speed, in metres per second.</summary>
        public float Speed { get; }

        /// <summary>Further than this from the player, it comes back behind them at once.</summary>
        public float CatchUpDistance { get; }

        /// <summary>It goes for what can be soaked up this close to the player.</summary>
        public float SeekRadius { get; }

        /// <summary>How long soaking takes.</summary>
        public float SoakSeconds { get; }

        /// <summary>It starts soaking this close to what it soaks up.</summary>
        public float ArrivalDistance { get; }
    }

    /// <summary>Where a companion is after a step, and what happened during it.</summary>
    public readonly struct CompanionStep
    {
        public CompanionStep(Vector3 position, bool moved, bool caughtUp, bool soaked)
        {
            Position = position;
            Moved = moved;
            CaughtUp = caughtUp;
            Soaked = soaked;
        }

        /// <summary>Its new position, at the height it had: the caller puts it on the ground.</summary>
        public Vector3 Position { get; }

        /// <summary>Whether it walked during the step.</summary>
        public bool Moved { get; }

        /// <summary>Whether it came back behind the player at once, being too far.</summary>
        public bool CaughtUp { get; }

        /// <summary>Whether it finished soaking during the step.</summary>
        public bool Soaked { get; }
    }

    /// <summary>
    /// What a companion does, without Unity: it follows the player, and when it is empty
    /// it goes by itself to the nearest thing it can soak up close to the player, soaks it
    /// up, then follows again, full. Squeezing it empties it. It moves on the ground plane;
    /// the caller keeps it on the ground.
    /// </summary>
    public sealed class CompanionBrain
    {
        private readonly CompanionTuning tuning;
        private readonly ItemDefinition carried;
        private IAbsorbable target;
        private float soakElapsed;

        /// <param name="carriedItem">What it carries once full: it only goes for what yields this.</param>
        public CompanionBrain(CompanionTuning tuning, ItemDefinition carriedItem)
        {
            this.tuning = tuning;
            carried = carriedItem ? carriedItem : throw new ArgumentNullException(nameof(carriedItem));
        }

        public CompanionActivity Activity { get; private set; }

        public bool IsFull { get; private set; }

        /// <summary>What it walks to or soaks up, null while following.</summary>
        public IAbsorbable Target => target;

        /// <summary>How far soaking has gone, from 0 to 1; 1 once full, 0 while empty and not soaking.</summary>
        public float Fullness => IsFull ? 1f
            : Activity == CompanionActivity.Soaking && tuning.SoakSeconds > 0f ? Mathf.Clamp01(soakElapsed / tuning.SoakSeconds)
            : 0f;

        /// <param name="position">Where it stands.</param>
        /// <param name="player">Where the player stands.</param>
        /// <param name="playerForward">Where the player looks: catching up puts it behind them, out of sight.</param>
        /// <param name="sources">What could be soaked up.</param>
        public CompanionStep Step(
            Vector3 position,
            Vector3 player,
            Vector3 playerForward,
            IReadOnlyList<IAbsorbable> sources,
            float deltaTime)
        {
            if (FlatDistance(position, player) > tuning.CatchUpDistance)
            {
                StopSeeking();
                return new CompanionStep(Behind(player, playerForward, position.y), moved: false, caughtUp: true, soaked: false);
            }

            if (deltaTime <= 0f)
            {
                return new CompanionStep(position, moved: false, caughtUp: false, soaked: false);
            }

            if (Activity != CompanionActivity.Following && !StillWorthIt(player))
            {
                StopSeeking();
            }

            if (Activity == CompanionActivity.Following && !IsFull)
            {
                target = Nearest(position, player, sources);
                if (target != null)
                {
                    Activity = CompanionActivity.Seeking;
                }
            }

            switch (Activity)
            {
                case CompanionActivity.Soaking:
                    return Soak(position, deltaTime);
                case CompanionActivity.Seeking:
                    return Seek(position, deltaTime);
                default:
                    return Follow(position, player, deltaTime);
            }
        }

        /// <summary>Empties it, when squeezed. Returns false when it was not full.</summary>
        public bool TryEmpty()
        {
            if (!IsFull)
            {
                return false;
            }

            IsFull = false;
            return true;
        }

        private CompanionStep Soak(Vector3 position, float deltaTime)
        {
            soakElapsed += deltaTime;
            if (soakElapsed < tuning.SoakSeconds)
            {
                return new CompanionStep(position, moved: false, caughtUp: false, soaked: false);
            }

            var soaked = target.Absorb();
            IsFull = soaked;
            StopSeeking();
            return new CompanionStep(position, moved: false, caughtUp: false, soaked: soaked);
        }

        private CompanionStep Seek(Vector3 position, float deltaTime)
        {
            var goal = target.Position;
            goal.y = position.y;
            var next = Vector3.MoveTowards(position, goal, tuning.Speed * deltaTime);
            if (FlatDistance(next, goal) <= tuning.ArrivalDistance)
            {
                Activity = CompanionActivity.Soaking;
                soakElapsed = 0f;
            }

            return new CompanionStep(next, moved: next != position, caughtUp: false, soaked: false);
        }

        private CompanionStep Follow(Vector3 position, Vector3 player, float deltaTime)
        {
            var distance = FlatDistance(position, player);
            if (distance <= tuning.FollowDistance)
            {
                return new CompanionStep(position, moved: false, caughtUp: false, soaked: false);
            }

            var goal = player;
            goal.y = position.y;
            var step = Mathf.Min(tuning.Speed * deltaTime, distance - tuning.FollowDistance);
            var next = Vector3.MoveTowards(position, goal, step);
            return new CompanionStep(next, moved: true, caughtUp: false, soaked: false);
        }

        /// <summary>Whether the target still holds something and the player is still close to it.</summary>
        private bool StillWorthIt(Vector3 player)
        {
            return target != null
                && target.CanBeAbsorbed
                && FlatDistance(target.Position, player) <= tuning.SeekRadius;
        }

        private IAbsorbable Nearest(Vector3 position, Vector3 player, IReadOnlyList<IAbsorbable> sources)
        {
            IAbsorbable nearest = null;
            var nearestDistance = float.PositiveInfinity;
            for (var index = 0; index < sources.Count; index++)
            {
                var source = sources[index];
                if (source.Yield != carried || !source.CanBeAbsorbed
                    || FlatDistance(source.Position, player) > tuning.SeekRadius)
                {
                    continue;
                }

                var distance = FlatDistance(source.Position, position);
                if (distance < nearestDistance)
                {
                    nearest = source;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private void StopSeeking()
        {
            target = null;
            soakElapsed = 0f;
            Activity = CompanionActivity.Following;
        }

        private Vector3 Behind(Vector3 player, Vector3 playerForward, float height)
        {
            var forward = new Vector3(playerForward.x, 0f, playerForward.z);
            forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            var behind = player - forward * tuning.FollowDistance;
            behind.y = height;
            return behind;
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
