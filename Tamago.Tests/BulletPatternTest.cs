using NUnit.Framework;
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
        public void DoesNotAllowEmptyRoot()
        {
            var xml = @"<bulletml/>";
            Assert.Throws<ParseException>(() => new BulletPattern(xml));
        }

        [Test]
        public void ThrowsParseExceptionIfMissingTopAction()
        {
            var xml = @"
              <bulletml>
                <action label=""hi""><fire><bullet/></fire></action>
              </bulletml>
            ";
            Assert.Throws<ParseException>(() => new BulletPattern(xml));
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
            var action = pattern.FindAction("abc");
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
        public void ThrowsKeyNotFoundExceptionIfActionDoesNotExist()
        {
            var xml = @"
              <bulletml>
                <action label=""top""/>
              </bulletml>
            ";

            var pattern = new BulletPattern(xml);
            Assert.Throws<KeyNotFoundException>(() => pattern.FindAction("abc"));
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
                <action label=""top""/>
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
                <action label=""top""/>
                <fire label=""foo"">
                  <bullet>
                    <action>
                      <fire><bullet/></fire>
                    </action>
                  </bullet>
                </fire>
              </bulletml>
            ";

            Assert.DoesNotThrow(() => CreateTopLevelBullet(xml));
        }

        [Test]
        public void LooksUpFireByLabel()
        {
            var xml = @"
              <bulletml>
                <action label=""top""/>
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

            var fire1 = pattern.FindFire("foo");
            Assert.AreEqual("foo", fire1.Label);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 3), fire1.Speed);
            Assert.AreEqual(new Direction(DirectionType.Aim, 10), fire1.Direction);

            var fire2 = pattern.FindFire("bar");
            Assert.AreEqual("bar", fire2.Label);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 7), fire2.Speed);
            Assert.AreEqual(new Direction(DirectionType.Aim, 17), fire2.Direction);
        }

        [Test]
        public void ThrowsKeyNotFoundExceptionIfFireDoesNotExist()
        {
            var xml = @"
              <bulletml>
                <action label=""top""/>
              </bulletml>
            ";

            var pattern = new BulletPattern(xml);
            Assert.Throws<KeyNotFoundException>(() => pattern.FindFire("abc"));
        }

        #endregion

        [Test]
        public void ThrowsParseExceptionIfAnyRootLevelBulletHasNoLabel()
        {
            var xml = @"
              <bulletml>
                <action label=""top""/>
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
                <action label=""top""/>
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
                <action label=""top""/>
                <bullet label=""foo"">
                  <action>
                    <fire><bullet/></fire>
                  </action>
                </bullet>
              </bulletml>
            ";

            Assert.DoesNotThrow(() => CreateTopLevelBullet(xml));
        }

        [Test]
        public void LooksUpBulletByLabel()
        {
            var xml = @"
              <bulletml>
                <action label=""top""/>
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

            var bullet1 = pattern.FindBullet("foo");
            Assert.AreEqual("foo", bullet1.Label);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 3), bullet1.Speed);
            Assert.AreEqual(new Direction(DirectionType.Aim, 10), bullet1.Direction);

            var bullet2 = pattern.FindBullet("bar");
            Assert.AreEqual("bar", bullet2.Label);
            Assert.AreEqual(new Speed(SpeedType.Absolute, 7), bullet2.Speed);
            Assert.AreEqual(new Direction(DirectionType.Aim, 17), bullet2.Direction);
        }

        [Test]
        public void ThrowsKeyNotFoundExceptionIfBulletDoesNotExist()
        {
            var xml = @"
              <bulletml>
                <action label=""top""/>
              </bulletml>
            ";

            var pattern = new BulletPattern(xml);
            Assert.Throws<KeyNotFoundException>(() => pattern.FindBullet("abc"));
        }

    }
}
