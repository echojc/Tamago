using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;fire&gt; node.
    /// </summary>
    public class FireRef : Task
    {
        /// <summary>
        /// The bullet to fire.
        /// </summary>
        public BulletRef BulletRef { get; private set; }

        /// <summary>
        /// The speed at which to fire the bullet. Overrides any settings specified by the bullet.
        /// </summary>
        public Speed? Speed { get; private set; }

        /// <summary>
        /// The direction at which to fire the bullet. Overrides any settings specified by the bullet.
        /// </summary>
        public Direction? Direction { get; private set; }

        /// <summary>
        /// Name of this node that can be referenced using &lt;fireRef&gt;.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// True if the bullet has been fired.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Parses a &lt;fire&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;fire&gt; node.</param>
        public FireRef(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var bulletRef = node.Element("bullet");
            if (bulletRef == null)
                throw new ParseException("<fire> node requires a <bullet> node.");

            var speed = node.Element("speed");
            if (speed != null)
                Speed = new Speed(speed);

            var direction = node.Element("direction");
            if (direction != null)
                Direction = new Direction(direction);

            var label = node.Attribute("label");
            if (label != null)
                Label = label.Value;

            BulletRef = new BulletRef(bulletRef);
        }

        /// <summary>
        /// Fires this bullet in the context of the parent bullet.
        /// </summary>
        /// <param name="bullet">The parent bullet firing this bullet.</param>
        /// <returns>True always</returns>
        public bool Run(Bullet bullet)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            if (IsCompleted)
                return true;

            // create bullet from definition
            var newBullet = BulletRef.Create(parent: bullet);

            // override with fire attributes
            if (Speed != null)
            {
                Speed s = Speed.Value;
                switch (s.Type)
                {
                    // ABA compatibility
                    // 'relative' speed behaves like 'sequence'
                    case SpeedType.Relative:
                    case SpeedType.Sequence:
                        newBullet.Speed = bullet.FireSpeed + s.Value;
                        break;
                    case SpeedType.Absolute:
                    default:
                        newBullet.Speed = s.Value;
                        break;
                }
            }

            if (Direction != null)
            {
                Direction d = Direction.Value;

                float result;
                switch (d.Type)
                {
                    case DirectionType.Relative:
                        result = bullet.Direction + d.Value;
                        break;
                    case DirectionType.Sequence:
                        result = bullet.FireDirection + d.Value;
                        break;
                    case DirectionType.Absolute:
                        result = d.Value;
                        break;
                    case DirectionType.Aim:
                    default:
                        result = bullet.AimDirection + d.Value;
                        break;
                }

                newBullet.Direction = MathHelper.WrapAngle(result);
            }

            IsCompleted = true;
            return true;
        }
    }
}
