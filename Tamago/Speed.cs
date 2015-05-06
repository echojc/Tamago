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
        /// In ABA compatibility mode, this is treated identically to <see cref="SpeedType.Sequence"/>.
        /// </summary>
        Relative,
        /// <summary>
        /// The value is relative to the speed of the last executed &lt;fire&gt; node.
        /// </summary>
        Sequence
    }

    /// <summary>
    /// Represents a &lt;speed&gt; node. Speed values are measured in pixel per frame.
    /// </summary>
    public struct Speed
    {
        private static Speed _default = new Speed(SpeedType.Absolute, 1);

        /// <summary>
        /// A static instance for a default absolute speed of 1.
        /// </summary>
        public static Speed Default
        {
            get { return _default; }
        }

        /// <summary>
        /// What this speed value is relative to.
        /// </summary>
        public SpeedType Type { get; private set; }

        /// <summary>
        /// The magnitude of this speed instance.
        /// </summary>
        public float Value { get; private set; }

        /// <summary>
        /// Represents a &lt;speed&gt; node.
        /// </summary>
        /// <param name="type">Specifies how to interpret the given value.</param>
        /// <param name="value">The value or offset for the speed type.</param>
        public Speed(SpeedType type, float value)
            : this()
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Represents a &lt;speed&gt; node.
        /// </summary>
        /// <param name="node">The node to construct this instance from.</param>
        public Speed(XElement node)
            : this()
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var typeAttr = node.Attribute("type");
            if (typeAttr != null)
            {
                SpeedType type;
                Enum.TryParse(typeAttr.Value, true, out type);
                Type = type;
            }
            else
                Type = default(SpeedType);

            Value = float.Parse(node.Value);
        }

        #region Boilerplate

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
