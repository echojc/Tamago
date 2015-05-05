using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class FireRefTest : TestBase
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
            Assert.Throws<ArgumentNullException>(() => new FireRef(null));
        }

        [Test]
        public void ThrowsParseExceptionIfNoBulletNode()
        {
            var node = XElement.Parse(@"
              <fire/>
            ");

            Assert.Throws<ParseException>(() => new FireRef(node));
        }

        [Test]
        public void ThrowsArgumentNullIfBulletToRunAgainstIsNull()
        {
            var node = XElement.Parse(@"
              <fire>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireRef(node);
            Assert.Throws<ArgumentNullException>(() => fireRef.Run(null));
        }

        [Test]
        public void DefaultsDirectionToAimOffset0AndSpeedTo1()
        {
            var node = XElement.Parse(@"
              <fire>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireRef(node);
            fireRef.Run(TestBullet);

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
            
            var fireRef = new FireRef(node);
            fireRef.Run(TestBullet);

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
            
            var fireRef = new FireRef(node);
            fireRef.Run(TestBullet);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(2, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150), bullet.Direction);
        }

        [Test]
        public void UsesFireSpeedRelativeIfSetABA()
        {
            var node = XElement.Parse(@"
              <fire>
                <speed type=""relative"">2</speed>
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireRef(node);
            fireRef.Run(TestBullet);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(3.5f, bullet.Speed);
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
            
            var fireRef = new FireRef(node);
            fireRef.Run(TestBullet);

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
            
            var fireRef = new FireRef(node);
            fireRef.Run(TestBullet);

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
            
            var fireRef = new FireRef(node);
            fireRef.Run(TestBullet);

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
            
            var fireRef = new FireRef(node);
            fireRef.Run(TestBullet);

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
            
            var fireRef = new FireRef(node);
            fireRef.Run(TestBullet);

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
            
            var fireRef = new FireRef(node);
            fireRef.Run(TestBullet);

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
            
            var fireRef = new FireRef(node);
            fireRef.Run(TestBullet);

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

            var fireRef = new FireRef(node);
            fireRef.Run(TestBullet);

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

            var fireRef = new FireRef(node);
            fireRef.Run(TestBullet);

            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(2.3f, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(234), bullet.Direction, 0.00001f);
        }

        [Test]
        [Ignore]
        public void AcceptsBulletRefInPlaceOfBullet()
        { }

        [Test]
        public void ParsesLabel()
        {
            var node = XElement.Parse(@"
              <fire label=""my label &amp;"">
                <bullet/>
              </fire>
            ");
            
            var fireRef = new FireRef(node);
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
            
            var fireRef = new FireRef(node);
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
            
            var fireRef = new FireRef(node);
            Assert.False(fireRef.IsCompleted);

            fireRef.Run(TestBullet);
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
            
            var fireRef = new FireRef(node);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            fireRef.Run(TestBullet);
            Assert.AreEqual(2, TestManager.Bullets.Count);

            fireRef.Run(TestBullet);
            Assert.AreEqual(2, TestManager.Bullets.Count);
        }
    }
}
