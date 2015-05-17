using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class BulletRefTest : TestBase
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
            Assert.Throws<ArgumentNullException>(() => new BulletRef(null));
        }

        [Test]
        public void ThrowsArgumentExceptionIfNodeIsNotBullet()
        {
            var node = XElement.Parse(@"<foo/>");
            Assert.Throws<ArgumentException>(() => new BulletRef(node));
        }

        [Test]
        public void ThrowsArgumentNullIfParentToCreateFromIsNull()
        {
            var node = XElement.Parse(@"
              <bullet/>
            ");

            var bulletRef = new BulletRef(node);
            Assert.Throws<ArgumentNullException>(() => bulletRef.Create(null));
        }

        [Test]
        public void DefaultsDirectionToAimOffset0AndSpeedTo1()
        {
            var node = XElement.Parse(@"
              <bullet/>
            ");

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);

            Assert.AreEqual(1, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150), bullet.Direction);
        }

        [Test]
        public void DefaultsPositionToSameAsParent()
        {
            var node = XElement.Parse(@"
              <bullet/>
            ");

            TestBullet.X = 4.2f;
            TestBullet.Y = 2.3f;

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);

            Assert.AreEqual(4.2f, bullet.X);
            Assert.AreEqual(2.3f, bullet.Y);
            Assert.AreEqual(1, bullet.Speed);
            // complicated maths here
            Assert.AreEqual(-1.39514f, bullet.Direction, 0.00001f);
        }

        [Test]
        public void UsesSpeedDefaultIfSet()
        {
            var node = XElement.Parse(@"
              <bullet>
                <speed>2</speed>
              </bullet>
            ");

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);

            Assert.AreEqual(2, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150), bullet.Direction);
        }

        [Test]
        public void UsesSpeedAbsoluteIfSet()
        {
            var node = XElement.Parse(@"
              <bullet>
                <speed type=""absolute"">2</speed>
              </bullet>
            ");

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);

            Assert.AreEqual(2, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150), bullet.Direction);
        }

        [Test]
        public void UsesSpeedRelativeIfSet()
        {
            var node = XElement.Parse(@"
              <bullet>
                <speed type=""relative"">2</speed>
              </bullet>
            ");

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);

            Assert.AreEqual(4.5f, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150), bullet.Direction);
        }

        [Test]
        public void UsesSpeedSequenceIfSet()
        {
            var node = XElement.Parse(@"
              <bullet>
                <speed type=""sequence"">2</speed>
              </bullet>
            ");

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);

            Assert.AreEqual(3.5f, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150), bullet.Direction);
        }

        [Test]
        public void DefaultsToEmptyAction()
        {
            var node = XElement.Parse(@"
              <bullet/>
            ");

            var bulletRef = new BulletRef(node);
            Assert.True(bulletRef.Action.IsCompleted);
        }

        [Test]
        public void ParsesActionNode()
        {
            var node = XElement.Parse(@"
              <bullet>
                <action>
                  <fire><bullet/></fire>
                </action>
              </bullet>
            ");

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);
            Assert.False(bulletRef.Action.IsCompleted);
            Assert.AreEqual(1, bulletRef.Action.Tasks.Count);

            bulletRef.Action.Run(bullet);
            Assert.True(bulletRef.Action.IsCompleted);
            Assert.AreEqual(3, TestManager.Bullets.Count);
        }

        [Test]
        public void ActionsAreNotShared()
        {
            var node = XElement.Parse(@"
              <bullet>
                <action>
                  <fire><bullet/></fire>
                </action>
              </bullet>
            ");

            var bulletRef = new BulletRef(node);
            var bullet1 = bulletRef.Create(TestBullet);
            var bullet2 = bulletRef.Create(TestBullet);
            var bullet3 = bulletRef.Create(TestBullet);
            Assert.False(bulletRef.Action.IsCompleted);
            Assert.False(bullet1.Action.IsCompleted);
            Assert.False(bullet2.Action.IsCompleted);
            Assert.False(bullet3.Action.IsCompleted);

            bullet1.Update();
            bullet2.Update();
            Assert.False(bulletRef.Action.IsCompleted);
            Assert.True(bullet1.Action.IsCompleted);
            Assert.True(bullet2.Action.IsCompleted);
            Assert.False(bullet3.Action.IsCompleted);
            Assert.AreEqual(6, TestManager.Bullets.Count);
        }

        [Test]
        [Ignore]
        public void AcceptsActionRefInPlaceOfAction()
        { }

        [Test]
        [Ignore]
        public void ParsesAndRunsMultipleActions()
        { }

        [Test]
        public void UsesFireDirectionDefaultIfSet()
        {
            var node = XElement.Parse(@"
              <bullet>
                <direction>45</direction>
              </bullet>
            ");

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);

            Assert.AreEqual(1, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(195), bullet.Direction, 0.00001f);
        }

        [Test]
        public void UsesFireDirectionAimIfSet()
        {
            var node = XElement.Parse(@"
              <bullet>
                <direction type=""aim"">25</direction>
              </bullet>
            ");

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);

            Assert.AreEqual(1, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(175), bullet.Direction, 0.00001f);
        }

        [Test]
        public void UsesFireDirectionAbsoluteIfSet()
        {
            var node = XElement.Parse(@"
              <bullet>
                <direction type=""absolute"">30</direction>
              </bullet>
            ");

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);

            Assert.AreEqual(1, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(30), bullet.Direction, 0.00001f);
        }

        [Test]
        public void UsesFireDirectionRelativeIfSet()
        {
            var node = XElement.Parse(@"
              <bullet>
                <direction type=""relative"">30</direction>
              </bullet>
            ");

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);

            Assert.AreEqual(1, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(130), bullet.Direction, 0.00001f);
        }

        [Test]
        public void UsesFireDirectionSequenceIfSet()
        {
            var node = XElement.Parse(@"
              <bullet>
                <direction type=""sequence"">30</direction>
              </bullet>
            ");

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);

            Assert.AreEqual(1, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(230), bullet.Direction, 0.00001f);
        }

        [Test]
        public void UsesBothFireSpeedAndFireDirectionIfSet()
        {
            var node = XElement.Parse(@"
              <bullet>
                <direction>88.9</direction>
                <speed>1.23</speed>
              </bullet>
            ");

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);

            Assert.AreEqual(1.23f, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150 + 88.9f), bullet.Direction, 0.00001f);
        }

        [Test]
        public void ParsesLabel()
        {
            var node = XElement.Parse(@"
              <bullet label=""my label &amp;""/>
            ");

            var bulletRef = new BulletRef(node);
            Assert.AreEqual("my label &", bulletRef.Label);
        }

        [Test]
        public void DefaultsLabelToNull()
        {
            var node = XElement.Parse(@"
              <bullet/>
            ");

            var bulletRef = new BulletRef(node);
            Assert.Null(bulletRef.Label);
        }

        [Test]
        public void AcceptsExpressionsInDirectionAndSpeed()
        {
            var node = XElement.Parse(@"
              <bullet>
                <direction type=""absolute"">4+5</direction>
                <speed>1+2</speed>
              </bullet>
            ");

            var bulletRef = new BulletRef(node);
            var bullet = bulletRef.Create(TestBullet);

            Assert.AreEqual(3, bullet.Speed);
            Assert.AreEqual(MathHelper.ToRadians(9), bullet.Direction, 0.00001f);
        }
    }
}
