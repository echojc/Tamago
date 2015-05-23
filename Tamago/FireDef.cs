using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;fire&gt; node.
    /// </summary>
    public class FireDef : IFire
    {
        /// <summary>
        /// The bullet to fire.
        /// </summary>
        public BulletDef BulletRef { get; private set; }

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
        /// For cloning.
        /// </summary>
        private FireDef(BulletDef bulletRef, Speed? speed, Direction? direction, string label)
        {
            BulletRef = bulletRef;
            Speed = speed;
            Direction = direction;
            Label = label;
            Reset();
        }

        /// <summary>
        /// Parses a &lt;fire&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;fire&gt; node.</param>
        public FireDef(XElement node, BulletPattern pattern)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (pattern == null) throw new ArgumentNullException("pattern");
            if (node.Name.LocalName != "fire") throw new ArgumentException("node");

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

            BulletRef = new BulletDef(bulletRef, pattern);
        }

        /// <summary>
        /// Resets this task to its pre-run state.
        /// </summary>
        public void Reset()
        {
            IsCompleted = false;
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
                var speed = s.Value.Evaluate();
                switch (s.Type)
                {
                    case SpeedType.Relative:
                        newBullet.Speed = bullet.Speed + speed;
                        break;
                    case SpeedType.Sequence:
                        newBullet.Speed = bullet.FireSpeed + speed;
                        break;
                    case SpeedType.Absolute:
                    default:
                        newBullet.Speed = speed;
                        break;
                }
            }

            if (Direction != null)
            {
                Direction d = Direction.Value;

                float result;
                var direction = MathHelper.ToRadians(d.Value.Evaluate());
                switch (d.Type)
                {
                    case DirectionType.Relative:
                        result = bullet.Direction + direction;
                        break;
                    case DirectionType.Sequence:
                        result = bullet.FireDirection + direction;
                        break;
                    case DirectionType.Absolute:
                        result = direction;
                        break;
                    case DirectionType.Aim:
                    default:
                        result = bullet.AimDirection + direction;
                        break;
                }

                newBullet.Direction = MathHelper.NormalizeAngle(result);
            }

            IsCompleted = true;
            return true;
        }

        /// <summary>
        /// Copies this task and resets it.
        /// </summary>
        /// <returns>A reset copy of this task.</returns>
        public Task Copy()
        {
            return new FireDef(BulletRef, Speed, Direction, Label);
        }
    }
}
