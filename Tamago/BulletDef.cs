using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;bullet&gt; node.
    /// </summary>
    public class BulletDef
    {
        /// <summary>
        /// The actions performed by this bullet.
        /// </summary>
        public ActionDef Action { get; private set; }

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
        /// <param name="pattern">The pattern this node belongs to.</param>
        public BulletDef(XElement node, BulletPattern pattern)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (pattern == null) throw new ArgumentNullException("pattern");
            if (node.Name.LocalName != "bullet") throw new ArgumentException("node");

            var speed = node.Element("speed");
            Speed = speed != null ? new Speed(speed) : Speed.One;

            var direction = node.Element("direction");
            Direction = direction != null ? new Direction(direction) : Direction.Aim;

            var label = node.Attribute("label");
            if (label != null)
                Label = label.Value;

            var actionRef = node.Element("action");
            if (actionRef != null)
                Action = new ActionDef(actionRef, pattern);
            else
                Action = ActionDef.Default;
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
            newBullet.SetPattern((ActionDef)Action.Copy(), isTopLevel: false);

            var speed = Speed.Value.Evaluate();
            switch (Speed.Type)
            {
                case SpeedType.Relative:
                    newBullet.Speed = parent.Speed + speed;
                    break;
                case SpeedType.Sequence:
                    newBullet.Speed = parent.FireSpeed + speed;
                    break;
                case SpeedType.Absolute:
                default:
                    newBullet.Speed = speed;
                    break;
            }

            float result;
            var direction = MathHelper.ToRadians(Direction.Value.Evaluate());
            switch (Direction.Type)
            {
                case DirectionType.Relative:
                    result = parent.Direction + direction;
                    break;
                case DirectionType.Sequence:
                    result = parent.FireDirection + direction;
                    break;
                case DirectionType.Absolute:
                    result = direction;
                    break;
                case DirectionType.Aim:
                default:
                    result = parent.AimDirection + direction;
                    break;
            }

            newBullet.Direction = MathHelper.NormalizeAngle(result);

            newBullet.X = parent.X;
            newBullet.Y = parent.Y;

            return newBullet;
        }
    }
}
