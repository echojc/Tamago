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
            TestBullet.SetPattern(ActionDef.Default, isTopLevel: false);
            TestBullet.Speed = DefaultSpeed;
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeSpeed(null));
        }

        [Test]
        public void ThrowsArgumentExceptionIfNodeIsNotChangeSpeed()
        {
            var node = XElement.Parse(@"<foo/>");
            Assert.Throws<ArgumentException>(() => new ChangeSpeed(node));
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
            Assert.Throws<ArgumentNullException>(() => change.Run(null, EmptyArray));
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
            Assert.AreEqual(new Expression(3), change.Term);
        }

        [Test]
        public void SetsSpeedToFinalSpeedIfTermIsZero()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed>2.5</speed>
                <term>0</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.False(change.IsCompleted);

            change.Run(TestBullet, EmptyArray);
            Assert.True(change.IsCompleted);
            TestBullet.Update();
            Assert.AreEqual(2.5f, TestBullet.Speed);
        }

        [Test]
        public void SetsSpeedToFinalSpeedIfTermIsLessThanZero()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed>2.5</speed>
                <term>-42</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.False(change.IsCompleted);

            change.Run(TestBullet, EmptyArray);
            Assert.True(change.IsCompleted);
            TestBullet.Update();
            Assert.AreEqual(2.5f, TestBullet.Speed);
        }

        [Test]
        public void SetsSpeedToFinalSpeedIfTermIsLessThanZeroSequence()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed type=""sequence"">2.5</speed>
                <term>-42</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.False(change.IsCompleted);

            change.Run(TestBullet, EmptyArray);
            Assert.True(change.IsCompleted);
            TestBullet.Update();
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

            Assert.True(change.Run(TestBullet, EmptyArray));
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet, EmptyArray));
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet, EmptyArray));
            Assert.True(change.IsCompleted);

            Assert.True(change.Run(TestBullet, EmptyArray));
            Assert.True(change.IsCompleted);
        }

        [Test]
        public void SetsIsCompletedWhenRunXTimesRoundingDown()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed>2.5</speed>
                <term>1.9</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet, EmptyArray));
            Assert.True(change.IsCompleted);

            Assert.True(change.Run(TestBullet, EmptyArray));
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

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + (2.5f - DefaultSpeed) / 3, TestBullet.Speed);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + (2.5f - DefaultSpeed) / 3 * 2, TestBullet.Speed);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(2.5f, TestBullet.Speed);

            Assert.True(change.Run(TestBullet, EmptyArray));
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

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + (2.5f - DefaultSpeed) / 3, TestBullet.Speed);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + (2.5f - DefaultSpeed) / 3 * 2, TestBullet.Speed);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(2.5f, TestBullet.Speed);

            Assert.True(change.Run(TestBullet, EmptyArray));
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

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 2.5f / 3, TestBullet.Speed);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 2.5f / 3 * 2, TestBullet.Speed);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 2.5f, TestBullet.Speed);

            Assert.True(change.Run(TestBullet, EmptyArray));
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

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 0.8f, TestBullet.Speed);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 1.6f, TestBullet.Speed);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 2.4f, TestBullet.Speed);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 2.4f, TestBullet.Speed);
        }

        [Test]
        public void CanBeReset()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed type=""sequence"">0.8</speed>
                <term>3</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 0.8f, TestBullet.Speed, 0.00001f);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 1.6f, TestBullet.Speed, 0.00001f);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 2.4f, TestBullet.Speed, 0.00001f);

            change.Reset();
            Assert.False(change.IsCompleted);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 3.2f, TestBullet.Speed, 0.00001f);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 4.0f, TestBullet.Speed, 0.00001f);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 4.8f, TestBullet.Speed, 0.00001f);

            Assert.True(change.Run(TestBullet, EmptyArray));
            TestBullet.Update();
            Assert.AreEqual(DefaultSpeed + 4.8f, TestBullet.Speed, 0.00001f);
        }

        [Test]
        public void AcceptsExpressionsInAllFields()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed>3+4</speed>
                <term>1+2</term>
              </changeSpeed>
            ");

            var change = new ChangeSpeed(node);
            Assert.AreEqual(new Speed(SpeedType.Absolute, new Expression("3+4")), change.Speed);
            Assert.AreEqual(new Expression("1+2"), change.Term);
        }
        
        [Test]
        public void EvalsParamsRankRand()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed type=""relative"">$rank + $rand</speed>
                <term>$2</term>
              </changeSpeed>
            ");

            var term = 2.5f;
            var change = new ChangeSpeed(node);
            change.Run(TestBullet, new[] { 1.2f, term, 3.4f });

            TestBullet.Update();
            var expectedDelta = (Helpers.TestManager.TestRand + Helpers.TestManager.TestRank) / (int)term;
            Assert.AreEqual(DefaultSpeed +  expectedDelta, TestBullet.Speed);
        }
        
        [Test]
        public void Clones()
        {
            var node = XElement.Parse(@"
              <changeSpeed>
                <speed>1</speed>
                <term>2</term>
              </changeSpeed>
            ");

            var change1 = new ChangeSpeed(node);
            var change2 = (ChangeSpeed)change1.Copy();
            Assert.AreNotSame(change1, change2);

            Assert.AreEqual(new Speed(SpeedType.Absolute, 1), change2.Speed);
            Assert.AreEqual(new Expression(2), change2.Term);
        }
    }
}
