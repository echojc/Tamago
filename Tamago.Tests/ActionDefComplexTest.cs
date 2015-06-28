using NUnit.Framework;
using System;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class ActionDefComplexTest : TestBase
    {
        [Test]
        public void LastIncompleteChangeSpeedTakesPriority()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire><bullet><action>
                    <changeSpeed><speed>5</speed><term>4</term></changeSpeed>
                    <wait>1</wait>
                    <changeSpeed><speed>3</speed><term>2</term></changeSpeed>
                  </action></bullet></fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var bullet = TestManager.Bullets[1];
            Assert.AreEqual(1, bullet.Speed);

            // changeSpeed #1, frame 1
            TestManager.Update();
            Assert.AreEqual(2f, bullet.Speed);

            // changeSpeed #2, frame 1
            TestManager.Update();
            Assert.AreEqual(2.5f, bullet.Speed);

            // changeSpeed #2, frame 2
            TestManager.Update();
            Assert.AreEqual(3f, bullet.Speed);

            // changeSpeed #1, frame 4
            TestManager.Update();
            Assert.AreEqual(5f, bullet.Speed);

            // static
            TestManager.Update();
            Assert.AreEqual(5f, bullet.Speed);
        }

        [Test]
        public void LastIncompleteChangeDirectionTakesPriority()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <direction type=""absolute"">180</direction>
                    <speed>0</speed> <!-- easier to work out angles -->
                    <bullet><action>
                      <changeDirection><direction>40</direction><term>4</term></changeDirection>
                      <wait>1</wait>
                      <changeDirection><direction>-40</direction><term>2</term></changeDirection>
                    </action></bullet>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var bullet = TestManager.Bullets[1];
            Assert.AreEqual(MathHelper.ToRadians(180), bullet.Direction);

            // changeDirection #1, frame 1
            TestManager.Update();
            Assert.AreEqual(MathHelper.ToRadians(190), bullet.Direction, 0.00001f);

            // changeDirection #2, frame 1
            TestManager.Update();
            Assert.AreEqual(MathHelper.ToRadians(165), bullet.Direction, 0.00001f);

            // changeDirection #2, frame 2
            TestManager.Update();
            Assert.AreEqual(MathHelper.ToRadians(140), bullet.Direction, 0.00001f);

            // changeDirection #1, frame 4
            TestManager.Update();
            Assert.AreEqual(MathHelper.ToRadians(220), bullet.Direction, 0.00001f);

            // static
            TestManager.Update();
            Assert.AreEqual(MathHelper.ToRadians(220), bullet.Direction, 0.00001f);
        }

        [Test]
        public void LastIncompleteAccelTakesPriority()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <direction type=""absolute"">180</direction>
                    <speed>0</speed> <!-- easier to work out velocities -->
                    <bullet><action>
                      <accel>
                        <horizontal>2</horizontal>
                        <vertical>4</vertical>
                        <term>4</term>
                      </accel>
                      <wait>1</wait>
                      <accel>
                        <horizontal>-2</horizontal>
                        <vertical>-1</vertical>
                        <term>2</term>
                      </accel>
                    </action></bullet>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var bullet = TestManager.Bullets[1];
            Assert.AreEqual(0, bullet.VelocityX);
            Assert.AreEqual(0, bullet.VelocityY);

            // accel #1, frame 1
            TestManager.Update();
            Assert.AreEqual(0.5f, bullet.VelocityX);
            Assert.AreEqual(1.0f, bullet.VelocityY);

            // accel #2, frame 1
            TestManager.Update();
            Assert.AreEqual(-0.75f, bullet.VelocityX);
            Assert.AreEqual(0.0f, bullet.VelocityY);

            // accel #2, frame 2
            TestManager.Update();
            Assert.AreEqual(-2.0f, bullet.VelocityX);
            Assert.AreEqual(-1.0f, bullet.VelocityY);

            // accel #1, frame 4
            TestManager.Update();
            Assert.AreEqual(2.0f, bullet.VelocityX);
            Assert.AreEqual(4.0f, bullet.VelocityY);

            // static
            TestManager.Update();
            Assert.AreEqual(2.0f, bullet.VelocityX);
            Assert.AreEqual(4.0f, bullet.VelocityY);
        }

        [Test]
        public void ResolvesRefsInClonedRepeatActions()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <repeat>
                    <times>1</times>
                    <actionRef label=""foo""/>
                  </repeat>
                </action>
                <action label=""foo"">
                  <repeat>
                    <times>1</times>
                    <actionRef label=""bar""/>
                  </repeat>
                </action>
                <action label=""bar"">
                  <fire><bullet/></fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);
        }

        [Test]
        public void ResolvesRefsInClonedActionActions()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <actionRef label=""foo""/>
                </action>
                <action label=""foo"">
                  <actionRef label=""bar""/>
                </action>
                <action label=""bar"">
                  <fire><bullet/></fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);
        }

        [Test]
        public void ParamPassingToMakeCircles()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""cycle"">
                  <repeat>
                    <times>$1</times>
                    <action>
                      <fire>
                        <direction type=""sequence"">360/$1</direction>
                        <bullet/>
                      </fire>
                      <wait>$2</wait>
                    </action>
                  </repeat>
                </action>

                <action label=""circle"">
                  <actionRef label=""cycle"">
                    <param>$1</param>
                    <param>0</param>
                  </actionRef>
                </action>

                <action label=""top"">
                  <actionRef label=""circle"">
                    <param>12</param>
                  </actionRef>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(13, TestManager.Bullets.Count);
            TestManager.Bullets.ForEach(b =>
                {
                    Assert.AreEqual(0, b.X);
                    Assert.AreEqual(0, b.Y);
                });

            TestManager.Update();
            var r32 = (float)(Math.Sqrt(3) / 2);
            Assert.AreEqual(13, TestManager.Bullets.Count);
            var x = new float[]{
                0,
                0.5f,   r32,  1,
                 r32,  0.5f,  0,
               -0.5f,  -r32, -1,
                -r32, -0.5f,  0,
            };
            var y = new float[]{
                0,
                -r32, -0.5f,  0,
                0.5f,   r32,  1,
                 r32,  0.5f,  0,
               -0.5f,  -r32, -1,
            };

            for (int i = 0; i < TestManager.Bullets.Count; i++)
            {
                Assert.AreEqual(x[i], TestManager.Bullets[i].X, 0.00001f);
                Assert.AreEqual(y[i], TestManager.Bullets[i].Y, 0.00001f);
            }
        }

        [Test]
        public void VanishesImmediatelyHaltsExecution()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <direction type=""absolute"">90</direction>
                    <bullet>
                      <action>
                        <fire>
                          <direction type=""absolute"">270</direction>
                          <bullet/>
                        </fire>
                        <vanish/>
                        <fire>
                          <direction type=""absolute"">280</direction>
                          <bullet/>
                        </fire>
                      </action>
                    </bullet>
                  </fire>
                  <vanish/>
                  <fire>
                    <direction type=""absolute"">180</direction>
                    <bullet/>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);
            Assert.True(TestManager.Bullets[0].IsVanished);
            Assert.AreEqual(MathHelper.ToRadians(90), TestManager.Bullets[1].Direction);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);
            Assert.AreEqual(MathHelper.ToRadians(90), TestManager.Bullets[1].Direction);
            Assert.True(TestManager.Bullets[1].IsVanished);
            Assert.AreEqual(MathHelper.ToRadians(270), TestManager.Bullets[2].Direction);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);
            Assert.False(TestManager.Bullets[2].IsVanished);
        }

        [Test]
        public void ComplexParamPassingWithRepeatContextVars()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <actionRef label=""spray"">
                    <param>20</param><!-- angle -->
                    <param>4</param><!-- count -->
                  </actionRef>
                </action>
                <action label=""spray"">
                  <fire>
                    <direction type=""absolute"">180</direction>
                    <bullet>
                      <action>
                        <repeat>
                          <times>$2</times>
                          <action>
                            <fire>
                              <direction type=""aim"">$i * ($1 / ($times - 1)) - ($1 / 2)</direction>
                              <bullet/>
                            </fire>
                          </action>
                        </repeat>
                        <vanish/>
                      </action>
                    </bullet>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);
            Assert.True(TestManager.Bullets[0].IsVanished);
            Assert.False(TestManager.Bullets[1].IsVanished);

            TestManager.Update();
            Assert.AreEqual(6, TestManager.Bullets.Count);
            Assert.True(TestManager.Bullets[1].IsVanished);
            Assert.AreEqual(MathHelper.ToRadians(180), TestManager.Bullets[1].Direction);

            var directions = new float[] { -10f, -10f / 3, 10f / 3, 10f };
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(
                    MathHelper.ToRadians(180 + directions[i]),
                    TestManager.Bullets[i + 2].Direction,
                    0.00001f);
            }
        }
    }
}
