using NUnit.Framework;
using System;

namespace Tamago.Tests
{
    [TestFixture]
    public abstract class TestBase
    {
        internal Helpers.TestManager TestManager;

        [SetUp]
        public virtual void SetUp()
        {
            TestManager = new Helpers.TestManager();
        }

        internal Bullet CreateTopLevelBullet(string xml)
        {
            var bullet = TestManager.CreateBullet();
            var pattern = BulletPattern.ParseString(xml);
            bullet.SetPattern(pattern.TopLevelAction, isTopLevel: true);
            return bullet;
        }
    }
}
