using NUnit.Framework;
using System;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class RepeatTest : TestBase
    {
        internal Bullet TestBullet;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TestBullet = TestManager.CreateBullet();
            TestBullet.SetPattern(ActionRef.Default, isTopLevel: false);
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Repeat(null));

        }

        [Test]
        public void ThrowsArgumentNullIfBulletToRunAgainstIsNull()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>1</times>
                <action/>
              </repeat>
            ");

            var repeat = new Repeat(node);
            Assert.Throws<ArgumentNullException>(() => repeat.Run(null));
        }

        [Test]
        public void ThrowsParseExceptionIfNoTimes()
        {
            var node = XElement.Parse(@"
              <repeat>
                <action/>
              </repeat>
            ");

            Assert.Throws<ParseException>(() => new Repeat(node));
        }

        [Test]
        public void ThrowsParseExceptionIfNoAction()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>3</times>
              </repeat>
            ");

            Assert.Throws<ParseException>(() => new Repeat(node));
        }

        [Test]
        public void ParsesTimesRoundingDown()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>3.9</times>
                <action/>
              </repeat>
            ");

            var repeat = new Repeat(node);
            Assert.AreEqual(3, repeat.Times);
        }

        [Test]
        public void ExecutesActionXTimesWhenRun()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>3</times>
                <action>
                  <fire>
                    <bullet/>
                  </fire>
                </action>
              </repeat>
            ");

            var repeat = new Repeat(node);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            repeat.Run(TestBullet);
            Assert.AreEqual(4, TestManager.Bullets.Count);
        }

        [Test]
        public void ExecutesOnce()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>3</times>
                <action>
                  <fire>
                    <bullet/>
                  </fire>
                </action>
              </repeat>
            ");

            var repeat = new Repeat(node);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            repeat.Run(TestBullet);
            Assert.AreEqual(4, TestManager.Bullets.Count);

            repeat.Run(TestBullet);
            Assert.AreEqual(4, TestManager.Bullets.Count);
        }

        [Test]
        public void CompletesAfterRunning()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>3</times>
                <action/>
              </repeat>
            ");

            var repeat = new Repeat(node);
            Assert.False(repeat.IsCompleted);

            repeat.Run(TestBullet);
            Assert.True(repeat.IsCompleted);
        }

        [Test]
        public void ObeysWait()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>3</times>
                <action>
                  <fire><bullet/></fire>
                  <wait>1</wait>
                  <fire><bullet/></fire>
                </action>
              </repeat>
            ");

            var repeat = new Repeat(node);
            Assert.False(repeat.IsCompleted);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            Assert.False(repeat.Run(TestBullet));
            Assert.False(repeat.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            Assert.False(repeat.Run(TestBullet));
            Assert.False(repeat.IsCompleted);
            Assert.AreEqual(4, TestManager.Bullets.Count);

            Assert.False(repeat.Run(TestBullet));
            Assert.False(repeat.IsCompleted);
            Assert.AreEqual(6, TestManager.Bullets.Count);

            Assert.True(repeat.Run(TestBullet));
            Assert.True(repeat.IsCompleted);
            Assert.AreEqual(7, TestManager.Bullets.Count);

            Assert.True(repeat.Run(TestBullet));
            Assert.True(repeat.IsCompleted);
            Assert.AreEqual(7, TestManager.Bullets.Count);
        }

        [Test]
        public void CanBeReset()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>3</times>
                <action>
                  <fire>
                    <bullet/>
                  </fire>
                </action>
              </repeat>
            ");

            var repeat = new Repeat(node);
            Assert.False(repeat.IsCompleted);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            repeat.Run(TestBullet);
            Assert.True(repeat.IsCompleted);
            Assert.AreEqual(4, TestManager.Bullets.Count);

            repeat.Reset();
            Assert.False(repeat.IsCompleted);

            repeat.Run(TestBullet);
            Assert.True(repeat.IsCompleted);
            Assert.AreEqual(7, TestManager.Bullets.Count);

            repeat.Run(TestBullet);
            Assert.True(repeat.IsCompleted);
            Assert.AreEqual(7, TestManager.Bullets.Count);
        }

        [Test]
        [Ignore]
        public void AcceptsActionRefInPlaceOfAction()
        { }
    }
}
