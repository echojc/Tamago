using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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
            Assert.True(root.IsCompleted);
            Assert.True(root.IsVanished);

            // spawned bullets should move on the next frame
            Assert.NotNull(bullet);
            Assert.AreEqual(0, bullet.X);
            Assert.AreEqual(0, bullet.Y);
            Assert.True(bullet.IsCompleted);
            Assert.False(bullet.IsVanished);

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);

            // root does not move
            Assert.NotNull(root);
            Assert.AreEqual(0, root.X);
            Assert.AreEqual(0, root.Y);
            Assert.True(root.IsCompleted);
            Assert.True(root.IsVanished);

            Assert.NotNull(bullet);
            Assert.AreEqual(0.5f, bullet.X);
            Assert.AreEqual(-(float)(0.5 * Math.Sqrt(3)), bullet.Y);
            Assert.True(bullet.IsCompleted);
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
            Assert.False(bullet1.IsCompleted);
            Assert.False(bullet1.IsVanished);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(0, bullet1.X, 0.00001f);
            Assert.AreEqual(20, bullet1.Y, 0.00001f);
            Assert.True(bullet1.IsCompleted);
            Assert.False(bullet1.IsVanished);

            var bullet2 = TestManager.Bullets[2];
            Assert.AreEqual(0, bullet2.X, 0.00001f);
            Assert.AreEqual(0, bullet2.Y, 0.00001f);
            Assert.True(bullet2.IsCompleted);
            Assert.False(bullet2.IsVanished);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(0, bullet1.X, 0.00001f);
            Assert.AreEqual(40, bullet1.Y, 0.00001f);
            Assert.True(bullet1.IsCompleted);
            Assert.False(bullet1.IsVanished);

            Assert.AreEqual(1, bullet2.X, 0.00001f);
            Assert.AreEqual(0, bullet2.Y, 0.00001f);
            Assert.True(bullet2.IsCompleted);
            Assert.False(bullet2.IsVanished);
        }

        [Test]
        public void BulletsRunMultipleActionsVanishAfterAllComplete()
        {
            var root = CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <direction type=""absolute"">200</direction>
                    <bullet>
                      <action>
                        <wait>1</wait>
                        <fire>
                          <direction type=""absolute"">50</direction>
                          <bullet/>
                        </fire>
                      </action>
                      <action>
                        <fire>
                          <direction type=""absolute"">100</direction>
                          <bullet/>
                        </fire>
                      </action>
                    </bullet>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            // bullets don't run on the turn they're created
            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);
            var bullet = TestManager.Bullets.Last();
            Assert.AreEqual(MathHelper.ToRadians(200), bullet.Direction);
            Assert.True(root.IsVanished);
            Assert.True(root.IsCompleted);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);
            Assert.AreEqual(MathHelper.ToRadians(100), TestManager.Bullets.Last().Direction);
            Assert.False(bullet.IsCompleted);
            Assert.False(bullet.IsVanished);

            TestManager.Update();
            Assert.AreEqual(4, TestManager.Bullets.Count);
            Assert.AreEqual(MathHelper.ToRadians(50), TestManager.Bullets.Last().Direction);
            Assert.True(bullet.IsCompleted);
            Assert.False(bullet.IsVanished);

            TestManager.Update();
            Assert.AreEqual(4, TestManager.Bullets.Count);
            Assert.True(bullet.IsCompleted);
            Assert.False(bullet.IsVanished);
        }

        [Test]
        public void MutableStateInTasksAreNotShared()
        {
            var root = CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <direction type=""absolute"">0</direction>
                    <bullet>
                      <actionRef label=""b""/>
                    </bullet>
                  </fire>
                  <fire>
                    <direction type=""absolute"">90</direction>
                    <bullet>
                      <actionRef label=""a""/>
                    </bullet>
                  </fire>
                  <fire>
                    <direction type=""absolute"">180</direction>
                    <bullet>
                      <actionRef label=""b""/>
                    </bullet>
                  </fire>
                  <fire>
                    <direction type=""absolute"">210</direction>
                    <bullet>
                      <actionRef label=""a""/>
                      <actionRef label=""b""/>
                    </bullet>
                  </fire>
                </action>
                <action label=""a"">
                  <wait>1</wait>
                  <changeSpeed>
                    <speed>4</speed>
                    <term>2</term>
                  </changeSpeed>
                </action>
                <action label=""b"">
                  <changeDirection>
                    <direction type=""absolute"">90</direction>
                    <term>3</term>
                  </changeDirection>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(5, TestManager.Bullets.Count);

            var b1 = TestManager.Bullets[1];
            var b2 = TestManager.Bullets[2];
            var b3 = TestManager.Bullets[3];
            var b4 = TestManager.Bullets[4];

            Assert.AreEqual(1, b1.Speed);
            Assert.AreEqual(MathHelper.ToRadians(0), b1.Direction, 0.00001f);
            Assert.AreEqual(1, b2.Speed);
            Assert.AreEqual(MathHelper.ToRadians(90), b2.Direction, 0.00001f);
            Assert.AreEqual(1, b3.Speed);
            Assert.AreEqual(MathHelper.ToRadians(180), b3.Direction, 0.00001f);
            Assert.AreEqual(1, b4.Speed);
            Assert.AreEqual(MathHelper.ToRadians(210), b4.Direction, 0.00001f);

            TestManager.Update();
            Assert.AreEqual(1, b1.Speed);
            Assert.AreEqual(MathHelper.ToRadians(30), b1.Direction, 0.00001f);
            Assert.AreEqual(1, b2.Speed);
            Assert.AreEqual(MathHelper.ToRadians(90), b2.Direction, 0.00001f);
            Assert.AreEqual(1, b3.Speed);
            Assert.AreEqual(MathHelper.ToRadians(150), b3.Direction, 0.00001f);
            Assert.AreEqual(1, b4.Speed);
            Assert.AreEqual(MathHelper.ToRadians(170), b4.Direction, 0.00001f);

            TestManager.Update();
            Assert.AreEqual(1, b1.Speed);
            Assert.AreEqual(MathHelper.ToRadians(60), b1.Direction, 0.00001f);
            Assert.AreEqual(2.5, b2.Speed);
            Assert.AreEqual(MathHelper.ToRadians(90), b2.Direction, 0.00001f);
            Assert.AreEqual(1, b3.Speed);
            Assert.AreEqual(MathHelper.ToRadians(120), b3.Direction, 0.00001f);
            Assert.AreEqual(2.5, b4.Speed);
            Assert.AreEqual(MathHelper.ToRadians(130), b4.Direction, 0.00001f);

            // stabilises once animations are done
            for (int i = 0; i < 2; i++)
            {
                TestManager.Update();
                Assert.AreEqual(1, b1.Speed);
                Assert.AreEqual(MathHelper.ToRadians(90), b1.Direction, 0.00001f);
                Assert.AreEqual(4, b2.Speed);
                Assert.AreEqual(MathHelper.ToRadians(90), b2.Direction, 0.00001f);
                Assert.AreEqual(1, b3.Speed);
                Assert.AreEqual(MathHelper.ToRadians(90), b3.Direction, 0.00001f);
                Assert.AreEqual(4, b4.Speed);
                Assert.AreEqual(MathHelper.ToRadians(90), b4.Direction, 0.00001f);
            }
        }

        [Test]
        public void ThrowsArgumentNullOnSetParamsToNull()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top""/>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);
            var root = TestManager.Bullets[0];
            Assert.Throws<ArgumentNullException>(() => root.SetParams(null));
        }

        [Test]
        public void ByDefaultParamsResolveToZero()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <direction type=""absolute"">$1</direction>
                    <speed>$2</speed>
                    <bullet/>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);
            var root = TestManager.Bullets[0];

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var b1 = TestManager.Bullets.Last();
            Assert.AreEqual(0, b1.Direction);
            Assert.AreEqual(0, b1.Speed);
        }

        [Test]
        public void OutOfBoundsParamsResolveToZero()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <direction type=""absolute"">$1</direction>
                    <speed>$2</speed>
                    <bullet/>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            var root = TestManager.Bullets[0];
            root.SetParams(new[] { 22.22f });

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var b2 = TestManager.Bullets.Last();
            Assert.AreEqual(MathHelper.ToRadians(22.22f), b2.Direction);
            Assert.AreEqual(0, b2.Speed);
        }

        [Test]
        public void UsesParameters()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <direction type=""absolute"">$1</direction>
                    <speed>$2</speed>
                    <bullet/>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            var root = TestManager.Bullets[0];
            root.SetParams(new[] { 33.33f, 1.2f, 4.4f });

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var b3 = TestManager.Bullets.Last();
            Assert.AreEqual(MathHelper.ToRadians(33.33f), b3.Direction);
            Assert.AreEqual(1.2f, b3.Speed);
        }
    }
}
