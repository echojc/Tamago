using System;

namespace Tamago
{
    public interface IFire : Task
    {
        /// <summary>
        /// The bullet to fire.
        /// </summary>
        BulletDef BulletRef { get; }

        /// <summary>
        /// The speed at which to fire the bullet. Overrides any settings specified by the bullet.
        /// </summary>
        Speed? Speed { get; }

        /// <summary>
        /// The direction at which to fire the bullet. Overrides any settings specified by the bullet.
        /// </summary>
        Direction? Direction { get; }

        /// <summary>
        /// Name of this node that can be referenced using &lt;fireRef&gt;.
        /// </summary>
        string Label { get; }
    }
}
