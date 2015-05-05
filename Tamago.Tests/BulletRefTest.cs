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
        //internal Bullet TestBullet;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TestManager.SetPlayerPosition(1, (float)Math.Sqrt(3));

            //TestBullet = TestManager.CreateBullet();
            //TestBullet.Speed = 2.5f;
            //TestBullet.FireSpeed = 1.5f;
            //TestBullet.Direction = MathHelper.ToRadians(100);
            //TestBullet.FireDirection = MathHelper.ToRadians(200);
        }

        [Test]
        public void EmptyBulletNodeIsValid()
        {
            var node = XElement.Parse(@"
              <bullet/>
            ");

            Assert.DoesNotThrow(() => new BulletRef(node));
        }
    }
}
