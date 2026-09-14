using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>
    /// Proves the PlayMode runner actually executes. An empty test project and a
    /// broken test runner look identical from the outside, so this assembly ships
    /// with a test from the start - see docs/unity-init.md.
    /// </summary>
    public sealed class PlayModeRunnerSmokeTests
    {
        [UnityTest]
        public IEnumerator GameObject_SurvivesAFrame()
        {
            var subject = new GameObject(nameof(GameObject_SurvivesAFrame));

            yield return null;

            Assert.That(subject, Is.Not.Null, "The PlayMode runner did not reach the next frame.");
            Object.Destroy(subject);
        }
    }
}
