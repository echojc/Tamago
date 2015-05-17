using NUnit.Framework;
using System;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class WaitTest : TestBase
    {
        internal Bullet TestBullet;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TestBullet = TestManager.CreateBullet();
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Wait(null));
        }

        [Test]
        public void ThrowsArgumentExceptionIfNodeIsNotWait()
        {
            var node = XElement.Parse(@"<foo/>");
            Assert.Throws<ArgumentException>(() => new Wait(node));
        }

        [Test]
        public void ThrowsParseExceptionIfNoWaitTime()
        {
            var node = XElement.Parse(@"
              <wait/>
            ");

            Assert.Throws<ParseException>(() => new Wait(node));
        }

        [Test]
        public void ThrowsArgumentNullIfBulletToRunAgainstIsNull()
        {
            var node = XElement.Parse(@"
              <wait>1</wait>
            ");

            var wait = new Wait(node);
            Assert.Throws<ArgumentNullException>(() => wait.Run(null));
        }

        [Test]
        public void ParsesWaitDurationAsExpression()
        {
            var node = XElement.Parse(@"
              <wait>42 + 1</wait>
            ");

            var wait = new Wait(node);
            Assert.AreEqual(new Expression("42+1"), wait.Duration);
        }

        [Test]
        public void CompletesAfterExecutingRunXTimes()
        {
            var node = XElement.Parse(@"
              <wait>2</wait>
            ");

            var wait = new Wait(node);
            Assert.False(wait.IsCompleted);

            Assert.False(wait.Run(TestBullet));
            Assert.False(wait.IsCompleted);

            Assert.False(wait.Run(TestBullet));
            Assert.True(wait.IsCompleted);

            Assert.True(wait.Run(TestBullet));
        }

        [Test]
        public void CompletesAfterExecutingRunXTimesRoundedDown()
        {
            var node = XElement.Parse(@"
              <wait>2.999</wait>
            ");

            var wait = new Wait(node);
            Assert.False(wait.IsCompleted);

            Assert.False(wait.Run(TestBullet));
            Assert.False(wait.IsCompleted);

            Assert.False(wait.Run(TestBullet));
            Assert.True(wait.IsCompleted);

            Assert.True(wait.Run(TestBullet));
        }

        [Test]
        public void CanBeReset()
        {
            var node = XElement.Parse(@"
              <wait>2</wait>
            ");

            var wait = new Wait(node);
            Assert.False(wait.IsCompleted);

            Assert.False(wait.Run(TestBullet));
            Assert.False(wait.IsCompleted);

            Assert.False(wait.Run(TestBullet));
            Assert.True(wait.IsCompleted);

            Assert.True(wait.Run(TestBullet));

            wait.Reset();
            Assert.False(wait.IsCompleted);

            Assert.False(wait.Run(TestBullet));
            Assert.False(wait.IsCompleted);

            Assert.False(wait.Run(TestBullet));
            Assert.True(wait.IsCompleted);

            Assert.True(wait.Run(TestBullet));
        }

        [Test]
        public void Clones()
        {
            var node = XElement.Parse(@"
              <wait>2</wait>
            ");

            var wait1 = new Wait(node);
            var wait2 = (Wait)wait1.Copy();
            Assert.AreNotSame(wait1, wait2);

            Assert.AreEqual(new Expression(2), wait2.Duration);
        }
    }
}
