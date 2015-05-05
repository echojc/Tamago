using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Tamago.Tests
{
    [TestFixture]
    public class BasicTest : TestBase
    {
        [Test]
        public void SanityCheck()
        {
            TestManager.SetPlayerPosition(1, -(float)Math.Sqrt(3));

            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <bullet/>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var root = TestManager.Bullets.Find(b => b.IsTopLevel == true);
            Assert.NotNull(root);
            Assert.AreEqual(0, root.X);
            Assert.AreEqual(0, root.Y);

            var bullet = TestManager.Bullets.Find(b => b.IsTopLevel == false);
            Assert.NotNull(bullet);
            Assert.AreEqual(0.5f, bullet.X);
            Assert.AreEqual(-(float)(0.5 * Math.Sqrt(3)), bullet.Y);
        }
    }
}
