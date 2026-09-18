using Fungiiiii.Runtime.AI;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class PatrolAgentTests
    {
        [Test]
        public void TickMovesAgentTowardCurrentWaypoint()
        {
            var agentObject = new GameObject("PatrolAgentTestAgent");
            var waypointObject = new GameObject("PatrolAgentTestWaypoint");
            try
            {
                waypointObject.transform.position = new Vector3(4f, 0f, 0f);
                var agent = agentObject.AddComponent<PatrolAgent>();
                agent.MovementSpeed = 2f;
                agent.WaypointArrivalDistance = 0.01f;
                agent.SetWaypoints(new[] { waypointObject.transform });

                agent.Tick(0.5f);

                Assert.That(agentObject.transform.position.x, Is.EqualTo(1f).Within(0.0001f));
                Assert.That(agent.CurrentWaypointIndex, Is.EqualTo(0));
                Assert.That(agent.HasReachedEnd, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(agentObject);
                Object.DestroyImmediate(waypointObject);
            }
        }

        [Test]
        public void LoopingAgentAdvancesAndWrapsRoute()
        {
            var agentObject = new GameObject("PatrolAgentTestAgent");
            var firstWaypointObject = new GameObject("PatrolAgentTestFirstWaypoint");
            var secondWaypointObject = new GameObject("PatrolAgentTestSecondWaypoint");
            try
            {
                firstWaypointObject.transform.position = new Vector3(1f, 0f, 0f);
                secondWaypointObject.transform.position = new Vector3(2f, 0f, 0f);
                var agent = agentObject.AddComponent<PatrolAgent>();
                agent.MovementSpeed = 10f;
                agent.WaypointArrivalDistance = 0.01f;
                agent.SetWaypoints(new[] { firstWaypointObject.transform, secondWaypointObject.transform });

                agent.Tick(0.2f);
                agent.Tick(0.2f);

                Assert.That(agent.CurrentWaypointIndex, Is.EqualTo(0));
                Assert.That(agent.HasReachedEnd, Is.False);
                Assert.That(agentObject.transform.position, Is.EqualTo(secondWaypointObject.transform.position));
            }
            finally
            {
                Object.DestroyImmediate(agentObject);
                Object.DestroyImmediate(firstWaypointObject);
                Object.DestroyImmediate(secondWaypointObject);
            }
        }

        [Test]
        public void NonLoopingAgentStopsAtFinalWaypoint()
        {
            var agentObject = new GameObject("PatrolAgentTestAgent");
            var waypointObject = new GameObject("PatrolAgentTestWaypoint");
            try
            {
                waypointObject.transform.position = new Vector3(1f, 0f, 0f);
                var agent = agentObject.AddComponent<PatrolAgent>();
                agent.MovementSpeed = 10f;
                agent.WaypointArrivalDistance = 0.01f;
                agent.Loop = false;
                agent.SetWaypoints(new[] { waypointObject.transform });

                agent.Tick(0.2f);
                var finalPosition = agentObject.transform.position;
                agent.Tick(0.2f);

                Assert.That(agent.HasReachedEnd, Is.True);
                Assert.That(agentObject.transform.position, Is.EqualTo(finalPosition));
            }
            finally
            {
                Object.DestroyImmediate(agentObject);
                Object.DestroyImmediate(waypointObject);
            }
        }
    }
}
