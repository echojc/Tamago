using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class ActionRefTest : TestBase
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
                <action label=""foo"">
                  <wait>1</wait>
                  <fire><bullet/></fire>
                </action>
              </bulletml>
            ");
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ActionRef(null, DummyPattern));
        }

        [Test]
        public void ThrowsArgumentNullIfPatternIsNull()
        {
            var node = XElement.Parse(@"
              <actionRef label=""foo""/>
            ");
            Assert.Throws<ArgumentNullException>(() => new ActionRef(node, null));
        }

        [Test]
        public void ThrowsArgumentExceptionIfNodeIsNotAction()
        {
            var node = XElement.Parse(@"<foo/>");
            Assert.Throws<ArgumentException>(() => new ActionRef(node, DummyPattern));
        }

        [Test]
        public void ThrowsParseExceptionIfNoLabel()
        {
            var node = XElement.Parse(@"
              <actionRef/>
            ");
            Assert.Throws<ParseException>(() => new ActionRef(node, DummyPattern));
        }

        [Test]
        public void ParsesLabel()
        {
            var node = XElement.Parse(@"
              <actionRef label=""foo""/>
            ");

            var action = new ActionRef(node, DummyPattern);
            Assert.AreEqual("foo", action.Label);
        }

        [Test]
        public void ResolvesAndClonesAction()
        {
            var node = XElement.Parse(@"
              <actionRef label=""foo""/>
            ");

            var action = new ActionRef(node, FooPattern);
            var underlying = action.Action;
            var foo = FooPattern.CopyAction("foo");

            Assert.AreNotSame(foo, underlying);
            Assert.AreEqual(foo.Tasks.Count, underlying.Tasks.Count);
            underlying.Tasks.Zip(foo.Tasks, (a, b) =>
                {
                    // this really should be equals
                    // but i cbf implementing .Equals 
                    Assert.IsInstanceOf(a.GetType(), b);
                    Assert.AreNotSame(a, b);
                    return true;
                });
        }

        [Test]
        public void ImplementsTasks()
        {
            var node = XElement.Parse(@"
              <actionRef label=""foo""/>
            ");

            var action = new ActionRef(node, FooPattern);
            var foo = FooPattern.CopyAction("foo");

            Assert.AreEqual(foo.Tasks.Count, action.Tasks.Count);
            action.Tasks.Zip(foo.Tasks, (a, b) =>
                {
                    // this really should be equals
                    // but i cbf implementing .Equals 
                    Assert.IsInstanceOf(a.GetType(), b);
                    Assert.AreNotSame(a, b);
                    return true;
                });
        }

        [Test]
        public void ImplementsRun()
        {
            var node = XElement.Parse(@"
              <actionRef label=""foo""/>
            ");

            var action = new ActionRef(node, FooPattern);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            // <wait>1</wait>
            action.Run(TestBullet);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            // <fire/>
            action.Run(TestBullet);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            action.Run(TestBullet);
            Assert.AreEqual(2, TestManager.Bullets.Count);
        }

        [Test]
        public void ImplementsIsCompleted()
        {
            var node = XElement.Parse(@"
              <actionRef label=""foo""/>
            ");

            var action = new ActionRef(node, FooPattern);
            Assert.False(action.IsCompleted);

            // <wait>1</wait>
            action.Run(TestBullet);
            Assert.False(action.IsCompleted);

            // <fire/>
            action.Run(TestBullet);
            Assert.True(action.IsCompleted);

            action.Run(TestBullet);
            Assert.True(action.IsCompleted);
        }

        [Test]
        public void ImplementsReset()
        {
            var node = XElement.Parse(@"
              <actionRef label=""foo""/>
            ");

            var action = new ActionRef(node, FooPattern);

            action.Run(TestBullet);
            action.Run(TestBullet);
            Assert.True(action.IsCompleted);

            action.Reset();
            Assert.False(action.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            // <wait>1</wait>
            action.Run(TestBullet);
            Assert.False(action.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            // <fire/>
            action.Run(TestBullet);
            Assert.True(action.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);

            action.Run(TestBullet);
            Assert.True(action.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);
        }

        [Test]
        public void ImplementsCopy()
        {
            var node = XElement.Parse(@"
              <actionRef label=""foo"">
                <param>1+2</param>
                <param>3+4</param>
              </actionRef>
            ");

            var action1 = new ActionRef(node, FooPattern);
            var action2 = (ActionRef)action1.Copy();

            Assert.AreEqual(action1.Label, action2.Label);
            CollectionAssert.AreEqual(action1.Params, action2.Params);

            var foo = FooPattern.CopyAction("foo");
            Assert.AreEqual(foo.Tasks.Count, action2.Tasks.Count);
            action2.Tasks.Zip(foo.Tasks, (a, b) =>
                {
                    // this really should be equals
                    // but i cbf implementing .Equals 
                    Assert.IsInstanceOf(a.GetType(), b);
                    Assert.AreNotSame(a, b);
                    return true;
                });
        }

        [Test]
        public void ParsesParamsAsExpressions()
        {
            var node = XElement.Parse(@"
              <actionRef label=""foo"">
                <param>1+2</param>
                <param>3+4</param>
              </actionRef>
            ");

            var action = new ActionRef(node, DummyPattern);
            CollectionAssert.AreEqual(new[] {
                new Expression("1+2"),
                new Expression("3+4")
            }, action.Params);
        }

        [Test]
        public void EvaluatesParamsAndReplacesAllExistingValues()
        {
            var barPattern = new BulletPattern(@"
              <bulletml>
                <action label=""bar"">
                  <fire>
                    <direction type=""absolute"">$1</direction>
                    <speed>$2 + $i</speed>
                    <bullet/>
                  </fire>
                </action>
              </bulletml>
            ");

            var node = XElement.Parse(@"
              <actionRef label=""bar"">
                <param>12.34 + $i</param>
                <param>$2 + $rank + $rand</param>
              </actionRef>
            ");

            var action = new ActionRef(node, barPattern);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            var args = new[] { 1.2f, 2.3f, 3.4f };
            var rest = new Dictionary<string, float>()
            {
                { "i", 4.2f }
            };
            action.Run(TestBullet, args, rest);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var targetDir = 12.34f + rest["i"];
            var targetSpeed = args[1] + Helpers.TestManager.TestRand + Helpers.TestManager.TestRank;

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(MathHelper.ToRadians(targetDir), bullet.Direction, 0.00001f);
            Assert.AreEqual(targetSpeed, bullet.Speed, 0.00001f);
        }
    }
}
