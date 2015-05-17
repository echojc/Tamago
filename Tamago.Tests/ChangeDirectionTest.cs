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
            TestBullet.SetPattern(ActionDef.Default, isTopLevel: false);
            TestBullet.Direction = DefaultDirection;
            TestBullet.Speed = 0;
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeDirection(null));
        }

        [Test]
        public void ThrowsArgumentExceptionIfNodeIsNotChangeDirection()
        {
            var node = XElement.Parse(@"<foo/>");
            Assert.Throws<ArgumentException>(() => new ChangeDirection(node));
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
            var expectedDirection = new Direction(DirectionType.Aim, new Expression(100));
            Assert.AreEqual(expectedDirection, change.Direction);
            Assert.AreEqual(new Expression(3), change.Term);
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
            Assert.False(change.IsCompleted);

            change.Run(TestBullet);
            Assert.True(change.IsCompleted);
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
            Assert.False(change.IsCompleted);

            change.Run(TestBullet);
            Assert.True(change.IsCompleted);
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(100), TestBullet.Direction);
        }

        [Test]
        public void SetsDirectionToFinalDirectionIfTermIsLessThanZeroSequence()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction type=""sequence"">100</direction>
                <term>-42</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.False(change.IsCompleted);

            change.Run(TestBullet);
            Assert.True(change.IsCompleted);
            TestBullet.Update();
            Assert.AreEqual(DefaultDirection, TestBullet.Direction);
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
        public void SetsIsCompletedWhenRunXTimesRoundingDown()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction>2.5</direction>
                <term>1.9</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
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

        [Test]
        public void CanBeReset()
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
            Assert.AreEqual(DefaultDirection + MathHelper.ToRadians(13), TestBullet.Direction, 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultDirection + MathHelper.ToRadians(26), TestBullet.Direction, 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultDirection + MathHelper.ToRadians(39), TestBullet.Direction, 0.00001f);

            change.Reset();
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultDirection + MathHelper.ToRadians(52), TestBullet.Direction, 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultDirection + MathHelper.ToRadians(65), TestBullet.Direction, 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultDirection + MathHelper.ToRadians(78), TestBullet.Direction, 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(DefaultDirection + MathHelper.ToRadians(78), TestBullet.Direction, 0.00001f);
        }

        [Test]
        public void WorksAcrossNormalisedBoundaryClockwise()
        {
            TestBullet.Direction = MathHelper.ToRadians(160);

            var node = XElement.Parse(@"
              <changeDirection>
                <direction type=""absolute"">-160</direction>
                <term>4</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(170), TestBullet.Direction, 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            // only way to compare across the boundary
            Assert.LessOrEqual(MathHelper.NormalizeAngle(MathHelper.ToRadians(180) - TestBullet.Direction), 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(-170), TestBullet.Direction, 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(-160), TestBullet.Direction, 0.00001f);

        }

        [Test]
        public void WorksAcrossNormalisedBoundaryAntiClockwise()
        {
            TestBullet.Direction = MathHelper.ToRadians(-160);

            var node = XElement.Parse(@"
              <changeDirection>
                <direction type=""absolute"">160</direction>
                <term>4</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(-170), TestBullet.Direction, 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            // only way to compare across the boundary
            Assert.LessOrEqual(MathHelper.NormalizeAngle(MathHelper.ToRadians(180) - TestBullet.Direction), 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(170), TestBullet.Direction, 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(160), TestBullet.Direction, 0.00001f);
        }

        [Test]
        public void ChoosesShortestDistanceClockwiseCrossingBoundary()
        {
            TestBullet.Direction = MathHelper.ToRadians(160);

            var node = XElement.Parse(@"
              <changeDirection>
                <direction type=""absolute"">-20.01</direction>
                <term>2</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(-110.005f), TestBullet.Direction, 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(-20.01f), TestBullet.Direction, 0.00001f);
        }

        [Test]
        public void ChoosesShortestDistanceAntiClockwiseCrossingBoundary()
        {
            TestBullet.Direction = MathHelper.ToRadians(-160);

            var node = XElement.Parse(@"
              <changeDirection>
                <direction type=""absolute"">20.01</direction>
                <term>2</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(110.005f), TestBullet.Direction, 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(20.01f), TestBullet.Direction, 0.00001f);
        }

        [Test]
        public void ChoosesShortestDistanceClockwiseWithoutCrossingBoundary()
        {
            TestBullet.Direction = MathHelper.ToRadians(-160);

            var node = XElement.Parse(@"
              <changeDirection>
                <direction type=""absolute"">19.99</direction>
                <term>2</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(-70.005f), TestBullet.Direction, 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(19.99f), TestBullet.Direction, 0.00001f);
        }

        [Test]
        public void ChoosesShortestDistanceAntiClockwiseWithoutCrossingBoundary()
        {
            TestBullet.Direction = MathHelper.ToRadians(160);

            var node = XElement.Parse(@"
              <changeDirection>
                <direction type=""absolute"">-19.99</direction>
                <term>2</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(70.005f), TestBullet.Direction, 0.00001f);

            Assert.True(change.Run(TestBullet));
            TestBullet.Update();
            Assert.AreEqual(MathHelper.ToRadians(-19.99f), TestBullet.Direction, 0.00001f);
        }

        [Test]
        public void AcceptsExpressionsInAllFields()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction type=""absolute"">3+4</direction>
                <term>1+2</term>
              </changeDirection>
            ");

            var change = new ChangeDirection(node);
            Assert.AreEqual(new Direction(DirectionType.Absolute, new Expression("3+4")), change.Direction);
            Assert.AreEqual(new Expression("1+2"), change.Term);
        }
        
        [Test]
        public void Clones()
        {
            var node = XElement.Parse(@"
              <changeDirection>
                <direction>1</direction>
                <term>2</term>
              </changeDirection>
            ");

            var change1 = new ChangeDirection(node);
            var change2 = (ChangeDirection)change1.Copy();
            Assert.AreNotSame(change1, change2);

            Assert.AreEqual(new Direction(DirectionType.Aim, 1), change2.Direction);
            Assert.AreEqual(new Expression(2), change2.Term);
        }
    }
}
