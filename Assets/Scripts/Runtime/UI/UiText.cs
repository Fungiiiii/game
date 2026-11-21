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
        public const string HintStopAiming = "hint.stop_aiming";
        public const string HintSwitchItem = "hint.switch_item";
        public const string HintInventory = "hint.inventory";
        public const string HintGather = "hint.gather";
        public const string HintDeposit = "hint.deposit";
        public const string HintTakeOrPlace = "hint.take_or_place";
        public const string HintHalfOrOne = "hint.half_or_one";
        public const string HintGatherAll = "hint.gather_all";
        public const string HintClose = "hint.close";
        public const string HintDrop = "hint.drop";
    }
}
