using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Ways to specify how to intepret the speed value.
    /// </summary>
    public enum SpeedType
    {
        /// <summary>
        /// The value is not relative to anything. (Default)
        /// </summary>
        Absolute = 0,
        /// <summary>
        /// The value is relative to the speed of the parent bullet.
        ///
        /// If used with &lt;changeSpeed&gt;, the value is relative to the current speed of the bullet.
        /// </summary>
        Relative,
        /// <summary>
        /// The value is relative to the speed of the last executed &lt;fire&gt; node.
        /// 
        /// If used with &lt;changeSpeed&gt;, the value is added to the current speed once per frame.
        /// </summary>
        Sequence
    }

    /// <summary>
    /// Represents a &lt;speed&gt;, &lt;horizontal&gt;, or &lt;vertical&gt; node. Speed values are measured in pixel per frame.
    /// </summary>
    public struct Speed
    {
        private static Speed _zero = new Speed(SpeedType.Absolute, 0);
        private static Speed _one = new Speed(SpeedType.Absolute, 1);

        /// <summary>
        /// A static instance for an absolute speed of 0.
        /// </summary>
        public static Speed Zero
        {
            get { return _zero; }
        }

        /// <summary>
        /// A static instance for an absolute speed of 1.
        /// </summary>
        public static Speed One
        {
            get { return _one; }
        }

        /// <summary>
        /// What this speed value is relative to.
        /// </summary>
        public SpeedType Type { get; private set; }

        /// <summary>
        /// The magnitude of this speed instance.
        /// </summary>
        public Expression Value { get; private set; }

        /// <summary>
        /// Represents a &lt;speed&gt;, &lt;horizontal&gt;, or &lt;vertical&gt; node.
        /// </summary>
        /// <param name="type">Specifies how to interpret the given value.</param>
        /// <param name="value">The value or offset for the speed type.</param>
        public Speed(SpeedType type, float value)
            : this()
        {
            Type = type;
            Value = new Expression(value);
        }

        /// <summary>
        /// Represents a &lt;speed&gt;, &lt;horizontal&gt;, or &lt;vertical&gt; node.
        /// </summary>
        /// <param name="type">Specifies how to interpret the given value.</param>
        /// <param name="expr">The expression to obtain value or offset for the speed type.</param>
        public Speed(SpeedType type, Expression expr)
            : this()
        {
            Type = type;
            Value = expr;
        }

        /// <summary>
        /// Represents a &lt;speed&gt;, &lt;horizontal&gt;, or &lt;vertical&gt; node.
        /// </summary>
        /// <param name="node">The node to construct this instance from.</param>
        public Speed(XElement node)
            : this()
        {
            if (node == null) throw new ArgumentNullException("node");
            if (node.Name.LocalName != "speed" &&
                node.Name.LocalName != "horizontal" &&
                node.Name.LocalName != "vertical")
                throw new ArgumentException("node");

            var typeAttr = node.Attribute("type");
            if (typeAttr != null)
            {
                SpeedType type;
                Enum.TryParse(typeAttr.Value, true, out type);
                Type = type;
            }
            else
                Type = default(SpeedType);

            Value = new Expression(node.Value);
        }

        #region Boilerplate

        public override string ToString()
        {
            return "Speed [Type=" + Type + ",Value=" + Value + "]";
        }

        public override bool Equals(object obj)
        {
            return obj is Speed && this == (Speed)obj;
        }

        public override int GetHashCode()
        {
            return (527 + Type.GetHashCode()) * 31 + Value.GetHashCode();
        }

        public static bool operator ==(Speed x, Speed y)
        {
            return x.Type == y.Type && x.Value == y.Value;
        }

        public static bool operator !=(Speed x, Speed y)
        {
            return !(x == y);
        }

        #endregion
    }
}
