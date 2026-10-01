using System;
using NUnit.Framework;

namespace SweetBazaar.Core.Tests
{
    public class BoxTests
    {
        [Test]
        public void NewBox_IsEmpty()
        {
            var box = new Box(4);

            Assert.IsTrue(box.IsEmpty);
            Assert.IsFalse(box.IsFull);
            Assert.IsFalse(box.IsClosed);
            Assert.AreEqual(Box.NoCandy, box.Top);
            Assert.AreEqual(0, box.TopRunLength);
            Assert.AreEqual(4, box.FreeSlots);
        }

        [Test]
        public void TopRunLength_CountsIdenticalCandiesOnTop()
        {
            var box = new Box(4, new[] { 0, 1, 1 });

            Assert.AreEqual(1, box.Top);
            Assert.AreEqual(2, box.TopRunLength);
        }

        [Test]
        public void TopRunLength_CoversWholeBoxWhenAllIdentical()
        {
            var box = new Box(4, new[] { 2, 2, 2 });

            Assert.AreEqual(3, box.TopRunLength);
        }

        [Test]
        public void FullBoxOfOneType_IsClosed()
        {
            var box = new Box(4, new[] { 3, 3, 3, 3 });

            Assert.IsTrue(box.IsFull);
            Assert.IsTrue(box.IsClosed);
        }

        [Test]
        public void FullMixedBox_IsNotClosed()
        {
            var box = new Box(4, new[] { 3, 3, 3, 1 });

            Assert.IsTrue(box.IsFull);
            Assert.IsFalse(box.IsClosed);
        }

        [Test]
        public void PartlyFilledBoxOfOneType_IsNotClosed()
        {
            var box = new Box(4, new[] { 3, 3, 3 });

            Assert.IsFalse(box.IsClosed);
        }

        [Test]
        public void Constructor_RejectsMoreCandiesThanCapacity()
        {
            Assert.Throws<ArgumentException>(() => new Box(2, new[] { 0, 0, 0 }));
        }

        [Test]
        public void Constructor_RejectsNegativeCandyId()
        {
            Assert.Throws<ArgumentException>(() => new Box(4, new[] { 0, -1 }));
        }

        [Test]
        public void Constructor_RejectsCapacityBelowOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Box(0));
        }
    }
}
