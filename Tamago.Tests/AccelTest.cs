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
            TestBullet.SetPattern(ActionRef.Default, isTopLevel: false);
            TestBullet.VelocityX = DefaultVelocityX;
            TestBullet.VelocityY = DefaultVelocityY;
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Accel(null));
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
            Assert.AreEqual(3, accel.Term);
        }

        [Test]
        public void ParsesTermRoundingDown()
        {
            var node = XElement.Parse(@"
              <accel>
                <term>3.9</term>
              </accel>
            ");

            var accel = new Accel(node);
            Assert.AreEqual(3, accel.Term);
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
            Assert.Throws<ArgumentNullException>(() => accel.Run(null));
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

            Assert.True(accel.Run(TestBullet));
            Assert.False(accel.IsCompleted);

            Assert.True(accel.Run(TestBullet));
            Assert.False(accel.IsCompleted);

            Assert.True(accel.Run(TestBullet));
            Assert.True(accel.IsCompleted);

            Assert.True(accel.Run(TestBullet));
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
            Assert.True(accel.IsCompleted);

            accel.Run(TestBullet);
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
            Assert.True(accel.IsCompleted);

            accel.Run(TestBullet);
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
            Assert.True(accel.IsCompleted);

            accel.Run(TestBullet);
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
            accel.Run(TestBullet);
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

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + (2.3f - DefaultVelocityX) / 3,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + (4.2f - DefaultVelocityY) / 3,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + (2.3f - DefaultVelocityX) / 3 * 2,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + (4.2f - DefaultVelocityY) / 3 * 2,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(2.3f, TestBullet.VelocityX);
            Assert.AreEqual(4.2f, TestBullet.VelocityY);

            accel.Run(TestBullet);
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

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + (2.3f - DefaultVelocityX) / 3,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + (4.2f - DefaultVelocityY) / 3,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + (2.3f - DefaultVelocityX) / 3 * 2,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + (4.2f - DefaultVelocityY) / 3 * 2,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(2.3f, TestBullet.VelocityX);
            Assert.AreEqual(4.2f, TestBullet.VelocityY);

            accel.Run(TestBullet);
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

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 2.3f / 3,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 4.2f / 3,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 2.3f / 3 * 2,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 4.2f / 3 * 2,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(DefaultVelocityX + 2.3f, TestBullet.VelocityX);
            Assert.AreEqual(DefaultVelocityY + 4.2f, TestBullet.VelocityY);

            accel.Run(TestBullet);
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

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 0.3,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 0.7,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 0.6,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 1.4,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 0.9f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 2.1f,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
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

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 2.3f / 3,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 0.7,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 2.3f / 3 * 2,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 1.4,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 2.3f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 2.1f,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
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

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 0.3,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 0.7,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 0.6,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 1.4,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
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

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 1.2f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 2.8f,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 1.5f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 3.5f,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
            TestBullet.Update();
            Assert.AreEqual(
                DefaultVelocityX + 1.8f,
                TestBullet.VelocityX,
                0.00001f);
            Assert.AreEqual(
                DefaultVelocityY + 4.2f,
                TestBullet.VelocityY,
                0.00001f);

            accel.Run(TestBullet);
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
    }
}