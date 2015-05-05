using System;

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
    public class Direction
    {
        private static Direction _default = new Direction(DirectionType.Aim, 0);

        /// <summary>
        /// A static instance for a default direction aiming at the player.
        /// </summary>
        public static Direction Default
        {
            get { return _default; }
        }

        /// <summary>
        /// What this direction value is relative to.
        /// </summary>
        public DirectionType Type { get; private set; }

        /// <summary>
        /// The magnitude of this direction instance.
        /// </summary>
        public float Value { get; private set; }

        /// <summary>
        /// Represents a &lt;direction&gt; node.
        /// </summary>
        /// <param name="type">Specifies how to interpret the given value.</param>
        /// <param name="value">The value or offset for the direction type.</param>
        public Direction(DirectionType type, float value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Calculates the absolute direction for this instance against the given bullet.
        /// </summary>
        /// <param name="bullet">The bullet to base calculations on.</param>
        /// <returns>The direction as an absolute value.</returns>
        public float Evaluate(Bullet bullet)
        {
            float result;
            switch (Type)
            {
                case DirectionType.Relative:
                    result = bullet.Direction + Value;
                    break;
                case DirectionType.Sequence:
                    result = bullet.FireDirection + Value;
                    break;
                case DirectionType.Absolute:
                    result = Value;
                    break;
                case DirectionType.Aim:
                default:
                    result = bullet.AimDirection + Value;
                    break;
            }
            return MathHelper.WrapAngle(result);
        }
    }
}
