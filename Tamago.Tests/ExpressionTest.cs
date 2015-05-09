using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago.Tests
{
    [TestFixture]
    public class ExpressionTest
    {
        [Test]
        public void DoesNotParsePlainDot()
        {
            TestDelegate del = () => new Expression(".");
            var thrown = Assert.Throws<ParseException>(del);
            StringAssert.Contains("'[.]'", thrown.Message);
        }

        [Test]
        public void DoesNotParseIntegerWithTrailingDot()
        {
            TestDelegate del = () => new Expression("123.");
            var thrown = Assert.Throws<ParseException>(del);
            StringAssert.Contains("'123[.]'", thrown.Message);
        }

        [Test]
        public void ParsesFractionals()
        {
            var expr = new Expression(".123");
            Assert.AreEqual(0.123f, expr.Evaluate());
        }

        [Test]
        public void ParsesIntegers()
        {
            var expr = new Expression("123");
            Assert.AreEqual(123, expr.Evaluate());
        }

        [Test]
        public void ParsesDecimals()
        {
            var expr = new Expression("123.123");
            Assert.AreEqual(123.123f, expr.Evaluate());
        }

        [Test]
        public void ParsesNegativeTerms()
        {
            var expr1 = new Expression("-123.123");
            var expr2 = new Expression("-.123");
            var expr3 = new Expression("-123");
            var expr4 = new Expression("--123.123");

            Assert.AreEqual(-123.123f, expr1.Evaluate());
            Assert.AreEqual(-.123f, expr2.Evaluate());
            Assert.AreEqual(-123, expr3.Evaluate());
            Assert.AreEqual(123.123f, expr4.Evaluate());
        }
    }
}
