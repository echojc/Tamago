using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class BulletRefTest : TestBase
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
                <bullet label=""foo"">
                  <speed>3</speed>
                  <direction type=""absolute"">150</direction>
                  <action>
                    <wait>1</wait>
                    <fire><bullet/></fire>
                  </action>
                </bullet>
              </bulletml>
            ");
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BulletRef(null, DummyPattern));
        }

        [Test]
        public void ThrowsArgumentNullIfPatternIsNull()
        {
            var node = XElement.Parse(@"
              <bulletRef label=""foo""/>
            ");
            Assert.Throws<ArgumentNullException>(() => new BulletRef(node, null));
        }

        [Test]
        public void ThrowsArgumentExceptionIfNodeIsNotBulletRef()
        {
            var node = XElement.Parse(@"<foo/>");
            Assert.Throws<ArgumentException>(() => new BulletRef(node, DummyPattern));
        }

        [Test]
        public void ThrowsParseExceptionIfNoLabel()
        {
            var node = XElement.Parse(@"
              <bulletRef/>
            ");
            Assert.Throws<ParseException>(() => new BulletRef(node, DummyPattern));
        }

        [Test]
        public void ParsesLabel()
        {
            var node = XElement.Parse(@"
              <bulletRef label=""foo""/>
            ");

            var bullet = new BulletRef(node, DummyPattern);
            Assert.AreEqual("foo", bullet.Label);
        }

        [Test]
        public void ResolvesBullet()
        {
            var node = XElement.Parse(@"
              <bulletRef label=""foo""/>
            ");

            var bullet = new BulletRef(node, FooPattern);
            var foo = FooPattern.FindBullet("foo");

            Assert.AreSame(foo, bullet.Bullet);
        }

        [Test]
        public void ImplementsActions()
        {
            var node = XElement.Parse(@"
              <bulletRef label=""foo""/>
            ");

            var bullet = new BulletRef(node, FooPattern);
            var foo = FooPattern.FindBullet("foo");
            Assert.AreEqual(1, bullet.Actions.Count);
            Assert.AreEqual(1, foo.Actions.Count);

            var bulletActions = bullet.Actions[0];
            var fooActions = foo.Actions[0];
            Assert.AreEqual(fooActions.Tasks.Count, bulletActions.Tasks.Count);
            bulletActions.Tasks.Zip(fooActions.Tasks, (a, b) =>
                {
                    // this really should be equals
                    // but i cbf implementing .Equals 
                    Assert.IsInstanceOf(a.GetType(), b);
                    Assert.AreNotSame(a, b);
                    return true;
                });
        }

        [Test]
        public void ImplementsSpeed()
        {
            var node = XElement.Parse(@"
              <bulletRef label=""foo""/>
            ");

            var bullet = new BulletRef(node, FooPattern);
            var foo = FooPattern.FindBullet("foo");
            Assert.AreEqual(foo.Speed, bullet.Speed);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 3), bullet.Speed);
        }

        [Test]
        public void ImplementsDirection()
        {
            var node = XElement.Parse(@"
              <bulletRef label=""foo""/>
            ");

            var bullet = new BulletRef(node, FooPattern);
            var foo = FooPattern.FindBullet("foo");
            Assert.AreEqual(foo.Direction, bullet.Direction);
            Assert.AreEqual(new Direction(DirectionType.Absolute, 150), bullet.Direction);
        }

        [Test]
        public void ImplementsCreate()
        {
            var node = XElement.Parse(@"
              <bulletRef label=""foo""/>
            ");

            var bullet = new BulletRef(node, FooPattern);
            var foo = FooPattern.FindBullet("foo");

            var bulletBullet = bullet.Create(TestBullet, EmptyArray);
            var fooBullet = foo.Create(TestBullet, EmptyArray);

            Assert.AreEqual(fooBullet.Speed, bulletBullet.Speed);
            Assert.AreEqual(3, bulletBullet.Speed);

            Assert.AreEqual(fooBullet.Direction, bulletBullet.Direction);
            Assert.AreEqual(MathHelper.ToRadians(150), bulletBullet.Direction);
        }

        [Test]
        [Ignore]
        public void InjectsParams()
        {
        }
    }
}
