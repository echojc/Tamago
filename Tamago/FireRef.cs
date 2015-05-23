using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;fireRef&gt; node.
    /// </summary>
    public class FireRef : IFire
    {
        private BulletPattern _pattern;
        private FireDef _fire;

        /// <summary>
        /// The underlying fire. Resolution is delayed until the first time this property is read.
        /// </summary>
        public FireDef Fire
        {
            get
            {
                if (_fire == null)
                    _fire = (FireDef)_pattern.FindFire(Label).Copy();
                return _fire;
            }
        }

        /// <summary>
        /// For cloning.
        /// </summary>
        private FireRef(string label, BulletPattern pattern)
        {
            Label = label;
            _pattern = pattern;
        }

        /// <summary>
        /// Parses a &lt;fireRef&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;fireRef&gt; node.</param>
        /// <param name="pattern">The pattern this node belongs to.</param>
        public FireRef(XElement node, BulletPattern pattern)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (pattern == null) throw new ArgumentNullException("pattern");
            if (node.Name.LocalName != "fireRef") throw new ArgumentException("node");

            var label = node.Attribute("label");
            if (label == null)
                throw new ParseException("<fireRef> node requires a label.");
            Label = label.Value;

            _pattern = pattern;
        }

        /// <summary>
        /// The bullet to fire.
        /// </summary>
        public BulletDef BulletRef
        {
            get { return Fire.BulletRef; }
        }

        /// <summary>
        /// The speed at which to fire the bullet. Overrides any settings specified by the bullet.
        /// </summary>
        public Speed? Speed
        {
            get { return Fire.Speed; }
        }

        /// <summary>
        /// The direction at which to fire the bullet. Overrides any settings specified by the bullet.
        /// </summary>
        public Direction? Direction
        {
            get { return Fire.Direction; }
        }

        /// <summary>
        /// The name of the fire to resolve to.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// True if the underlying fire task has completed.
        /// </summary>
        public bool IsCompleted
        {
            get { return Fire.IsCompleted; }
        }

        /// <summary>
        /// Resets the underlying fire task.
        /// </summary>
        public void Reset()
        {
            Fire.Reset();
        }

        /// <summary>
        /// Runs the underlying fire task.
        /// </summary>
        /// <param name="bullet">The parent bullet firing this bullet.</param>
        /// <returns>True always</returns>
        public bool Run(Bullet bullet)
        {
            return Fire.Run(bullet);
        }

        /// <summary>
        /// Copies this fire reference with the underlying fire unresolved.
        /// </summary>
        /// <returns>A fire reference that has not been resolved.</returns>
        public Task Copy()
        {
            return new FireRef(Label, _pattern);
        }
    }
}
