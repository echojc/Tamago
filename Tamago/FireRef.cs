using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private Expression[] _params;

        /// <summary>
        /// The underlying fire. Resolution is delayed until the first time this property is read.
        /// </summary>
        public FireDef Fire
        {
            get
            {
                if (_fire == null)
                    _fire = (FireDef)_pattern.Fires[Label].Copy();
                return _fire;
            }
        }

        /// <summary>
        /// The parameters for nested expressions.
        /// </summary>
        public ReadOnlyCollection<Expression> Params
        {
            get
            {
                return Array.AsReadOnly(_params);
            }
        }

        /// <summary>
        /// For cloning.
        /// </summary>
        private FireRef(string label, BulletPattern pattern, Expression[] args)
        {
            Label = label;
            _pattern = pattern;
            _params = args;
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

            var args = node.Elements("param");
            _params = args.Select(p => new Expression(p.Value)).ToArray();

            _pattern = pattern;
        }

        /// <summary>
        /// The bullet to fire.
        /// </summary>
        public IBulletDefinition Bullet
        {
            get { return Fire.Bullet; }
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
        /// <param name="args">Values for params in expressions.</param>
        /// <param name="manager">BulletManager for <see cref="Rand"/> and <see cref="Rank"/> in expressions.</param>
        /// <returns>True always</returns>
        public bool Run(Bullet bullet, float[] args)
        {
            float[] newArgs = new float[_params.Length];
            for (int i = 0; i < newArgs.Length; i++)
                newArgs[i] = _params[i].Evaluate(args, bullet.BulletManager);

            return Fire.Run(bullet, newArgs);
        }

        /// <summary>
        /// Copies this fire reference with the underlying fire unresolved.
        /// </summary>
        /// <returns>A fire reference that has not been resolved.</returns>
        public ITask Copy()
        {
            return new FireRef(Label, _pattern, _params);
        }
    }
}
