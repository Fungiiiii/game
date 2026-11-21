using System.Collections;
using System.Linq;
using Fungiiiii.Capture;
using Fungiiiii.Companions;
using Fungiiiii.Game;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using Fungiiiii.Runtime.Player;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>The Spongeus taken out to follow the player, and the water ferns of the mist (docs/features/spongeus-fougere.md).</summary>
    public sealed class SpongeusCompanionPlayModeTests
    {
        private const string ScenePath = "Assets/Scenes/Game/Game.unity";
        private const float LoadTimeoutSeconds = 60f;
        private const string SpongeusId = "spongeus";
        private const string WaterId = "water";
        private const string NeedsSpongeus = "label.needs_spongeus";

        /// <summary>The mist's zone, within the playable forest (x, z, width, depth).</summary>
        private static readonly Rect MistArea = new Rect(-290f, -60f, 580f, 180f);

        /// <summary>Looking down this much, the view's centre meets the ground about 2.3 m ahead.</summary>
        private const float LookDownDegrees = 35f;

        /// <summary>Where something on the ground is under the centre of the view, looking down.</summary>
        private const float AimedDistance = 2.2f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");

        private GameBootstrap game;

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        private IEnumerator LoadGame()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            var elapsed = 0f;
            game = null;
            while (elapsed < LoadTimeoutSeconds && (game == null || !game.IsReady))
            {
                game = Object.FindFirstObjectByType<GameBootstrap>();
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(game, Is.Not.Null, "The scene has no GameBootstrap.");
            Assert.That(game.IsReady, Is.True, "The game was not ready in time.");
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

        private static IEnumerator WaitUntil(System.Func<bool> condition, float timeoutSeconds)
        {
            var elapsed = 0f;
            while (!condition() && elapsed < timeoutSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private Transform Player => Object.FindFirstObjectByType<PlayerMotor>().transform;

        private static float Flat(Vector3 a, Vector3 b)
        {
            return new Vector2(a.x - b.x, a.z - b.z).magnitude;
        }

        /// <summary>Puts <paramref name="count"/> Spongeus in the second quick bar slot and selects it.</summary>
        private IEnumerator HoldSpongeus(int count)
        {
            var catalogItem = FindItem(SpongeusId);
            var slot = Object.FindFirstObjectByType<PlayerMotor>().GetComponent<Backpack>().Layout.QuickBarSlot(1);
            Assert.That(game.Inventory.AddToSlot(slot, catalogItem, count), Is.EqualTo(count));
            game.Hotbar.Select(1);
            yield return null;
            Assert.That(game.Hands.ItemInHand, Is.SameAs(catalogItem), "The Spongeus is not in hand.");
        }

        private ItemDefinition FindItem(string id)
        {
            var companion = game.Companions.HeldBehaviours.Select(held => held.Item).FirstOrDefault(item => item.Id == id);
            if (companion != null)
            {
                return companion;
            }

            var plant = game.RefillingPlants.Plants.First();
            Assert.That(plant.Yield.Id, Is.EqualTo(id), $"No item '{id}' found.");
            return plant.Yield;
        }

        private IEnumerator TakeOut()
        {
            yield return HoldSpongeus(1);
            game.Hands.Behaviour.UsePrimary();
            yield return null;
            Assert.That(game.Companions.Active, Is.Not.Null, "The left click did not take the Spongeus out.");
        }

        /// <summary>
        /// Small things on the ground close by sit below the 25° cone around the view, as
        /// for a player: the view looks down at them, as the player would.
        /// </summary>
        private IEnumerator LookDown()
        {
            game.ViewCamera.GetComponent<FirstPersonCamera>().SetView(game.ViewCamera.transform.eulerAngles.y, LookDownDegrees);
            yield return null;
        }

        /// <summary>Puts a plant on the ground in front of the camera, full or empty.</summary>
        private RefillingPlant PlantInView(bool full, float distance = 2f)
        {
            var plant = game.RefillingPlants.Plants.First();
            plant.Restart(full ? 1f : 0f);
            var view = game.ViewCamera.transform;
            var forward = Vector3.ProjectOnPlane(view.forward, Vector3.up).normalized;
            var point = view.position + forward * distance;
            point.y = Player.position.y - 1.1f;
            plant.transform.position = point;
            return plant;
        }

        [UnityTest]
        public IEnumerator WaterFerns_GrowOnlyInTheMist_SomeFullSomeNot()
        {
            yield return LoadGame();
            var plants = game.RefillingPlants.Plants;

            Assert.That(plants.Count, Is.GreaterThan(30), "Too few water ferns.");
            foreach (var plant in plants)
            {
                var position = new Vector2(plant.transform.position.x, plant.transform.position.z);
                Assert.That(MistArea.Contains(position), Is.True, $"A water fern grows out of the mist at {position}.");
            }

            Assert.That(plants.Any(plant => plant.IsFull), Is.True, "No water fern is full at the start.");
            Assert.That(plants.Any(plant => !plant.IsFull), Is.True, "Every water fern is full at the start.");
        }

        [UnityTest]
        public IEnumerator NoDecorFern_IsLeft_ThatCouldBeMistakenForAWaterFern()
        {
            yield return LoadGame();
            var lookAlike = game.RefillingPlants.Plants.First().Definition.LookAlikes.Single();
            var data = Terrain.activeTerrain.terrainData;
            var prototype = System.Array.FindIndex(data.treePrototypes, tree => tree.prefab == lookAlike);

            Assert.That(prototype, Is.GreaterThanOrEqualTo(0), "The forest's Terrain draws no decor fern: the test checks nothing.");
            Assert.That(data.treeInstances.Count(tree => tree.prototypeIndex == prototype), Is.Zero, "Decor ferns are still drawn by the Terrain.");
            Assert.That(
                Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(placed => placed.name == lookAlike.name),
                Is.Empty,
                "A hand-placed decor fern is still in the forest.");
        }

        [UnityTest]
        public IEnumerator AFullFern_LooksWet_Drips_AndShowsATextInsteadOfAKey_AndEDoesNothing()
        {
            yield return LoadGame();
            yield return LookDown();
            var plant = PlantInView(full: true, AimedDistance);
            yield return null;
            yield return null;

            var prompt = game.Interactor.Prompt;
            Assert.That(game.Interactor.Targeted, Is.SameAs(plant), "A full fern in front of the player is not aimed at.");
            Assert.That(prompt.IsTextVisible, Is.True, "No text over the full fern.");
            Assert.That(prompt.TextKey, Is.EqualTo(NeedsSpongeus));
            Assert.That(prompt.IsKeyVisible, Is.False, "A key is shown over the fern.");
            Assert.That(game.Interactor.Hints, Is.Empty, "The fern adds a key hint.");
            Assert.That(plant.Interact(game.Inventory), Is.False);
            Assert.That(game.Inventory.Count(WaterId), Is.Zero, "The player took water themselves.");
            Assert.That(plant.IsDripping, Is.True, "No drop falls from the full fern.");
            var wet = ModelBlock(plant);
            Assert.That(wet.GetColor(BaseColorId).r, Is.EqualTo(plant.Definition.FullTint.r).Within(0.01f), "The full fern is not darker.");
            Assert.That(wet.GetFloat(SmoothnessId), Is.EqualTo(plant.Definition.FullSmoothness).Within(0.01f), "The full fern has no sheen.");
        }

        [UnityTest]
        public IEnumerator AnEmptyFern_LooksDry_DoesNotDrip_AndCannotBeAimedAt()
        {
            yield return LoadGame();
            yield return LookDown();
            var plant = PlantInView(full: false, AimedDistance);
            yield return null;
            yield return null;

            Assert.That(game.Interactor.Targeted, Is.Not.SameAs(plant));
            Assert.That(plant.IsDripping, Is.False, "A drop falls from an empty fern.");
            Assert.That(plant.Model.GetComponentInChildren<Renderer>().HasPropertyBlock(), Is.False, "An empty fern still looks wet.");
        }

        [UnityTest]
        public IEnumerator ALeftClick_TakesTheSpongeusOut_AndOnlyOneAtATime()
        {
            yield return LoadGame();
            yield return HoldSpongeus(2);
            var hints = game.Hands.Hints.Select(hint => hint.LabelKey).ToArray();
            Assert.That(hints, Does.Contain(UiText.HintRelease), "No take-out hint with the Spongeus in hand.");

            game.Hands.Behaviour.UsePrimary();
            yield return null;

            var companion = game.Companions.Active;
            Assert.That(companion, Is.Not.Null);
            Assert.That(game.Inventory.Count(SpongeusId), Is.EqualTo(1), "The Spongeus did not leave the inventory.");
            Assert.That(Flat(companion.transform.position, Player.position), Is.LessThan(2.5f), "It is not put down in front of the player.");
            Assert.That(game.Hands.Hints.Select(hint => hint.LabelKey), Does.Not.Contain(UiText.HintRelease), "The take-out hint is still shown with a Spongeus out.");

            game.Hands.Behaviour.UsePrimary();
            yield return null;
            Assert.That(game.Inventory.Count(SpongeusId), Is.EqualTo(1), "A second Spongeus was taken out.");
            Assert.That(Object.FindObjectsByType<Companion>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TheSpongeus_FollowsThePlayer_AndComesBackWhenLeftFarBehind()
        {
            yield return LoadGame();
            yield return TakeOut();
            var companion = game.Companions.Active;

            companion.transform.position = Player.position + new Vector3(10f, 0f, 0f);
            yield return Wait(2.5f);
            Assert.That(Flat(companion.transform.position, Player.position), Is.LessThan(3.2f), "It did not walk back to its place by the player.");

            companion.transform.position = Player.position + new Vector3(60f, 0f, 0f);
            yield return null;
            yield return null;
            Assert.That(Flat(companion.transform.position, Player.position), Is.LessThan(3f), "Left far behind, it did not come back at once.");
        }

        [UnityTest]
        public IEnumerator TheSpongeus_SoaksUpAFullFernNearThePlayer_ItsCapDarkens_AndASqueezeGivesWater()
        {
            yield return LoadGame();
            yield return TakeOut();
            var companion = game.Companions.Active;
            var plant = PlantInView(full: true, distance: 5f);

            yield return WaitUntil(() => companion.Brain.IsFull, 5f);
            Assert.That(companion.Brain.IsFull, Is.True, "The Spongeus did not soak up the fern near the player.");
            Assert.That(plant.IsFull, Is.False, "The fern stayed full.");
            yield return null;

            Assert.That(companion.ShownFullness, Is.EqualTo(1f));
            var cap = CapTint(companion);
            Assert.That(cap.r, Is.EqualTo(companion.Definition.FullTint.r).Within(0.01f), "The cap is not darker.");
            Assert.That(companion.HintKey, Is.EqualTo(UiText.HintSqueeze));

            Assert.That(companion.Interact(game.Inventory), Is.True);
            Assert.That(game.Inventory.Count(WaterId), Is.EqualTo(1), "Squeezing gave no water.");
            Assert.That(companion.Brain.IsFull, Is.False);
            Assert.That(companion.HintKey, Is.Null, "An empty Spongeus can still be squeezed.");
            Assert.That(companion.Interact(game.Inventory), Is.False, "An empty Spongeus was squeezed.");
            Assert.That(game.Pickups.Lines.Count, Is.GreaterThan(0), "The water was not shown as collected.");

            yield return Wait(1.3f);
            Assert.That(companion.ShownFullness, Is.Zero, "The cap did not go back to its colour.");
        }

        [UnityTest]
        public IEnumerator Aimed_TheSpongeus_ShowsTheKey_WithoutRingUntilHeld_AndTheHoldHint()
        {
            yield return LoadGame();
            yield return TakeOut();
            var companion = game.Companions.Active;
            yield return LookDown();
            var view = game.ViewCamera.transform;
            var forward = Vector3.ProjectOnPlane(view.forward, Vector3.up).normalized;
            // Close in front of the player: it stays, being within its follow distance.
            companion.transform.position = Player.position + forward * AimedDistance;
            yield return null;
            yield return null;

            Assert.That(game.Interactor.Targeted, Is.SameAs(companion));
            var prompt = game.Interactor.Prompt;
            Assert.That(prompt.IsKeyVisible, Is.True);
            Assert.That(prompt.IsHoldRingVisible, Is.False, "The ring shows before E is held.");
            Assert.That(game.Interactor.Hints.Select(hint => hint.LabelKey), Is.EqualTo(new[] { UiText.HintTakeBack }));
        }

        [UnityTest]
        public IEnumerator HoldingE_TakesItBack_SqueezedFirstWhenFull()
        {
            yield return LoadGame();
            yield return TakeOut();
            var companion = game.Companions.Active;
            PlantInView(full: true, distance: 5f);
            yield return WaitUntil(() => companion.Brain.IsFull, 5f);
            Assert.That(companion.Brain.IsFull, Is.True);

            Assert.That(companion.InteractHold(game.Inventory), Is.True);
            yield return null;

            Assert.That(game.Companions.Active, Is.Null);
            Assert.That(companion == null, Is.True, "The Spongeus is still in the world.");
            Assert.That(game.Inventory.Count(SpongeusId), Is.EqualTo(1), "The Spongeus did not come back.");
            Assert.That(game.Inventory.Count(WaterId), Is.EqualTo(1), "Its water was lost.");
        }

        [UnityTest]
        public IEnumerator Reset_RemovesTheSpongeusOut()
        {
            yield return LoadGame();
            yield return TakeOut();

            game.ResetGame();
            yield return null;

            Assert.That(game.Companions.Active, Is.Null);
            Assert.That(Object.FindObjectsByType<Companion>(FindObjectsSortMode.None), Is.Empty);
        }

        private static MaterialPropertyBlock ModelBlock(RefillingPlant plant)
        {
            var block = new MaterialPropertyBlock();
            plant.Model.GetComponentInChildren<Renderer>().GetPropertyBlock(block);
            return block;
        }

        private static Color CapTint(Companion companion)
        {
            var block = new MaterialPropertyBlock();
            foreach (var renderer in companion.GetComponentsInChildren<Renderer>())
            {
                var materials = renderer.sharedMaterials;
                for (var slot = 0; slot < materials.Length; slot++)
                {
                    if (materials[slot] == companion.Definition.FullPartMaterial)
                    {
                        renderer.GetPropertyBlock(block, slot);
                        return block.GetColor(BaseColorId);
                    }
                }
            }

            Assert.Fail("The Spongeus has no cap drawn with its full part's material.");
            return Color.white;
        }
    }
}
