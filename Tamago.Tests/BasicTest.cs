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
            Assert.True(root.Action.IsCompleted);
            Assert.True(root.IsVanished);

            var bullet = TestManager.Bullets.Find(b => b.IsTopLevel == false);
            Assert.NotNull(bullet);
            Assert.AreEqual(0.5f, bullet.X);
            Assert.AreEqual(-(float)(0.5 * Math.Sqrt(3)), bullet.Y);
            Assert.True(bullet.Action.IsCompleted);
            Assert.False(bullet.IsVanished);
        }

        [Test]
        public void ActionsAreRunBeforeBulletsMove()
        {
            TestManager.SetPlayerPosition(1, -(float)Math.Sqrt(3));

            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <direction type=""absolute"">180</direction>
                    <speed>20</speed>
                    <bullet>
                      <action>
                        <fire>
                          <direction type=""absolute"">90</direction>
                          <bullet/>
                        </fire>
                      </action>
                    </bullet>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            var root = TestManager.Bullets[0];
            Assert.True(root.IsTopLevel);
            Assert.AreEqual(0, root.X);
            Assert.AreEqual(0, root.Y);

            var bullet1 = TestManager.Bullets[1];
            Assert.AreEqual(0, bullet1.X, 0.00001f);
            Assert.AreEqual(20, bullet1.Y, 0.00001f);
            Assert.True(bullet1.Action.IsCompleted);
            Assert.False(bullet1.IsVanished);

            var bullet2 = TestManager.Bullets[2];
            Assert.AreEqual(1, bullet2.X, 0.00001f);
            Assert.AreEqual(0, bullet2.Y, 0.00001f);
            Assert.True(bullet2.Action.IsCompleted);
            Assert.False(bullet2.IsVanished);
        }
    }
}
