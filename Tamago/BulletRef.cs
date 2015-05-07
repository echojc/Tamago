using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;bullet&gt; node.
    /// </summary>
    public class BulletRef
    {
        /// <summary>
        /// The actions performed by this bullet.
        /// </summary>
        public ActionRef Action { get; private set; }

        /// <summary>
        /// The default speed for this bullet. Can be overidden by a parent &lt;fire&rt; node.
        /// </summary>
        public Speed Speed { get; private set; }

        /// <summary>
        /// The default direction for this bullet. Can be overidden by a parent &lt;fire&rt; node.
        /// </summary>
        public Direction Direction { get; private set; }

        /// <summary>
        /// Name of this node that can be referenced using &lt;bulletRef&gt;.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Parses a &lt;bullet&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;bullet&gt; node.</param>
        public BulletRef(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var speed = node.Element("speed");
            Speed = speed != null ? new Speed(speed) : Speed.One;

            var direction = node.Element("direction");
            Direction = direction != null ? new Direction(direction) : Direction.Aim;

            var label = node.Attribute("label");
            if (label != null)
                Label = label.Value;

            var actionRef = node.Element("action");
            if (actionRef != null)
                Action = new ActionRef(actionRef);
            else
                Action = ActionRef.Default;
        }

        /// <summary>
        /// Creates a bullet from the context of the parent bullet.
        /// </summary>
        /// <param name="bullet">The parent bullet to create this bullet from.</param>
        public Bullet Create(Bullet parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            var newBullet = parent.BulletManager.CreateBullet();
            newBullet.SetPattern(Action, isTopLevel: false);

            switch (Speed.Type)
            {
                case SpeedType.Relative:
                    newBullet.Speed = parent.Speed + Speed.Value;
                    break;
                case SpeedType.Sequence:
                    newBullet.Speed = parent.FireSpeed + Speed.Value;
                    break;
                case SpeedType.Absolute:
                default:
                    newBullet.Speed = Speed.Value;
                    break;
            }

            float direction;
            switch (Direction.Type)
            {
                case DirectionType.Relative:
                    direction = parent.Direction + Direction.Value;
                    break;
                case DirectionType.Sequence:
                    direction = parent.FireDirection + Direction.Value;
                    break;
                case DirectionType.Absolute:
                    direction = Direction.Value;
                    break;
                case DirectionType.Aim:
                default:
                    direction = parent.AimDirection + Direction.Value;
                    break;
            }

            newBullet.Direction = MathHelper.WrapAngle(direction);

            newBullet.X = parent.X;
            newBullet.Y = parent.Y;

            return newBullet;
        }
    }
}
