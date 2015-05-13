using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago
{
    /// <summary>
    /// A skeleton implementation.
    /// 
    /// Consider implementation the interface directly if more find-grained control is necessary.
    /// </summary>
    public abstract class BulletManager : IBulletManager
    {
        /// <summary>
        /// The current X position of the player.
        /// </summary>
        public abstract float PlayerX { get; }

        /// <summary>
        /// The current Y position of the player.
        /// </summary>
        public abstract float PlayerY { get; }

        /// <summary>
        /// Generates a random number between 0 and 1 inclusive.
        /// </summary>
        public abstract float Rand { get; }

        /// <summary>
        /// Gets the difficulty of the level between 0 and 1 inclusive.
        /// </summary>
        public abstract float Rank { get; }

        protected List<Bullet> Bullets = new List<Bullet>();

        /// <summary>
        /// Runs the actions of all bullets managed by this instance.
        /// 
        /// Spawned bullets from &lt;fire&gt; nodes are not run until the next time this method is called.
        /// </summary>
        public virtual void Update()
        {
            // spawned bullets run on the next frame
            for (int i = Bullets.Count - 1; i >= 0; i--)
                Bullets[i].Update();
        }

        /// <summary>
        /// Generates a new bullet for this BulletManager.
        /// 
        /// The bullet will not be run on the frame it is created due to the implementation of <see cref="Update">Update</see>.
        /// </summary>
        /// <returns></returns>
        public virtual Bullet CreateBullet()
        {
            var b = new Bullet(this);
            Bullets.Add(b);
            return b;
        }
    }

    /// <summary>
    /// Provides bullets with access to game state.
    /// </summary>
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
        /// Generates a random number between 0 and 1 inclusive.
        /// </summary>
        float Rand { get; }

        /// <summary>
        /// Gets the difficulty of the level between 0 and 1 inclusive.
        /// </summary>
        float Rank { get; }

        /// <summary>
        /// Generate a new bullet and register it with this IBulletManager.
        /// The new bullet should NOT be run this frame.
        /// </summary>
        /// <returns>A new, default bullet.</returns>
        Bullet CreateBullet();
    }
}
