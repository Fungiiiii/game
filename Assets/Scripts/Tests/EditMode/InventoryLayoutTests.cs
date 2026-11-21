using System;
using Fungiiiii.Inventory;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class InventoryLayoutTests
    {
        [Test]
        public void TheQuickBar_ComesAfterTheBag()
        {
            var layout = new InventoryLayout(12, 3);

            Assert.That(layout.SlotCount, Is.EqualTo(15));
            Assert.That(layout.QuickBarFirstSlot, Is.EqualTo(12));
            Assert.That(layout.QuickBarSlot(2), Is.EqualTo(14));
        }

        [Test]
        public void ASlot_IsInTheQuickBar_OrInTheBag()
        {
            var layout = new InventoryLayout(12, 3);

            Assert.That(layout.IsInQuickBar(11), Is.False);
            Assert.That(layout.IsInQuickBar(12), Is.True);
            Assert.That(layout.IsInQuickBar(14), Is.True);
            Assert.That(layout.IsInQuickBar(15), Is.False);
        }

        [Test]
        public void TheStandardLayout_IsTheArtboards()
        {
            Assert.That(InventoryLayout.Standard, Is.EqualTo(new InventoryLayout(12, 3)));
        }

        [Test]
        public void AQuickBarWithoutSlot_OrASlotPastIt_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new InventoryLayout(12, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new InventoryLayout(-1, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new InventoryLayout(12, 3).QuickBarSlot(3));
        }
    }
}
