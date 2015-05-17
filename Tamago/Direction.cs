using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Ways to specify how to intepret the direction value.
    /// </summary>
    public enum DirectionType
    {
        /// <summary>
        /// The value is relative to where the player is (see <see cref="IBulletManager"/>). (Default)
        /// </summary>
        Aim = 0,
        /// <summary>
        /// The value is relative to directly upwards.
        /// </summary>
        Absolute,
        /// <summary>
        /// The value is relative to the direction of the parent bullet.
        /// </summary>
        Relative,
        /// <summary>
        /// The value is relative to the direction of the last executed &lt;fire&gt; node.
        /// </summary>
        Sequence
    }

    /// <summary>
    /// Represents a &lt;direction&gt; node. Direction values are measured in radians, clockwise.
    /// </summary>
    public struct Direction
    {
        private static Direction _aim = new Direction(DirectionType.Aim, 0);

        /// <summary>
        /// A static instance for a direction aiming at the player.
        /// </summary>
        public static Direction Aim
        {
            get { return _aim; }
        }

        /// <summary>
        /// What this direction value is relative to.
        /// </summary>
        public DirectionType Type { get; private set; }

        /// <summary>
        /// The angle in radians, measured from directly up as 0, clockwise.
        /// </summary>
        public Expression Value { get; private set; }

        /// <summary>
        /// Represents a &lt;direction&gt; node.
        /// </summary>
        /// <param name="type">Specifies how to interpret the given value.</param>
        /// <param name="value">The angle offset or value.</param>
        public Direction(DirectionType type, float value)
            : this()
        {
            Type = type;
            Value = new Expression(value);
        }

        /// <summary>
        /// Represents a &lt;direction&gt; node.
        /// </summary>
        /// <param name="type">Specifies how to interpret the given value.</param>
        /// <param name="expr">The expression to obtain the angle offset or value.</param>
        public Direction(DirectionType type, Expression expr)
            : this()
        {
            Type = type;
            Value = expr;
        }

        /// <summary>
        /// Represents a &lt;direction&gt; node.
        /// </summary>
        /// <param name="node">The node to construct this instance from.</param>
        public Direction(XElement node)
            : this()
        {
            if (node == null) throw new ArgumentNullException();
            if (node.Name.LocalName != "direction") throw new ArgumentException("node");

            var typeAttr = node.Attribute("type");
            if (typeAttr != null)
            {
                DirectionType type;
                Enum.TryParse(typeAttr.Value, true, out type);
                Type = type;
            }
            else
                Type = default(DirectionType);

            Value = new Expression(node.Value);
        }

        #region Boilerplate

        public override string ToString()
        {
            return "Direction [Type=" + Type + ",Value=" + Value + "]";
        }

        public override bool Equals(object obj)
        {
            return obj is Direction && this == (Direction)obj;
        }

        public override int GetHashCode()
        {
            return (527 + Type.GetHashCode()) * 31 + Value.GetHashCode();
        }

        public static bool operator ==(Direction x, Direction y)
        {
            return x.Type == y.Type && x.Value == y.Value;
        }

        public static bool operator !=(Direction x, Direction y)
        {
            return !(x == y);
        }

        #endregion
    }
}
