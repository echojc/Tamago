﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class BulletPatternTest : TestBase
    {
        [Test]
        public void ThrowsArgumentNullIfParsingNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BulletPattern(null));
        }

        [Test]
        public void ThrowsParseExceptionIfXmlIsInvalid()
        {
            var xml = "<hi";
            Assert.Throws<ParseException>(() => new BulletPattern(xml));
        }

        [Test]
        public void ThrowsParseExceptionIfRootElementIsNotBulletMl()
        {
            var xml = @"<action label=""top""/>";
            Assert.Throws<ParseException>(() => new BulletPattern(xml));
        }

        [Test]
        public void ParsesVersion()
        {
            var xml = @"<bulletml version=""1""/>";
            var pattern = new BulletPattern(xml);
            Assert.AreEqual(1, pattern.Version);
        }

        [Test]
        public void UsesLatestVersionIfMissing()
        {
            var xml = @"<bulletml/>";
            var pattern = new BulletPattern(xml);
            Assert.AreEqual(BulletPattern.LatestVersion, pattern.Version);
        }

        #region Actions

        [Test]
        public void ThrowsParseExceptionIfAnyRootLevelActionHasNoLabel()
        {
            var xml = @"
              <bulletml>
                <action><fire><bullet/></fire></action>
                <action label=""top""><fire><bullet/></fire></action>
              </bulletml>
            ";
            Assert.Throws<ParseException>(() => new BulletPattern(xml));
        }

        [Test]
        public void ThrowsParseExceptionIfMultipleActionsHaveTheSameLabel()
        {
            var xml = @"
              <bulletml>
                <action label=""top""><fire><bullet/></fire></action>
                <action label=""top""><fire><bullet/></fire></action>
              </bulletml>
            ";
            Assert.Throws<ParseException>(() => new BulletPattern(xml));
        }

        [Test]
        public void AllowsNestedActionsWithNoLabel()
        {
            var xml = @"
              <bulletml>
                <action label=""top"">
                  <action><fire><bullet/></fire></action>
                </action>
              </bulletml>
            ";

            CreateTopLevelBullet(xml);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);
        }

        [Test]
        public void LooksUpActionByLabel()
        {
            var xml = @"
              <bulletml>
                <action label=""top"">
                  <fire><speed>3</speed><bullet/></fire>
                </action>
                <action label=""abc"">
                  <fire><speed>7</speed><bullet/></fire>
                </action>
              </bulletml>
            ";

            var pattern = new BulletPattern(xml);
            var action = pattern.CopyAction("abc");
            Assert.AreEqual("abc", action.Label);

            var bullet = TestManager.CreateBullet();
            bullet.SetPattern(action, isTopLevel: false);
            Assert.AreEqual(1, TestManager.Bullets.Count);

            TestManager.Update();
            Assert.AreEqual(2, TestManager.Bullets.Count);

            var created = TestManager.Bullets.Last();
            Assert.AreEqual(7, created.Speed);
        }

        [Test]
        public void CopiesActionsOnLookup()
        {
            var xml = @"
              <bulletml>
                <action label=""top"">
                  <fire><speed>3</speed><bullet/></fire>
                </action>
              </bulletml>
            ";

            var pattern = new BulletPattern(xml);
            var action1 = pattern.CopyAction("top");
            Assert.AreEqual("top", action1.Label);

            var bullet = TestManager.CreateBullet();
            bullet.SetPattern(action1, isTopLevel: false);
            TestManager.Update();

            Assert.True(action1.IsCompleted);
            var action2 = pattern.CopyAction("top");
            Assert.False(action2.IsCompleted);
            Assert.AreNotSame(action1, action2);
        }

        #endregion

        #region Fires

        [Test]
        public void ThrowsParseExceptionIfAnyRootLevelFireHasNoLabel()
        {
            var xml = @"
              <bulletml>
                <action label=""top""/>
                <fire><bullet/></fire>
                <fire label=""foo""><bullet/></fire>
              </bulletml>
            ";
            Assert.Throws<ParseException>(() => new BulletPattern(xml));
        }

        [Test]
        public void ThrowsParseExceptionIfMultipleFiresHaveTheSameLabel()
        {
            var xml = @"
              <bulletml>
                <fire label=""foo""><bullet/></fire>
                <fire label=""foo""><bullet/></fire>
              </bulletml>
            ";
            Assert.Throws<ParseException>(() => new BulletPattern(xml));
        }

        [Test]
        public void AllowsNestedFiresWithNoLabel()
        {
            var xml = @"
              <bulletml>
                <fire label=""foo"">
                  <bullet>
                    <action>
                      <fire><bullet/></fire>
                    </action>
                  </bullet>
                </fire>
              </bulletml>
            ";

            Assert.DoesNotThrow(() => new BulletPattern(xml));
        }

        [Test]
        public void LooksUpFireByLabel()
        {
            var xml = @"
              <bulletml>
                <fire label=""foo"">
                  <speed>3</speed>
                  <direction>10</direction>
                  <bullet/>
                </fire>
                <fire label=""bar"">
                  <speed>7</speed>
                  <direction>17</direction>
                  <bullet/>
                </fire>
              </bulletml>
            ";

            var pattern = new BulletPattern(xml);

            var fire1 = pattern.CopyFire("foo");
            Assert.AreEqual("foo", fire1.Label);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 3), fire1.Speed);
            Assert.AreEqual(new Direction(DirectionType.Aim, 10), fire1.Direction);

            var fire2 = pattern.CopyFire("bar");
            Assert.AreEqual("bar", fire2.Label);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 7), fire2.Speed);
            Assert.AreEqual(new Direction(DirectionType.Aim, 17), fire2.Direction);
        }

        [Test]
        public void CopiesFiresOnLookup()
        {
            var xml = @"
              <bulletml>
                <fire label=""foo"">
                  <speed>3</speed>
                  <direction>10</direction>
                  <bullet/>
                </fire>
              </bulletml>
            ";

            var pattern = new BulletPattern(xml);
            var fire1 = pattern.CopyFire("foo");
            Assert.AreEqual("foo", fire1.Label);

            var bullet = TestManager.CreateBullet();
            fire1.Run(bullet);

            Assert.True(fire1.IsCompleted);
            var fire2 = pattern.CopyFire("foo");
            Assert.False(fire2.IsCompleted);
            Assert.AreNotSame(fire1, fire2);
        }

        #endregion

        #region Bullets

        [Test]
        public void ThrowsParseExceptionIfAnyRootLevelBulletHasNoLabel()
        {
            var xml = @"
              <bulletml>
                <bullet/>
                <bullet label=""foo""/>
              </bulletml>
            ";
            Assert.Throws<ParseException>(() => new BulletPattern(xml));
        }

        [Test]
        public void ThrowsParseExceptionIfMultipleBulletsHaveTheSameLabel()
        {
            var xml = @"
              <bulletml>
                <bullet label=""foo""/>
                <bullet label=""foo""/>
              </bulletml>
            ";
            Assert.Throws<ParseException>(() => new BulletPattern(xml));
        }

        [Test]
        public void AllowsNestedBulletsWithNoLabel()
        {
            var xml = @"
              <bulletml>
                <bullet label=""foo"">
                  <action>
                    <fire><bullet/></fire>
                  </action>
                </bullet>
              </bulletml>
            ";

            Assert.DoesNotThrow(() => new BulletPattern(xml));
        }

        [Test]
        public void LooksUpBulletByLabel()
        {
            var xml = @"
              <bulletml>
                <bullet label=""foo"">
                  <speed>3</speed>
                  <direction>10</direction>
                </bullet>
                <bullet label=""bar"">
                  <speed>7</speed>
                  <direction>17</direction>
                </bullet>
              </bulletml>
            ";

            var pattern = new BulletPattern(xml);

            var bullet1 = pattern.CopyBullet("foo");
            Assert.AreEqual("foo", bullet1.Label);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 3), bullet1.Speed);
            Assert.AreEqual(new Direction(DirectionType.Aim, 10), bullet1.Direction);

            var bullet2 = pattern.CopyBullet("bar");
            Assert.AreEqual("bar", bullet2.Label);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 7), bullet2.Speed);
            Assert.AreEqual(new Direction(DirectionType.Aim, 17), bullet2.Direction);
        }

        #endregion 
    }
}
