using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Tamago.Tests
{
    [TestFixture]
    public abstract class TestBase
    {
        internal Helpers.TestManager TestManager;

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
            bullet.SetPattern(pattern.CopyAction(name), isTopLevel: true);
            return bullet;
        }
    }

    public static class TestHelper
    {
        public static bool Run(this ITask task, Bullet bullet, float[] args = null, Dictionary<string, float> rest = null)
        {
            return task.Run(
                bullet,
                args ?? new float[] { },
                rest ?? new Dictionary<string, float>());
        }

        public static Bullet Create(this IBulletDefinition bulletDef, Bullet bullet, float[] args = null, Dictionary<string, float> rest = null)
        {
            return bulletDef.Create(
                bullet,
                args ?? new float[] { },
                rest ?? new Dictionary<string, float>());
        }
    }
}
