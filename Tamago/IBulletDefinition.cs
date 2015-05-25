using System;
using System.Collections.Generic;
using System.Linq;

namespace Tamago
{
    /// <summary>
    /// Used for subclassing &lt;bullet&gt; and &lt;bulletRef&gt; nodes.
    /// </summary>
    public interface IBulletDefinition : ILabelled
    {
        /// <summary>
        /// The actions performed by this bullet.
        /// </summary>
        List<IAction> Actions { get; }

        /// <summary>
        /// The default speed for this bullet. Can be overidden by a parent &lt;fire&rt; node.
        /// </summary>
        Speed Speed { get; }

        /// <summary>
        /// The default direction for this bullet. Can be overidden by a parent &lt;fire&rt; node.
        /// </summary>
        Direction Direction { get; }

        /// <summary>
        /// Creates a bullet from the context of the parent bullet.
        /// </summary>
        /// <param name="bullet">The parent bullet to create this bullet from.</param>
        /// <param name="args">Values for params in expressions.</param>
        Bullet Create(Bullet parent, float[] args);
    }
}
