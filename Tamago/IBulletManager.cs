using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago
{
    public interface IBulletManager
    {
        /// <summary>
        /// The current X position of the player.
        /// </summary>
        float PlayerX { get; }

        /// <summary>
        /// The current Y position of the player.
        /// </summary>
        float PlayerY { get; }

        /// <summary>
        /// Generate a new bullet and register it with this IBulletManager.
        /// </summary>
        /// <returns>A new, default bullet.</returns>
        Bullet CreateBullet();
    }
}
