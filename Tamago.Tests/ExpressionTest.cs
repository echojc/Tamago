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
        public void ThrowsArgumentNulls()
        {
            Assert.Throws<ArgumentNullException>(() => new Expression(null));
            Assert.Throws<ArgumentNullException>(() => new Expression("1").Evaluate((float[])null));
            Assert.Throws<ArgumentNullException>(() => new Expression("1").Evaluate((Func<int, float>)null));
        }

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
            var expr5 = new Expression("-1.2 * 2.5");
            var expr6 = new Expression("2.5 + -1.2");

            Assert.AreEqual(-123.123f, expr1.Evaluate(), 0.00001f);
            Assert.AreEqual(-.123f, expr2.Evaluate(), 0.00001f);
            Assert.AreEqual(-123, expr3.Evaluate(), 0.00001f);
            Assert.AreEqual(123.123f, expr4.Evaluate(), 0.00001f);
            Assert.AreEqual(-3f, expr5.Evaluate(), 0.00001f);
            Assert.AreEqual(1.3f, expr6.Evaluate(), 0.00001f);
        }

        [Test]
        public void EvalsMinusNegative()
        {
            var expr = new Expression("2.5--1.2");

            Assert.AreEqual(3.7f, expr.Evaluate(), 0.00001f);
        }

        [Test]
        public void EvalsNegativeMultiplicationDivision()
        {
            var expr1 = new Expression("1.2 / -2.5");
            var expr2 = new Expression("-1.2 / 2.5");
            var expr3 = new Expression("-1.2 / -2.5");
            var expr4 = new Expression("2.5 * -1.2");
            var expr5 = new Expression("-2.5 * 1.2");
            var expr6 = new Expression("-2.5 * -1.2");

            Assert.AreEqual(-0.48f, expr1.Evaluate(), 0.00001f);
            Assert.AreEqual(-0.48f, expr2.Evaluate(), 0.00001f);
            Assert.AreEqual(0.48f, expr3.Evaluate(), 0.00001f);
            Assert.AreEqual(-3f, expr4.Evaluate(), 0.00001f);
            Assert.AreEqual(-3f, expr5.Evaluate(), 0.00001f);
            Assert.AreEqual(3f, expr6.Evaluate(), 0.00001f);
        }

        [Test]
        public void EvalsNegativeModuloUsingCSRules()
        {
            var expr1 = new Expression("2.5 % -1.2");
            var expr2 = new Expression("-2.5 % 1.2");
            var expr3 = new Expression("-2.5 % -1.2");

            Assert.AreEqual(0.1f, expr1.Evaluate(), 0.00001f);
            Assert.AreEqual(-0.1f, expr2.Evaluate(), 0.00001f);
            Assert.AreEqual(-0.1f, expr3.Evaluate(), 0.00001f);
        }

        [Test]
        public void ReturnsZeroWhenDivisionAndModuloByZero()
        {
            var expr1 = new Expression("1 / 0");
            var expr2 = new Expression("1 % 0");

            Assert.AreEqual(0f, expr1.Evaluate(), 0.00001f);
            Assert.AreEqual(0f, expr2.Evaluate(), 0.00001f);
        }

        [Test]
        public void ParsesPlus()
        {
            var expr = new Expression("1.2 + 2.5");
            Assert.AreEqual(3.7f, expr.Evaluate(), 0.00001f);
        }

        [Test]
        public void ParsesMinus()
        {
            var expr = new Expression("1.2 - 2.5");
            Assert.AreEqual(-1.3f, expr.Evaluate(), 0.00001f);
        }

        [Test]
        public void ParsesTimes()
        {
            var expr = new Expression("1.2 * 2.5");
            Assert.AreEqual(3f, expr.Evaluate(), 0.00001f);
        }

        [Test]
        public void ParsesDivide()
        {
            var expr = new Expression("1.2 / 2.5");
            Assert.AreEqual(0.48f, expr.Evaluate(), 0.00001f);
        }

        [Test]
        public void ParsesModulo()
        {
            var expr = new Expression("2.5 % 1.2");
            Assert.AreEqual(0.1f, expr.Evaluate(), 0.00001f);
        }

        [Test]
        public void ParsesParamsWithOutOfRangeBehevaiour()
        {
            var expr1 = new Expression("$1 + $2");
            var expr2 = new Expression("$1");
            var expr3 = new Expression("$1 + $2 + $3");

            Assert.AreEqual(3.7f, expr1.Evaluate(new[] { 1.2f, 2.5f }), 0.00001f);
            Assert.AreEqual(0f, expr2.Evaluate(), 0.00001f);
            Assert.AreEqual(3.7f, expr3.Evaluate(new[] { 1.2f, 2.5f }), 0.00001f);
        }
    }
}
