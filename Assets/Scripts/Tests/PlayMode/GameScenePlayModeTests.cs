using System.Collections;
using System.Linq;
using Fungiiiii.Capture;
using Fungiiiii.Controls;
using Fungiiiii.Game;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using Fungiiiii.Outlines;
using Fungiiiii.Runtime.Player;
using Fungiiiii.Runtime.World;
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
    public sealed class GameScenePlayModeTests
    {
        private const string ScenePath = "Assets/Scenes/Game/Game.unity";

        // Ids of the game's items in the item catalog: stable keys, saved and sent over the network.
        private const string NetId = "net";
        private const string WoodId = "wood";
        private const string SporeId = "spore";
        private const string PebbleId = "pebble";
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

        private static IEnumerator LoadGame()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
#endif
            var elapsed = 0f;
            GameBootstrap game = null;
            while (elapsed < LoadTimeoutSeconds && (game == null || !game.IsReady))
            {
                game = Object.FindFirstObjectByType<GameBootstrap>();
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(game, Is.Not.Null, "The scene has no GameBootstrap.");
            Assert.That(game.IsReady, Is.True, "The game was not ready in time.");
            game.Intro.Skip();
        }

        /// <summary>
        /// Loads the game and puts the net in the first slot of the quick bar, selected: the
        /// player now crafts it, and these tests are about the net, not about crafting it.
        /// </summary>
        private static IEnumerator LoadGameWithNet()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            game.Inventory.AddToSlot(InventoryLayout.Standard.QuickBarFirstSlot, game.NetItem, 1);
            game.Hotbar.Select(0);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneLoad_LoadsTheForest_WithOnePlayerAndOneCamera()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();

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
                renderingCameras[0].GetComponent<FirstPersonCamera>(),
                Is.Not.Null,
                "The forest's camera is not the player's view.");

            Assert.That(game.Mushrooms.Count, Is.GreaterThan(0));
            Assert.That(game.Gatherables.Count, Is.GreaterThan(0));
            Assert.That(game.Hole, Is.Null, "The Champimaison exists before the seed is planted.");
            Assert.That(game.MoleHoles.Holes.Count, Is.GreaterThan(0), "No mole hole to plant the seed in.");
            Assert.That(game.IsInventoryOpen, Is.False);

            for (var index = 0; index < game.Inventory.SlotCount; index++)
            {
                Assert.That(game.Inventory.GetSlot(index).IsEmpty, Is.True, "The player must start empty-handed.");
            }
        }

        [UnityTest]
        public IEnumerator TheInventory_IsTheOneThePlayerCarries_AndTheScreenShowsIt()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var backpack = Object.FindFirstObjectByType<PlayerMotor>().GetComponent<Backpack>();

            Assert.That(backpack, Is.Not.Null, "The player carries no backpack.");
            Assert.That(game.Inventory, Is.SameAs(backpack.Inventory));
            Assert.That(game.InventoryView.Inventory, Is.SameAs(backpack.Inventory));
        }

        [UnityTest]
        public IEnumerator ThePlayer_StartsEmptyHanded_TheNetPutInTheQuickBarIsInHand()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();

            Assert.That(game.Inventory.Count(NetId), Is.Zero, "The net is given at the start.");
            Assert.That(game.Hotbar.IsVisible, Is.True, "The quick bar is not on screen.");
            Assert.That(game.Hotbar.SlotCount, Is.EqualTo(3));
            Assert.That(game.Hotbar.Selected, Is.EqualTo(0));
            Assert.That(game.Thrower.IsEquipped, Is.False, "Something is in hand.");

            game.Inventory.AddToSlot(InventoryLayout.Standard.QuickBarFirstSlot, game.NetItem, 1);
            yield return null;

            Assert.That(game.Thrower.IsEquipped, Is.True);
            Assert.That(game.Thrower.IsNetInHandShown, Is.True, "The net is not in hand.");
        }

        [UnityTest]
        public IEnumerator SelectingAnotherSlot_PutsTheNetAway_AndSelectingItAgainGivesItBack()
        {
            yield return LoadGameWithNet();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var origin = game.Mushrooms[0].transform.position + Vector3.up * 3f;

            game.Hotbar.Select(1);
            yield return null;

            Assert.That(game.Thrower.IsEquipped, Is.False);
            Assert.That(game.Thrower.IsNetInHandShown, Is.False, "The net is still in hand.");
            Assert.That(game.Thrower.TryThrow(origin, Vector3.down), Is.False, "A net put away was thrown.");

            game.Hotbar.Select(0);
            yield return null;

            Assert.That(game.Thrower.IsEquipped, Is.True);
            Assert.That(game.Thrower.IsNetInHandShown, Is.True, "The net is not back in hand.");
            Assert.That(game.Thrower.TryThrow(origin, Vector3.down), Is.True);
        }

        [UnityTest]
        public IEnumerator ANetThrown_ComesBack_EvenWhenAnotherSlotIsSelected()
        {
            yield return LoadGameWithNet();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var origin = Object.FindFirstObjectByType<PlayerMotor>().transform.position + Vector3.forward * 2f;

            Assert.That(game.Thrower.TryThrow(origin, Vector3.down), Is.True);
            game.Hotbar.Select(2);

            var elapsed = 0f;
            while (elapsed < ThrowTimeoutSeconds && game.Thrower.Cycle.Phase != NetPhase.Ready)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.That(game.Thrower.Cycle.Phase, Is.EqualTo(NetPhase.Ready), "The net never came back.");
            Assert.That(game.Thrower.IsNetInHandShown, Is.False, "The net came back in hand while put away.");
        }

        [UnityTest]
        public IEnumerator MovingTheNetOutOfTheSelectedSlot_PutsItAway()
        {
            yield return LoadGameWithNet();
            var game = Object.FindFirstObjectByType<GameBootstrap>();

            Assert.That(game.Inventory.MoveOrSwap(InventoryLayout.Standard.QuickBarFirstSlot, InventoryLayout.Standard.QuickBarFirstSlot + 1), Is.True);
            Assert.That(game.Thrower.IsEquipped, Is.False, "The net is in hand from an unselected slot.");

            game.Hotbar.Select(1);
            Assert.That(game.Thrower.IsEquipped, Is.True, "The net is not in hand from its new slot.");
        }

        [UnityTest]
        public IEnumerator TheInventory_HidesTheQuickBar_AndThePause_FreezesIt()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();

            game.SetInventoryOpen(true);
            Assert.That(game.Hotbar.IsVisible, Is.False, "The quick bar shows over the inventory.");
            Assert.That(game.Controls.IsMenuOpen, Is.True, "The inventory does not take the controls.");

            game.SetInventoryOpen(false);
            Assert.That(game.Hotbar.IsVisible, Is.True);
            Assert.That(game.Controls.IsMenuOpen, Is.False);

            game.Screens.Open(game.Pause);
            Assert.That(game.Controls.IsMenuOpen, Is.True, "The quick bar still responds in pause.");

            game.Screens.Close(game.Pause);
            Assert.That(game.Controls.IsMenuOpen, Is.False);
        }

        [UnityTest]
        public IEnumerator Gathering_ShowsWhatWasCollected_OneLinePerItem()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var sticks = game.Gatherables.Where(candidate => candidate.IsAvailable && candidate.Item.Id == WoodId).Take(2).ToArray();
            Assert.That(sticks.Length, Is.EqualTo(2), "Fewer than two sticks to gather.");

            Assert.That(game.Pickups.Lines, Is.Empty);
            sticks[0].Interact(game.Inventory);
            sticks[1].Interact(game.Inventory);
            yield return null;

            Assert.That(game.Pickups.Lines.Count, Is.EqualTo(1), "Two sticks made two lines.");
            Assert.That(game.Pickups.Lines[0].Item.Id, Is.EqualTo(WoodId));
            Assert.That(game.Pickups.Lines[0].Count, Is.EqualTo(2));
            Assert.That(game.Pickups.Lines[0].Text, Does.StartWith("+2"));

            yield return Wait(3f);
            Assert.That(game.Pickups.Lines, Is.Empty, "The line did not fade out.");
        }

        [UnityTest]
        public IEnumerator AnItemDroppedFromTheInventory_LiesInFront_ThenCanBePickedUpAgain()
        {
            yield return LoadGameWithNet();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var netSlot = InventoryLayout.Standard.QuickBarFirstSlot;
            game.SetInventoryOpen(true);
            yield return null;

            game.InventoryView.Focus(netSlot, true);
            game.InventoryView.DropFromFocusedSlot();

            Assert.That(game.Inventory.GetSlot(netSlot).IsEmpty, Is.True, "The net is still in its slot.");
            var dropped = game.Dropped.Items.Single(item => item.Item.Id == NetId);
            var player = Object.FindFirstObjectByType<PlayerMotor>().transform.position;
            Assert.That(Vector3.Distance(Flat(dropped.transform.position), Flat(player)), Is.LessThan(2.5f), "It is not in front of the player.");
            var picture = dropped.GetComponentInChildren<FacesCamera>();
            Assert.That(picture, Is.Not.Null, "It does not lie as its picture.");
            Assert.That(picture.GetComponent<Renderer>().sharedMaterial.mainTexture, Is.SameAs(dropped.Item.Icon.texture), "Its picture is not its icon, as in the inventory.");

            game.SetInventoryOpen(false);
            yield return Wait(1f);
            Assert.That(dropped.Interact(game.Inventory), Is.True);
            yield return null;

            Assert.That(game.Inventory.Count(NetId), Is.EqualTo(1), "The net did not come back.");
            Assert.That(game.Dropped.Items.Any(item => item.Item.Id == NetId), Is.False);
            Assert.That(game.Pickups.Lines.Count, Is.EqualTo(1), "Picking it up was not shown.");
        }

        private static Vector3 Flat(Vector3 point)
        {
            return new Vector3(point.x, 0f, point.z);
        }

        [UnityTest]
        public IEnumerator SpaceAndA_Jump_ThePlayerOfTheGame()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var motor = Object.FindFirstObjectByType<PlayerMotor>();
            var jump = game.Controls.Player.Jump;
            Assert.That(jump.bindings.Any(binding => binding.path == "<Keyboard>/space"), Is.True, "Space does not jump.");
            Assert.That(jump.bindings.Any(binding => binding.path == "<Gamepad>/buttonSouth"), Is.True, "A does not jump.");

            var elapsed = 0f;
            while (!motor.IsGrounded && elapsed < 3f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // The height itself is tested on bare ground (PlayerJumpPlayModeTests); here, that
            // the player of the game leaves the ground, whatever the frames after loading last.
            var ground = motor.transform.position.y;
            Assert.That(motor.TryJump(), Is.True);
            var peak = ground;
            elapsed = 0f;
            while (elapsed < 1f)
            {
                peak = Mathf.Max(peak, motor.transform.position.y);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(peak, Is.GreaterThan(ground + 0.5f), "The player of the game does not jump.");
        }

        [UnityTest]
        public IEnumerator GatheringASpot_PutsOneItemInTheInventory_AndHidesIt()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var spot = game.Gatherables[0];

            Assert.That(spot.Interact(game.Inventory), Is.True);
            Assert.That(game.Inventory.Count(spot.Item.Id), Is.EqualTo(1));
            Assert.That(spot.IsAvailable, Is.False);
            Assert.That(spot.Interact(game.Inventory), Is.False, "A gathered spot was gathered twice.");
        }

        /// <summary>
        /// Replaces SticksAndMushrooms_AreSpreadOverTheWholeMap: since the playtest of
        /// 20/11 they are kept around the player instead of over the whole map.
        /// </summary>
        [UnityTest]
        public IEnumerator SticksMushroomsAndPebbles_AreAroundThePlayer()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var terrain = Terrain.activeTerrain;
            var player = Object.FindFirstObjectByType<PlayerMotor>().transform.position;

            // Since the playtest of 22/11, half as many sticks and mushrooms.
            foreach (var (itemId, expected) in new[] { (WoodId, 2), (SporeId, 3), (PebbleId, 4) })
            {
                var positions = game.Gatherables
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
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();

            var sticks = game.StickSpots.Select(spot => spot.Position).ToArray();
            foreach (var mushroom in game.MushroomSpots)
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
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var timeScale = Time.timeScale;

            // Gathered over and over, they come back in models drawn at random.
            Time.timeScale = 20f;
            for (var round = 0; round < 12; round++)
            {
                var available = game.Gatherables.Where(candidate => candidate.IsAvailable && candidate.Item.Id == SporeId).ToArray();
                Assert.That(available, Is.Not.Empty);
                Assert.That(available[0].Interact(game.Inventory), Is.True);

                var elapsed = 0f;
                while (elapsed < 3f && game.Gatherables.Count(candidate => candidate.IsAvailable && candidate.Item.Id == SporeId) < available.Length)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            Time.timeScale = timeScale;

            var mushrooms = game.Gatherables.Where(candidate => candidate.Item.Id == SporeId).ToArray();
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
        private static void PlaceInView(GameBootstrap game, Gatherable gatherable, float angleDegrees, float distance)
        {
            var view = game.ViewCamera.transform;
            var direction = Quaternion.Euler(0f, angleDegrees, 0f) * view.forward;
            gatherable.transform.position = view.position + direction * distance - Vector3.up * 0.3f;
        }

        private static Color OutlineColor(Gatherable gatherable)
        {
            return gatherable.GetComponentsInChildren<MeshRenderer>(true)
                .First(renderer => renderer.name == "Outline")
                .sharedMaterial.GetColor("_Color");
        }

        private static Gatherable[] TwoAvailable(GameBootstrap game)
        {
            return game.Gatherables.Where(gatherable => gatherable.IsAvailable).Take(2).ToArray();
        }

        [UnityTest]
        public IEnumerator TheTarget_IsOutlinedInCyan_AndWhatIsNotAimedAt_StaysCream()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var pair = TwoAvailable(game);

            PlaceInView(game, pair[0], 0f, 1.5f);
            PlaceInView(game, pair[1], 40f, 1.5f);
            yield return null;
            yield return null;

            Assert.That(game.Interactor.Targeted, Is.SameAs(pair[0]));
            Assert.That(OutlineColor(pair[0]), Is.EqualTo(Cyan).Using(ColorEqualityComparer.Instance));
            Assert.That(OutlineColor(pair[1]), Is.EqualTo(Cream).Using(ColorEqualityComparer.Instance), "An item not aimed at is outlined as a target.");
        }

        [UnityTest]
        public IEnumerator TheKey_ShowsEAboveTheTarget_AndFollowsIt()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var prompt = game.Interactor.Prompt;
            var camera = game.ViewCamera;
            var pair = TwoAvailable(game);

            PlaceInView(game, pair[0], 0f, 1.5f);
            PlaceInView(game, pair[1], 40f, 1.5f);
            yield return null;
            yield return null;

            Assert.That(prompt.IsVisible, Is.True, "No key over the target.");
            Assert.That(prompt.Label, Is.EqualTo("E"));
            Assert.That(prompt.IsPressMarkVisible, Is.True, "No triangle over the key of an item to gather.");
            Assert.That(prompt.IsHoldBorderVisible, Is.False, "An item to gather shows a key to hold.");
            Assert.That(Vector2.Distance(prompt.ScreenPoint, camera.WorldToScreenPoint(pair[0].PromptPoint)), Is.LessThan(2f));
            Assert.That(pair[0].PromptPoint.y, Is.GreaterThan(pair[0].transform.position.y + 0.25f), "The key is not above the item.");

            PlaceInView(game, pair[0], 60f, 1.5f);
            PlaceInView(game, pair[1], 0f, 1.5f);
            yield return null;
            yield return null;

            Assert.That(game.Interactor.Targeted, Is.SameAs(pair[1]));
            Assert.That(Vector2.Distance(prompt.ScreenPoint, camera.WorldToScreenPoint(pair[1].PromptPoint)), Is.LessThan(2f), "The key did not follow the new target.");
        }

        private static string[] HintLabels(GameBootstrap game)
        {
            return game.Hints.Shown.Select(hint => hint.LabelKey).ToArray();
        }

        [UnityTest]
        public IEnumerator InGame_TheHints_TellWhatTheNetDoes_ThenSwitchingAndTheInventory()
        {
            yield return LoadGameWithNet();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            yield return null;

            Assert.That(HintLabels(game), Is.EqualTo(new[] { UiText.HintThrowNet, UiText.HintAim, UiText.HintSwitchItem, UiText.HintInventory }));
            Assert.That(game.Hints.ShownGlyphs[0].Kind, Is.EqualTo(GlyphKind.MouseLeft), "The net is not thrown with the left click.");
            Assert.That(game.Hints.ShownGlyphs[2].Kind, Is.EqualTo(GlyphKind.MouseWheel));
            Assert.That(game.Hints.ShownGlyphs[3], Is.EqualTo(new Glyph(GlyphKind.Key, "Tab")));
        }

        [UnityTest]
        public IEnumerator WhileAiming_TheHints_SayThrow_AndStillAim()
        {
            yield return LoadGameWithNet();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            game.Thrower.HoldAim = true;
            yield return null;
            yield return null;

            Assert.That(HintLabels(game).Take(2), Is.EqualTo(new[] { UiText.HintThrow, UiText.HintAim }));
        }

        [UnityTest]
        public IEnumerator WithATarget_GatherIsWrittenRightOfItsKey_NotInTheCorner()
        {
            yield return LoadGameWithNet();
            var game = Object.FindFirstObjectByType<GameBootstrap>();

            PlaceInView(game, TwoAvailable(game)[0], 0f, 1.5f);
            yield return null;
            yield return null;

            // Since the playtest of 22/11, what E does is read next to its key, over the target.
            Assert.That(game.Interactor.Prompt.PressActionKey, Is.EqualTo(UiText.HintGather));
            Assert.That(HintLabels(game), Is.EqualTo(new[] { UiText.HintThrowNet, UiText.HintAim, UiText.HintSwitchItem, UiText.HintInventory }));
        }

        [UnityTest]
        public IEnumerator TheInventory_HidesTheVitalsAndTheObjective_AndKeepsItsHintsUnderItsPanels()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var vitals = Object.FindFirstObjectByType<VitalGauges>();
            Assert.That(vitals.IsVisible && game.ObjectiveHud.IsVisible, Is.True, "The vitals or the objective are hidden in play.");

            game.SetInventoryOpen(true);
            yield return null;
            yield return null;
            Assert.That(vitals.IsVisible, Is.False, "The vitals show under the inventory panel.");
            Assert.That(game.ObjectiveHud.IsVisible, Is.False, "The objective shows under the crafting panel.");

            // Every menu follows the screen's height: whatever the width, the hints stay under the panels.
            var hints = new Vector3[4];
            ((RectTransform)game.InventoryView.HintsPanel.transform).GetWorldCorners(hints);
            var panel = new Vector3[4];
            ((RectTransform)game.Crafting.transform).GetWorldCorners(panel);
            Assert.That(hints[1].y, Is.LessThan(panel[0].y), "The hints are not under the panels.");
            Assert.That(hints[0].y, Is.GreaterThanOrEqualTo(0f), "The hints are below the screen.");

            game.SetInventoryOpen(false);
            yield return null;
            Assert.That(vitals.IsVisible && game.ObjectiveHud.IsVisible, Is.True, "The vitals or the objective did not come back.");
        }

        [UnityTest]
        public IEnumerator TheInventory_ShowsItsOwnHints_AndThePause_None()
        {
            yield return LoadGameWithNet();
            var game = Object.FindFirstObjectByType<GameBootstrap>();

            game.SetInventoryOpen(true);
            yield return null;
            // Under its panels, not bottom right where the crafting panel is.
            var own = game.InventoryView.HintsPanel;
            Assert.That(game.Hints.Shown, Is.Empty, "The game's hints show under the crafting panel.");
            // Gathering all is optional: with dropping, there are five, one too many.
            Assert.That(own.Shown.Select(hint => hint.LabelKey).ToArray(), Is.EqualTo(new[] { UiText.HintTakeOrPlace, UiText.HintHalfOrOne, UiText.HintDrop, UiText.HintClose }));
            Assert.That(own.ShownGlyphs[2].Text, Is.EqualTo("Q"), "Dropping is not on Q.");

            game.Screens.Back();
            game.Screens.Back();
            yield return null;
            Assert.That(game.Pause.IsPaused, Is.True);
            Assert.That(game.Hints.Shown, Is.Empty, "Hints show over the pause.");

            game.Screens.Back();
            yield return null;
            Assert.That(HintLabels(game), Has.Member(UiText.HintThrowNet));
        }

        [UnityTest]
        public IEnumerator TheKey_IsHidden_WithoutTarget_InPause_AndWithTheInventoryOpen()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var prompt = game.Interactor.Prompt;

            yield return null;
            Assert.That(prompt.IsVisible, Is.False, "A key shows without a target.");

            PlaceInView(game, TwoAvailable(game)[0], 0f, 1.5f);
            yield return null;
            yield return null;
            Assert.That(prompt.IsVisible, Is.True);

            game.Screens.Open(game.Pause);
            yield return null;
            Assert.That(prompt.IsVisible, Is.False, "The key shows in pause.");

            game.Screens.Close(game.Pause);
            yield return null;
            yield return null;
            Assert.That(prompt.IsVisible, Is.True);

            game.SetInventoryOpen(true);
            yield return null;
            Assert.That(prompt.IsVisible, Is.False, "The key shows over the inventory.");
        }

        private static IEnumerator Wait(float seconds)
        {
            // The frame first, then its time: the time of the frame the wait starts in was
            // spent before the wait, and the frame that ends the loading lasts long.
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }
        }

        [UnityTest]
        public IEnumerator TheArc_ShowsOnlyWhileAiming()
        {
            yield return LoadGameWithNet();
            var thrower = Object.FindFirstObjectByType<GameBootstrap>().Thrower;

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
            yield return LoadGameWithNet();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var camera = game.ViewCamera;
            Assert.That(camera.fieldOfView, Is.EqualTo(60f).Within(0.01f));

            game.Thrower.HoldAim = true;
            yield return Wait(0.3f);
            Assert.That(camera.fieldOfView, Is.EqualTo(45f).Within(0.01f));
            Assert.That(camera.GetComponent<FirstPersonCamera>().LookScale, Is.EqualTo(45f / 60f).Within(0.001f), "The look is not slowed while aiming.");

            game.Thrower.HoldAim = false;
            yield return Wait(0.3f);
            Assert.That(camera.fieldOfView, Is.EqualTo(60f).Within(0.01f));
            Assert.That(camera.GetComponent<FirstPersonCamera>().LookScale, Is.EqualTo(1f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator ThePauseAndTheInventory_DropTheAim_AndWidenTheViewAsWhenReleased()
        {
            yield return LoadGameWithNet();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var camera = game.ViewCamera;
            game.Thrower.HoldAim = true;
            yield return Wait(0.3f);

            game.Screens.Open(game.Pause);
            Assert.That(game.Thrower.IsAiming, Is.False, "The pause kept the aim.");
            Assert.That(game.Thrower.VisibleDotCount, Is.Zero);
            yield return null;
            Assert.That(camera.fieldOfView, Is.LessThan(60f), "The view widened at once.");
            yield return Wait(0.3f);
            Assert.That(camera.fieldOfView, Is.EqualTo(60f).Within(0.01f), "The view did not widen back under the pause.");

            game.Screens.Close(game.Pause);
            yield return Wait(0.3f);
            Assert.That(game.Thrower.IsAiming, Is.True);

            game.SetInventoryOpen(true);
            Assert.That(game.Thrower.IsAiming, Is.False, "The inventory kept the aim.");
            yield return null;
            Assert.That(camera.fieldOfView, Is.LessThan(60f), "The view widened at once.");
            yield return Wait(0.3f);
            Assert.That(camera.fieldOfView, Is.EqualTo(60f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator TheNet_IsThrownWhileAiming_AsWithoutAiming()
        {
            yield return LoadGameWithNet();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var origin = Object.FindFirstObjectByType<PlayerMotor>().transform.position + Vector3.forward * 2f;

            game.Thrower.HoldAim = true;
            yield return null;
            Assert.That(game.Thrower.TryThrow(origin, Vector3.down), Is.True);
            yield return null;
            Assert.That(game.Thrower.VisibleDotCount, Is.Zero, "The arc stays while the net is out.");
        }

        [UnityTest]
        public IEnumerator OnlyTheMushroomAimedAt_IsOutlined_InTheColourOfItsChance()
        {
            yield return LoadGameWithNet();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var thrower = game.Thrower;
            var target = game.Mushrooms[0];

            // The wild mushrooms keep to their area in the mist, far from where the player
            // arrives: the player goes to the target, which can then be put where the net lands.
            // The view stays level, as on arrival, so that the net lands out of the alert distance.
            PlaytestLoopGame.StandBefore(target.transform.position, 10f);
            Object.FindFirstObjectByType<FirstPersonCamera>().SetView(0f, 0f);

            // Only the mushrooms near the player are out: it comes out once the player is there.
            yield return PlaytestLoopGame.Wait(0.6f);
            Assert.That(target.gameObject.activeSelf, Is.True, "The mushroom did not come out with the player beside it.");

            foreach (var mushroom in game.Mushrooms)
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
            Assert.That(game.TargetOutline.Shown, Is.SameAs(target));
            Assert.That(OutlinesOf(target).All(renderer => renderer.enabled), Is.True, "The mushroom aimed at is not outlined.");
            foreach (var other in game.Mushrooms.Where(mushroom => mushroom != target))
            {
                Assert.That(OutlinesOf(other).Any(renderer => renderer.enabled), Is.False, $"{other.name} is outlined but not aimed at.");
            }

            Assert.That(
                game.TargetOutline.Color,
                Is.EqualTo(NetChanceColors.Evaluate(thrower.PreviewChance)).Using(ColorEqualityComparer.Instance),
                "The outline is not the colour of the chance.");
            Assert.That(OutlinesOf(target)[0].sharedMaterial.GetColor("_Color"), Is.EqualTo(game.TargetOutline.Color).Using(ColorEqualityComparer.Instance));

            thrower.HoldAim = false;
            yield return null;
            yield return null;
            Assert.That(game.TargetOutline.Shown, Is.Null);
            Assert.That(OutlinesOf(target).Any(renderer => renderer.enabled), Is.False, "The outline stays after the aim.");
        }

        private static MeshRenderer[] OutlinesOf(WildMushroom mushroom)
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
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var terrain = Terrain.activeTerrain;
            var data = terrain.terrainData;
            var prototypes = data.treePrototypes;
            var trunks = data.treeInstances
                .Where(tree => prototypes[tree.prototypeIndex].prefab.name.StartsWith("PF_Tree"))
                .Select(tree => terrain.GetPosition() + Vector3.Scale(tree.position, data.size))
                .ToArray();

            foreach (var stick in game.Gatherables.Where(gatherable => gatherable.IsAvailable && gatherable.Item.Id == WoodId))
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
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
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
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var player = Object.FindFirstObjectByType<PlayerMotor>().transform;
            var sticks = game.Gatherables.Where(gatherable => gatherable.Item.Id == WoodId).ToArray();
            var maximum = sticks.Count(stick => stick.IsAvailable);
            var gathered = sticks.First(stick => stick.IsAvailable);

            // Gathered from afar, its spot may be drawn again at once: it does not count as staying.
            var before = sticks
                .Where(stick => stick.IsAvailable && stick != gathered)
                .Select(stick => stick.transform.position)
                .ToArray();

            Assert.That(gathered.Interact(game.Inventory), Is.True);
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
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();

            Assert.That(
                Object.FindObjectsByType<LineRenderer>(FindObjectsSortMode.None).Where(line => line.name == "Ground Ring"),
                Is.Empty,
                "A ground ring is still drawn.");

            foreach (var itemId in new[] { WoodId, SporeId })
            {
                var gatherable = game.Gatherables.First(candidate => candidate.IsAvailable && candidate.Item.Id == itemId);
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
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var view = Object.FindFirstObjectByType<FirstPersonCamera>();

            Assert.That(game.Pause.IsPaused, Is.False);
            Assert.That(game.Pause.ResumeButton.gameObject.activeInHierarchy, Is.False, "The pause menu shows before any pause.");

            game.Screens.Open(game.Pause);
            yield return null;

            Assert.That(Time.timeScale, Is.Zero, "Time still runs.");
            Assert.That(game.Pause.ResumeButton.gameObject.activeInHierarchy, Is.True, "The pause menu is not shown.");
            Assert.That(game.Pause.HomeButton.gameObject.activeInHierarchy, Is.True);
            Assert.That(game.Pause.QuitButton.gameObject.activeInHierarchy, Is.True);
            Assert.That(view.enabled, Is.False, "The view still turns.");
            Assert.That(game.Controls.IsMenuOpen, Is.True, "The net can still be thrown.");
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None), "The pointer is not free.");

            game.Pause.ResumeButton.Button.onClick.Invoke();
            yield return null;

            Assert.That(game.Pause.IsPaused, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(game.Pause.ResumeButton.gameObject.activeInHierarchy, Is.False);
            Assert.That(view.enabled, Is.True);
            Assert.That(game.Controls.IsMenuOpen, Is.False);

            // A second pause shows the menu as the first did, no plank standing out.
            game.Screens.Open(game.Pause);
            yield return new WaitForSecondsRealtime(0.5f);

            Assert.That(
                game.Pause.ResumeButton.GetComponentInChildren<PlankGraphic>().Highlight,
                Is.Zero,
                "A plank stands out with the mouse away.");
            Assert.That(game.Pause.GetComponent<SelectFirstOnMove>(), Is.Not.Null, "The gamepad has no first plank to select.");
            game.Screens.Close(game.Pause);
        }

        [UnityTest]
        public IEnumerator Escape_WithTheInventoryOpen_ClosesIt_WithoutPausing()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var view = Object.FindFirstObjectByType<FirstPersonCamera>();
            game.SetInventoryOpen(true);
            yield return null;

            game.Screens.Back();
            yield return null;

            Assert.That(game.IsInventoryOpen, Is.False);
            Assert.That(game.Pause.IsPaused, Is.False, "Escape paused over the inventory.");
            Assert.That(view.enabled, Is.True, "The view does not turn again.");
            Assert.That(game.Hotbar.IsVisible, Is.True);
        }

        [UnityTest]
        public IEnumerator Escape_WithNothingOpen_Pauses_AndTab_DoesNotOpenTheInventoryOverThePause()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();

            game.Screens.Back();
            yield return null;
            Assert.That(game.Pause.IsPaused, Is.True);

            game.Screens.Toggle(game.InventoryView);
            yield return null;
            Assert.That(game.IsInventoryOpen, Is.False, "The inventory opened over the pause.");

            game.Screens.Back();
            yield return null;
            Assert.That(game.Pause.IsPaused, Is.False);
            Assert.That(game.Controls.IsMenuOpen, Is.False);
        }

        [UnityTest]
        public IEnumerator Home_GoesBackToTheHomeScreen_WithTimeRunning()
        {
            yield return LoadGame();
            var game = Object.FindFirstObjectByType<GameBootstrap>();

            game.Screens.Open(game.Pause);
            yield return null;
            game.Pause.HomeButton.Button.onClick.Invoke();

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
            Assert.That(Object.FindFirstObjectByType<GameBootstrap>(), Is.Null, "The game is still loaded.");
        }

        [UnityTest]
        public IEnumerator ACapture_PutsTheMushroomInTheInventory()
        {
            yield return LoadGameWithNet();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var target = game.Mushrooms.First(mushroom => mushroom.Profile == NetTargetProfile.Common);
            var item = game.ItemFor(target);

            for (var attempt = 0; attempt < MaxCaptureAttempts && game.Inventory.Count(item.Id) == 0; attempt++)
            {
                // A failed try leaves the mushroom running and the net out: both back, calm and in hand.
                target.ResetToSpawn();
                game.Thrower.ResetNet();
                yield return null;

                // Dropped from above, so the landing does not depend on the aim of a long throw.
                var origin = target.transform.position + Vector3.up * 3f;
                Assert.That(game.Thrower.TryThrow(origin, Vector3.down), Is.True);

                var elapsed = 0f;
                var returned = false;
                game.Thrower.Cycle.Returned += OnReturned;
                while (elapsed < ThrowTimeoutSeconds && !returned)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                game.Thrower.Cycle.Returned -= OnReturned;
                Assert.That(returned, Is.True, "The net never came back.");

                void OnReturned() => returned = true;
            }

            Assert.That(game.Inventory.Count(item.Id), Is.EqualTo(1), "No capture put the mushroom in the inventory.");
        }
    }
}
