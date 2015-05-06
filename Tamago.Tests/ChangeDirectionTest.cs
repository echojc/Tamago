using NUnit.Framework;
using System;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class ChangeDirectionTest : TestBase
    {
        internal Bullet TestBullet;

        internal static readonly float DefaultDirection = MathHelper.ToRadians(50);
        internal static readonly float DefaultAimDirection = MathHelper.ToRadians(150);

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TestManager.SetPlayerPosition(1, (float)Math.Sqrt(3));

            TestBullet = TestManager.CreateBullet();
            TestBullet.SetPattern(ActionRef.Default, isTopLevel: false);
            TestBullet.Direction = DefaultDirection;

        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeDirection(null));
        }

        [Test]
        public void ThrowsParseExceptionIfEmptyNode()
        {
            var node = XElement.Parse(@"
              <changeDirection/>
            ");

            Assert.Throws<ParseException>(() => new ChangeDirection(node));
        }

        [Test]
        public void ThrowsParseExceptionIfNoDirectionNode()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <term>2</term>
              </changeDirection>
            ");

            Assert.Throws<ParseException>(() => new ChangeDirection(node));
        }

        [Test]
        public void ThrowsParseExceptionIfNoTermNode()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction>1</direction>
              </changeDirection>
            ");

            Assert.Throws<ParseException>(() => new ChangeDirection(node));
        }

        [Test]
        public void ThrowsArgumentNullIfBulletToRunAgainstIsNull()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction>2</direction>
                <term>1</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.Throws<ArgumentNullException>(() => change.Run(null));
        }

        [Test]
        public void ParsesDirectionAndTermParameters()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction>100</direction>
                <term>3</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            var expectedDirection = new Direction(DirectionType.Aim, MathHelper.ToRadians(100));
            Assert.AreEqual(expectedDirection, change.Direction);
            Assert.AreEqual(3, change.Term);
        }

        [Test]
        public void ParsesTermRoundingDown()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction>100</direction>
                <term>3.9</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.AreEqual(3, change.Term);
        }

        [Test]
        public void SetsDirectionToFinalDirectionIfTermIsZero()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction type=""absolute"">100</direction>
                <term>0</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.True(change.IsCompleted);

            change.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(100), TestBullet.Direction);
        }

        [Test]
        public void SetsDirectionToFinalDirectionIfTermIsLessThanZero()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction type=""absolute"">100</direction>
                <term>-42</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.True(change.IsCompleted);

            change.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(100), TestBullet.Direction);
        }

        [Test]
        public void SetsIsCompletedWhenRunXTimes()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction>2.5</direction>
                <term>3</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
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
        public void RunsCorrectlyDirectionDefault()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction>20</direction>
                <term>3</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(
                DefaultDirection + (DefaultAimDirection + MathHelper.ToRadians(20) - DefaultDirection) / 3,
                TestBullet.Direction,
                0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(
                DefaultDirection + (DefaultAimDirection + MathHelper.ToRadians(20) - DefaultDirection) / 3 * 2,
                TestBullet.Direction,
                0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(
                DefaultAimDirection + MathHelper.ToRadians(20),
                TestBullet.Direction,
                0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(
                DefaultAimDirection + MathHelper.ToRadians(20),
                TestBullet.Direction,
                0.00001f);
        }

        [Test]
        public void RunsCorrectlyDirectionAbsolute()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction type=""absolute"">100</direction>
                <term>3</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultDirection + (MathHelper.ToRadians(100) - DefaultDirection) / 3, TestBullet.Direction);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultDirection + (MathHelper.ToRadians(100) - DefaultDirection) / 3 * 2, TestBullet.Direction);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(100), TestBullet.Direction);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(100), TestBullet.Direction);
        }

        [Test]
        public void RunsCorrectlyDirectionRelative()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction type=""relative"">50</direction>
                <term>3</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(
                DefaultDirection + MathHelper.ToRadians(50) / 3,
                TestBullet.Direction,
                0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(
                DefaultDirection + MathHelper.ToRadians(50) / 3 * 2,
                TestBullet.Direction,
                0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(
                DefaultDirection + MathHelper.ToRadians(50),
                TestBullet.Direction,
                0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(
                DefaultDirection + MathHelper.ToRadians(50),
                TestBullet.Direction,
                0.00001f);
        }

        [Test]
        public void RunsCorrectlyDirectionSequence()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction type=""sequence"">13</direction>
                <term>3</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultDirection + MathHelper.ToRadians(13), TestBullet.Direction);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultDirection + MathHelper.ToRadians(26), TestBullet.Direction);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultDirection + MathHelper.ToRadians(39), TestBullet.Direction);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultDirection + MathHelper.ToRadians(39), TestBullet.Direction);
        }
    }
}
