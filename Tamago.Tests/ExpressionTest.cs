using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago.Tests
{
    [TestFixture]
    public class ExpressionTest : TestBase
    {
        [Test]
        public void ThrowsArgumentNulls()
        {
            Assert.Throws<ArgumentNullException>(() => new Expression(null));
            Assert.Throws<ArgumentNullException>(() => new Expression("1").Evaluate((float[])null));
            Assert.Throws<ArgumentNullException>(() => new Expression("1").Evaluate((Func<int, float>)null));
        }

        [Test]
        public void DoesNotParseEmptyString()
        {
            TestDelegate del = () => new Expression("");
            var thrown = Assert.Throws<ParseException>(del);
            StringAssert.Contains("'[]'", thrown.Message);
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
        public void DoesNotParseIsolatedMinus()
        {
            TestDelegate del = () => new Expression("1 + - + 2");
            var thrown = Assert.Throws<ParseException>(del);
            StringAssert.Contains("'1 + -[ ]+ 2'", thrown.Message);
        }

        [Test]
        public void DoesNotParseMultipleOperators()
        {
            TestDelegate del = () => new Expression("1 * + 2");
            var thrown = Assert.Throws<ParseException>(del);
            StringAssert.Contains("'1 * [+] 2'", thrown.Message);
        }

        [Test]
        public void DoesNotParseMismatchedRightParens()
        {
            TestDelegate del = () => new Expression("(1");
            var thrown = Assert.Throws<ParseException>(del);
            StringAssert.Contains("'(1[]'", thrown.Message);
        }

        [Test]
        public void DoesNotParseMismatchedLeftParens()
        {
            TestDelegate del = () => new Expression("1)");
            var thrown = Assert.Throws<ParseException>(del);
            StringAssert.Contains("'1[)]'", thrown.Message);
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
        public void ThrowsParseErrorOnOverflow()
        {
            Assert.Throws<ParseException>(() => new Expression("999999999999999999999"));
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

        [Test]
        public void ParsesParamsIgnoringLeadingZeroes()
        {
            var expr = new Expression("$00000000000000000000002");
            Assert.AreEqual(3.7f, expr.Evaluate(new[] { 0.8f, 3.7f, 2.5f }), 0.00001f);
        }

        [Test]
        public void ThrowsParseExceptionOnParamIndexOverflow()
        {
            Assert.Throws<ParseException>(() => new Expression("$99999999999999999999999999"));
        }

        [Test]
        public void ParsesRand()
        {
            var expr = new Expression("$rand");
            Assert.AreEqual(Helpers.TestManager.TestRand, expr.Evaluate(_ => 0, TestManager), 0.00001f);
        }

        [Test]
        public void ParsesRank()
        {
            var expr = new Expression("$rank");
            Assert.AreEqual(Helpers.TestManager.TestRank, expr.Evaluate(_ => 0, TestManager), 0.00001f);
        }

        [Test]
        public void OperatorsHavePrecedence()
        {
            Assert.AreEqual(0, Operator.Plus.CompareTo(Operator.Minus));
            Assert.AreEqual(0, Operator.Minus.CompareTo(Operator.Plus));

            Assert.AreEqual(0, Operator.Times.CompareTo(Operator.Divide));
            Assert.AreEqual(0, Operator.Times.CompareTo(Operator.Modulo));
            Assert.AreEqual(0, Operator.Divide.CompareTo(Operator.Modulo));

            Assert.AreEqual(-1, Operator.Plus.CompareTo(Operator.Modulo));
            Assert.AreEqual(1, Operator.Times.CompareTo(Operator.Minus));
        }

        [Test]
        public void ObeysOrderOfOperations()
        {
            var expr = new Expression("1 + 2 * 3 - -4 / 5");
            Assert.AreEqual(7.8f, expr.Evaluate(), 0.00001f);
        }

        [Test]
        public void ParsesAndObeysParentheses()
        {
            var expr = new Expression("2 + (2 * (1 + 3)) / ((4) + 3)");
            Assert.AreEqual(3 + (1f / 7), expr.Evaluate(), 0.00001f);
        }

        [Test]
        public void ParsesComplexEquation()
        {
            var expr = new Expression("360*$rand + ($1/((1-$rank)+1))");
            Assert.AreEqual(52.90698f, expr.Evaluate(new[] { 8f }, TestManager), 0.00001f);
        }

        [Test]
        public void EquivalentExpressionsAreEqual()
        {
            var expr1 = new Expression("2 + 4 * 3");
            var expr2 = new Expression(" 2 + (4*3) ");
            Assert.AreEqual(expr1, expr2);
            Assert.AreEqual(expr1.GetHashCode(), expr2.GetHashCode());
        }

        [Test]
        public void NonEquivalentExpressionsAreNotEqual()
        {
            var expr1 = new Expression("2 + 4 * 3");
            var expr2 = new Expression("(2 + 4) * 3");
            Assert.AreNotEqual(expr1, expr2);
            Assert.AreNotEqual(expr1.GetHashCode(), expr2.GetHashCode());
        }
    }
}
