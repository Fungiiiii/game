using System.Collections;
using Fungiiiii.Runtime.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class ArrivalPlayModeTests
    {
        private GameObject thing;

        [SetUp]
        public void SetUp()
        {
            thing = new GameObject("Arriving Thing");
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(thing);
        }

        [UnityTest]
        public IEnumerator AGrowingThing_StartsWithNoSize_AndEndsAtItsScale()
        {
            thing.transform.localScale = Vector3.one * 1.4f;
            var growIn = thing.AddComponent<GrowIn>();

            growIn.Play(0.3f);

            Assert.That(thing.transform.localScale.x, Is.Zero);
            Assert.That(growIn.IsPlaying, Is.True);

            yield return new WaitForSeconds(0.5f);

            Assert.That(thing.transform.localScale.x, Is.EqualTo(1.4f).Within(0.001f));
            Assert.That(growIn.IsPlaying, Is.False);
        }

        [UnityTest]
        public IEnumerator AFallingThing_StartsAbove_AndLandsOnItsPose_TurnedBack()
        {
            var pose = new Vector3(3f, 1f, -2f);
            var rotation = Quaternion.Euler(10f, 40f, 0f);
            thing.transform.SetPositionAndRotation(pose, rotation);
            var fallIn = thing.AddComponent<FallIn>();

            fallIn.Play(2f, 0.3f);

            Assert.That(thing.transform.position.y, Is.EqualTo(pose.y + 2f).Within(0.001f));

            yield return new WaitForSeconds(ArrivalCurves.FallSeconds(2f) + 0.2f);

            Assert.That(Vector3.Distance(thing.transform.position, pose), Is.LessThan(0.001f));
            Assert.That(Quaternion.Angle(thing.transform.rotation, rotation), Is.LessThan(0.1f));
            Assert.That(fallIn.IsPlaying, Is.False);
        }
    }
}
