using UnityEngine;

namespace Fungiiiii.Combat
{
    /// <summary>How the slingshot shoots: its pebbles' speed and fall, their damage, its pace.</summary>
    [CreateAssetMenu(fileName = "SlingshotSettings", menuName = "Fungiiiii/Slingshot Settings")]
    public sealed class SlingshotSettings : ScriptableObject
    {
        [Tooltip("Speed of a pebble as it leaves the slingshot, in metres per second.")]
        [SerializeField, Min(1f)]
        private float shotSpeed = 22f;

        [Tooltip("Downward acceleration of a pebble in flight, in m/s².")]
        [SerializeField, Min(0f)]
        private float gravity = 9.81f;

        [Tooltip("Health a pebble takes from the mushroom it hits.")]
        [SerializeField, Min(0f)]
        private float damage = 1f;

        [Tooltip("Seconds between two shots.")]
        [SerializeField, Min(0f)]
        private float secondsBetweenShots = 0.6f;

        [Tooltip("A pebble hits a mushroom passing this close to its centre, in metres.")]
        [SerializeField, Min(0.05f)]
        private float hitRadius = 0.45f;

        [Tooltip("A pebble that hits nothing is gone after this long, in seconds.")]
        [SerializeField, Min(0.1f)]
        private float maxFlightSeconds = 3f;

        [Tooltip("Field of view while aiming, in degrees.")]
        [SerializeField, Range(10f, 90f)]
        private float aimFieldOfView = 45f;

        public float ShotSpeed => shotSpeed;

        public float Gravity => gravity;

        public float Damage => damage;

        public float SecondsBetweenShots => secondsBetweenShots;

        public float HitRadius => hitRadius;

        public float MaxFlightSeconds => maxFlightSeconds;

        public float AimFieldOfView => aimFieldOfView;

        /// <summary>Settings that exist only at runtime, for tests.</summary>
        public static SlingshotSettings Create(float shotSpeed, float gravity, float damage, float secondsBetweenShots, float hitRadius)
        {
            var settings = CreateInstance<SlingshotSettings>();
            settings.shotSpeed = shotSpeed;
            settings.gravity = gravity;
            settings.damage = damage;
            settings.secondsBetweenShots = secondsBetweenShots;
            settings.hitRadius = hitRadius;
            return settings;
        }
    }
}
