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
            Assert.That(game.CurrentObjective, Is.EqualTo(Objective.GrowChampimaison), "Awake, the objective is not to grow the Champimaison.");
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
        }

        [UnityTest]
        public IEnumerator ThePanel_ShowsOnlyWhereItsKeysWork_AndReadingItAimsAtTheChampimaison()
        {
            // Since the playtest of 23/11: the panel showed from 8 m, its keys worked from 3.
            yield return Load();
            var game = Bootstrap;
            var champimaison = Plant(game);
            game.Inventory.Add(champimaison.Network.Progress.Required(1).Item, 2);
            var look = game.ViewCamera.GetComponent<Fungiiiii.Runtime.Player.FirstPersonCamera>();
            var panel = Object.FindFirstObjectByType<RequirementPanel>();

            StandBefore(champimaison.transform.position, 4.5f);
            look.LookAt(champimaison.PanelPoint + Vector3.up * 0.3f);
            yield return null;
            yield return null;
            Assert.That(panel.IsVisible, Is.True, "The panel is hidden within reach.");
            Assert.That(game.Interactor.Targeted, Is.SameAs(champimaison), "Reading the panel from 4.5 m does not aim at the Champimaison.");
            Assert.That(panel.HoldKey, Is.EqualTo(UiText.HintDeposit), "The panel read from 4.5 m has no key.");

            StandBefore(champimaison.transform.position, 6.5f);
            look.LookAt(champimaison.PanelPoint);
            yield return null;
            yield return null;
            Assert.That(panel.IsVisible, Is.False, "The panel shows from where its keys do nothing.");
            Assert.That(game.Interactor.Targeted, Is.Not.SameAs(champimaison), "The Champimaison is used from 6.5 m.");
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
        public IEnumerator OnceGrownOnce_TheObjective_SuggestsCatchingASpongeusWithTheNet_UntilOneIsCaught()
        {
            yield return Load();
            var game = Bootstrap;
            var hud = game.ObjectiveHud;
            var champimaison = Plant(game);
            yield return null;
            Assert.That(hud.SuggestsCapture, Is.False, "The net is suggested before the seed is awake.");

            var progress = champimaison.Network.Progress;
            game.Inventory.Add(progress.Required(0).Item, progress.Required(0).Amount);
            game.Inventory.Add(progress.Required(1).Item, progress.Required(1).Amount);
            champimaison.InteractHold(game.Inventory);
            yield return null;
            Assert.That(hud.SuggestsCapture, Is.True, "Nothing says the net catches a Spongeus, for water.");
            Assert.That(hud.CaptureRow.TextKey, Is.EqualTo(UiText.ObjectiveOptionalCapture));
            Assert.That(hud.CaptureRow.SourceKey, Is.EqualTo(UiText.ObjectiveOptionalCaptureHint), "The line does not say it is for water.");
            Assert.That(hud.CaptureRow.CountText, Is.Empty, "The optional line has a count.");
            Assert.That(hud.CaptureRow.transform.GetSiblingIndex(), Is.Zero, "The optional line does not come first.");

            // Caught, then put down: the line is gone for good.
            var spongeus = game.Companions.HeldBehaviours.First().Item;
            game.Inventory.Add(spongeus, 1);
            yield return null;
            Assert.That(hud.SuggestsCapture, Is.False, "The net is still suggested once a Spongeus is caught.");
            game.Inventory.Remove(spongeus.Id, 1);
            yield return null;
            Assert.That(hud.SuggestsCapture, Is.False, "The suggestion came back once the Spongeus left the inventory.");
        }

        [UnityTest]
        public IEnumerator OnceAwake_ThePanel_ShowsTheThreadComing_WithItsKeyGreyedUntilOneWaits()
        {
            // Since the playtest of 23/11: when the next thread comes, and that E collects it.
            yield return Load();
            var game = Bootstrap;
            var champimaison = Plant(game);
            var panel = Object.FindFirstObjectByType<RequirementPanel>();
            StandBefore(champimaison.transform.position, 3f);
            game.ViewCamera.GetComponent<Fungiiiii.Runtime.Player.FirstPersonCamera>().LookAt(champimaison.PanelPoint + Vector3.up * 0.3f);
            yield return null;
            yield return null;
            Assert.That(panel.IsVisible, Is.True);
            Assert.That(panel.ShowsProduction, Is.False, "The thread shows before the seed is awake.");

            var progress = champimaison.Network.Progress;
            game.Inventory.Add(progress.Required(0).Item, progress.Required(0).Amount);
            game.Inventory.Add(progress.Required(1).Item, progress.Required(1).Amount);
            champimaison.InteractHold(game.Inventory);
            champimaison.Thread.Tick(5f);
            yield return null;
            var row = panel.Production;
            Assert.That(panel.ShowsProduction, Is.True, "Nothing says thread is produced.");
            Assert.That(row.CanCollect, Is.False, "The key is lit with no thread to collect.");
            Assert.That(row.CountText, Is.EqualTo($"0/{champimaison.Thread.MaxStored}"));
            Assert.That(row.ShowsBar, Is.True);
            Assert.That(row.Progress, Is.GreaterThan(0f), "The bar to the next thread is empty.");
            Assert.That(row.NextKey, Is.EqualTo(UiText.PanelThreadNext));

            champimaison.Thread.Tick(1000f);
            yield return null;
            Assert.That(row.CanCollect, Is.True, "The key stays greyed with thread waiting.");
            Assert.That(row.CountText, Is.EqualTo($"{champimaison.Thread.MaxStored}/{champimaison.Thread.MaxStored}"));
            Assert.That(row.ShowsBar, Is.False, "A full stock still shows a bar.");
            Assert.That(row.NextKey, Is.EqualTo(UiText.PanelThreadFull));
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

            // Since the playtest of 23/11, its key is on its production line, in the panel.
            StandBefore(champimaison.transform.position, 4f);
            yield return null;
            yield return null;
            var panel = Object.FindFirstObjectByType<RequirementPanel>();
            Assert.That(panel.ShowsProduction, Is.True, "Nothing says thread waits.");
            Assert.That(panel.Production.CanCollect, Is.True, "The key to collect the thread is greyed.");
            Assert.That(panel.PressKey, Is.Not.EqualTo(UiText.HintCollectThread), "Collecting the thread is also in the panel's foot.");

            Assert.That(champimaison.Interact(game.Inventory), Is.True);
            Assert.That(game.Inventory.Count(Thread(game).Id), Is.EqualTo(stored));
            yield return null;
            Assert.That(panel.Production.CanCollect, Is.False, "The key stays lit once the thread is collected.");
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
