using System.Collections;
using System.Linq;
using Fungiiiii.Capture;
using Fungiiiii.Controls;
using Fungiiiii.Demo;
using Fungiiiii.Inventory;
using Fungiiiii.Runtime.Player;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class DemoLoopScenePlayModeTests
    {
        private const string ScenePath = "Assets/Scenes/Prototype/DemoLoopScene.unity";

        // Ids of the demo's items in the item catalog: stable keys, saved and sent over the network.
        private const string NetId = "net";
        private const string WoodId = "wood";
        private const string SporeId = "spore";
        private const string ForestScenePath = "Assets/Scenes/Biomes/MysticForest.unity";

        /// <summary>The forest is large: loading it additively takes a few seconds in batch mode.</summary>
        private const float LoadTimeoutSeconds = 60f;

        /// <summary>Flight, freeze and return of a net dropped from just above, with margin.</summary>
        private const float ThrowTimeoutSeconds = 5f;

        /// <summary>A Spongeus caught by surprise has 60 %: 12 failures in a row are below 1 in 50 000.</summary>
        private const int MaxCaptureAttempts = 12;

        [TearDown]
        public void RestoreTime()
        {
            // A test that fails while the game is paused or sped up must not slow the next ones.
            Time.timeScale = 1f;
        }

        private static IEnumerator LoadDemo()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
#endif
            var elapsed = 0f;
            DemoLoopBootstrap demo = null;
            while (elapsed < LoadTimeoutSeconds && (demo == null || !demo.IsReady))
            {
                demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(demo, Is.Not.Null, "The scene has no DemoLoopBootstrap.");
            Assert.That(demo.IsReady, Is.True, "The demo was not ready in time.");
        }

        [UnityTest]
        public IEnumerator SceneLoad_LoadsTheForest_WithOnePlayerAndOneCamera()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();

            Assert.That(SceneManager.GetSceneByPath(ForestScenePath).isLoaded, Is.True, "The forest is not loaded.");
            Assert.That(
                Object.FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None),
                Has.Length.EqualTo(1),
                "There must be exactly one player.");
            Assert.That(
                Object.FindObjectsByType<FirstPersonTestPlayer>(FindObjectsSortMode.None),
                Is.Empty,
                "The forest's test player is still active.");

            var renderingCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)
                .Where(camera => camera.isActiveAndEnabled)
                .ToArray();
            Assert.That(renderingCameras, Has.Length.EqualTo(1), "There must be exactly one camera.");
            Assert.That(
                renderingCameras[0].GetComponent<PrototypeFirstPersonCamera>(),
                Is.Not.Null,
                "The forest's camera is not the player's view.");

            Assert.That(demo.Mushrooms.Count, Is.GreaterThan(0));
            Assert.That(demo.Gatherables.Count, Is.GreaterThan(0));
            Assert.That(demo.Hole, Is.Null, "The Champimaison hole is left out of the playtest.");
            Assert.That(demo.IsInventoryOpen, Is.False);

            for (var index = 0; index < demo.Inventory.SlotCount; index++)
            {
                if (index != InventoryLayout.Standard.QuickBarFirstSlot)
                {
                    Assert.That(demo.Inventory.GetSlot(index).IsEmpty, Is.True, "The inventory must start with the net only.");
                }
            }
        }

        [UnityTest]
        public IEnumerator TheInventory_IsTheOneThePlayerCarries_AndTheScreenShowsIt()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var backpack = Object.FindFirstObjectByType<PlayerMotor>().GetComponent<Backpack>();

            Assert.That(backpack, Is.Not.Null, "The player carries no backpack.");
            Assert.That(demo.Inventory, Is.SameAs(backpack.Inventory));
            Assert.That(demo.InventoryView.Inventory, Is.SameAs(backpack.Inventory));
        }

        [UnityTest]
        public IEnumerator TheNet_StartsInTheFirstSlotOfTheQuickBar_InHand()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();

            var first = demo.Inventory.GetSlot(InventoryLayout.Standard.QuickBarFirstSlot);
            Assert.That(first.ItemId, Is.EqualTo(NetId));
            Assert.That(first.Quantity, Is.EqualTo(1));
            Assert.That(demo.Inventory.Count(NetId), Is.EqualTo(1), "There must be one net.");

            Assert.That(demo.Hotbar.IsVisible, Is.True, "The quick bar is not on screen.");
            Assert.That(demo.Hotbar.SlotCount, Is.EqualTo(3));
            Assert.That(demo.Hotbar.Selected, Is.EqualTo(0));
            Assert.That(demo.Thrower.IsEquipped, Is.True);
            Assert.That(demo.Thrower.IsNetInHandShown, Is.True, "The net is not in hand.");
        }

        [UnityTest]
        public IEnumerator SelectingAnotherSlot_PutsTheNetAway_AndSelectingItAgainGivesItBack()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var origin = demo.Mushrooms[0].transform.position + Vector3.up * 3f;

            demo.Hotbar.Select(1);
            yield return null;

            Assert.That(demo.Thrower.IsEquipped, Is.False);
            Assert.That(demo.Thrower.IsNetInHandShown, Is.False, "The net is still in hand.");
            Assert.That(demo.Thrower.TryThrow(origin, Vector3.down), Is.False, "A net put away was thrown.");

            demo.Hotbar.Select(0);
            yield return null;

            Assert.That(demo.Thrower.IsEquipped, Is.True);
            Assert.That(demo.Thrower.IsNetInHandShown, Is.True, "The net is not back in hand.");
            Assert.That(demo.Thrower.TryThrow(origin, Vector3.down), Is.True);
        }

        [UnityTest]
        public IEnumerator ANetThrown_ComesBack_EvenWhenAnotherSlotIsSelected()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var origin = Object.FindFirstObjectByType<PlayerMotor>().transform.position + Vector3.forward * 2f;

            Assert.That(demo.Thrower.TryThrow(origin, Vector3.down), Is.True);
            demo.Hotbar.Select(2);

            var elapsed = 0f;
            while (elapsed < ThrowTimeoutSeconds && demo.Thrower.Cycle.Phase != NetPhase.Ready)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.That(demo.Thrower.Cycle.Phase, Is.EqualTo(NetPhase.Ready), "The net never came back.");
            Assert.That(demo.Thrower.IsNetInHandShown, Is.False, "The net came back in hand while put away.");
        }

        [UnityTest]
        public IEnumerator MovingTheNetOutOfTheSelectedSlot_PutsItAway()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();

            Assert.That(demo.Inventory.MoveOrSwap(InventoryLayout.Standard.QuickBarFirstSlot, InventoryLayout.Standard.QuickBarFirstSlot + 1), Is.True);
            Assert.That(demo.Thrower.IsEquipped, Is.False, "The net is in hand from an unselected slot.");

            demo.Hotbar.Select(1);
            Assert.That(demo.Thrower.IsEquipped, Is.True, "The net is not in hand from its new slot.");
        }

        [UnityTest]
        public IEnumerator TheInventory_HidesTheQuickBar_AndThePause_FreezesIt()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();

            demo.SetInventoryOpen(true);
            Assert.That(demo.Hotbar.IsVisible, Is.False, "The quick bar shows over the inventory.");
            Assert.That(demo.Controls.IsMenuOpen, Is.True, "The inventory does not take the controls.");

            demo.SetInventoryOpen(false);
            Assert.That(demo.Hotbar.IsVisible, Is.True);
            Assert.That(demo.Controls.IsMenuOpen, Is.False);

            demo.Screens.Open(demo.Pause);
            Assert.That(demo.Controls.IsMenuOpen, Is.True, "The quick bar still responds in pause.");

            demo.Screens.Close(demo.Pause);
            Assert.That(demo.Controls.IsMenuOpen, Is.False);
        }

        [UnityTest]
        public IEnumerator Reset_PutsTheNetBackInTheFirstSlot_InHand()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();

            demo.Inventory.MoveOrSwap(InventoryLayout.Standard.QuickBarFirstSlot, 0);
            demo.Hotbar.Select(2);
            demo.ResetDemo();
            yield return null;

            Assert.That(demo.Inventory.GetSlot(InventoryLayout.Standard.QuickBarFirstSlot).ItemId, Is.EqualTo(NetId));
            Assert.That(demo.Inventory.Count(NetId), Is.EqualTo(1), "Reset made a second net.");
            Assert.That(demo.Hotbar.Selected, Is.EqualTo(0));
            Assert.That(demo.Thrower.IsEquipped, Is.True);
        }

        [UnityTest]
        public IEnumerator GatheringASpot_PutsOneItemInTheInventory_AndHidesIt()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var spot = demo.Gatherables[0];

            Assert.That(spot.Interact(demo.Inventory), Is.True);
            Assert.That(demo.Inventory.Count(spot.Item.Id), Is.EqualTo(1));
            Assert.That(spot.IsAvailable, Is.False);
            Assert.That(spot.Interact(demo.Inventory), Is.False, "A gathered spot was gathered twice.");
        }

        /// <summary>
        /// Replaces SticksAndMushrooms_AreSpreadOverTheWholeMap: since the playtest of
        /// 20/11 they are kept around the player instead of over the whole map.
        /// </summary>
        [UnityTest]
        public IEnumerator SticksAndMushrooms_AreAroundThePlayer()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var terrain = Terrain.activeTerrain;
            var player = Object.FindFirstObjectByType<PlayerMotor>().transform.position;

            foreach (var (itemId, expected) in new[] { (WoodId, 4), (SporeId, 5) })
            {
                var positions = demo.Gatherables
                    .Where(gatherable => gatherable.IsAvailable && gatherable.Item.Id == itemId)
                    .Select(gatherable => gatherable.transform.position)
                    .ToArray();

                Assert.That(positions.Count(position => GroundDistance(position, player) <= 50f), Is.EqualTo(expected), $"Not {expected} {itemId} within 50 m.");

                foreach (var position in positions)
                {
                    var ground = terrain.SampleHeight(position) + terrain.GetPosition().y;
                    Assert.That(position.y, Is.EqualTo(ground).Within(1f), $"A {itemId} is not on the ground.");
                    Assert.That(GroundDistance(position, player), Is.GreaterThanOrEqualTo(8f), $"A {itemId} starts right next to the player.");
                }
            }
        }

        [UnityTest]
        public IEnumerator NoMushroomSpot_IsCloseToAStickSpot()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();

            var sticks = demo.StickSpots.Select(spot => spot.Position).ToArray();
            foreach (var mushroom in demo.MushroomSpots)
            {
                foreach (var stick in sticks)
                {
                    Assert.That(GroundDistance(mushroom.Position, stick), Is.GreaterThanOrEqualTo(1.5f), "A mushroom stands on a stick.");
                }
            }
        }

        [UnityTest]
        public IEnumerator MushroomsAppear_InSeveralModels_AllOutlined()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var timeScale = Time.timeScale;

            // Gathered over and over, they come back in models drawn at random.
            Time.timeScale = 20f;
            for (var round = 0; round < 12; round++)
            {
                var available = demo.Gatherables.Where(candidate => candidate.IsAvailable && candidate.Item.Id == SporeId).ToArray();
                Assert.That(available, Is.Not.Empty);
                Assert.That(available[0].Interact(demo.Inventory), Is.True);

                var elapsed = 0f;
                while (elapsed < 3f && demo.Gatherables.Count(candidate => candidate.IsAvailable && candidate.Item.Id == SporeId) < available.Length)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            Time.timeScale = timeScale;

            var mushrooms = demo.Gatherables.Where(candidate => candidate.Item.Id == SporeId).ToArray();
            var models = mushrooms.Select(mushroom => mushroom.GetComponentInChildren<LODGroup>(true).name).Distinct().ToArray();
            Assert.That(models, Has.Length.GreaterThanOrEqualTo(2), "Every mushroom has the same model.");
            Assert.That(models, Is.SubsetOf(new[] { "PF_MushroomCluster", "PF_MushroomCluster_02", "PF_MushroomCluster_03", "PF_MushroomCluster_04" }));

            foreach (var mushroom in mushrooms)
            {
                Assert.That(
                    mushroom.GetComponentsInChildren<MeshRenderer>(true).Any(renderer => renderer.name == "Outline"),
                    Is.True,
                    $"{mushroom.GetComponentInChildren<LODGroup>(true).name} has no outline.");
            }
        }

        private static readonly Color Cyan = new Color32(0x6F, 0xD6, 0xEA, 0xFF);
        private static readonly Color Cream = new Color32(0xF2, 0xE6, 0xCC, 0xFF);

        /// <summary>
        /// Moves a gatherable in front of the camera, turned by <paramref name="angleDegrees"/>
        /// around the vertical, so that the point the player aims at is on that line.
        /// </summary>
        private static void PlaceInView(DemoLoopBootstrap demo, Gatherable gatherable, float angleDegrees, float distance)
        {
            var view = demo.ViewCamera.transform;
            var direction = Quaternion.Euler(0f, angleDegrees, 0f) * view.forward;
            gatherable.transform.position = view.position + direction * distance - Vector3.up * 0.3f;
        }

        private static Color OutlineColor(Gatherable gatherable)
        {
            return gatherable.GetComponentsInChildren<MeshRenderer>(true)
                .First(renderer => renderer.name == "Outline")
                .sharedMaterial.GetColor("_Color");
        }

        private static Gatherable[] TwoAvailable(DemoLoopBootstrap demo)
        {
            return demo.Gatherables.Where(gatherable => gatherable.IsAvailable).Take(2).ToArray();
        }

        [UnityTest]
        public IEnumerator TheTarget_IsOutlinedInCyan_AndWhatIsNotAimedAt_StaysCream()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var pair = TwoAvailable(demo);

            PlaceInView(demo, pair[0], 0f, 1.5f);
            PlaceInView(demo, pair[1], 40f, 1.5f);
            yield return null;
            yield return null;

            Assert.That(demo.Interactor.Targeted, Is.SameAs(pair[0]));
            Assert.That(OutlineColor(pair[0]), Is.EqualTo(Cyan).Using(ColorEqualityComparer.Instance));
            Assert.That(OutlineColor(pair[1]), Is.EqualTo(Cream).Using(ColorEqualityComparer.Instance), "An item not aimed at is outlined as a target.");
        }

        [UnityTest]
        public IEnumerator TheKey_ShowsEAboveTheTarget_AndFollowsIt()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var prompt = demo.Interactor.Prompt;
            var camera = demo.ViewCamera;
            var pair = TwoAvailable(demo);

            PlaceInView(demo, pair[0], 0f, 1.5f);
            PlaceInView(demo, pair[1], 40f, 1.5f);
            yield return null;
            yield return null;

            Assert.That(prompt.IsVisible, Is.True, "No key over the target.");
            Assert.That(prompt.Label, Is.EqualTo("E"));
            Assert.That(Vector2.Distance(prompt.ScreenPoint, camera.WorldToScreenPoint(pair[0].PromptPoint)), Is.LessThan(2f));
            Assert.That(pair[0].PromptPoint.y, Is.GreaterThan(pair[0].transform.position.y + 0.25f), "The key is not above the item.");

            PlaceInView(demo, pair[0], 60f, 1.5f);
            PlaceInView(demo, pair[1], 0f, 1.5f);
            yield return null;
            yield return null;

            Assert.That(demo.Interactor.Targeted, Is.SameAs(pair[1]));
            Assert.That(Vector2.Distance(prompt.ScreenPoint, camera.WorldToScreenPoint(pair[1].PromptPoint)), Is.LessThan(2f), "The key did not follow the new target.");
        }

        private static string[] HintLabels(DemoLoopBootstrap demo)
        {
            return demo.Hints.Shown.Select(hint => hint.LabelKey).ToArray();
        }

        [UnityTest]
        public IEnumerator InGame_TheHints_TellWhatTheNetDoes_ThenSwitchingAndTheInventory()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            yield return null;

            Assert.That(HintLabels(demo), Is.EqualTo(new[] { UiText.HintThrowNet, UiText.HintAim, UiText.HintSwitchItem, UiText.HintInventory }));
            Assert.That(demo.Hints.ShownGlyphs[0].Kind, Is.EqualTo(GlyphKind.MouseLeft), "The net is not thrown with the left click.");
            Assert.That(demo.Hints.ShownGlyphs[2].Kind, Is.EqualTo(GlyphKind.MouseWheel));
            Assert.That(demo.Hints.ShownGlyphs[3], Is.EqualTo(new Glyph(GlyphKind.Key, "Tab")));
        }

        [UnityTest]
        public IEnumerator WhileAiming_TheHints_SayThrow_AndReleaseToStop()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            demo.Thrower.HoldAim = true;
            yield return null;
            yield return null;

            Assert.That(HintLabels(demo).Take(2), Is.EqualTo(new[] { UiText.HintThrow, UiText.HintStopAiming }));
        }

        [UnityTest]
        public IEnumerator WithATarget_GatherComesFirst_AndSwitchingIsLeftOut()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();

            PlaceInView(demo, TwoAvailable(demo)[0], 0f, 1.5f);
            yield return null;
            yield return null;

            Assert.That(HintLabels(demo), Is.EqualTo(new[] { UiText.HintGather, UiText.HintThrowNet, UiText.HintAim, UiText.HintInventory }));
            Assert.That(demo.Hints.ShownGlyphs[0], Is.EqualTo(new Glyph(GlyphKind.Key, "E")));
        }

        [UnityTest]
        public IEnumerator TheInventory_ShowsItsOwnHints_AndThePause_None()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();

            demo.SetInventoryOpen(true);
            yield return null;
            Assert.That(HintLabels(demo), Is.EqualTo(new[] { UiText.HintTakeOrPlace, UiText.HintHalfOrOne, UiText.HintGatherAll, UiText.HintClose }));
            Assert.That(demo.Hints.ShownGlyphs[2].Kind, Is.EqualTo(GlyphKind.MouseLeft), "Gathering all is not a double click.");

            demo.Screens.Back();
            demo.Screens.Back();
            yield return null;
            Assert.That(demo.Pause.IsPaused, Is.True);
            Assert.That(demo.Hints.Shown, Is.Empty, "Hints show over the pause.");

            demo.Screens.Back();
            yield return null;
            Assert.That(HintLabels(demo), Has.Member(UiText.HintThrowNet));
        }

        [UnityTest]
        public IEnumerator TheKey_IsHidden_WithoutTarget_InPause_AndWithTheInventoryOpen()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var prompt = demo.Interactor.Prompt;

            yield return null;
            Assert.That(prompt.IsVisible, Is.False, "A key shows without a target.");

            PlaceInView(demo, TwoAvailable(demo)[0], 0f, 1.5f);
            yield return null;
            yield return null;
            Assert.That(prompt.IsVisible, Is.True);

            demo.Screens.Open(demo.Pause);
            yield return null;
            Assert.That(prompt.IsVisible, Is.False, "The key shows in pause.");

            demo.Screens.Close(demo.Pause);
            yield return null;
            yield return null;
            Assert.That(prompt.IsVisible, Is.True);

            demo.SetInventoryOpen(true);
            yield return null;
            Assert.That(prompt.IsVisible, Is.False, "The key shows over the inventory.");
        }

        private static IEnumerator Wait(float seconds)
        {
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator TheArc_ShowsOnlyWhileAiming()
        {
            yield return LoadDemo();
            var thrower = Object.FindFirstObjectByType<DemoLoopBootstrap>().Thrower;

            yield return null;
            Assert.That(thrower.VisibleDotCount, Is.Zero, "The arc shows without aiming.");
            Assert.That(thrower.IsImpactCircleShown, Is.False, "The impact circle shows without aiming.");

            thrower.HoldAim = true;
            yield return null;
            yield return null;
            Assert.That(thrower.IsAiming, Is.True);
            Assert.That(thrower.VisibleDotCount, Is.InRange(1, 24));
            Assert.That(thrower.IsImpactCircleShown, Is.True, "No impact circle while aiming at the ground ahead.");

            thrower.HoldAim = false;
            yield return null;
            yield return null;
            Assert.That(thrower.VisibleDotCount, Is.Zero);
            Assert.That(thrower.IsImpactCircleShown, Is.False);
        }

        [UnityTest]
        public IEnumerator Aiming_NarrowsTheViewTo45_AndReleasingWidensItBackTo60()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var camera = demo.ViewCamera;
            Assert.That(camera.fieldOfView, Is.EqualTo(60f).Within(0.01f));

            demo.Thrower.HoldAim = true;
            yield return Wait(0.3f);
            Assert.That(camera.fieldOfView, Is.EqualTo(45f).Within(0.01f));
            Assert.That(camera.GetComponent<PrototypeFirstPersonCamera>().LookScale, Is.EqualTo(45f / 60f).Within(0.001f), "The look is not slowed while aiming.");

            demo.Thrower.HoldAim = false;
            yield return Wait(0.3f);
            Assert.That(camera.fieldOfView, Is.EqualTo(60f).Within(0.01f));
            Assert.That(camera.GetComponent<PrototypeFirstPersonCamera>().LookScale, Is.EqualTo(1f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator ThePauseAndTheInventory_DropTheAim_AndWidenTheViewAtOnce()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var camera = demo.ViewCamera;
            demo.Thrower.HoldAim = true;
            yield return Wait(0.3f);

            demo.Screens.Open(demo.Pause);
            Assert.That(demo.Thrower.IsAiming, Is.False, "The pause kept the aim.");
            Assert.That(camera.fieldOfView, Is.EqualTo(60f).Within(0.01f));
            Assert.That(demo.Thrower.VisibleDotCount, Is.Zero);

            demo.Screens.Close(demo.Pause);
            yield return Wait(0.3f);
            Assert.That(demo.Thrower.IsAiming, Is.True);

            demo.SetInventoryOpen(true);
            Assert.That(demo.Thrower.IsAiming, Is.False, "The inventory kept the aim.");
            Assert.That(camera.fieldOfView, Is.EqualTo(60f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator TheNet_IsThrownWhileAiming_AsWithoutAiming()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var origin = Object.FindFirstObjectByType<PlayerMotor>().transform.position + Vector3.forward * 2f;

            demo.Thrower.HoldAim = true;
            yield return null;
            Assert.That(demo.Thrower.TryThrow(origin, Vector3.down), Is.True);
            yield return null;
            Assert.That(demo.Thrower.VisibleDotCount, Is.Zero, "The arc stays while the net is out.");
        }

        [UnityTest]
        public IEnumerator OnlyTheMushroomAimedAt_IsOutlined_InTheColourOfItsChance()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var thrower = demo.Thrower;
            var target = demo.Mushrooms[0];

            foreach (var mushroom in demo.Mushrooms)
            {
                Assert.That(OutlinesOf(mushroom), Is.Not.Empty, $"{mushroom.name} has no outline: is its model readable?");
                Assert.That(OutlinesOf(mushroom).Any(renderer => renderer.enabled), Is.False, $"{mushroom.name} is outlined without aiming.");
            }

            thrower.HoldAim = true;
            yield return null;
            yield return null;
            Assert.That(thrower.IsImpactCircleShown, Is.True);
            target.transform.position = thrower.PreviewLandingPoint;
            yield return null;
            yield return null;

            Assert.That(thrower.PreviewTarget, Is.SameAs(target));
            Assert.That(demo.TargetOutline.Shown, Is.SameAs(target));
            Assert.That(OutlinesOf(target).All(renderer => renderer.enabled), Is.True, "The mushroom aimed at is not outlined.");
            foreach (var other in demo.Mushrooms.Where(mushroom => mushroom != target))
            {
                Assert.That(OutlinesOf(other).Any(renderer => renderer.enabled), Is.False, $"{other.name} is outlined but not aimed at.");
            }

            Assert.That(
                demo.TargetOutline.Color,
                Is.EqualTo(NetChanceColors.Evaluate(thrower.PreviewChance)).Using(ColorEqualityComparer.Instance),
                "The outline is not the colour of the chance.");
            Assert.That(OutlinesOf(target)[0].sharedMaterial.GetColor("_Color"), Is.EqualTo(demo.TargetOutline.Color).Using(ColorEqualityComparer.Instance));

            thrower.HoldAim = false;
            yield return null;
            yield return null;
            Assert.That(demo.TargetOutline.Shown, Is.Null);
            Assert.That(OutlinesOf(target).Any(renderer => renderer.enabled), Is.False, "The outline stays after the aim.");
        }

        private static MeshRenderer[] OutlinesOf(NetDemoMushroom mushroom)
        {
            return mushroom.Model.GetComponentsInChildren<MeshRenderer>(true).Where(renderer => renderer.name == "Outline").ToArray();
        }

        private static float GroundDistance(Vector3 a, Vector3 b)
        {
            var flat = a - b;
            flat.y = 0f;
            return flat.magnitude;
        }

        [UnityTest]
        public IEnumerator EveryStick_LiesAtTheFootOfATree()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var terrain = Terrain.activeTerrain;
            var data = terrain.terrainData;
            var prototypes = data.treePrototypes;
            var trunks = data.treeInstances
                .Where(tree => prototypes[tree.prototypeIndex].prefab.name.StartsWith("PF_Tree"))
                .Select(tree => terrain.GetPosition() + Vector3.Scale(tree.position, data.size))
                .ToArray();

            foreach (var stick in demo.Gatherables.Where(gatherable => gatherable.IsAvailable && gatherable.Item.Id == WoodId))
            {
                var nearest = float.PositiveInfinity;
                foreach (var trunk in trunks)
                {
                    var flat = trunk - stick.transform.position;
                    flat.y = 0f;
                    nearest = Mathf.Min(nearest, flat.magnitude);
                }

                // Its own tree is 1 to 2.5 m away; a neighbouring tree may be a little closer.
                Assert.That(nearest, Is.LessThanOrEqualTo(2.6f), "A stick lies away from every tree.");
            }
        }

        [UnityTest]
        public IEnumerator NoDecorMushroomIsLeft_ThatCannotBeGathered()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var data = Terrain.activeTerrain.terrainData;
            var prototypes = data.treePrototypes;

            Assert.That(
                data.treeInstances.Count(tree => prototypes[tree.prototypeIndex].prefab.name == "PF_MushroomCluster"),
                Is.Zero,
                "The Terrain still draws decor mushrooms.");

            // Every mushroom cluster left in view belongs to a gatherable.
            var clusters = Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None)
                .Where(group => group.name.StartsWith("PF_MushroomCluster"))
                .ToArray();
            Assert.That(clusters, Is.Not.Empty);
            foreach (var cluster in clusters)
            {
                Assert.That(
                    cluster.GetComponentInParent<Gatherable>(),
                    Is.Not.Null,
                    $"{cluster.name} is shown but cannot be gathered.");
            }

#if UNITY_EDITOR
            // The forest keeps its mushrooms on disk: only this session's copy lost them.
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TerrainData>("Assets/Scenes/Biomes/MysticForestTerrainData.asset");
            Assert.That(asset.treeInstanceCount, Is.GreaterThan(data.treeInstanceCount));
#endif
        }

        [UnityTest]
        public IEnumerator AGatheredStick_IsReplacedOnALaterTick_AwayFromThePlayer()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var player = Object.FindFirstObjectByType<PlayerMotor>().transform;
            var sticks = demo.Gatherables.Where(gatherable => gatherable.Item.Id == WoodId).ToArray();
            var maximum = sticks.Count(stick => stick.IsAvailable);
            var gathered = sticks.First(stick => stick.IsAvailable);

            // Gathered from afar, its spot may be drawn again at once: it does not count as staying.
            var before = sticks
                .Where(stick => stick.IsAvailable && stick != gathered)
                .Select(stick => stick.transform.position)
                .ToArray();

            Assert.That(gathered.Interact(demo.Inventory), Is.True);
            Assert.That(sticks.Count(stick => stick.IsAvailable), Is.EqualTo(maximum - 1));

            // A tick is 5 s of game time: sped up, it comes within a second.
            var timeScale = Time.timeScale;
            Time.timeScale = 20f;
            var elapsed = 0f;
            while (elapsed < 5f && sticks.Count(stick => stick.IsAvailable) < maximum)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Time.timeScale = timeScale;

            Assert.That(sticks.Count(stick => stick.IsAvailable), Is.EqualTo(maximum), "No stick came back.");
            var arrived = sticks.Where(stick => stick.IsAvailable).Select(stick => stick.transform.position).Except(before).ToArray();
            Assert.That(arrived, Has.Length.EqualTo(1));
            var toArrived = arrived[0] - player.position;
            toArrived.y = 0f;
            Assert.That(toArrived.magnitude, Is.GreaterThanOrEqualTo(20f), "The stick appeared next to the player.");
        }

        [UnityTest]
        public IEnumerator WhatCanBeGathered_IsOutlined_AndTheOutlineChangesWhenAimedAt()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();

            Assert.That(
                Object.FindObjectsByType<LineRenderer>(FindObjectsSortMode.None).Where(line => line.name == "Ground Ring"),
                Is.Empty,
                "A ground ring is still drawn.");

            foreach (var itemId in new[] { WoodId, SporeId })
            {
                var gatherable = demo.Gatherables.First(candidate => candidate.IsAvailable && candidate.Item.Id == itemId);
                var outlines = gatherable.GetComponentsInChildren<MeshRenderer>()
                    .Where(renderer => renderer.name == "Outline")
                    .ToArray();

                Assert.That(outlines, Is.Not.Empty, $"{itemId} has no outline.");
                Assert.That(outlines[0].sharedMaterial.shader.name, Is.EqualTo("Fungiiiii/ItemOutline"));
                Assert.That(outlines[0].GetComponent<MeshFilter>().sharedMesh.vertexCount, Is.GreaterThan(0));

                var idle = outlines[0].sharedMaterial;
                gatherable.SetTargeted(true);
                Assert.That(outlines[0].sharedMaterial, Is.Not.SameAs(idle), "Aiming at it does not change its outline.");
                gatherable.SetTargeted(false);
                Assert.That(outlines[0].sharedMaterial, Is.SameAs(idle));
            }
        }

        [UnityTest]
        public IEnumerator Pausing_FreezesTheGame_AndResumingGivesItBack()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var view = Object.FindFirstObjectByType<PrototypeFirstPersonCamera>();

            Assert.That(demo.Pause.IsPaused, Is.False);
            Assert.That(demo.Pause.ResumeButton.gameObject.activeInHierarchy, Is.False, "The pause menu shows before any pause.");

            demo.Screens.Open(demo.Pause);
            yield return null;

            Assert.That(Time.timeScale, Is.Zero, "Time still runs.");
            Assert.That(demo.Pause.ResumeButton.gameObject.activeInHierarchy, Is.True, "The pause menu is not shown.");
            Assert.That(demo.Pause.HomeButton.gameObject.activeInHierarchy, Is.True);
            Assert.That(demo.Pause.QuitButton.gameObject.activeInHierarchy, Is.True);
            Assert.That(view.enabled, Is.False, "The view still turns.");
            Assert.That(demo.Controls.IsMenuOpen, Is.True, "The net can still be thrown.");
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None), "The pointer is not free.");

            demo.Pause.ResumeButton.Button.onClick.Invoke();
            yield return null;

            Assert.That(demo.Pause.IsPaused, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(demo.Pause.ResumeButton.gameObject.activeInHierarchy, Is.False);
            Assert.That(view.enabled, Is.True);
            Assert.That(demo.Controls.IsMenuOpen, Is.False);

            // A second pause shows the menu as the first did, Resume standing out.
            demo.Screens.Open(demo.Pause);
            yield return new WaitForSecondsRealtime(0.5f);

            Assert.That(
                demo.Pause.ResumeButton.GetComponentInChildren<PlankGraphic>().Highlight,
                Is.EqualTo(1f).Within(0.01f),
                "Resume does not stand out on the second pause.");
            demo.Screens.Close(demo.Pause);
        }

        [UnityTest]
        public IEnumerator Escape_WithTheInventoryOpen_ClosesIt_WithoutPausing()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var view = Object.FindFirstObjectByType<PrototypeFirstPersonCamera>();
            demo.SetInventoryOpen(true);
            yield return null;

            demo.Screens.Back();
            yield return null;

            Assert.That(demo.IsInventoryOpen, Is.False);
            Assert.That(demo.Pause.IsPaused, Is.False, "Escape paused over the inventory.");
            Assert.That(view.enabled, Is.True, "The view does not turn again.");
            Assert.That(demo.Hotbar.IsVisible, Is.True);
        }

        [UnityTest]
        public IEnumerator Escape_WithNothingOpen_Pauses_AndTab_DoesNotOpenTheInventoryOverThePause()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();

            demo.Screens.Back();
            yield return null;
            Assert.That(demo.Pause.IsPaused, Is.True);

            demo.Screens.Toggle(demo.InventoryView);
            yield return null;
            Assert.That(demo.IsInventoryOpen, Is.False, "The inventory opened over the pause.");

            demo.Screens.Back();
            yield return null;
            Assert.That(demo.Pause.IsPaused, Is.False);
            Assert.That(demo.Controls.IsMenuOpen, Is.False);
        }

        [UnityTest]
        public IEnumerator Home_GoesBackToTheHomeScreen_WithTimeRunning()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();

            demo.Screens.Open(demo.Pause);
            yield return null;
            demo.Pause.HomeButton.Button.onClick.Invoke();

            MainMenu menu = null;
            var elapsed = 0f;
            while (elapsed < LoadTimeoutSeconds && (menu == null || menu.PlayButton == null))
            {
                menu = Object.FindFirstObjectByType<MainMenu>();
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(menu, Is.Not.Null, "Home did not load the home screen.");
            Assert.That(menu.PlayButton, Is.Not.Null);
            Assert.That(Time.timeScale, Is.EqualTo(1f), "Time is still stopped on the home screen.");
            Assert.That(Object.FindFirstObjectByType<DemoLoopBootstrap>(), Is.Null, "The game is still loaded.");
        }

        [UnityTest]
        public IEnumerator ACapture_PutsTheMushroomInTheInventory()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var target = demo.Mushrooms.First(mushroom => mushroom.Profile == NetTargetProfile.Common);
            var item = demo.ItemFor(target);

            for (var attempt = 0; attempt < MaxCaptureAttempts && demo.Inventory.Count(item.Id) == 0; attempt++)
            {
                demo.ResetDemo();
                yield return null;

                // Dropped from above, so the landing does not depend on the aim of a long throw.
                var origin = target.transform.position + Vector3.up * 3f;
                Assert.That(demo.Thrower.TryThrow(origin, Vector3.down), Is.True);

                var elapsed = 0f;
                var returned = false;
                demo.Thrower.Cycle.Returned += OnReturned;
                while (elapsed < ThrowTimeoutSeconds && !returned)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                demo.Thrower.Cycle.Returned -= OnReturned;
                Assert.That(returned, Is.True, "The net never came back.");

                void OnReturned() => returned = true;
            }

            Assert.That(demo.Inventory.Count(item.Id), Is.EqualTo(1), "No capture put the mushroom in the inventory.");
        }
    }
}
