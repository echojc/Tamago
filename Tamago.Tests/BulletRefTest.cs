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
            var foo = FooPattern.CopyBullet("foo");

            Assert.AreSame(foo, bullet.Bullet);
        }

        [Test]
        public void ImplementsActions()
        {
            var node = XElement.Parse(@"
              <bulletRef label=""foo""/>
            ");

            var bullet = new BulletRef(node, FooPattern);
            var foo = FooPattern.CopyBullet("foo");
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
            var foo = FooPattern.CopyBullet("foo");
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
            var foo = FooPattern.CopyBullet("foo");
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
            var foo = FooPattern.CopyBullet("foo");

            var bulletBullet = bullet.Create(TestBullet);
            var fooBullet = foo.Create(TestBullet);

            Assert.AreEqual(fooBullet.Speed, bulletBullet.Speed);
            Assert.AreEqual(3, bulletBullet.Speed);

            Assert.AreEqual(fooBullet.Direction, bulletBullet.Direction);
            Assert.AreEqual(MathHelper.ToRadians(150), bulletBullet.Direction);
        }

        [Test]
        public void ParsesParamsAsExpressions()
        {
            var node = XElement.Parse(@"
              <bulletRef label=""foo"">
                <param>1+2</param>
                <param>3+4</param>
              </bulletRef>
            ");

            var bullet = new BulletRef(node, DummyPattern);
            CollectionAssert.AreEqual(new[] {
                new Expression("1+2"),
                new Expression("3+4")
            }, bullet.Params);
        }

        [Test]
        public void EvaluatesParamsAndReplacesAllExistingValues()
        {
            var barPattern = new BulletPattern(@"
              <bulletml>
                <bullet label=""bar"">
                  <action>
                    <fire>
                      <direction type=""absolute"">$1</direction>
                      <speed>$2 + $i</speed>
                      <bullet/>
                    </fire>
                  </action>
                </bullet>
              </bulletml>
            ");

            var node = XElement.Parse(@"
              <bulletRef label=""bar"">
                <param>12.34 + $i</param>
                <param>$2 + $rank + $rand</param>
              </bulletRef>
            ");

            var bulletRef = new BulletRef(node, barPattern);
            var args = new[] { 1.2f, 2.3f, 3.4f };
            var rest = new Dictionary<string, float>()
            {
                { "i", 4.2f }
            };
            var bullet = bulletRef.Create(TestBullet, args, rest);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            bullet.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            var targetDir = 12.34f + rest["i"];
            var targetSpeed = args[1] + Helpers.TestManager.TestRand + Helpers.TestManager.TestRank;

            var last = TestManager.Bullets.Last();
            Assert.AreEqual(MathHelper.ToRadians(targetDir), last.Direction, 0.00001f);
            Assert.AreEqual(targetSpeed, last.Speed, 0.00001f);
        }
    }
}
