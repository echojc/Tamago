using NUnit.Framework;
using System;
using System.Collections.Generic;
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
        public void EvalsRand()
        {
            var node = XElement.Parse(@"
              <wait>$rand</wait>
            ");

            var wait = new Wait(node);
            var times = 7;

            TestManager.SetRand(times);
            for (int i = 0; i < times; i++)
            {
                Assert.False(wait.IsCompleted);
                Assert.False(wait.Run(TestBullet));
            }

            Assert.True(wait.IsCompleted);
            Assert.True(wait.Run(TestBullet));
        }

        [Test]
        public void EvalsRank()
        {
            var node = XElement.Parse(@"
              <wait>$rank</wait>
            ");

            var wait = new Wait(node);
            var times = 5;

            TestManager.SetRank(times);
            for (int i = 0; i < times; i++)
            {
                Assert.False(wait.IsCompleted);
                Assert.False(wait.Run(TestBullet));
            }

            Assert.True(wait.IsCompleted);
            Assert.True(wait.Run(TestBullet));
        }

        [Test]
        public void EvalsParams()
        {
            var node = XElement.Parse(@"
              <wait>$2</wait>
            ");

            var wait = new Wait(node);
            var times = 11;
            var array = new[] { 1.2f, times, 2.3f };

            for (int i = 0; i < times; i++)
            {
                Assert.False(wait.IsCompleted);
                Assert.False(wait.Run(TestBullet, array));
            }

            Assert.True(wait.IsCompleted);
            Assert.True(wait.Run(TestBullet, array));
        }

        [Test]
        public void EvalsRest()
        {
            var node = XElement.Parse(@"
              <wait>$i</wait>
            ");

            var wait = new Wait(node);
            var times = 3;
            var rest = new Dictionary<string, float>()
            {
                { "i", times }
            };

            for (int i = 0; i < times; i++)
            {
                Assert.False(wait.IsCompleted);
                Assert.False(wait.Run(TestBullet, rest: rest));
            }

            Assert.True(wait.IsCompleted);
            Assert.True(wait.Run(TestBullet, rest: rest));
        }

        [Test]
        public void NoopsIfDurationIsZero()
        {
            var node = XElement.Parse(@"
              <wait>0</wait>
            ");

            var wait = new Wait(node);
            Assert.False(wait.IsCompleted);

            Assert.True(wait.Run(TestBullet));
            Assert.True(wait.IsCompleted);
        }

        [Test]
        public void NoopsIfDurationIsLessThanZero()
        {
            var node = XElement.Parse(@"
              <wait>-42</wait>
            ");

            var wait = new Wait(node);
            Assert.False(wait.IsCompleted);

            Assert.True(wait.Run(TestBullet));
            Assert.True(wait.IsCompleted);
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
