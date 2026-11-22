using System.Collections;
using System.Linq;
using Fungiiiii.Champimaison;
using Fungiiiii.Game;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Fungiiiii.Tests.PlayMode.PlaytestLoopGame;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>The seed, the mole holes and the Champimaison of the playtest loop.</summary>
    public sealed class ChampimaisonPlayModeTests
    {
        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator TheSeed_LiesAtTheFootOfTheGiantTree_AndIsPickedUp()
        {
            yield return Load();
            var game = Bootstrap;
            var seed = game.Dropped.Items.SingleOrDefault(item => item.Item == Seed(game));
            Assert.That(seed, Is.Not.Null, "The seed is not on the ground.");

            var giantTree = new Vector3(25f, 0f, -105f);
            var flat = seed.transform.position - giantTree;
            flat.y = 0f;
            Assert.That(flat.magnitude, Is.LessThan(12f), "The seed is not at the foot of the giant tree.");
            Assert.That(game.CurrentObjective, Is.EqualTo(Objective.FindSeed));
            var glow = seed.GetComponentInChildren<Light>();
            Assert.That(glow != null && glow.enabled, Is.True, "The seed does not glow.");
            Assert.That(glow.color.b, Is.GreaterThan(glow.color.g), "The seed's glow is not violet.");

            yield return Wait(1f);
            Assert.That(seed.Interact(game.Inventory), Is.True);
            yield return null;

            Assert.That(game.Inventory.Count(Seed(game).Id), Is.EqualTo(1));
            Assert.That(game.CurrentObjective, Is.EqualTo(Objective.PlantSeed));
            Assert.That(game.ObjectiveHud.ObjectiveKey, Is.EqualTo(UiText.ObjectivePlantSeed));
        }

        [UnityTest]
        public IEnumerator AMoleHole_AsksForTheSeed_InHand_ThenPlantsIt_AndTheOthersStop()
        {
            yield return Load();
            var game = Bootstrap;
            var hole = game.MoleHoles.Holes[0];

            Assert.That(game.MoleHoles.Holes.All(each => each.IsOutlined), Is.True, "A mole hole is not outlined.");
            Assert.That(hole.Prompt, Is.EqualTo(PlantingPrompt.NeedsSeed));
            Assert.That(hole.LabelKey, Is.EqualTo(UiText.LabelNeedsSeed));

            game.Inventory.AddToSlot(0, Seed(game), 1);
            Assert.That(hole.Prompt, Is.EqualTo(PlantingPrompt.HoldSeed));
            Assert.That(hole.LabelKey, Is.EqualTo(UiText.LabelHoldSeed));
            Assert.That(hole.Interact(game.Inventory), Is.False, "The seed was planted from the backpack.");

            game.Inventory.TakeFromSlot(0, 1, out _);
            PutInHand(game, Seed(game));
            Assert.That(hole.Prompt, Is.EqualTo(PlantingPrompt.Plant));
            Assert.That(hole.HintKey, Is.EqualTo(UiText.HintPlant));
            Assert.That(hole.Interact(game.Inventory), Is.True);
            yield return null;

            Assert.That(game.Hole, Is.Not.Null, "No Champimaison grew.");
            Assert.That(game.Inventory.Count(Seed(game).Id), Is.Zero, "The seed was not used.");
            foreach (var other in game.MoleHoles.Holes.Where(each => each != hole))
            {
                Assert.That(other.IsOutlined, Is.False, "Another mole hole is still outlined.");
                Assert.That(other.Prompt, Is.EqualTo(PlantingPrompt.None));
            }

            Assert.That(game.CurrentObjective, Is.EqualTo(Objective.AwakenSeed));
        }

        [UnityTest]
        public IEnumerator ThePanel_ListsTheCost_AndHoldingTheKey_DropsWhatIsMissing()
        {
            yield return Load();
            var game = Bootstrap;
            var champimaison = Plant(game);
            StandBefore(champimaison.transform.position, 3f);
            var water = champimaison.Network.Progress.Required(0).Item;
            var spore = champimaison.Network.Progress.Required(1).Item;
            game.Inventory.Add(spore, 5);
            yield return null;
            yield return null;

            var panel = Object.FindFirstObjectByType<RequirementPanel>();
            Assert.That(panel.IsVisible, Is.True, "The panel is not shown near the Champimaison.");
            Assert.That(panel.TitleKey, Is.EqualTo(UiText.PanelAwakenTitle));
            Assert.That(panel.Rows.Count(row => row.isActiveAndEnabled), Is.EqualTo(2));
            Assert.That(panel.Rows[1].ShownCarried, Is.EqualTo(5));
            Assert.That(champimaison.HoldHintKey, Is.EqualTo(UiText.HintDeposit));

            Assert.That(champimaison.InteractHold(game.Inventory), Is.True);
            yield return null;
            Assert.That(game.Inventory.Count(spore.Id), Is.EqualTo(2), "More spores were taken than missing.");
            Assert.That(panel.Rows[1].CountText, Is.EqualTo("3/3"));
            Assert.That(panel.Rows[1].IsComplete, Is.True);
            Assert.That(champimaison.HoldHintKey, Is.Null, "Nothing left to drop, yet the key says it drops.");

            game.Inventory.Add(water, 1);
            yield return null;
            Assert.That(champimaison.HoldHintKey, Is.EqualTo(UiText.HintAwaken));
            Assert.That(champimaison.InteractHold(game.Inventory), Is.True);
            yield return null;

            Assert.That(champimaison.Network.Stage, Is.EqualTo(1), "The seed did not wake up.");
            Assert.That(panel.TitleKey, Is.EqualTo(UiText.PanelGrowTitle));
            Assert.That(game.CurrentObjective, Is.EqualTo(Objective.CraftNet));
        }

        [UnityTest]
        public IEnumerator Aimed_TheChampimaison_ShowsItsKeysInItsPanel_NotOverIt()
        {
            yield return Load();
            var game = Bootstrap;
            var champimaison = Plant(game);
            game.Inventory.Add(champimaison.Network.Progress.Required(1).Item, 2);
            StandBefore(champimaison.transform.position, 2f);
            yield return WaitUntil(() => ReferenceEquals(game.Interactor.Targeted, champimaison), 2f);
            yield return null;

            Assert.That(game.Interactor.Targeted, Is.SameAs(champimaison), "The Champimaison is not aimed at from 2 m.");
            Assert.That(game.Interactor.Prompt.IsVisible, Is.False, "A key floats over the Champimaison.");
            var panel = Object.FindFirstObjectByType<RequirementPanel>();
            Assert.That(panel.HoldKey, Is.EqualTo(UiText.HintDeposit), "The panel does not show the key to hold.");
            Assert.That(panel.PressKey, Is.Empty, "A key to press is shown while a short press does nothing.");

            StandBefore(champimaison.transform.position, 7f);
            yield return null;
            yield return null;

            Assert.That(panel.IsVisible, Is.True, "The panel is hidden within its distance.");
            Assert.That(panel.HoldKey, Is.Empty, "The keys show while the Champimaison is not aimed at.");
        }

        [UnityTest]
        public IEnumerator OncePlanted_TheObjective_ListsWhatTheSeedNeeds_WithWhereToFindIt()
        {
            yield return Load();
            var game = Bootstrap;
            var hud = game.ObjectiveHud;
            Assert.That(hud.ShownNeeds, Is.Zero, "Needs show before the seed is planted.");

            var champimaison = Plant(game);
            yield return null;
            var progress = champimaison.Network.Progress;
            Assert.That(hud.ShownNeeds, Is.EqualTo(progress.LineCount), "The objective does not list what the seed needs.");
            for (var line = 0; line < progress.LineCount; line++)
            {
                var row = hud.NeedRows[line];
                Assert.That(row.CountText, Is.EqualTo($"0/{progress.Required(line).Amount}"));
                Assert.That(row.ShowsSource, Is.True, $"No hint where to find {progress.Required(line).Item.name}.");
            }

            // Carried counts too; enough, the hint goes.
            game.Inventory.Add(progress.Required(0).Item, progress.Required(0).Amount);
            yield return null;
            Assert.That(hud.NeedRows[0].CountText, Is.EqualTo($"{progress.Required(0).Amount}/{progress.Required(0).Amount}"));
            Assert.That(hud.NeedRows[0].ShowsSource, Is.False);

            // Awake, the next stage's needs show.
            game.Inventory.Add(progress.Required(1).Item, progress.Required(1).Amount);
            champimaison.InteractHold(game.Inventory);
            yield return null;
            Assert.That(hud.ShownNeeds, Is.EqualTo(champimaison.Network.Progress.LineCount));
            Assert.That(hud.NeedRows[0].CountText, Does.StartWith("0/"), "The needs did not move on to the next stage.");
        }

        [UnityTest]
        public IEnumerator AnAwakeChampimaison_ProducesThread_ThatAShortPressCollects()
        {
            yield return Load();
            var game = Bootstrap;
            var champimaison = Plant(game);
            var progress = champimaison.Network.Progress;
            game.Inventory.Add(progress.Required(0).Item, progress.Required(0).Amount);
            game.Inventory.Add(progress.Required(1).Item, progress.Required(1).Amount);
            champimaison.InteractHold(game.Inventory);

            Assert.That(champimaison.Thread.IsRunning, Is.True, "The thread is not produced once awake.");
            champimaison.Thread.Tick(1000f);
            Assert.That(champimaison.HintKey, Is.EqualTo(UiText.HintCollectThread));
            var stored = champimaison.Thread.Stored;

            // Since the playtest of 22/11, on a line of its own under the panel, pulsing.
            StandBefore(champimaison.transform.position, 5f);
            yield return null;
            yield return null;
            Assert.That(game.ThreadTag.IsShown, Is.True, "Nothing says thread waits.");
            Assert.That(game.ThreadTag.ActionKey, Is.EqualTo(UiText.HintCollectThread));
            Assert.That(game.ThreadTag.IsPulsing, Is.True, "The thread's line does not pulse.");
            Assert.That(Object.FindFirstObjectByType<RequirementPanel>().PressKey, Is.Not.EqualTo(UiText.HintCollectThread), "Collecting the thread is still in the panel.");

            Assert.That(champimaison.Interact(game.Inventory), Is.True);
            Assert.That(game.Inventory.Count(Thread(game).Id), Is.EqualTo(stored));
            yield return null;
            Assert.That(game.ThreadTag.IsShown, Is.False, "The thread's line stays once collected.");
        }

        [UnityTest]
        public IEnumerator TheLastStage_OpensTheChampimaison_AndEnteringIt_EndsTheDemo()
        {
            yield return Load();
            var game = Bootstrap;
            var champimaison = Plant(game);
            foreach (var _ in Enumerable.Range(0, champimaison.Network.StageCount))
            {
                var progress = champimaison.Network.Progress;
                for (var line = 0; line < progress.LineCount; line++)
                {
                    game.Inventory.Add(progress.Required(line).Item, progress.Required(line).Amount);
                }

                champimaison.InteractHold(game.Inventory);
            }

            Assert.That(champimaison.Network.IsComplete, Is.True);
            Assert.That(champimaison.CanBeEntered, Is.True);
            Assert.That(champimaison.HintKey, Is.EqualTo(UiText.HintEnter));
            Assert.That(game.CurrentObjective, Is.EqualTo(Objective.EnterChampimaison));

            Assert.That(champimaison.Interact(game.Inventory), Is.True);
            yield return WaitUntil(() => SceneManager.GetSceneByPath(InteriorScenePath).isLoaded, 20f);

            Assert.That(SceneManager.GetSceneByPath(InteriorScenePath).isLoaded, Is.True, "The interior was not loaded.");
            var end = Object.FindFirstObjectByType<DemoEnd>();
            Assert.That(end, Is.Not.Null, "No end of the demo inside.");
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None), "The mouse is not free at the end.");
            Assert.That(end.QuitButton, Is.Not.Null, "The end of the demo has no way out of the game.");
            Assert.That(end.QuitButton.gameObject.activeInHierarchy, Is.True);
            Assert.That(end.QuitButton, Is.Not.SameAs(end.HomeButton));
        }
    }
}
