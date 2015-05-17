using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Tamago.Tests
{
    [TestFixture]
    public class BulletTest : TestBase
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
            var bullet = TestManager.Bullets.Find(b => b.IsTopLevel == false);

            // root does not move and vanishes once tasks have completed
            Assert.NotNull(root);
            Assert.AreEqual(0, root.X);
            Assert.AreEqual(0, root.Y);
            Assert.True(root.Action.IsCompleted);
            Assert.True(root.IsVanished);

            // spawned bullets should move on the next frame
            Assert.NotNull(bullet);
            Assert.AreEqual(0, bullet.X);
            Assert.AreEqual(0, bullet.Y);
            Assert.True(bullet.Action.IsCompleted);
            Assert.False(bullet.IsVanished);

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);

            // root does not move
            Assert.NotNull(root);
            Assert.AreEqual(0, root.X);
            Assert.AreEqual(0, root.Y);
            Assert.True(root.Action.IsCompleted);
            Assert.True(root.IsVanished);

            Assert.NotNull(bullet);
            Assert.AreEqual(0.5f, bullet.X);
            Assert.AreEqual(-(float)(0.5 * Math.Sqrt(3)), bullet.Y);
            Assert.True(bullet.Action.IsCompleted);
            Assert.False(bullet.IsVanished);
        }

        [Test]
        public void BulletPositionUpdatedBasedOnSpeedDirectionAndVelocities()
        {
            var bullet = TestManager.CreateBullet();
            bullet.SetPattern(ActionDef.Default, isTopLevel: false);

            bullet.X = -1.7f;
            bullet.Y = 4.9f;

            bullet.Speed = 4.2f;
            bullet.Direction = 2.3f;
            bullet.VelocityX = 1.2f;
            bullet.VelocityY = 3.4f;

            bullet.NewSpeed = null;
            bullet.NewDirection = null;
            bullet.NewVelocityX = null;
            bullet.NewVelocityY = null;

            bullet.Update();
            Assert.AreEqual(2.63196f, bullet.X, 0.00001f);
            Assert.AreEqual(11.09836f, bullet.Y, 0.00001f);
            Assert.AreEqual(4.2f, bullet.Speed);
            Assert.AreEqual(2.3f, bullet.Direction);
            Assert.AreEqual(1.2f, bullet.VelocityX);
            Assert.AreEqual(3.4f, bullet.VelocityY);
        }

        [Test]
        public void BulletPositionUpdatedAfterNewValuesAreTransferred()
        {
            var bullet = TestManager.CreateBullet();
            bullet.SetPattern(ActionDef.Default, isTopLevel: false);

            bullet.X = -1.7f;
            bullet.Y = 4.9f;

            bullet.Speed = 4.2f;
            bullet.Direction = 2.3f;
            bullet.VelocityX = 1.2f;
            bullet.VelocityY = 3.4f;

            bullet.NewSpeed = 2.4f;
            bullet.NewDirection = 3.2f;
            bullet.NewVelocityX = 2.1f;
            bullet.NewVelocityY = 4.3f;

            bullet.Update();
            Assert.AreEqual(0.25990f, bullet.X, 0.00001f);
            Assert.AreEqual(11.59591f, bullet.Y, 0.00001f);
            Assert.AreEqual(2.4f, bullet.Speed);
            Assert.AreEqual(MathHelper.NormalizeAngle(3.2f), bullet.Direction);
            Assert.AreEqual(2.1f, bullet.VelocityX);
            Assert.AreEqual(4.3f, bullet.VelocityY);
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
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var root = TestManager.Bullets[0];
            Assert.True(root.IsTopLevel);
            Assert.AreEqual(0, root.X);
            Assert.AreEqual(0, root.Y);

            var bullet1 = TestManager.Bullets[1];
            Assert.AreEqual(0, bullet1.X, 0.00001f);
            Assert.AreEqual(0, bullet1.Y, 0.00001f);
            Assert.False(bullet1.Action.IsCompleted);
            Assert.False(bullet1.IsVanished);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(0, bullet1.X, 0.00001f);
            Assert.AreEqual(20, bullet1.Y, 0.00001f);
            Assert.True(bullet1.Action.IsCompleted);
            Assert.False(bullet1.IsVanished);

            var bullet2 = TestManager.Bullets[2];
            Assert.AreEqual(0, bullet2.X, 0.00001f);
            Assert.AreEqual(0, bullet2.Y, 0.00001f);
            Assert.True(bullet2.Action.IsCompleted);
            Assert.False(bullet2.IsVanished);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(0, bullet1.X, 0.00001f);
            Assert.AreEqual(40, bullet1.Y, 0.00001f);
            Assert.True(bullet1.Action.IsCompleted);
            Assert.False(bullet1.IsVanished);

            Assert.AreEqual(1, bullet2.X, 0.00001f);
            Assert.AreEqual(0, bullet2.Y, 0.00001f);
            Assert.True(bullet2.Action.IsCompleted);
            Assert.False(bullet2.IsVanished);
        }

        [Test]
        [Ignore]
        public void MutableStateInTasksAreNotShared()
        {
            // play with ActionRef used in multiple places
        }
    }
}
