using NUnit.Framework;
using System;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class WaitTest : TestBase
    {
        internal Bullet TestBullet;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TestBullet = TestManager.CreateBullet();
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Wait(null));
        }

        [Test]
        public void ThrowsParseExceptionIfNoWaitTime()
        {
            var node = XElement.Parse(@"
              <wait/>
            ");

            Assert.Throws<ParseException>(() => new Wait(node));
        }

        [Test]
        public void ThrowsArgumentNullIfBulletToRunAgainstIsNull()
        {
            var node = XElement.Parse(@"
              <wait>1</wait>
            ");

            var wait = new Wait(node);
            Assert.Throws<ArgumentNullException>(() => wait.Run(null));
        }

        [Test]
        public void ParsesWaitDuration()
        {
            var node = XElement.Parse(@"
              <wait>42</wait>
            ");

            var wait = new Wait(node);
            Assert.AreEqual(42, wait.Duration);
        }

        [Test]
        public void RoundsFractionalDurationsDown()
        {
            var node = XElement.Parse(@"
              <wait>42.9999</wait>
            ");

            var wait = new Wait(node);
            Assert.AreEqual(42, wait.Duration);
        }

        [Test]
        public void CompletesAfterExecutingRunXPlusOneTimesABA()
        {
            var node = XElement.Parse(@"
              <wait>2</wait>
            ");

            var wait = new Wait(node);
            Assert.False(wait.IsCompleted);

            Assert.False(wait.Run(TestBullet));
            Assert.False(wait.IsCompleted);

            Assert.False(wait.Run(TestBullet));
            Assert.False(wait.IsCompleted);

            Assert.False(wait.Run(TestBullet));
            Assert.True(wait.IsCompleted);

            Assert.True(wait.Run(TestBullet));
        }
    }
}
