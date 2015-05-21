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
        [Ignore]
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
        [Ignore]
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
    }
}
