namespace Fungiiiii.Items
{
    /// <summary>
    /// What an item does while the player holds it: the capture net today, a tool or a
    /// food tomorrow. <see cref="PlayerHands"/> equips the item of the selected quick bar
    /// slot and hands it the use controls; the item never reads a control itself.
    /// </summary>
    public interface IHeldItem
    {
        /// <summary>Taken in hand: shown, ready to be used.</summary>
        void Equip();

        /// <summary>Put away: hidden; what it was doing with the controls stops.</summary>
        void Unequip();

        /// <summary>The primary use control was pressed: left click, right trigger.</summary>
        void UsePrimary();

        /// <summary>Called every frame: whether the secondary use control is held, right click or left trigger.</summary>
        void HoldSecondary(bool held);

        /// <summary>A menu took the controls: stop what the controls were doing, at once.</summary>
        void Cancel();
    }
}
