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
            if (speed != null)
            {
                var magnitude = float.Parse(speed.Value);
                var typeAttr = speed.Attribute("type");

                SpeedType type;
                if (typeAttr != null)
                    Enum.TryParse(typeAttr.Value, true, out type);
                else
                    type = default(SpeedType);

                Speed = new Speed(type, magnitude);
            }
            else
            {
                Speed = Speed.Default;
            }

            var direction = node.Element("direction");
            if (direction != null)
            {
                var radians = MathHelper.ToRadians(float.Parse(direction.Value));
                var typeAttr = direction.Attribute("type");

                DirectionType type;
                if (typeAttr != null)
                    Enum.TryParse(typeAttr.Value, true, out type);
                else
                    type = default(DirectionType);

                Direction = new Direction(type, radians);
            }
            else
            {
                Direction = Direction.Default;
            }

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

            newBullet.Speed = Speed.Evaluate(parent);
            newBullet.Direction = Direction.Evaluate(parent);
            newBullet.X = parent.X;
            newBullet.Y = parent.Y;

            return newBullet;
        }
    }
}
