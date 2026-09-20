using System;
using Fungiiiii.Runtime.Player;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class PlayerDodgeTests
    {
        private const float Tolerance = 0.0001f;

        private static PlayerDodge CreateDodge()
        {
            return new PlayerDodge(new PlayerDodge.Configuration(
                speed: 10f,
                durationSeconds: 0.25f,
                cooldownSeconds: 0.75f));
        }

        [Test]
        public void NewDodgeIsReadyAndStill()
        {
            var dodge = CreateDodge();

            Assert.That(dodge.IsReady, Is.True);
            Assert.That(dodge.IsDodging, Is.False);
            Assert.That(dodge.Velocity, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void StartingADodgeImposesItsVelocity()
        {
            var dodge = CreateDodge();

            var started = dodge.TryStart(Vector3.forward);

            Assert.That(started, Is.True);
            Assert.That(dodge.IsDodging, Is.True);
            Assert.That(dodge.Velocity.z, Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void DodgeDirectionIsNormalised()
        {
            var dodge = CreateDodge();

            dodge.TryStart(new Vector3(3f, 0f, 4f));

            Assert.That(dodge.Velocity.magnitude, Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void VerticalComponentOfDirectionIsIgnored()
        {
            var dodge = CreateDodge();

            dodge.TryStart(new Vector3(0f, 5f, 1f));

            Assert.That(dodge.Velocity.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(dodge.Velocity.z, Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void ZeroDirectionIsRefused()
        {
            var dodge = CreateDodge();

            var started = dodge.TryStart(Vector3.zero);

            Assert.That(started, Is.False);
            Assert.That(dodge.IsDodging, Is.False);
        }

        [Test]
        public void SecondDodgeIsRefusedWhileTheFirstRuns()
        {
            var dodge = CreateDodge();
            dodge.TryStart(Vector3.forward);

            var started = dodge.TryStart(Vector3.right);

            Assert.That(started, Is.False);
            Assert.That(dodge.Velocity.z, Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void DodgeStopsWhenItsDurationElapses()
        {
            var dodge = CreateDodge();
            dodge.TryStart(Vector3.forward);

            dodge.Tick(0.25f);

            Assert.That(dodge.IsDodging, Is.False);
            Assert.That(dodge.Velocity, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void DodgeIsRefusedDuringCooldown()
        {
            var dodge = CreateDodge();
            dodge.TryStart(Vector3.forward);
            dodge.Tick(0.5f);

            var started = dodge.TryStart(Vector3.forward);

            Assert.That(dodge.IsDodging, Is.False);
            Assert.That(dodge.IsReady, Is.False);
            Assert.That(started, Is.False);
        }

        [Test]
        public void DodgeBecomesReadyOnceCooldownElapses()
        {
            var dodge = CreateDodge();
            dodge.TryStart(Vector3.forward);
            dodge.Tick(0.75f);

            Assert.That(dodge.IsReady, Is.True);
            Assert.That(dodge.TryStart(Vector3.forward), Is.True);
        }

        [Test]
        public void NonPositiveDeltaTimeDoesNotAdvanceTimers()
        {
            var dodge = CreateDodge();
            dodge.TryStart(Vector3.forward);

            dodge.Tick(0f);

            Assert.That(dodge.IsDodging, Is.True);
        }

        [Test]
        public void ConfigurationRejectsACooldownShorterThanTheDodge()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PlayerDodge.Configuration(speed: 10f, durationSeconds: 0.5f, cooldownSeconds: 0.2f));
        }

        [Test]
        public void ConfigurationRejectsNonPositiveSpeed()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PlayerDodge.Configuration(speed: 0f, durationSeconds: 0.25f, cooldownSeconds: 0.75f));
        }
    }
}
