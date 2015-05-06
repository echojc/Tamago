using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago
{
    public class Bullet
    {
        internal IBulletManager BulletManager;

        public ActionRef Action { get; protected set; }
        public bool IsTopLevel { get; protected set; }
        public bool IsVanished { get; protected set; }

        public virtual float X { get; set; }
        public virtual float Y { get; set; }
        public float Direction { get; set; }
        public float Speed { get; set; }
        public float FireDirection { get; set; }
        public float FireSpeed { get; set; }

        public float? NewDirection { get; protected internal set; }
        public float? NewSpeed { get; protected internal set; }

        public Bullet(IBulletManager manager)
        {
            BulletManager = manager;

            X = 0;
            Y = 0;
            Direction = 0;
            Speed = 0;
            FireDirection = 0;
            FireSpeed = 1;

            NewDirection = null;
            NewSpeed = null;
        }

        /// <summary>
        /// Initializes this bullet with some behaviour.
        /// </summary>
        /// <param name="action">What this bullet will do.</param>
        /// <param name="isTopLevel">Whether this is a top level bullet (i.e., &lt;action label="top"/&gt;). Top level bullets should be invisible.</param>
        public void SetPattern(ActionRef action, bool isTopLevel = false)
        {
            Action = action;
            IsTopLevel = isTopLevel;
            IsVanished = false;
        }

        /// <summary>
        /// Gets the angle in radians to point at the player.
        /// </summary>
        public float AimDirection
        {
            get { return (float)Math.Atan2(BulletManager.PlayerX - X, -(BulletManager.PlayerY - Y)); }
        }

        public void Update()
        {
            if (IsVanished)
                return;

            // actions are run before bullets move
            Action.Run(this);

            // apply queued changes if they exist
            if (NewSpeed != null)
            {
                Speed = NewSpeed.Value;
                NewSpeed = null;
            }
            if (NewDirection != null)
            {
                Direction = NewDirection.Value;
                NewDirection = null;
            }
            
            // formulae are atypical because of non-standard coordinate system
			X += (float)(Math.Sin(Direction) * Speed);
			Y += (float)(-Math.Cos(Direction) * Speed);

            if (IsTopLevel && Action.IsCompleted)
                Vanish();
        }

        /// <summary>
        /// Called when the bullet encounters a &lt;vanish&gt; node, or when a top level bullet completes its action.
        /// 
        /// <code>base.Vanish()</code> should be called if this is overridden.
        /// </summary>
        public virtual void Vanish()
        {
            IsVanished = true;
        }
    }
}
