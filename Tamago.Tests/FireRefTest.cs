using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class FireRefTest : TestBase
    {
        internal Bullet TestBullet;
        internal BulletPattern FooPattern;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TestBullet = TestManager.CreateBullet();
            TestBullet.SetPattern(ActionDef.Default, isTopLevel: false);

            FooPattern = new BulletPattern(@"
              <bulletml>
                <action label=""top""/>
                <fire label=""foo"">
                  <speed>2</speed>
                  <direction type=""absolute"">170</direction>
                  <bullet/>
                </fire>
              </bulletml>
            ");
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FireRef(null, DummyPattern));
        }

        [Test]
        public void ThrowsArgumentNullIfPatternIsNull()
        {
            var node = XElement.Parse(@"
              <fireRef label=""foo""/>
            ");
            Assert.Throws<ArgumentNullException>(() => new FireRef(node, null));
        }

        [Test]
        public void ThrowsArgumentExceptionIfNodeIsNotFireRef()
        {
            var node = XElement.Parse(@"<foo/>");
            Assert.Throws<ArgumentException>(() => new FireRef(node, DummyPattern));
        }

        [Test]
        public void ThrowsParseExceptionIfNoLabel()
        {
            var node = XElement.Parse(@"
              <fireRef/>
            ");
            Assert.Throws<ParseException>(() => new FireRef(node, DummyPattern));
        }
        
        [Test]
        public void ParsesLabel()
        {
            var node = XElement.Parse(@"
              <fireRef label=""foo""/>
            ");

            var fire = new FireRef(node, DummyPattern);
            Assert.AreEqual("foo", fire.Label);
        }

        [Test]
        public void ResolvesAndClonesFire()
        {
            var node = XElement.Parse(@"
              <fireRef label=""foo""/>
            ");

            var fire = new FireRef(node, FooPattern);
            var foo = FooPattern.FindFire("foo");

            Assert.AreNotSame(foo, fire);

            Assert.False(foo.IsCompleted);
            Assert.False(fire.IsCompleted);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            fire.Run(TestBullet);
            Assert.False(foo.IsCompleted);
            Assert.True(fire.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            fire.Run(TestBullet);
            Assert.False(foo.IsCompleted);
            Assert.True(fire.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            foo.Run(TestBullet);
            Assert.True(foo.IsCompleted);
            Assert.True(fire.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);

            foo.Run(TestBullet);
            Assert.True(foo.IsCompleted);
            Assert.True(fire.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);
        }

        [Test]
        public void ImplementsBulletRef()
        {
            var node = XElement.Parse(@"
              <fireRef label=""foo""/>
            ");

            var fire = new FireRef(node, FooPattern);
            var foo = FooPattern.FindFire("foo");
            Assert.AreSame(foo.BulletRef, fire.BulletRef);
        }

        [Test]
        public void ImplementsSpeed()
        {
            var node = XElement.Parse(@"
              <fireRef label=""foo""/>
            ");

            var fire = new FireRef(node, FooPattern);
            var foo = FooPattern.FindFire("foo");
            Assert.AreEqual(foo.Speed, fire.Speed);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 2), fire.Speed);
        }

        [Test]
        public void ImplementsDirection()
        {
            var node = XElement.Parse(@"
              <fireRef label=""foo""/>
            ");

            var fire = new FireRef(node, FooPattern);
            var foo = FooPattern.FindFire("foo");
            Assert.AreEqual(foo.Direction, fire.Direction);
            Assert.AreEqual(new Direction(DirectionType.Absolute, 170), fire.Direction);
        }

        [Test]
        public void ImplementsRun()
        {
            var node = XElement.Parse(@"
              <fireRef label=""foo""/>
            ");

            var fire = new FireRef(node, FooPattern);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            fire.Run(TestBullet);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(2, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(170), bullet.Direction);
        }

        [Test]
        public void ImplementsIsCompleted()
        {
            var node = XElement.Parse(@"
              <fireRef label=""foo""/>
            ");

            var fire = new FireRef(node, FooPattern);
            Assert.False(fire.IsCompleted);

            fire.Run(TestBullet);
            Assert.True(fire.IsCompleted);
        }

        [Test]
        public void ImplementsReset()
        {
            var node = XElement.Parse(@"
              <fireRef label=""foo""/>
            ");

            var fire = new FireRef(node, FooPattern);
            Assert.False(fire.IsCompleted);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            fire.Run(TestBullet);
            Assert.True(fire.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            fire.Run(TestBullet);
            Assert.True(fire.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            fire.Reset();
            Assert.False(fire.IsCompleted);

            fire.Run(TestBullet);
            Assert.True(fire.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);

            fire.Run(TestBullet);
            Assert.True(fire.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);
        }

        [Test]
        public void ImplementsCopy()
        {
            var node = XElement.Parse(@"
              <fireRef label=""foo""/>
            ");

            var fire1 = new FireRef(node, FooPattern);
            var fire2 = (FireRef)fire1.Copy();

            Assert.AreEqual(fire2.Speed, fire1.Speed);
            Assert.AreEqual(fire2.Direction, fire1.Direction);
            Assert.AreEqual(fire2.BulletRef, fire1.BulletRef);
            Assert.AreEqual(fire2.Label, fire1.Label);

            Assert.False(fire1.IsCompleted);
            Assert.False(fire2.IsCompleted);

            fire1.Run(TestBullet);
            Assert.True(fire1.IsCompleted);
            Assert.False(fire2.IsCompleted);

            fire2.Run(TestBullet);
            Assert.True(fire1.IsCompleted);
            Assert.True(fire2.IsCompleted);
        }
    }
}
