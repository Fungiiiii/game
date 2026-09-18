using System.Collections;
using Fungiiiii.Runtime.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class PatrolDemoPlayModeTests
    {
        [UnityTest]
        public IEnumerator RuntimeBootstrapCreatesMovingPatrolAgent()
        {
            yield return null;

            var demoRoot = GameObject.Find("Generic Patrol Demo");
            Assert.That(demoRoot, Is.Not.Null, "The runtime patrol demo was not created after scene load.");

            var agentObject = GameObject.Find("Patrol Agent (Cube Placeholder)");
            Assert.That(agentObject, Is.Not.Null, "The runtime patrol agent was not created.");
            Assert.That(agentObject.GetComponent<PatrolAgent>(), Is.Not.Null);

            try
            {
                var initialPosition = agentObject.transform.position;
                yield return new WaitForSeconds(0.3f);

                Assert.That(
                    Vector3.Distance(initialPosition, agentObject.transform.position),
                    Is.GreaterThan(0.05f),
                    "The patrol agent did not move during PlayMode.");
            }
            finally
            {
                if (demoRoot != null)
                {
                    Object.Destroy(demoRoot);
                }
            }
        }
    }
}
