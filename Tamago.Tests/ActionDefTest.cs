using NUnit.Framework;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class ActionDefTest : TestBase
    {
        internal Bullet TestBullet;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TestBullet = TestManager.CreateBullet();
            TestBullet.SetPattern(ActionDef.Default, isTopLevel: false);
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ActionDef(null, DummyPattern));
        }

        [Test]
        public void ThrowsArgumentNullIfPatternIsNull()
        {
            var node = XElement.Parse(@"
              <action/>
            ");
            Assert.Throws<ArgumentNullException>(() => new ActionDef(node, null));
        }

        [Test]
        public void ThrowsArgumentExceptionIfNodeIsNotAction()
        {
            var node = XElement.Parse(@"<foo/>");
            Assert.Throws<ArgumentException>(() => new ActionDef(node, DummyPattern));
        }

        [Test]
        public void ThrowsArgumentNullIfBulletToRunAgainstIsNull()
        {
            var node = XElement.Parse(@"
              <action/>
            ");

            var action = new ActionDef(node, DummyPattern);
            Assert.Throws<ArgumentNullException>(() => action.Run(null));
        }

        [Test]
        public void ParsesLabel()
        {
            var node = XElement.Parse(@"
              <action label=""my label &amp;""/>
            ");

            var action = new ActionDef(node, DummyPattern);
            Assert.AreEqual("my label &", action.Label);
        }

        [Test]
        public void AnEmptyNodeIsCompleted()
        {
            var node = XElement.Parse(@"
              <action/>
            ");

            var action = new ActionDef(node, DummyPattern);
            Assert.True(action.IsCompleted);
        }

        [Test]
        public void CanNestFire()
        {
            var node = XElement.Parse(@"
              <action>
                <fire>
                  <bullet/>
                </fire>
              </action>
            ");

            var action = new ActionDef(node, DummyPattern);
            Assert.False(action.IsCompleted);
            Assert.AreEqual(1, action.Tasks.Count);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);
        }

        [Test]
        public void CanNestWait()
        {
            var node = XElement.Parse(@"
              <action>
                <wait>2</wait>
              </action>
            ");

            var action = new ActionDef(node, DummyPattern);
            Assert.False(action.IsCompleted);
            Assert.AreEqual(1, action.Tasks.Count);

            Assert.False(action.Run(TestBullet));
            Assert.False(action.IsCompleted);

            Assert.False(action.Run(TestBullet));
            Assert.True(action.IsCompleted);

            Assert.True(action.Run(TestBullet));
        }

        [Test]
        public void CanNestVanish()
        {
            var node = XElement.Parse(@"
              <action>
                <vanish/>
              </action>
            ");

            var action = new ActionDef(node, DummyPattern);
            Assert.False(action.IsCompleted);
            Assert.AreEqual(1, action.Tasks.Count);
            Assert.False(TestBullet.IsVanished);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            Assert.True(TestBullet.IsVanished);
        }

        [Test]
        public void CanNestChangeSpeed()
        {
            var node = XElement.Parse(@"
              <action>
                <changeSpeed>
                  <speed>2</speed>
                  <term>1</term>
                </changeSpeed>
              </action>
            ");

            var action = new ActionDef(node, DummyPattern);
            Assert.False(action.IsCompleted);
            Assert.AreEqual(1, action.Tasks.Count);
            Assert.AreEqual(0, TestBullet.Speed);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            TestBullet.Update();
            Assert.AreEqual(2, TestBullet.Speed);
        }

        [Test]
        public void CanNestChangeDirection()
        {
            var node = XElement.Parse(@"
              <action>
                <changeDirection>
                  <direction>0</direction>
                  <term>1</term>
                </changeDirection>
              </action>
            ");

            var action = new ActionDef(node, DummyPattern);
            Assert.False(action.IsCompleted);
            Assert.AreEqual(1, action.Tasks.Count);
            Assert.AreEqual(0, TestBullet.Direction);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            TestBullet.Update();
            Assert.AreEqual(TestBullet.AimDirection, TestBullet.Direction);
        }

        [Test]
        public void CanNestAccel()
        {
            var node = XElement.Parse(@"
              <action>
                <accel>
                  <horizontal>1</horizontal>
                  <vertical>2</vertical>
                  <term>1</term>
                </accel>
              </action>
            ");

            var action = new ActionDef(node, DummyPattern);
            Assert.False(action.IsCompleted);
            Assert.AreEqual(1, action.Tasks.Count);
            Assert.AreEqual(0, TestBullet.VelocityX);
            Assert.AreEqual(0, TestBullet.VelocityY);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            TestBullet.Update();
            Assert.AreEqual(1, TestBullet.VelocityX);
            Assert.AreEqual(2, TestBullet.VelocityY);
        }

        [Test]
        public void CanNestRepeat()
        {
            var node = XElement.Parse(@"
              <action>
                <repeat>
                  <times>2</times>
                  <action>
                    <fire>
                      <bullet/>
                    </fire>
                  </action>
                </repeat>
              </action>
            ");

            var action = new ActionDef(node, DummyPattern);
            Assert.False(action.IsCompleted);
            Assert.AreEqual(1, action.Tasks.Count);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);
        }

        [Test]
        public void ObeysWaits()
        {
            var node = XElement.Parse(@"
              <action>
                <!-- frame 1 -->
                <fire><bullet/></fire>
                <wait>1</wait>
                <!-- frame 2 -->
                <fire><bullet/></fire>
              </action>
            ");

            var action = new ActionDef(node, DummyPattern);
            Assert.False(action.IsCompleted);
            Assert.AreEqual(3, action.Tasks.Count);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            Assert.False(action.Run(TestBullet));
            Assert.False(action.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);
        }

        [Test]
        public void Clones()
        {
            var node = XElement.Parse(@"
              <action label=""abc"">
                <fire><bullet/></fire>
                <wait>2</wait>
              </action>
            ");

            var action1 = new ActionDef(node, DummyPattern);
            var action2 = (ActionDef)action1.Copy();
            Assert.AreNotSame(action1, action2);

            Assert.AreEqual("abc", action2.Label);
            action1.Tasks.Zip(action2.Tasks, (a, b) =>
                {
                    // this really should be equals
                    // but i cbf implementing .Equals 
                    Assert.IsInstanceOf(a.GetType(), b);
                    Assert.AreNotSame(a, b);
                    return true;
                });
        }

        [Test]
        public void CanNestAction()
        {
            var node = XElement.Parse(@"
              <action>
                <action>
                  <fire><bullet/></fire>
                  <wait>1</wait>
                  <fire><bullet/></fire>
                </action>
              </action>
            ");

            var action = new ActionDef(node, DummyPattern);
            Assert.False(action.IsCompleted);
            Assert.AreEqual(1, action.Tasks.Count);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            Assert.False(action.Run(TestBullet));
            Assert.False(action.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);
        }

        [Test]
        public void CanNestActionRef()
        {
            var fooPattern = new BulletPattern(@"
              <bulletml>
                <action label=""top""/>
                <action label=""foo"">
                  <fire><bullet/></fire>
                  <wait>1</wait>
                  <fire><bullet/></fire>
                </action>
              </bulletml>
            ");

            var node = XElement.Parse(@"
              <action>
                <actionRef label=""foo""/>
              </action>
            ");

            var action = new ActionDef(node, fooPattern);
            Assert.False(action.IsCompleted);
            Assert.AreEqual(1, action.Tasks.Count);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            Assert.False(action.Run(TestBullet));
            Assert.False(action.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);
        }

        [Test]
        public void CanNestFireRef()
        {
            var fooPattern = new BulletPattern(@"
              <bulletml>
                <action label=""top""/>
                <fire label=""foo"">
                  <speed>2.718</speed>
                  <direction type=""absolute"">70</direction>
                  <bullet/>
                </fire>
              </bulletml>
            ");

            var node = XElement.Parse(@"
              <action>
                <fireRef label=""foo""/>
              </action>
            ");

            var action = new ActionDef(node, fooPattern);
            Assert.False(action.IsCompleted);
            Assert.AreEqual(1, action.Tasks.Count);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            Assert.True(action.Run(TestBullet));
            Assert.True(action.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(2.718f, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(70), bullet.Direction);
        }
    }
}
