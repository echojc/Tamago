using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tamago
{
    public class Expression
    {
        private List<Ast> rpn;

        public float Evaluate()
        {
            var evalStack = new Stack<float>();
            foreach (Ast ast in rpn)
            {
                if (ast is Const)
                {
                    evalStack.Push(((Const)ast).Value);
                }
                else if (ast is Operator)
                {
                    if (evalStack.Count < 2)
                        throw new InvalidOperationException("Cannot evaluate expression: not enough values on stack.");

                    var r = evalStack.Pop();
                    var l = evalStack.Pop();

                    switch (((Operator)ast).Value)
                    {
                        case Operator.Type.Plus:
                            evalStack.Push(l + r);
                            break;
                        case Operator.Type.Minus:
                            evalStack.Push(l - r);
                            break;
                        case Operator.Type.Times:
                            evalStack.Push(l * r);
                            break;
                        case Operator.Type.Divide:
                            if (r == 0)
                                evalStack.Push(0);
                            else
                                evalStack.Push(l / r);
                            break;
                        case Operator.Type.Modulo:
                            if (r == 0)
                                evalStack.Push(0);
                            else
                                evalStack.Push(l % r);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new NotImplementedException("Cannot evaluate expression: unknown ast.");
                }
            }
            return evalStack.Pop();
        }

        /// <summary>
        /// Parses an expression that can be evaluated later.
        /// </summary>
        /// <param name="input">The expression to parse</param>
        public Expression(string input)
        {
            var chars = input.Trim().ToCharArray();
            var index = 0;

            rpn = new List<Ast>();
            Ast ast;
            var opStack = new Stack<Ast>();

            // parse and shunt at the same time
            var result = ParseTerm(chars, index, out ast);
            if (result > 0)
            {
                AddTermToRpn(ast);

                // try to parse operator + term pair if not complete
                var lastResult = 0; // prevent infinite loops
                while (lastResult < result && result < chars.Length)
                {
                    result = ParseOperator(chars, result, out ast);
                    if (result < 0) break;
                    opStack.Push(ast);

                    result = ParseTerm(chars, result, out ast);
                    if (result < 0) break;
                    AddTermToRpn(ast);

                    rpn.Add(opStack.Pop());
                }
            }

            if (result < chars.Length)
            {
                if (result < 0)
                    result = -result;

                var hint = BuildSyntaxErrorHint(chars, result);
                throw new ParseException("Could not parse expression at '" + hint + "'");
            }
        }

        private void AddTermToRpn(Ast term)
        {
            if (term is Negate)
            {
                rpn.Add(Const.Zero);
                AddTermToRpn(((Negate)term).Ast);
                rpn.Add(Operator.Minus);
            }
            else
            {
                rpn.Add(term);
            }
        }

        protected string BuildSyntaxErrorHint(char[] chars, int pos)
        {
            int start = Math.Max(0, pos - 5);
            int end = Math.Min(chars.Length, pos + 5);

            return (start > 0 ? "..." : "")
                + new string(chars, start, pos - start)
                + "[" + chars[pos].ToString() + "]"
                + new string(chars, pos + 1, end - (pos + 1))
                + (end < chars.Length ? "..." : "");
        }

        /// <summary>
        /// op = /+|-|\*|\/|%/
        /// </summary>
        /// <returns>Index for the next token, or the negative of the index that failed to match.</returns>
        protected int ParseOperator(char[] input, int start, out Ast match)
        {
            match = new Fail();

            if (start >= input.Length)
                return -start;

            // skip all leading whitespace before the op
            while (char.IsWhiteSpace(input[start]))
                start++;

            switch (input[start])
            {
                case '+':
                    match = Operator.Plus;
                    break;
                case '-':
                    match = Operator.Minus;
                    break;
                case '*':
                    match = Operator.Times;
                    break;
                case '/':
                    match = Operator.Divide;
                    break;
                case '%':
                    match = Operator.Modulo;
                    break;
                default:
                    // couldn't match
                    return -start;
            }

            // skip all trailing whitespace after the op
            start++;
            while (char.IsWhiteSpace(input[start]))
                start++;

            return start;
        }

        /// <summary>
        /// term = /-{term}|{num}/
        /// </summary>
        /// <returns>Index for the next token, or the negative of the index that failed to match.</returns>
        protected int ParseTerm(char[] input, int start, out Ast match)
        {
            match = new Fail();

            if (start >= input.Length)
                return -start;

            if (input[start] == '-')
            {
                Ast ast;
                var end = ParseTerm(input, start + 1, out ast);
                match = new Negate(ast);
                return end;
            }
            else
            {
                Const number;
                var result = ParseNumber(input, start, out number);
                match = number;
                return result;
            }
        }

        /// <summary>
        /// num = /[0-9]+{frac}?|{frac}/
        /// </summary>
        /// <returns>Index for the next token, or the negative of the index that failed to match.</returns>
        protected int ParseNumber(char[] input, int start, out Const match)
        {
            int end = start;
            while (end < input.Length && input[end] >= '0' && input[end] <= '9')
                end++;

            if (end != start) // matched numbers
            {
                int integral = int.Parse(new string(input, start, end - start));

                float fractional;
                int end2 = ParseFractional(input, end, out fractional);

                if (end2 > 0)
                {
                    match = new Const(integral + fractional);
                    return end2;
                }
                else
                {
                    match = new Const(integral);
                    return end;
                }
            }

            float fractional2;
            var result = ParseFractional(input, start, out fractional2);
            match = new Const(fractional2);
            return result;
        }

        /// <summary>
        /// frac = /\.[0-9]+/
        /// </summary>
        /// <returns>Index for the next token, or the negative of the index that failed to match.</returns>
        protected int ParseFractional(char[] input, int start, out float match)
        {
            match = 0;

            if (start >= input.Length || input[start] != '.')
                return -start;

            int end = start + 1;
            while (end < input.Length && input[end] >= '0' && input[end] <= '9')
                end++;

            if (end == start + 1) // didn't match anything after '.'
                return -start;

            match = float.Parse(new string(input, start, end - start));
            return end;
        }
    }

    public interface Ast { }
    public struct Fail : Ast { }

    public struct Negate : Ast
    {
        public Ast Ast { get; private set; }
        public Negate(Ast ast)
            : this()
        {
            Ast = ast;
        }
    }

    public struct Const : Ast
    {
        public static readonly Const Zero = new Const(0);
        public float Value { get; private set; }
        public Const(float value)
            : this()
        {
            Value = value;
        }
    }

    public struct Param : Ast
    {
        public float Index { get; private set; }
        public Param(int index)
            : this()
        {
            Index = index;
        }
    }

    public struct Operator : Ast
    {
        public static readonly Operator Plus = new Operator(Operator.Type.Plus);
        public static readonly Operator Minus = new Operator(Operator.Type.Minus);
        public static readonly Operator Times = new Operator(Operator.Type.Times);
        public static readonly Operator Divide = new Operator(Operator.Type.Divide);
        public static readonly Operator Modulo = new Operator(Operator.Type.Modulo);

        public enum Type
        {
            Plus,
            Minus,
            Times,
            Divide,
            Modulo
        }
        public Type Value { get; private set; }
        private Operator(Type value)
            : this()
        {
            Value = value;
        }
    }
}
