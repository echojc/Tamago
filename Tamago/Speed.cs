using System;

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
    public class Speed
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
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Calculates the absolute speed for this instance against the given bullet.
        /// </summary>
        /// <param name="bullet">The bullet to base calculations on.</param>
        /// <returns>The speed as an absolute value.</returns>
        public float Evaluate(Bullet bullet)
        {
            switch (Type)
            {
                // ABA compatibility
                // 'relative' speed behaves like 'sequence'
                case SpeedType.Relative:
                case SpeedType.Sequence:
                    return bullet.FireSpeed + Value;
                case SpeedType.Absolute:
                default:
                    return Value;
            }
        }
    }
}
