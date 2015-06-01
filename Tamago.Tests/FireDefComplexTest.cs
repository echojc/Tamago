using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class FireDefComplexTest : TestBase
    {
        [Test]
        public void FireSequenceTakesPrecedenceOverBulletSequence()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <repeat>
                    <times>4</times>
                    <action>
                      <fire>
                        <direction type=""sequence"">90</direction>
                        <speed type=""sequence"">1</speed>
                        <bullet>
                          <direction type=""sequence"">10</direction>
                          <speed type=""sequence"">2</speed>
                        </bullet>
                      </fire>
                    </action>
                  </repeat>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            // bullets don't run the frame they're fired
            TestManager.Update();
            Assert.AreEqual(5, TestManager.Bullets.Count);
            TestManager.Bullets.ForEach(b =>
                {
                    Assert.AreEqual(0, b.X);
                    Assert.AreEqual(0, b.Y);
                });

            TestManager.Update();
            var x = new float[]{ 0, 2, 0, -4, 0 };
            var y = new float[]{ 0, 0, 3, 0, -5 };

            for (int i = 0; i < TestManager.Bullets.Count; i++)
            {
                Assert.AreEqual(x[i], TestManager.Bullets[i].X, 0.00001f);
                Assert.AreEqual(y[i], TestManager.Bullets[i].Y, 0.00001f);
            }
        }

        [Test]
        public void FireDirectionSequenceWorks()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <repeat>
                    <times>4</times>
                    <action>
                      <fire>
                        <direction type=""sequence"">90</direction>
                        <bullet/>
                      </fire>
                    </action>
                  </repeat>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            // bullets don't run the frame they're fired
            TestManager.Update();
            Assert.AreEqual(5, TestManager.Bullets.Count);
            TestManager.Bullets.ForEach(b =>
                {
                    Assert.AreEqual(0, b.X);
                    Assert.AreEqual(0, b.Y);
                });

            TestManager.Update();
            var x = new float[]{ 0, 1, 0, -1, 0 };
            var y = new float[]{ 0, 0, 1, 0, -1 };

            for (int i = 0; i < TestManager.Bullets.Count; i++)
            {
                Assert.AreEqual(x[i], TestManager.Bullets[i].X, 0.00001f);
                Assert.AreEqual(y[i], TestManager.Bullets[i].Y, 0.00001f);
            }
        }

        [Test]
        public void FireDirectionSequenceWorksAfterAim()
        {
            // direction = 150 degrees
            TestManager.SetPlayerPosition(1, (float)Math.Sqrt(3));

            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <direction type=""aim"">0</direction>
                    <bullet/>
                  </fire>
                  <fire>
                    <direction type=""sequence"">40</direction>
                    <bullet/>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(MathHelper.ToRadians(150), TestManager.Bullets[1].Direction, 0.00001f);
            Assert.AreEqual(MathHelper.ToRadians(190), TestManager.Bullets[2].Direction, 0.00001f);
        }

        [Test]
        public void FireDirectionSequenceWorksAfterRelative()
        {
            var root = CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <direction type=""relative"">30</direction>
                    <bullet/>
                  </fire>
                  <fire>
                    <direction type=""sequence"">30</direction>
                    <bullet/>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);
            root.Direction = MathHelper.ToRadians(30);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(MathHelper.ToRadians(60), TestManager.Bullets[1].Direction, 0.00001f);
            Assert.AreEqual(MathHelper.ToRadians(90), TestManager.Bullets[2].Direction, 0.00001f);
        }

        [Test]
        public void FireDirectionSequenceWorksAfterAbsolute()
        {
            var root = CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <direction type=""absolute"">90</direction>
                    <bullet/>
                  </fire>
                  <fire>
                    <direction type=""sequence"">30</direction>
                    <bullet/>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(MathHelper.ToRadians(90), TestManager.Bullets[1].Direction, 0.00001f);
            Assert.AreEqual(MathHelper.ToRadians(120), TestManager.Bullets[2].Direction, 0.00001f);
        }

        [Test]
        public void FireSpeedSequenceWorks()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <repeat>
                    <times>4</times>
                    <action>
                      <fire>
                        <direction type=""absolute"">180</direction>
                        <speed type=""sequence"">1</speed>
                        <bullet/>
                      </fire>
                    </action>
                  </repeat>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            // bullets don't run the frame they're fired
            TestManager.Update();
            Assert.AreEqual(5, TestManager.Bullets.Count);
            TestManager.Bullets.ForEach(b =>
                {
                    Assert.AreEqual(0, b.X);
                    Assert.AreEqual(0, b.Y);
                });

            TestManager.Update();
            // fireSpeed starts at 1
            var y = new float[]{ 0, 2, 3, 4, 5};

            for (int i = 0; i < TestManager.Bullets.Count; i++)
            {
                Assert.AreEqual(0, TestManager.Bullets[i].X, 0.00001f);
                Assert.AreEqual(y[i], TestManager.Bullets[i].Y, 0.00001f);
            }
        }

        [Test]
        public void FireSpeedSequenceWorksAfterRelative()
        {
            var root = CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <speed type=""relative"">2</speed>
                    <bullet/>
                  </fire>
                  <fire>
                    <speed type=""sequence"">-7</speed>
                    <bullet/>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);
            root.Speed = 3;

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(5, TestManager.Bullets[1].Speed);
            Assert.AreEqual(-2, TestManager.Bullets[2].Speed);
        }

        [Test]
        public void FireSpeedSequenceWorksAfterAbsolute()
        {
            var root = CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <speed type=""absolute"">2</speed>
                    <bullet/>
                  </fire>
                  <fire>
                    <speed type=""sequence"">3</speed>
                    <bullet/>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(2, TestManager.Bullets[1].Speed);
            Assert.AreEqual(5, TestManager.Bullets[2].Speed);
        }

        [Test]
        public void FireBulletDirectionSequenceWorks()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <repeat>
                    <times>4</times>
                    <action>
                      <fire>
                        <bullet>
                          <direction type=""sequence"">90</direction>
                        </bullet>
                      </fire>
                    </action>
                  </repeat>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            // bullets don't run the frame they're fired
            TestManager.Update();
            Assert.AreEqual(5, TestManager.Bullets.Count);
            TestManager.Bullets.ForEach(b =>
                {
                    Assert.AreEqual(0, b.X);
                    Assert.AreEqual(0, b.Y);
                });

            TestManager.Update();
            var x = new float[]{ 0, 1, 0, -1, 0 };
            var y = new float[]{ 0, 0, 1, 0, -1 };

            for (int i = 0; i < TestManager.Bullets.Count; i++)
            {
                Assert.AreEqual(x[i], TestManager.Bullets[i].X, 0.00001f);
                Assert.AreEqual(y[i], TestManager.Bullets[i].Y, 0.00001f);
            }
        }

        [Test]
        public void FireBulletDirectionSequenceWorksAfterAim()
        {
            // direction = 150 degrees
            TestManager.SetPlayerPosition(1, (float)Math.Sqrt(3));

            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <bullet>
                      <direction type=""aim"">0</direction>
                    </bullet>
                  </fire>
                  <fire>
                    <bullet>
                      <direction type=""sequence"">40</direction>
                    </bullet>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(MathHelper.ToRadians(150), TestManager.Bullets[1].Direction, 0.00001f);
            Assert.AreEqual(MathHelper.ToRadians(190), TestManager.Bullets[2].Direction, 0.00001f);
        }

        [Test]
        public void FireBulletDirectionSequenceWorksAfterRelative()
        {
            var root = CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <bullet>
                      <direction type=""relative"">30</direction>
                    </bullet>
                  </fire>
                  <fire>
                    <bullet>
                      <direction type=""sequence"">30</direction>
                    </bullet>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);
            root.Direction = MathHelper.ToRadians(30);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(MathHelper.ToRadians(60), TestManager.Bullets[1].Direction, 0.00001f);
            Assert.AreEqual(MathHelper.ToRadians(90), TestManager.Bullets[2].Direction, 0.00001f);
        }

        [Test]
        public void FireBulletDirectionSequenceWorksAfterAbsolute()
        {
            var root = CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <bullet>
                      <direction type=""absolute"">90</direction>
                    </bullet>
                  </fire>
                  <fire>
                    <bullet>
                      <direction type=""sequence"">30</direction>
                    </bullet>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(MathHelper.ToRadians(90), TestManager.Bullets[1].Direction, 0.00001f);
            Assert.AreEqual(MathHelper.ToRadians(120), TestManager.Bullets[2].Direction, 0.00001f);
        }

        [Test]
        public void FireBulletSpeedSequenceWorks()
        {
            CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <repeat>
                    <times>4</times>
                    <action>
                      <fire>
                        <direction type=""absolute"">180</direction>
                        <bullet>
                          <speed type=""sequence"">1</speed>
                        </bullet>
                      </fire>
                    </action>
                  </repeat>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            // bullets don't run the frame they're fired
            TestManager.Update();
            Assert.AreEqual(5, TestManager.Bullets.Count);
            TestManager.Bullets.ForEach(b =>
                {
                    Assert.AreEqual(0, b.X);
                    Assert.AreEqual(0, b.Y);
                });

            TestManager.Update();
            // fireSpeed starts at 1
            var y = new float[]{ 0, 2, 3, 4, 5};

            for (int i = 0; i < TestManager.Bullets.Count; i++)
            {
                Assert.AreEqual(0, TestManager.Bullets[i].X, 0.00001f);
                Assert.AreEqual(y[i], TestManager.Bullets[i].Y, 0.00001f);
            }
        }

        [Test]
        public void FireBulletSpeedSequenceWorksAfterRelative()
        {
            var root = CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <bullet>
                      <speed type=""relative"">2</speed>
                    </bullet>
                  </fire>
                  <fire>
                    <bullet>
                      <speed type=""sequence"">-7</speed>
                    </bullet>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);
            root.Speed = 3;

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(5, TestManager.Bullets[1].Speed);
            Assert.AreEqual(-2, TestManager.Bullets[2].Speed);
        }

        [Test]
        public void FireBulletSpeedSequenceWorksAfterAbsolute()
        {
            var root = CreateTopLevelBullet(@"
              <bulletml>
                <action label=""top"">
                  <fire>
                    <bullet>
                      <speed type=""absolute"">2</speed>
                    </bullet>
                  </fire>
                  <fire>
                    <bullet>
                      <speed type=""sequence"">3</speed>
                    </bullet>
                  </fire>
                </action>
              </bulletml>
            ");
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(3, TestManager.Bullets.Count);

            Assert.AreEqual(2, TestManager.Bullets[1].Speed);
            Assert.AreEqual(5, TestManager.Bullets[2].Speed);
        }
    }
}
