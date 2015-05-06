using NUnit.Framework;
using System;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class ChangeSpeedTest : TestBase
    {
        internal Bullet TestBullet;

        internal static readonly float DefaultSpeed = 1.2f;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TestManager.SetPlayerPosition(1, (float)Math.Sqrt(3));

            TestBullet = TestManager.CreateBullet();
            TestBullet.SetPattern(ActionRef.Default, isTopLevel: false);
            TestBullet.Speed = DefaultSpeed;
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeSpeed(null));
        }

        [Test]
        public void ThrowsParseExceptionIfEmptyNode()
        {
            var node = XElement.Parse(@"
              <changeSpeed/>
            ");

            Assert.Throws<ParseException>(() => new ChangeSpeed(node));
        }

        [Test]
        public void ThrowsParseExceptionIfNoSpeedNode()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <term>2</term>
              </changeSpeed>
            ");

            Assert.Throws<ParseException>(() => new ChangeSpeed(node));
        }

        [Test]
        public void ThrowsParseExceptionIfNoTermNode()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed>1</speed>
              </changeSpeed>
            ");

            Assert.Throws<ParseException>(() => new ChangeSpeed(node));
        }

        [Test]
        public void ThrowsArgumentNullIfBulletToRunAgainstIsNull()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed>2</speed>
                <term>1</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.Throws<ArgumentNullException>(() => change.Run(null));
        }

        [Test]
        public void ParsesSpeedAndTermParameters()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed>2.5</speed>
                <term>3</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 2.5f), change.Speed);
            Assert.AreEqual(3, change.Term);
        }

        [Test]
        public void ParsesTermRoundingDown()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed>2.5</speed>
                <term>3.9</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.AreEqual(3, change.Term);
        }

        [Test]
        public void DoesNotRunIfTermIsZero()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed>2.5</speed>
                <term>0</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.True(change.IsCompleted);

            change.Run(TestBullet);
            Assert.AreEqual(DefaultSpeed, TestBullet.Speed);
        }

        [Test]
        public void DoesNotRunIfTermIsLessThanZero()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed>2.5</speed>
                <term>-42</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.True(change.IsCompleted);

            change.Run(TestBullet);
            Assert.AreEqual(DefaultSpeed, TestBullet.Speed);
        }

        [Test]
        public void SetsIsCompletedWhenRunXTimes()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed>2.5</speed>
                <term>3</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            Assert.True(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            Assert.True(change.IsCompleted);
        }

        [Test]
        public void RunsCorrectlySpeedDefault()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed>2.5</speed>
                <term>3</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + (2.5f - DefaultSpeed) / 3, TestBullet.Speed);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + (2.5f - DefaultSpeed) / 3 * 2, TestBullet.Speed);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(2.5f, TestBullet.Speed);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(2.5f, TestBullet.Speed);
        }

        [Test]
        public void RunsCorrectlySpeedAbsolute()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed type=""absolute"">2.5</speed>
                <term>3</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + (2.5f - DefaultSpeed) / 3, TestBullet.Speed);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + (2.5f - DefaultSpeed) / 3 * 2, TestBullet.Speed);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(2.5f, TestBullet.Speed);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(2.5f, TestBullet.Speed);
        }

        [Test]
        public void RunsCorrectlySpeedRelative()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed type=""relative"">2.5</speed>
                <term>3</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 2.5f / 3, TestBullet.Speed);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 2.5f / 3 * 2, TestBullet.Speed);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 2.5f, TestBullet.Speed);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 2.5f, TestBullet.Speed);
        }

        [Test]
        public void RunsCorrectlySpeedSequence()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed type=""sequence"">0.8</speed>
                <term>3</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 0.8f, TestBullet.Speed);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 1.6f, TestBullet.Speed);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 2.4f, TestBullet.Speed);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 2.4f, TestBullet.Speed);
        }
    }
}
