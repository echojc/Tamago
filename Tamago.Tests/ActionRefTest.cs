using NUnit.Framework;
using System;
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
            var foo = FooPattern.FindAction("foo");

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
            var foo = FooPattern.FindAction("foo");

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
            action.Run(TestBullet, EmptyArray);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            // <fire/>
            action.Run(TestBullet, EmptyArray);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            action.Run(TestBullet, EmptyArray);
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
            action.Run(TestBullet, EmptyArray);
            Assert.False(action.IsCompleted);

            // <fire/>
            action.Run(TestBullet, EmptyArray);
            Assert.True(action.IsCompleted);

            action.Run(TestBullet, EmptyArray);
            Assert.True(action.IsCompleted);
        }

        [Test]
        public void ImplementsReset()
        {
            var node = XElement.Parse(@"
              <actionRef label=""foo""/>
            ");

            var action = new ActionRef(node, FooPattern);

            action.Run(TestBullet, EmptyArray);
            action.Run(TestBullet, EmptyArray);
            Assert.True(action.IsCompleted);

            action.Reset();
            Assert.False(action.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            // <wait>1</wait>
            action.Run(TestBullet, EmptyArray);
            Assert.False(action.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            // <fire/>
            action.Run(TestBullet, EmptyArray);
            Assert.True(action.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);

            action.Run(TestBullet, EmptyArray);
            Assert.True(action.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);
        }

        [Test]
        public void ImplementsCopy()
        {
            var node = XElement.Parse(@"
              <actionRef label=""foo""/>
            ");

            var action1 = new ActionRef(node, FooPattern);
            var action2 = (ActionRef)action1.Copy();

            Assert.AreEqual(action1.Label, action2.Label);

            var foo = FooPattern.FindAction("foo");
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
        [Ignore]
        public void InjectsParams()
        {
        }
    }
}
