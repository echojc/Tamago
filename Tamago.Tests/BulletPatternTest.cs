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
        public void ThrowsParseExceptionIfMissingTopAction()
        {
            var xml = @"
              <bulletml>
                <action label=""hi""><fire><bullet/></fire></action>
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
        public void ReturnsNullIfActionCannotBeResolved()
        {
            var xml = @"
              <bulletml>
                <action label=""top""/>
              </bulletml>
            ";

            var pattern = new BulletPattern(xml);
            var action = pattern.FindAction("abc");
            Assert.Null(action);
        }
    }
}
