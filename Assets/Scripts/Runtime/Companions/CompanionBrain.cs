using System;
using System.Collections.Generic;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Companions
{
    /// <summary>What a companion is busy with.</summary>
    public enum CompanionActivity
    {
        /// <summary>Walking after the player, or resting close to them.</summary>
        Following,

        /// <summary>Walking to something to soak up.</summary>
        Seeking,

        /// <summary>Soaking it up, standing on it.</summary>
        Soaking,
    }

    /// <summary>How a companion moves and behaves, from its definition.</summary>
    public readonly struct CompanionTuning
    {
        public CompanionTuning(
            float followDistance,
            float startDistance,
            float sideOffset,
            float walkSpeed,
            float runSpeed,
            float catchUpGain,
            float acceleration,
            float catchUpDistance,
            float seekRadius,
            float soakSeconds,
            float arrivalDistance)
        {
            if (followDistance < 0f || startDistance <= followDistance || walkSpeed <= 0f || runSpeed < walkSpeed
                || catchUpGain < 0f || acceleration <= 0f || catchUpDistance <= startDistance
                || seekRadius < 0f || soakSeconds < 0f || arrivalDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(followDistance), "Invalid companion tuning.");
            }

            FollowDistance = followDistance;
            StartDistance = startDistance;
            SideOffset = sideOffset;
            WalkSpeed = walkSpeed;
            RunSpeed = runSpeed;
            CatchUpGain = catchUpGain;
            Acceleration = acceleration;
            CatchUpDistance = catchUpDistance;
            SeekRadius = seekRadius;
            SoakSeconds = soakSeconds;
            ArrivalDistance = arrivalDistance;
        }

        /// <summary>Its place is this far behind the player, in metres.</summary>
        public float FollowDistance { get; }

        /// <summary>Resting, it sets off only once the player is further than this: a step or a turn does not move it.</summary>
        public float StartDistance { get; }

        /// <summary>Its place is this far to the player's right (negative: left), so that it walks beside them, not in their back.</summary>
        public float SideOffset { get; }

        /// <summary>Its speed close to the player, in metres per second.</summary>
        public float WalkSpeed { get; }

        /// <summary>Its top speed, to catch up: a little above the player's sprint.</summary>
        public float RunSpeed { get; }

        /// <summary>How much faster it goes per metre beyond <see cref="StartDistance"/>.</summary>
        public float CatchUpGain { get; }

        /// <summary>How quickly it speeds up and slows down, in metres per second squared.</summary>
        public float Acceleration { get; }

        /// <summary>Further than this from the player, it comes back behind them at once.</summary>
        public float CatchUpDistance { get; }

        /// <summary>It goes for what can be soaked up this close to the player.</summary>
        public float SeekRadius { get; }

        /// <summary>How long soaking takes.</summary>
        public float SoakSeconds { get; }

        /// <summary>It starts soaking this close to what it soaks up.</summary>
        public float ArrivalDistance { get; }
    }

    /// <summary>Where a companion is after a step, how fast it goes, and what happened during it.</summary>
    public readonly struct CompanionStep
    {
        public CompanionStep(Vector3 position, Vector3 velocity, bool caughtUp, bool soaked)
        {
            Position = position;
            Velocity = velocity;
            CaughtUp = caughtUp;
            Soaked = soaked;
        }

        /// <summary>Its new position, at the height it had: the caller puts it on the ground.</summary>
        public Vector3 Position { get; }

        /// <summary>Its velocity on the ground plane, in metres per second.</summary>
        public Vector3 Velocity { get; }

        /// <summary>Whether it walked during the step.</summary>
        public bool Moved => Velocity.sqrMagnitude > 0.0001f;

        /// <summary>Whether it came back behind the player at once, being too far.</summary>
        public bool CaughtUp { get; }

        /// <summary>Whether it finished soaking during the step.</summary>
        public bool Soaked { get; }
    }

    /// <summary>
    /// What a companion does, without Unity, as pets follow in other games: resting near
    /// the player, it sets off only once they have really gone (two distances, so that it
    /// does not fidget at every step), then heads for its place behind and beside them,
    /// walking when close and running to catch up, speeding up and braking smoothly. When
    /// it is empty, it goes by itself to the nearest thing it can soak up close to the
    /// player, soaks it up, then follows again, full. Squeezing it empties it. It moves on
    /// the ground plane; the caller keeps it on the ground.
    /// </summary>
    public sealed class CompanionBrain
    {
        /// <summary>Following, it rests once this close to its place.</summary>
        private const float PlaceReachedDistance = 0.3f;

        private readonly CompanionTuning tuning;
        private readonly ItemDefinition carried;
        private IAbsorbable target;
        private float soakElapsed;
        private Vector3 velocity;
        private bool resting = true;

        /// <param name="carriedItem">What it carries once full: it only goes for what yields this.</param>
        public CompanionBrain(CompanionTuning tuning, ItemDefinition carriedItem)
        {
            this.tuning = tuning;
            carried = carriedItem ? carriedItem : throw new ArgumentNullException(nameof(carriedItem));
        }

        public CompanionActivity Activity { get; private set; }

        public bool IsFull { get; private set; }

        /// <summary>Following and standing still near the player, until they go.</summary>
        public bool IsResting => Activity == CompanionActivity.Following && resting;

        /// <summary>Its velocity on the ground plane.</summary>
        public Vector3 Velocity => velocity;

        /// <summary>What it walks to or soaks up, null while following.</summary>
        public IAbsorbable Target => target;

        /// <summary>How far soaking has gone, from 0 to 1; 1 once full, 0 while empty and not soaking.</summary>
        public float Fullness => IsFull ? 1f
            : Activity == CompanionActivity.Soaking && tuning.SoakSeconds > 0f ? Mathf.Clamp01(soakElapsed / tuning.SoakSeconds)
            : 0f;

        /// <param name="position">Where it stands.</param>
        /// <param name="player">Where the player stands.</param>
        /// <param name="playerForward">Where the player looks: its place is behind them, and catching up puts it there, out of sight.</param>
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
                velocity = Vector3.zero;
                resting = true;
                return new CompanionStep(Place(player, playerForward, position.y), velocity, caughtUp: true, soaked: false);
            }

            if (deltaTime <= 0f)
            {
                return new CompanionStep(position, velocity, caughtUp: false, soaked: false);
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
                    return Follow(position, player, playerForward, deltaTime);
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
            var next = Brake(position, deltaTime);
            soakElapsed += deltaTime;
            if (soakElapsed < tuning.SoakSeconds)
            {
                return new CompanionStep(next, velocity, caughtUp: false, soaked: false);
            }

            var soaked = target.Absorb();
            IsFull = soaked;
            StopSeeking();
            return new CompanionStep(next, velocity, caughtUp: false, soaked: soaked);
        }

        private CompanionStep Seek(Vector3 position, float deltaTime)
        {
            var goal = target.Position;
            goal.y = position.y;
            if (FlatDistance(position, goal) <= tuning.ArrivalDistance)
            {
                Activity = CompanionActivity.Soaking;
                soakElapsed = 0f;
                return new CompanionStep(Brake(position, deltaTime), velocity, caughtUp: false, soaked: false);
            }

            // Aims a little inside its arrival distance, so that it gets there while still moving.
            var next = MoveTowards(position, goal, tuning.WalkSpeed, tuning.ArrivalDistance * 0.5f, deltaTime);
            return new CompanionStep(next, velocity, caughtUp: false, soaked: false);
        }

        private CompanionStep Follow(Vector3 position, Vector3 player, Vector3 playerForward, float deltaTime)
        {
            var toPlayer = FlatDistance(position, player);
            if (resting && toPlayer > tuning.StartDistance)
            {
                resting = false;
            }

            if (resting)
            {
                return new CompanionStep(Brake(position, deltaTime), velocity, caughtUp: false, soaked: false);
            }

            var place = Place(player, playerForward, position.y);
            if (FlatDistance(position, place) <= PlaceReachedDistance)
            {
                resting = true;
                return new CompanionStep(Brake(position, deltaTime), velocity, caughtUp: false, soaked: false);
            }

            // Walking close by, faster the further behind it is, up to its run.
            var speed = Mathf.Min(
                tuning.RunSpeed,
                tuning.WalkSpeed + Mathf.Max(0f, toPlayer - tuning.StartDistance) * tuning.CatchUpGain);
            var next = MoveTowards(position, place, speed, PlaceReachedDistance * 0.5f, deltaTime);
            return new CompanionStep(next, velocity, caughtUp: false, soaked: false);
        }

        /// <summary>
        /// Heads for <paramref name="goal"/> at up to <paramref name="speed"/>, braking in
        /// time to stop at <paramref name="stopDistance"/> from it: arrive steering.
        /// </summary>
        private Vector3 MoveTowards(Vector3 position, Vector3 goal, float speed, float stopDistance, float deltaTime)
        {
            var offset = goal - position;
            offset.y = 0f;
            var distance = offset.magnitude;
            var left = Mathf.Max(0f, distance - stopDistance);
            var arriving = Mathf.Sqrt(2f * tuning.Acceleration * left);
            var desired = distance > 0.0001f ? offset / distance * Mathf.Min(speed, arriving) : Vector3.zero;
            velocity = Vector3.MoveTowards(velocity, desired, tuning.Acceleration * deltaTime);
            return position + velocity * deltaTime;
        }

        private Vector3 Brake(Vector3 position, float deltaTime)
        {
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, tuning.Acceleration * deltaTime);
            return position + velocity * deltaTime;
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
            resting = false;
        }

        /// <summary>Its place: behind the player and beside them.</summary>
        private Vector3 Place(Vector3 player, Vector3 playerForward, float height)
        {
            var forward = new Vector3(playerForward.x, 0f, playerForward.z);
            forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            var right = new Vector3(forward.z, 0f, -forward.x);
            var place = player - forward * tuning.FollowDistance + right * tuning.SideOffset;
            place.y = height;
            return place;
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
