using System;

namespace Tamago
{
    /// <summary>
    /// Used for subclassing &lt;fire&gt; and &lt;fireRef&gt; nodes.
    /// </summary>
    public interface IFire : ITask, ILabelled
    {
        /// <summary>
        /// The bullet to fire.
        /// </summary>
        IBulletDefinition Bullet { get; }

        /// <summary>
        /// The speed at which to fire the bullet. Overrides any settings specified by the bullet.
        /// </summary>
        Speed? Speed { get; }

        /// <summary>
        /// The direction at which to fire the bullet. Overrides any settings specified by the bullet.
        /// </summary>
        Direction? Direction { get; }
    }
}
