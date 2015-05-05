using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago
{
    public class Bullet
    {
        private ActionRef Action;
        internal IBulletManager BulletManager;

        public bool IsTopLevel { get; private set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Direction { get; set; }
        public float Speed { get; set; }
        public float FireDirection { get; set; }
        public float FireSpeed { get; set; }

        public Bullet(IBulletManager manager)
        {
            BulletManager = manager;

            X = 0;
            Y = 0;
            Direction = 0;
            Speed = 0;
            FireDirection = 0;
            FireSpeed = 1;
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
        }

        /// <summary>
        /// Gets the bearing in radians to point at the player position.
        /// </summary>
        public float AimDirection
        {
            get { return (float)Math.Atan2(BulletManager.PlayerX - X, -(BulletManager.PlayerY - Y)); }
        }

        public void Update()
        {
            // formulae are atypical because of non-standard coordinate system
			X += (float)(Math.Sin(Direction) * Speed);
			Y += (float)(-Math.Cos(Direction) * Speed);

            Action.Run(this);
        }
    }
}
