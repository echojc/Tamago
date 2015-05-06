using NUnit.Framework;
using System;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class VanishTest : TestBase
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
            Assert.Throws<ArgumentNullException>(() => new Vanish(null));
        }

        [Test]
        public void ThrowsArgumentNullIfBulletToRunAgainstIsNull()
        {
            var node = XElement.Parse(@"
              <vanish/>
            ");

            var vanish = new Vanish(node);
            Assert.Throws<ArgumentNullException>(() => vanish.Run(null));
        }

        [Test]
        public void VanishesTheBulletItIsRunAgainst()
        {
            var node = XElement.Parse(@"
              <vanish/>
            ");

            var vanish = new Vanish(node);
            Assert.False(TestBullet.IsVanished);

            vanish.Run(TestBullet);
            Assert.True(TestBullet.IsVanished);
        }

        [Test]
        public void CompletesAfterRunning()
        {
            var node = XElement.Parse(@"
              <vanish/>
            ");

            var vanish = new Vanish(node);
            Assert.False(vanish.IsCompleted);

            Assert.True(vanish.Run(TestBullet));
            Assert.True(vanish.IsCompleted);
        }
    }
}
