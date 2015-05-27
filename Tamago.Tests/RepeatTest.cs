﻿using NUnit.Framework;
using System;
using System.Linq;
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
            TestBullet.SetPattern(ActionDef.Default, isTopLevel: false);
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Repeat(null, DummyPattern));
        }

        [Test]
        public void ThrowsArgumentNullIfPatternIsNull()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>1</times>
                <action/>
              </repeat>
            ");
            Assert.Throws<ArgumentNullException>(() => new Repeat(node, null));
        }

        [Test]
        public void ThrowsArgumentExceptionIfNodeIsNotRepeat()
        {
            var node = XElement.Parse(@"<foo/>");
            Assert.Throws<ArgumentException>(() => new Repeat(node, DummyPattern));
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

            var repeat = new Repeat(node, DummyPattern);
            Assert.Throws<ArgumentNullException>(() => repeat.Run(null, EmptyArray));
        }

        [Test]
        public void ThrowsParseExceptionIfNoTimes()
        {
            var node = XElement.Parse(@"
              <repeat>
                <action/>
              </repeat>
            ");

            Assert.Throws<ParseException>(() => new Repeat(node, DummyPattern));
        }

        [Test]
        public void ThrowsParseExceptionIfNoActionOrActionRef()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>3</times>
              </repeat>
            ");

            Assert.Throws<ParseException>(() => new Repeat(node, DummyPattern));
        }

        [Test]
        public void ThrowsParseExceptionIfBothActionAndActionRef()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>3</times>
                <action/>
                <actionRef label=""foo""/>
              </repeat>
            ");

            Assert.Throws<ParseException>(() => new Repeat(node, DummyPattern));
        }

        [Test]
        public void ParsesTimesAsExpression()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>2+1</times>
                <action/>
              </repeat>
            ");

            var repeat = new Repeat(node, DummyPattern);
            Assert.AreEqual(new Expression("2+1"), repeat.Times);
        }

        [Test]
        public void ParsesRand()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>$rand</times>
                <action>
                  <fire><bullet/></fire>
                </action>
              </repeat>
            ");

            var repeat = new Repeat(node, DummyPattern);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.SetRand(2);
            repeat.Run(TestBullet, EmptyArray);
            Assert.AreEqual(3, TestManager.Bullets.Count);
        }

        [Test]
        public void ParsesRank()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>$rank</times>
                <action>
                  <fire><bullet/></fire>
                </action>
              </repeat>
            ");

            var repeat = new Repeat(node, DummyPattern);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.SetRank(3);
            repeat.Run(TestBullet, EmptyArray);
            Assert.AreEqual(4, TestManager.Bullets.Count);
        }

        [Test]
        public void ParsesParams()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>$2</times>
                <action>
                  <fire><bullet/></fire>
                </action>
              </repeat>
            ");

            var repeat = new Repeat(node, DummyPattern);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            repeat.Run(TestBullet, new[] { 1.2f, 2, 3.4f });
            Assert.AreEqual(3, TestManager.Bullets.Count);
        }

        [Test]
        public void PassesEvalValuesToInnerAction()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>1</times>
                <action>
                  <fire>
                    <direction type=""absolute"">$2</direction>
                    <speed>$rank / $rand</speed>
                    <bullet/>
                  </fire>
                </action>
              </repeat>
            ");

            var repeat = new Repeat(node, DummyPattern);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.SetRand(1.23f);
            TestManager.SetRank(7.89f);
            repeat.Run(TestBullet, new[] { 1.2f, 123.4f, 3.4f });
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(MathHelper.ToRadians(123.4f), bullet.Direction);
            Assert.AreEqual(7.89f / 1.23f, bullet.Speed, 0.00001f);
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

            var repeat = new Repeat(node, DummyPattern);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            repeat.Run(TestBullet, EmptyArray);
            Assert.AreEqual(4, TestManager.Bullets.Count);
        }

        [Test]
        public void ExecutesActionXTimesWhenRunRoundingDown()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>3.9</times>
                <action>
                  <fire>
                    <bullet/>
                  </fire>
                </action>
              </repeat>
            ");

            var repeat = new Repeat(node, DummyPattern);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            repeat.Run(TestBullet, EmptyArray);
            Assert.AreEqual(4, TestManager.Bullets.Count);
        }

        [Test]
        public void NopsIfTimesIsZero()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>0</times>
                <action>
                  <fire>
                    <bullet/>
                  </fire>
                </action>
              </repeat>
            ");

            var repeat = new Repeat(node, DummyPattern);
            Assert.False(repeat.IsCompleted);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            repeat.Run(TestBullet, EmptyArray);
            Assert.True(repeat.IsCompleted);
            Assert.AreEqual(1, TestManager.Bullets.Count);
        }

        [Test]
        public void NopsIfTimesIsLessThanZero()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>-42</times>
                <action>
                  <fire>
                    <bullet/>
                  </fire>
                </action>
              </repeat>
            ");

            var repeat = new Repeat(node, DummyPattern);
            Assert.False(repeat.IsCompleted);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            repeat.Run(TestBullet, EmptyArray);
            Assert.True(repeat.IsCompleted);
            Assert.AreEqual(1, TestManager.Bullets.Count);
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

            var repeat = new Repeat(node, DummyPattern);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            repeat.Run(TestBullet, EmptyArray);
            Assert.AreEqual(4, TestManager.Bullets.Count);

            repeat.Run(TestBullet, EmptyArray);
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

            var repeat = new Repeat(node, DummyPattern);
            Assert.False(repeat.IsCompleted);

            repeat.Run(TestBullet, EmptyArray);
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

            var repeat = new Repeat(node, DummyPattern);
            Assert.False(repeat.IsCompleted);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            Assert.False(repeat.Run(TestBullet, EmptyArray));
            Assert.False(repeat.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            Assert.False(repeat.Run(TestBullet, EmptyArray));
            Assert.False(repeat.IsCompleted);
            Assert.AreEqual(4, TestManager.Bullets.Count);

            Assert.False(repeat.Run(TestBullet, EmptyArray));
            Assert.False(repeat.IsCompleted);
            Assert.AreEqual(6, TestManager.Bullets.Count);

            Assert.True(repeat.Run(TestBullet, EmptyArray));
            Assert.True(repeat.IsCompleted);
            Assert.AreEqual(7, TestManager.Bullets.Count);

            Assert.True(repeat.Run(TestBullet, EmptyArray));
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

            var repeat = new Repeat(node, DummyPattern);
            Assert.False(repeat.IsCompleted);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            repeat.Run(TestBullet, EmptyArray);
            Assert.True(repeat.IsCompleted);
            Assert.AreEqual(4, TestManager.Bullets.Count);

            repeat.Reset();
            Assert.False(repeat.IsCompleted);

            repeat.Run(TestBullet, EmptyArray);
            Assert.True(repeat.IsCompleted);
            Assert.AreEqual(7, TestManager.Bullets.Count);

            repeat.Run(TestBullet, EmptyArray);
            Assert.True(repeat.IsCompleted);
            Assert.AreEqual(7, TestManager.Bullets.Count);
        }

        [Test]
        public void CanBeNested()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>3</times>
                <action>
                  <repeat>
                    <times>2</times>
                    <action>
                      <fire><bullet/></fire>
                    </action>
                  </repeat>
                </action>
              </repeat>
            ");

            var repeat = new Repeat(node, DummyPattern);
            Assert.False(repeat.IsCompleted);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            Assert.True(repeat.Run(TestBullet, EmptyArray));
            Assert.True(repeat.IsCompleted);
            Assert.AreEqual(7, TestManager.Bullets.Count);

            Assert.True(repeat.Run(TestBullet, EmptyArray));
            Assert.True(repeat.IsCompleted);
            Assert.AreEqual(7, TestManager.Bullets.Count);
        }
        
        [Test]
        public void Clones()
        {
            var node = XElement.Parse(@"
              <repeat>
                <times>1</times>
                <action/>
              </repeat>
            ");

            var repeat1 = new Repeat(node, DummyPattern);
            var repeat2 = (Repeat)repeat1.Copy();
            Assert.AreNotSame(repeat1, repeat2);

            Assert.AreEqual(new Expression(1), repeat2.Times);
            Assert.AreNotSame(repeat1.Action, repeat2.Action);
        }

        [Test]
        public void AcceptsActionRefInPlaceOfAction()
        {
            TestManager.Bullets.Clear();

            CreateTopLevelBullet(@"
              <bulletml> 
                <action label=""top"">
                  <repeat>
                    <times>2</times>
                    <actionRef label=""abc""/>
                  </repeat>
                </action>
                <action label=""abc"">
                  <fire><bullet/></fire>
                </action>
              </bulletml> 
            ");

            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);
        }
    }
}
