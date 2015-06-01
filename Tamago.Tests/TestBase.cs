using NUnit.Framework;
using System;

namespace Tamago.Tests
{
    [TestFixture]
    public abstract class TestBase
    {
        internal Helpers.TestManager TestManager;

        internal static readonly float[] EmptyArray = new float[] { };

        /// <summary>
        /// Allows tests to construct internal nodes directly.
        /// </summary>
        internal BulletPattern DummyPattern;

        [SetUp]
        public virtual void SetUp()
        {
            TestManager = new Helpers.TestManager();
            DummyPattern = new BulletPattern(@"<bulletml/>");
        }

        internal Bullet CreateTopLevelBullet(string xml, string name = "top")
        {
            var bullet = TestManager.CreateBullet();
            var pattern = new BulletPattern(xml);
            bullet.SetPattern(pattern.Actions[name], isTopLevel: true);
            return bullet;
        }
    }
}
