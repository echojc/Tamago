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
            Assert.Throws<ArgumentNullException>(() => BulletPattern.ParseString(null));
        }

        [Test]
        public void ThrowsParseExceptionIfXmlIsInvalid()
        {
            var xml = "<hi";
            Assert.Throws<ParseException>(() => BulletPattern.ParseString(xml));
        }

        [Test]
        public void ThrowsParseExceptionIfRootElementIsNotBulletMl()
        {
            var xml = @"<action label=""top""/>";
            Assert.Throws<ParseException>(() => BulletPattern.ParseString(xml));
        }

        [Test]
        public void AcceptsEmptyRoot()
        {
            var xml = @"<bulletml/>";
            Assert.DoesNotThrow(() => BulletPattern.ParseString(xml));
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
            Assert.Throws<ParseException>(() => BulletPattern.ParseString(xml));
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
    }
}
