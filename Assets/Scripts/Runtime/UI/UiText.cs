namespace Fungiiiii.UI
{
    /// <summary>
    /// Keys of the texts the menus show. The texts themselves live in the "UI" string
    /// table of the Localization package, one column per language: nothing the player
    /// reads is written in code.
    /// </summary>
    internal static class UiText
    {
        public const string Table = "UI";

        public const string GameTitle = "game.title";
        public const string MenuPlay = "menu.play";
        public const string MenuQuit = "menu.quit";
        public const string PauseResume = "pause.resume";
        public const string PauseHome = "pause.home";
        public const string PauseQuit = "pause.quit";
        public const string InventoryTitle = "inventory.title";
        public const string InventoryHotbar = "inventory.hotbar";

        // Key hints: what a control does, shown after it, bottom right of the screen.
        public const string HintThrowNet = "hint.throw_net";
        public const string HintAim = "hint.aim";
        public const string HintThrow = "hint.throw";
        public const string HintSwitchItem = "hint.switch_item";
        public const string HintInventory = "hint.inventory";
        public const string HintGather = "hint.gather";
        public const string HintDeposit = "hint.deposit";
        public const string HintTakeOrPlace = "hint.take_or_place";
        public const string HintHalfOrOne = "hint.half_or_one";
        public const string HintGatherAll = "hint.gather_all";
        public const string HintClose = "hint.close";
        public const string HintDrop = "hint.drop";

        // Companions: taken out of the quick bar, squeezed, taken back by holding E.
        public const string HintRelease = "hint.release";
        public const string HintSqueeze = "hint.squeeze";
        public const string HintSneak = "hint.sneak";
        public const string HintTakeBack = "hint.take_back";

        // The Champimaison: woken up, grown, its thread collected, entered.
        public const string HintAwaken = "hint.awaken";
        public const string HintUpgrade = "hint.upgrade";
        public const string HintCollectThread = "hint.collect_thread";
        public const string HintEnter = "hint.enter";

        // Mole holes: where the seed is planted.
        public const string HintPlant = "hint.plant";
        public const string LabelNeedsSeed = "label.needs_seed";
        public const string LabelHoldSeed = "label.hold_seed";

        // The panel over the Champimaison: what its next stage costs.
        public const string PanelAwakenTitle = "panel.awaken_title";
        public const string PanelGrowTitle = "panel.grow_title";
        public const string PanelInBackpack = "panel.in_backpack";
        public const string PanelHold = "panel.hold";
        public const string PanelThreadNext = "panel.thread_next";
        public const string PanelThreadFull = "panel.thread_full";
        public const string PickupLine = "pickup.line";

        /// <summary>Shown after a first press during the opening shot: a second one skips it.</summary>
        public const string IntroSkipAgain = "intro.skip_again";

        // The crafting panel, next to the inventory.
        public const string CraftTitle = "craft.title";
        public const string CraftPossible = "craft.possible";
        public const string CraftMissing = "craft.missing";
        public const string CraftChoose = "craft.choose";
        public const string CraftTracked = "craft.tracked";
        public const string HintCraft = "hint.craft";
        public const string HintTrack = "hint.track";
        public const string HintUntrack = "hint.untrack";

        // The objective, top right, and the recipes tracked under it.
        public const string ObjectiveTitle = "objective.title";
        public const string ObjectiveFindSeed = "objective.find_seed";
        public const string ObjectivePlantSeed = "objective.plant_seed";
        public const string ObjectiveAwakenSeed = "objective.awaken_seed";
        public const string ObjectiveGrow = "objective.grow";
        public const string ObjectiveEnter = "objective.enter";
        public const string ObjectiveOptionalCapture = "objective.optional_capture";
        public const string ObjectiveOptionalCaptureHint = "objective.optional_capture.hint";

        // The slingshot in hand.
        public const string HintShoot = "hint.shoot";
        public const string HintNoPebble = "hint.no_pebble";

        // The end of the demo, inside the Champimaison.
        public const string DemoEndTitle = "demo_end.title";
        public const string DemoEndThanks = "demo_end.thanks";
    }
}
