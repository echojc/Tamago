using NUnit.Framework;
using System;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class AccelTest : TestBase
    {
        internal Bullet TestBullet;

        private readonly static float DefaultVelocityX = 1.23f;
        private readonly static float DefaultVelocityY = 2.34f;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TestManager.SetPlayerPosition(1, (float)Math.Sqrt(3));

            TestBullet = TestManager.CreateBullet();
            TestBullet.SetPattern(ActionDef.Default, isTopLevel: false);
            TestBullet.VelocityX = DefaultVelocityX;
            TestBullet.VelocityY = DefaultVelocityY;
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Accel(null));
        }

        [Test]
        public void ThrowsArgumentExceptionIfNodeIsNotAccel()
        {
            var node = XElement.Parse(@"<foo/>");
            Assert.Throws<ArgumentException>(() => new Accel(node));
        }

        [Test]
        public void ThrowsParseExceptionIfNoTerm()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal>2</horizontal>
                <vertical>3</vertical>
              </accel>
            ");

            Assert.Throws<ParseException>(() => new Accel(node));
        }

        [Test]
        public void DefaultsHorizontalToNull()
        {
            var node = XElement.Parse(@"
              <accel>
                <vertical>2</vertical>
                <term>3</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.Null(accel.VelocityX);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 2), accel.VelocityY);
        }

        [Test]
        public void DefaultsVerticalToNull()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal>2</horizontal>
                <term>3</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 2), accel.VelocityX);
            Assert.Null(accel.VelocityY);
        }

        [Test]
        public void DefaultsBothHorizontalAndVerticalToNull()
        {
            var node = XElement.Parse(@"
              <accel>
                <term>3</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.Null(accel.VelocityX);
            Assert.Null(accel.VelocityY);
        }

        [Test]
        public void ParsesHorizontalVerticalTerm()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal>4.2</horizontal>
                <vertical>2.3</vertical>
                <term>3</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 4.2f), accel.VelocityX);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 2.3f), accel.VelocityY);
            Assert.AreEqual(3, accel.Term.Evaluate());
        }

        [Test]
        public void ThrowsArgumentNullIfBulletToRunAgainstIsNull()
        {
            var node = XElement.Parse(@"
              <accel>
                <term>3</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.Throws<ArgumentNullException>(() => accel.Run(null, EmptyArray));
        }

        [Test]
        public void SetsIsCompletedWhenRunXTimes()
        {
            var node = XElement.Parse(@"
              <accel>
                <term>3</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.False(accel.IsCompleted);

            Assert.True(accel.Run(TestBullet, EmptyArray));
            Assert.False(accel.IsCompleted);

            Assert.True(accel.Run(TestBullet, EmptyArray));
            Assert.False(accel.IsCompleted);

            Assert.True(accel.Run(TestBullet, EmptyArray));
            Assert.True(accel.IsCompleted);

            Assert.True(accel.Run(TestBullet, EmptyArray));
            Assert.True(accel.IsCompleted);
        }

        [Test]
        public void SetsIsCompletedWhenRunXTimesRoundingDown()
        {
            var node = XElement.Parse(@"
              <accel>
                <term>1.1</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.False(accel.IsCompleted);

            Assert.True(accel.Run(TestBullet, EmptyArray));
            Assert.True(accel.IsCompleted);

            Assert.True(accel.Run(TestBullet, EmptyArray));
            Assert.True(accel.IsCompleted);
        }

        [Test]
        public void SetsVelocitiesToFinalIfTermIsZero()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal>2.3</horizontal>
                <vertical>4.2</vertical>
                <term>0</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.False(accel.IsCompleted);

            accel.Run(TestBullet, EmptyArray);
            Assert.True(accel.IsCompleted);
            TestBullet.Update();
            Assert.AreEqual(2.3f, TestBullet.VelocityX);
            Assert.AreEqual(4.2f, TestBullet.VelocityY);
        }

        [Test]
        public void SetsVelocitiesToFinalIfTermIsLessThanZero()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal>2.3</horizontal>
                <vertical>4.2</vertical>
                <term>-42</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.False(accel.IsCompleted);

            accel.Run(TestBullet, EmptyArray);
            Assert.True(accel.IsCompleted);
            TestBullet.Update();
            Assert.AreEqual(2.3f, TestBullet.VelocityX);
            Assert.AreEqual(4.2f, TestBullet.VelocityY);
        }

        [Test]
        public void SetsVelocitiesToFinalIfTermIsLessThanZeroSequence()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal type=""sequence"">2.3</horizontal>
                <vertical type=""sequence"">4.2</vertical>
                <term>-42</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.False(accel.IsCompleted);

            accel.Run(TestBullet, EmptyArray);
            Assert.True(accel.IsCompleted);
            TestBullet.Update();
            Assert.AreEqual(DefaultVelocityX, TestBullet.VelocityX);
            Assert.AreEqual(DefaultVelocityY, TestBullet.VelocityY);
        }

        [Test]
        public void DefaultVelocitiesAreNullNotZero()
        {
            var node = XElement.Parse(@"
              <accel>
                <term>3</term>
              </accel>
            ");

            var accel = new Accel(node);
            accel.Run(TestBullet, EmptyArray);
            Assert.Null(TestBullet.NewVelocityX);
            Assert.Null(TestBullet.NewVelocityY);
        }

        [Test]
        public void RunsCorrectlyForDefaultType()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal>2.3</horizontal>
                <vertical>4.2</vertical>
                <term>3</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.False(accel.IsCompleted);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + (2.3f - DefaultVelocityX) / 3,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + (4.2f - DefaultVelocityY) / 3,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + (2.3f - DefaultVelocityX) / 3 * 2,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + (4.2f - DefaultVelocityY) / 3 * 2,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(2.3f, TestBullet.VelocityX);
            Assert.AreEqual(4.2f, TestBullet.VelocityY);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(2.3f, TestBullet.VelocityX);
            Assert.AreEqual(4.2f, TestBullet.VelocityY);
        }

        [Test]
        public void RunsCorrectlyForAbsoluteType()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal type=""absolute"">2.3</horizontal>
                <vertical type=""absolute"">4.2</vertical>
                <term>3</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.False(accel.IsCompleted);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + (2.3f - DefaultVelocityX) / 3,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + (4.2f - DefaultVelocityY) / 3,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + (2.3f - DefaultVelocityX) / 3 * 2,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + (4.2f - DefaultVelocityY) / 3 * 2,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(2.3f, TestBullet.VelocityX);
            Assert.AreEqual(4.2f, TestBullet.VelocityY);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(2.3f, TestBullet.VelocityX);
            Assert.AreEqual(4.2f, TestBullet.VelocityY);
        }

        [Test]
        public void RunsCorrectlyForRelativeType()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal type=""relative"">2.3</horizontal>
                <vertical type=""relative"">4.2</vertical>
                <term>3</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.False(accel.IsCompleted);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 2.3f / 3,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 4.2f / 3,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 2.3f / 3 * 2,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 4.2f / 3 * 2,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(DefaultVelocityX + 2.3f, TestBullet.VelocityX);
            Assert.AreEqual(DefaultVelocityY + 4.2f, TestBullet.VelocityY);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(DefaultVelocityX + 2.3f, TestBullet.VelocityX);
            Assert.AreEqual(DefaultVelocityY + 4.2f, TestBullet.VelocityY);
        }

        [Test]
        public void RunsCorrectlyForSequenceType()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal type=""sequence"">0.3</horizontal>
                <vertical type=""sequence"">0.7</vertical>
                <term>3</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.False(accel.IsCompleted);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 0.3,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 0.7,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 0.6,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 1.4,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 0.9f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 2.1f,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 0.9f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 2.1f,
                TestBullet.VelocityY,
                0.00001f);
        }

        [Test]
        public void RunsCorrectlyForMixedTypes()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal type=""relative"">2.3</horizontal>
                <vertical type=""sequence"">0.7</vertical>
                <term>3</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.False(accel.IsCompleted);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 2.3f / 3,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 0.7,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 2.3f / 3 * 2,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 1.4,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 2.3f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 2.1f,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 2.3f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 2.1f,
                TestBullet.VelocityY,
                0.00001f);
        }

        [Test]
        public void CanBeReset()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal type=""sequence"">0.3</horizontal>
                <vertical type=""sequence"">0.7</vertical>
                <term>3</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.False(accel.IsCompleted);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 0.3,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 0.7,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 0.6,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 1.4,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 0.9f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 2.1f,
                TestBullet.VelocityY,
                0.00001f);

            accel.Reset();
            Assert.False(accel.IsCompleted);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 1.2f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 2.8f,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 1.5f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 3.5f,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 1.8f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 4.2f,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 1.8f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 4.2f,
                TestBullet.VelocityY,
                0.00001f);
        }

        [Test]
        public void RunsCorrectlyForDecimalTermRoundingDown()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal>2.3</horizontal>
                <vertical>4.2</vertical>
                <term>1.99</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.False(accel.IsCompleted);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(2.3f, TestBullet.VelocityX);
            Assert.AreEqual(4.2f, TestBullet.VelocityY);

            accel.Run(TestBullet, EmptyArray);
            TestBullet.Update();
            Assert.AreEqual(2.3f, TestBullet.VelocityX);
            Assert.AreEqual(4.2f, TestBullet.VelocityY);
        }

        [Test]
        public void AcceptsExpressionsInAllFields()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal>1+2</horizontal>
                <vertical>3+4</vertical>
                <term>5+6</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.AreEqual(new Speed(SpeedType.Absolute, new Expression("1+2")), accel.VelocityX);
            Assert.AreEqual(new Speed(SpeedType.Absolute, new Expression("3+4")), accel.VelocityY);
            Assert.AreEqual(new Expression("5+6"), accel.Term);
        }
        
        [Test]
        public void EvalsParamsRankRand()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal>$rand</horizontal>
                <vertical>$rank</vertical>
                <term>$2</term>
              </accel>
            ");

            var term = 2.5f;
            var accel = new Accel(node);
            accel.Run(TestBullet, new[] { 1.2f, term, 3.4f });

            TestBullet.Update();
            var expectedX = DefaultVelocityX + (Helpers.TestManager.TestRand - DefaultVelocityX) / (int)term;
            Assert.AreEqual(expectedX, TestBullet.VelocityX);

            var expectedY = DefaultVelocityY + (Helpers.TestManager.TestRank - DefaultVelocityY) / (int)term;
            Assert.AreEqual(expectedY, TestBullet.VelocityY);
        }
        
        [Test]
        public void Clones()
        {
            var node = XElement.Parse(@"
              <accel>
                <horizontal>1</horizontal>
                <vertical>2</vertical>
                <term>3</term>
              </accel>
            ");

            var accel1 = new Accel(node);
            var accel2 = (Accel)accel1.Copy();
            Assert.AreNotSame(accel1, accel2);

            Assert.AreEqual(new Speed(SpeedType.Absolute, 1), accel2.VelocityX);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 2), accel2.VelocityY);
            Assert.AreEqual(new Expression(3), accel2.Term);
        }
    }
}