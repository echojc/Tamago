using System;
using System.Text.RegularExpressions;

namespace Tamago
{
    public class Expression
    {
        private static readonly Regex ws = new Regex(@"\s+");

        private float r;

        public float Evaluate()
        {
            return r;
        }

        /// <summary>
        /// Parses an expression that can be evaluated later.
        /// </summary>
        /// <param name="input">The expression to parse</param>
        public Expression(string input)
        {
            var chars = ws.Replace(input, "").ToCharArray();
            var index = 0;

            var result = ParseTerm(chars, index, out r);

            if (result < 0 || result != chars.Length)
            {
                if (result < 0)
                    result = -result;

                int start = Math.Max(0, result - 5);
                int end = Math.Min(chars.Length, result + 5);

                string output = (start > 0 ? "..." : "")
                    + new string(chars, start, result - start)
                    + "[" + chars[result].ToString() + "]"
                    + new string(chars, result + 1, end - (result + 1))
                    + (end < chars.Length ? "..." : "");

                throw new ParseException("Could not parse expression at '" + output + "'");
            }
        }

        /// <summary>
        /// term = /-{term}|{num}/
        /// </summary>
        /// <returns>Index for the next token, or the negative of the index that failed to match.</returns>
        protected int ParseTerm(char[] input, int start, out float match)
        {
            match = 0;
            if (start >= input.Length)
                return -start;

            if (input[start] == '-')
            {
                float term;
                var end = ParseTerm(input, start + 1, out term);
                match = -term;
                return end;
            }
            else
                return ParseNumber(input, start, out match);
        }

        /// <summary>
        /// num = /[0-9]+{frac}?|{frac}/
        /// </summary>
        /// <returns>Index for the next token, or the negative of the index that failed to match.</returns>
        protected int ParseNumber(char[] input, int start, out float match)
        {
            int end = start;
            while (end < input.Length && input[end] >= '0' && input[end] <= '9')
                end++;

            if (end != start) // matched numbers
            {
                match = int.Parse(new string(input, start, end - start));

                float frac;
                int end2 = ParseFractional(input, end, out frac);

                if (end2 > 0)
                {
                    match += frac;
                    return end2;
                }

                return end;
            }

            return ParseFractional(input, start, out match);
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
}
