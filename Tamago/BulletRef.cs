﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;fireRef&gt; node.
    /// </summary>
    public class BulletRef : IBulletDefinition
    {
        private BulletPattern _pattern;
        private BulletDef _bullet;

        /// <summary>
        /// The underlying bullet. Resolution is delayed until the first time this property is read.
        /// </summary>
        public BulletDef Bullet
        {
            get
            {
                if (_bullet == null)
                    _bullet = (BulletDef)_pattern.FindBullet(Label);
                return _bullet;
            }
        }

        /// <summary>
        /// Parses a &lt;bulletRef&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;bulletRef&gt; node.</param>
        /// <param name="pattern">The pattern this node belongs to.</param>
        public BulletRef(XElement node, BulletPattern pattern)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (pattern == null) throw new ArgumentNullException("pattern");
            if (node.Name.LocalName != "bulletRef") throw new ArgumentException("pattern");

            var label = node.Attribute("label");
            if (label == null)
                throw new ParseException("<bulletRef> node requires a label.");
            Label = label.Value;

            _pattern = pattern;
        }

        /// <summary>
        /// The actions performed by the underlying bullet.
        /// </summary>
        public List<IAction> Actions
        {
            get { return Bullet.Actions; }
        }

        /// <summary>
        /// The default speed for this bullet. Can be overidden by a parent &lt;fire&rt; node.
        /// </summary>
        public Speed Speed
        {
            get { return Bullet.Speed; }
        }

        /// <summary>
        /// The default direction for this bullet. Can be overidden by a parent &lt;fire&rt; node.
        /// </summary>
        public Direction Direction
        {
            get { return Bullet.Direction; }
        }

        /// <summary>
        /// The name of the bullet to resolve to.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Creates a bullet from the context of the parent bullet using the underlying bullet.
        /// </summary>
        /// <param name="bullet">The parent bullet to create this bullet from.</param>
        public Bullet Create(Bullet parent)
        {
            return Bullet.Create(parent);
        }
    }
}