using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class FireDefTest : TestBase
    {
        internal Bullet TestBullet;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TestManager.SetPlayerPosition(1, (float)Math.Sqrt(3));

            TestBullet = TestManager.CreateBullet();
            TestBullet.Speed = 2.5f;
            TestBullet.FireSpeed = 1.5f;
            TestBullet.Direction = MathHelper.ToRadians(100);
            TestBullet.FireDirection = MathHelper.ToRadians(200);
        }

        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FireDef(null, DummyPattern));
        }

        [Test]
        public void ThrowsArgumentNullIfPatternIsNull()
        {
            var node = XElement.Parse(@"
              <fire>
                <bullet/>
              </fire>
            ");
            Assert.Throws<ArgumentNullException>(() => new FireDef(node, null));
        }

        [Test]
        public void ThrowsArgumentExceptionIfNodeIsNotFireRef()
        {
            var node = XElement.Parse(@"<foo/>");
            Assert.Throws<ArgumentException>(() => new FireDef(node, DummyPattern));
        }

        [Test]
        public void ThrowsParseExceptionIfNoBulletOrBulletRefNode()
        {
            var node = XElement.Parse(@"
              <fire/>
            ");

            Assert.Throws<ParseException>(() => new FireDef(node, DummyPattern));
        }

        [Test]
        public void ThrowsParseExceptionIfBothBulletAndBulletRefNode()
        {
            var node = XElement.Parse(@"
              <fire>
                <bullet/>
                <bulletRef label=""foo""/>
              </fire>
            ");

            Assert.Throws<ParseException>(() => new FireDef(node, DummyPattern));
        }

        [Test]
        public void ThrowsArgumentNullIfBulletToRunAgainstIsNull()
        {
            var node = XElement.Parse(@"
              <fire>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            Assert.Throws<ArgumentNullException>(() => fireRef.Run(null, EmptyArray));
        }

        [Test]
        public void DefaultsDirectionToAimOffset0AndSpeedTo1()
        {
            var node = XElement.Parse(@"
              <fire>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(1, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150), bullet.Direction);
        }

        [Test]
        public void UsesFireSpeedDefaultIfSet()
        {
            var node = XElement.Parse(@"
              <fire>
                <speed>2</speed>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(2, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150), bullet.Direction);
        }

        [Test]
        public void UsesFireSpeedAbsoluteIfSet()
        {
            var node = XElement.Parse(@"
              <fire>
                <speed type=""absolute"">2</speed>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(2, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150), bullet.Direction);
        }

        [Test]
        public void UsesFireSpeedRelativeIfSet()
        {
            var node = XElement.Parse(@"
              <fire>
                <speed type=""relative"">2</speed>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(4.5f, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150), bullet.Direction);
        }

        [Test]
        public void UsesFireSpeedSequenceIfSet()
        {
            var node = XElement.Parse(@"
              <fire>
                <speed type=""sequence"">2</speed>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(3.5f, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150), bullet.Direction);
        }

        [Test]
        public void UsesFireDirectionDefaultIfSet()
        {
            var node = XElement.Parse(@"
              <fire>
                <direction>45</direction>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(1, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(195), bullet.Direction, 0.00001f);
        }

        [Test]
        public void UsesFireDirectionAimIfSet()
        {
            var node = XElement.Parse(@"
              <fire>
                <direction type=""aim"">25</direction>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(1, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(175), bullet.Direction, 0.00001f);
        }

        [Test]
        public void UsesFireDirectionAbsoluteIfSet()
        {
            var node = XElement.Parse(@"
              <fire>
                <direction type=""absolute"">30</direction>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(1, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(30), bullet.Direction, 0.00001f);
        }

        [Test]
        public void UsesFireDirectionRelativeIfSet()
        {
            var node = XElement.Parse(@"
              <fire>
                <direction type=""relative"">30</direction>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(1, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(130), bullet.Direction, 0.00001f);
        }

        [Test]
        public void UsesFireDirectionSequenceIfSet()
        {
            var node = XElement.Parse(@"
              <fire>
                <direction type=""sequence"">30</direction>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(1, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(230), bullet.Direction, 0.00001f);
        }

        [Test]
        public void UsesBothFireSpeedAndFireDirectionIfSet()
        {
            var node = XElement.Parse(@"
              <fire>
                <direction>88.9</direction>
                <speed>1.23</speed>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(1.23f, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150 + 88.9f), bullet.Direction, 0.00001f);
        }

        [Test]
        public void UsesBulletSettingsIfSpeedAndDirectionAreNotSet()
        {
            var node = XElement.Parse(@"
              <fire label=""my label &amp;"">
                <bullet>
                  <speed>4.2</speed>
                  <direction type=""absolute"">123</direction>
                </bullet>
              </fire>
            ");

            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(4.2f, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(123), bullet.Direction, 0.00001f);
        }

        [Test]
        public void OverridesBulletSettingsIfSpeedAndDirectionAreSet()
        {
            var node = XElement.Parse(@"
              <fire label=""my label &amp;"">
                <speed>2.3</speed>
                <direction type=""absolute"">234</direction>
                <bullet>
                  <speed>4.2</speed>
                  <direction type=""absolute"">123</direction>
                </bullet>
              </fire>
            ");

            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(2.3f, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(234), bullet.Direction, 0.00001f);
        }

        [Test]
        public void AcceptsBulletRefInPlaceOfBullet()
        {
            var fooPattern = new BulletPattern(@"
              <bulletml>
                <bullet label=""foo"">
                  <speed>3.5</speed>
                  <direction type=""absolute"">100</direction>
                </bullet>
              </bulletml>
            ");

            var node = XElement.Parse(@"
              <fire>
                <bulletRef label=""foo""/>
              </fire>
            ");

            var fire = new FireDef(node, fooPattern);
            Assert.False(fire.IsCompleted);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            Assert.True(fire.Run(TestBullet, EmptyArray));
            Assert.True(fire.IsCompleted);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(3.5f, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(100), bullet.Direction);
        }

        [Test]
        public void ParsesLabel()
        {
            var node = XElement.Parse(@"
              <fire label=""my label &amp;"">
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            Assert.AreEqual("my label &", fireRef.Label);
        }

        [Test]
        public void DefaultsLabelToNull()
        {
            var node = XElement.Parse(@"
              <fire>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            Assert.Null(fireRef.Label);
        }

        [Test]
        public void CompletesAfterFiringBullet()
        {
            var node = XElement.Parse(@"
              <fire>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            Assert.False(fireRef.IsCompleted);

            Assert.True(fireRef.Run(TestBullet, EmptyArray));
            Assert.True(fireRef.IsCompleted);
        }

        [Test]
        public void OnlyFiresBulletOnce()
        {
            var node = XElement.Parse(@"
              <fire>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            fireRef.Run(TestBullet, EmptyArray);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            fireRef.Run(TestBullet, EmptyArray);
            Assert.AreEqual(2, TestManager.Bullets.Count);
        }

        [Test]
        public void CanBeReset()
        {
            var node = XElement.Parse(@"
              <fire>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireDef(node, DummyPattern);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            fireRef.Run(TestBullet, EmptyArray);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            fireRef.Reset();
            fireRef.Run(TestBullet, EmptyArray);
            Assert.AreEqual(3, TestManager.Bullets.Count);

            fireRef.Run(TestBullet, EmptyArray);
            Assert.AreEqual(3, TestManager.Bullets.Count);
        }

        [Test]
        public void AcceptsExpressionsInDirectionAndSpeed()
        {
            var node = XElement.Parse(@"
              <fire>
                <direction type=""absolute"">4+5</direction>
                <speed>1+2</speed>
                <bullet/>
              </fire>
            ");

            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, EmptyArray);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(3, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(9), bullet.Direction, 0.00001f);
        }
        
        [Test]
        public void EvalsParamsRankRand()
        {
            var node = XElement.Parse(@"
              <fire>
                <direction type=""absolute"">$2</direction>
                <speed>$rand + $rank</speed>
                <bullet/>
              </fire>
            ");

            var dir = 123.45f;
            var fireRef = new FireDef(node, DummyPattern);
            fireRef.Run(TestBullet, new[] { 1.2f, dir, 3.4f });

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(Helpers.TestManager.TestRand + Helpers.TestManager.TestRank, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(dir), bullet.Direction, 0.00001f);
        }
        
        [Test]
        public void Clones()
        {
            var node = XElement.Parse(@"
              <fire label=""abc"">
                <direction>1</direction>
                <speed>2</speed>
                <bullet/>
              </fire>
            ");

            var fire1 = new FireDef(node, DummyPattern);
            var fire2 = (FireDef)fire1.Copy();
            Assert.AreNotSame(fire1, fire2);

            Assert.AreSame(fire1.Bullet, fire2.Bullet);
            Assert.AreEqual(new Direction(DirectionType.Aim, 1), fire2.Direction);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 2), fire2.Speed);
            Assert.AreEqual("abc", fire2.Label);
        }
    }
}
