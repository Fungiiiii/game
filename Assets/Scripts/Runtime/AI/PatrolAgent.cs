using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Runtime.AI
{
    /// <summary>
    /// Moves a world object through a sequence of waypoints.
    ///
    /// The component intentionally knows nothing about the object it moves. A future
    /// species, prop, or test marker can reuse the same movement behaviour.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PatrolAgent : MonoBehaviour
    {
        [SerializeField]
        private Transform[] waypoints = Array.Empty<Transform>();

        [SerializeField, Min(0f)]
        private float movementSpeed = 1.5f;

        [SerializeField, Min(0.001f)]
        private float waypointArrivalDistance = 0.1f;

        [SerializeField]
        private bool loop = true;

        private int currentWaypointIndex;

        /// <summary>
        /// Index of the waypoint currently being approached.
        /// </summary>
        public int CurrentWaypointIndex => currentWaypointIndex;

        /// <summary>
        /// True after a non-looping agent reaches its final valid waypoint.
        /// </summary>
        public bool HasReachedEnd { get; private set; }

        public float MovementSpeed
        {
            get => movementSpeed;
            set => movementSpeed = Mathf.Max(0f, value);
        }

        public float WaypointArrivalDistance
        {
            get => waypointArrivalDistance;
            set => waypointArrivalDistance = Mathf.Max(0.001f, value);
        }

        public bool Loop
        {
            get => loop;
            set => loop = value;
        }

        /// <summary>
        /// Replaces the route and starts at its first waypoint.
        /// </summary>
        public void SetWaypoints(IReadOnlyList<Transform> route)
        {
            if (route == null || route.Count == 0)
            {
                waypoints = Array.Empty<Transform>();
            }
            else
            {
                waypoints = new Transform[route.Count];
                for (var i = 0; i < route.Count; i++)
                {
                    waypoints[i] = route[i];
                }
            }

            ResetPatrol();
        }

        /// <summary>
        /// Restarts the current route from its first waypoint.
        /// </summary>
        public void ResetPatrol()
        {
            currentWaypointIndex = 0;
            HasReachedEnd = false;
        }

        /// <summary>
        /// Advances the agent by a deterministic amount of time.
        /// This is public so tests and other simulation drivers can tick the same
        /// movement behaviour without relying on frame timing.
        /// </summary>
        public bool Tick(float deltaTime)
        {
            if (deltaTime <= 0f || HasReachedEnd || !TryGetCurrentWaypoint(out var waypoint))
            {
                return false;
            }

            var toWaypoint = waypoint.position - transform.position;
            var distance = toWaypoint.magnitude;
            if (distance <= waypointArrivalDistance)
            {
                transform.position = waypoint.position;
                AdvanceWaypoint();
                return true;
            }

            var planarDirection = new Vector3(toWaypoint.x, 0f, toWaypoint.z);
            if (planarDirection.sqrMagnitude > 0.000001f)
            {
                transform.rotation = Quaternion.LookRotation(planarDirection, Vector3.up);
            }

            var distanceThisTick = movementSpeed * deltaTime;
            if (distanceThisTick >= distance)
            {
                transform.position = waypoint.position;
                AdvanceWaypoint();
            }
            else
            {
                transform.position += toWaypoint / distance * distanceThisTick;
            }

            return true;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private bool TryGetCurrentWaypoint(out Transform waypoint)
        {
            waypoint = null;
            if (waypoints == null || waypoints.Length == 0)
            {
                return false;
            }

            for (var offset = 0; offset < waypoints.Length; offset++)
            {
                var candidateIndex = (currentWaypointIndex + offset) % waypoints.Length;
                if (waypoints[candidateIndex] == null)
                {
                    continue;
                }

                currentWaypointIndex = candidateIndex;
                waypoint = waypoints[candidateIndex];
                return true;
            }

            return false;
        }

        private void AdvanceWaypoint()
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                HasReachedEnd = true;
                return;
            }

            if (!loop && currentWaypointIndex >= waypoints.Length - 1)
            {
                HasReachedEnd = true;
                return;
            }

            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        private void OnValidate()
        {
            movementSpeed = Mathf.Max(0f, movementSpeed);
            waypointArrivalDistance = Mathf.Max(0.001f, waypointArrivalDistance);
        }
    }
}
