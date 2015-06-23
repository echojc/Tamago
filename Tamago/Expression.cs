using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tamago
{
    public class Expression
    {
        private List<Ast> rpn;
        private string trimmedExpression;

        /// <summary>
        /// Parses an expression that can be evaluated later.
        /// </summary>
        /// <param name="input">The expression to parse</param>
        /// <param name="version">The version of the spec to parse for.</param>
        public Expression(string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            trimmedExpression = input.Trim();
            var chars = trimmedExpression.ToCharArray();
            var result = ParseExpression(chars, 0, out rpn);

            if (chars.Length == 0 || result < chars.Length)
            {
                if (result < 0)
                    result = -result;

                var hint = BuildSyntaxErrorHint(chars, result);
                throw new ParseException("Could not parse expression at '" + hint + "'");
            }
        }

        /// <summary>
        /// Creates an expression that evaluates to a constant value. Intended for testing purposes.
        /// </summary>
        /// <param name="constant">The constant value</param>
        public Expression(float constant)
        {
            rpn = new List<Ast>();
            rpn.Add(new Const(constant));
            trimmedExpression = constant.ToString();
        }

        /// <summary>
        /// Pretty prints an error at the given position.
        /// </summary>
        /// <param name="chars">The input expression.</param>
        /// <param name="pos">The position to highlight.</param>
        /// <param name="context">The number of characters to display around the error position.</param>
        protected string BuildSyntaxErrorHint(char[] chars, int pos, int context = 20)
        {
            int start = Math.Max(0, pos - context);
            int end = Math.Min(chars.Length, pos + context);

            var before = new string(chars, start, pos - start);
            var error = "[" + (pos < chars.Length ? chars[pos].ToString() : "") + "]";
            var after = pos < chars.Length ? new string(chars, pos + 1, end - (pos + 1)) : "";

            return (start > 0 ? "..." : "")
                + before + error + after
                + (end < chars.Length ? "..." : "");
        }

        #region Evaluation

        /// <summary>
        /// Evaluate the expression without a parameter resolver. All parameters will evaluate to 0.
        /// </summary>
        /// <returns>The result of evaluating this expression.</returns>
        public float Evaluate()
        {
            return Evaluate(_ => 0);
        }

        /// <summary>
        /// Evaluate the expression by looking up parameters as indexes in this array.
        /// Note that the array is 0-based but parameters are 1-based, so <code>$1</code> resolves to <code>array[0]</code>.
        /// Values out of range will return 0.
        /// </summary>
        /// <param name="param">The array to look up parameters with.</param>
        /// <param name="manager">Used to resolve <code>$rand</code> and <code>$rank</code>. If null, these variables will evaluate to 0.</param>
        /// <returns>The result of evaluating this expression.</returns>
        public float Evaluate(float[] param, IBulletManager manager = null)
        {
            return Evaluate(param, _ => 0, manager);
        }

        /// <summary>
        /// Evaluate the expression by looking up parameters as indexes in this array.
        /// Note that the array is 0-based but parameters are 1-based, so <code>$1</code> resolves to <code>array[0]</code>.
        /// Values that are out of range or indexes that are not ints will return the result of a call to the fallback function.
        /// </summary>
        /// <param name="param">The array to look up parameters with.</param>
        /// <param name="fallback">The function to invoke if the index is out of range or is not an int.</param>
        /// <param name="manager">Used to resolve <code>$rand</code> and <code>$rank</code>. If null, these variables will evaluate to 0.</param>
        /// <returns>The result of evaluating this expression.</returns>
        public float Evaluate(float[] param, Func<string, float> fallback, IBulletManager manager = null)
        {
            if (param == null) throw new ArgumentNullException("param");
            if (fallback == null) throw new ArgumentNullException("fallback");

            // create dictionary lookup
            var dict = new Dictionary<string, float>();
            for (int i = 1; i <= param.Length; i++)
                dict.Add(i.ToString(), param[i - 1]);

            return Evaluate(i => dict.ContainsKey(i) ? dict[i] : fallback(i), manager);
        }

        /// <summary>
        /// Evaluate the expression by using the supplied function to look up parameters.
        /// </summary>
        /// <param name="param">The function to call to look up parameters with.</param>
        /// <param name="manager">Used to resolve <code>$rand</code> and <code>$rank</code>. If null, these variables will evaluate to 0.</param>
        /// <returns>The result of evaluating this expression.</returns>
        public float Evaluate(Func<string, float> param, IBulletManager manager = null)
        {
            if (param == null)
                throw new ArgumentNullException("param");

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
                            throw new NotImplementedException("Cannot evaluate expression: unknown operator.");
                    }
                }
                else if (ast is Param)
                {
                    evalStack.Push(param(((Param)ast).Index));
                }
                else if (ast is Function)
                {
                    switch (((Function)ast).Name)
                    {
                        case "rand":
                            evalStack.Push(manager.Rand);
                            break;
                        case "rank":
                            evalStack.Push(manager.Rank);
                            break;
                        default:
                            throw new NotImplementedException("Cannot evaluate expression: unknown function.");
                    }
                }
                else
                {
                    throw new NotImplementedException("Cannot evaluate expression: unknown ast.");
                }
            }

            if (evalStack.Count != 1)
                throw new InvalidOperationException("Cannot evaluate expression: incorrect number of values left on stack.");

            return evalStack.Pop();
        }

        #endregion

        #region Parsers

        /// <summary>
        /// expr = /{term}({op}{term})*/
        /// </summary>
        /// <returns>Index for the next token, or the negative of the index that failed to match.</returns>
        protected int ParseExpression(char[] input, int start, out List<Ast> match)
        {
            match = new List<Ast>();
            List<Ast> asts;

            // parse and shunt at the same time
            start = ParseTerm(input, start, out asts);
            if (start < 0)
                return start;

            rpn.AddRange(asts);

            // try to parse operator + term pair if not complete
            var lastResult = 0; // prevent infinite loops
            var opStack = new Stack<Operator>();

            while (lastResult < start && start < input.Length)
            {
                Ast temp;
                int result = ParseOperator(input, start, out temp);
                if (result < 0)
                    break; // no more operators, we're done

                // shunt
                Operator op = (Operator)temp;
                while (opStack.Count > 0 && op.CompareTo(opStack.Peek()) <= 0)
                    rpn.Add(opStack.Pop());
                opStack.Push(op);

                start = ParseTerm(input, result, out asts);
                if (start < 0)
                    return -start;

                rpn.AddRange(asts);
            }

            // push remaining ops
            while (opStack.Count > 0)
                rpn.Add(opStack.Pop());

            return start;
        }

        /// <summary>
        /// term = /\({expr}\)|-{term}|{num}|{var}/
        /// </summary>
        /// <returns>Index for the next token, or the negative of the index that failed to match.</returns>
        protected int ParseTerm(char[] input, int start, out List<Ast> match)
        {
            match = new List<Ast>();

            if (start >= input.Length)
                return -start;

            // parens
            if (input[start] == '(')
            {
                var end = ParseExpression(input, start + 1, out match);
                if (end < 0) 
                    return end;

                if (end >= input.Length || input[end] != ')')
                    return -end;

                return end + 1; // drop right parens
            }

            // negative
            if (input[start] == '-')
            {
                List<Ast> ast;
                var end = ParseTerm(input, start + 1, out ast);
                if (end >= 0)
                {
                    match.Add(Const.Zero);
                    match.AddRange(ast);
                    match.Add(Operator.Minus);
                }
                return end;
            }

            // number
            Const number;
            var result = ParseNumber(input, start, out number);
            if (result > 0)
            {
                match.Add(number);
                return result;
            }

            // variable
            Ast var;
            result = ParseVariable(input, start, out var);
            if (result < 0)
                return result;

            match.Add(var);
            return result;
        }

        /// <summary>
        /// var = /\$([0-9]+|rank|rand|i|times)/
        /// </summary>
        /// <returns>Index for the next token, or the negative of the index that failed to match.</returns>
        protected int ParseVariable(char[] input, int start, out Ast match)
        {
            match = null;

            if (start >= input.Length || input[start] != '$')
                return -start;

            // try match number
            start++;
            int end = start;
            while (end < input.Length && input[end] >= '0' && input[end] <= '9')
                end++;

            if (end != start) // matched
            {
                // even though internally we convert it back to a string
                // we parse int here anyway because it's easier than manually trimming, etc.
                int arg;
                if (!int.TryParse(new string(input, start, end - start), out arg))
                    return -start;

                match = new Param(arg);
                return end;
            }

            // match vars
            if (CharArrayStartsWithString(input, start, "rank"))
            {
                match = Function.Rank;
                return start + 4;
            }
            else if (CharArrayStartsWithString(input, start, "rand"))
            {
                match = Function.Rand;
                return start + 4;
            }
            else if (CharArrayStartsWithString(input, start, "i"))
            {
                match = new Param("i");
                return start + 1;
            }
            else if (CharArrayStartsWithString(input, start, "times"))
            {
                match = new Param("times");
                return start + 5;
            }

            return -start;
        }

        /// <summary>
        /// Helper to match variables.
        /// </summary>
        /// <returns>True if the array starts with the same characters as the entire string.</returns>
        private bool CharArrayStartsWithString(char[] array, int offset, string target)
        {
            var end = target.Length + offset;

            if (array.Length < end)
                return false;

            for (int i = offset; i < end; i++)
            {
                if (array[i] != target[i - offset])
                    return false;
            }

            return true;
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
                int integral;
                if (!int.TryParse(new string(input, start, end - start), out integral))
                {
                    match = new Const(0);
                    return -start;
                }

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

            if (!float.TryParse(new string(input, start, end - start), out match))
                return -start;

            return end;
        }

        /// <summary>
        /// op = /+|-|\*|\/|%/
        /// </summary>
        /// <returns>Index for the next token, or the negative of the index that failed to match.</returns>
        protected int ParseOperator(char[] input, int start, out Ast match)
        {
            match = null;

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

        #endregion 

        #region Boilerplate

        public override string ToString()
        {
            return "Expression [" + trimmedExpression + "]";
        }

        public override bool Equals(object obj)
        {
            return obj is Expression && this == (Expression)obj;
        }

        public override int GetHashCode()
        {
            return rpn.Aggregate(41, (acc, next) => acc * 23 + next.GetHashCode());
        }

        public static bool operator ==(Expression x, Expression y)
        {
            return x.rpn.SequenceEqual(y.rpn);
        }

        public static bool operator !=(Expression x, Expression y)
        {
            return !(x == y);
        }

        #endregion
    }

    #region ASTs

    public interface Ast { }

    public struct Const : Ast
    {
        public static readonly Const Zero = new Const(0);
        public float Value { get; private set; }
        public Const(float value)
            : this()
        {
            Value = value;
        }

        #region Boilerplate

        public override string ToString()
        {
            return "Const [" + Value + "]";
        }

        public override bool Equals(object obj)
        {
            return obj is Const && this == (Const)obj;
        }

        public override int GetHashCode()
        {
            return "Const".GetHashCode() * Value.GetHashCode();
        }

        public static bool operator ==(Const x, Const y)
        {
            return x.Value == y.Value;
        }

        public static bool operator !=(Const x, Const y)
        {
            return !(x == y);
        }

        #endregion
    }

    public struct Param : Ast
    {
        public string Index { get; private set; }

        public Param(int index)
            : this(index.ToString())
        { }

        public Param(string index)
            : this()
        {
            Index = index;
        }

        #region Boilerplate

        public override string ToString()
        {
            return "Param [Index=" + Index + "]";
        }

        public override bool Equals(object obj)
        {
            return obj is Param && this == (Param)obj;
        }

        public override int GetHashCode()
        {
            return "Param".GetHashCode() * Index.GetHashCode();
        }

        public static bool operator ==(Param x, Param y)
        {
            return x.Index == y.Index;
        }

        public static bool operator !=(Param x, Param y)
        {
            return !(x == y);
        }

        #endregion
    }

    public struct Function : Ast
    {
        public static readonly Function Rand = new Function("rand");
        public static readonly Function Rank = new Function("rank");

        public string Name { get; private set; }
        public Function(string name)
            : this()
        {
            Name = name;
        }

        #region Boilerplate

        public override string ToString()
        {
            return "Function [" + Name + "]";
        }

        public override bool Equals(object obj)
        {
            return obj is Function && this == (Function)obj;
        }

        public override int GetHashCode()
        {
            return "Function".GetHashCode() * Name.GetHashCode();
        }

        public static bool operator ==(Function x, Function y)
        {
            return x.Name == y.Name;
        }

        public static bool operator !=(Function x, Function y)
        {
            return !(x == y);
        }

        #endregion
    }

    public struct Operator : Ast, IComparable<Operator>
    {
        public static readonly Operator Plus = new Operator(Operator.Type.Plus);
        public static readonly Operator Minus = new Operator(Operator.Type.Minus);
        public static readonly Operator Times = new Operator(Operator.Type.Times);
        public static readonly Operator Divide = new Operator(Operator.Type.Divide);
        public static readonly Operator Modulo = new Operator(Operator.Type.Modulo);

        public enum Type
        {
            Plus = 0,
            Minus = 1,
            Times = 10,
            Divide = 11,
            Modulo = 12
        }
        public Type Value { get; private set; }
        private Operator(Type value)
            : this()
        {
            Value = value;
        }

        public int CompareTo(Operator other)
        {
            return ((int)this.Value / 10) - ((int)other.Value / 10);
        }

        #region Boilerplate

        public override string ToString()
        {
            return "Operator [" + Value + "]";
        }

        public override bool Equals(object obj)
        {
            return obj is Operator && this == (Operator)obj;
        }

        public override int GetHashCode()
        {
            return "Operator".GetHashCode() * Value.GetHashCode();
        }

        public static bool operator ==(Operator x, Operator y)
        {
            return x.Value == y.Value;
        }

        public static bool operator !=(Operator x, Operator y)
        {
            return !(x == y);
        }

        #endregion
    }

    #endregion
}
