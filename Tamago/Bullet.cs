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

        public List<IAction> Actions { get; protected set; }
        public bool IsTopLevel { get; protected set; }
        public bool IsVanished { get; set; }
        public bool IsCompleted
        {
            get { return Actions.TrueForAll(a => a.IsCompleted); }
        }

        public virtual float X { get; set; }
        public virtual float Y { get; set; }

        public float Direction { get; set; }
        public float Speed { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }

        public float FireDirection { get; set; }
        public float FireSpeed { get; set; }

        public float? NewDirection { get; set; }
        public float? NewSpeed { get; set; }
        public float? NewVelocityX { get; set; }
        public float? NewVelocityY { get; set; }

        public Bullet(IBulletManager manager)
        {
            BulletManager = manager;

            X = 0;
            Y = 0;

            Direction = 0;
            Speed = 0;
            VelocityX = 0;
            VelocityY = 0;

            FireDirection = 0;
            FireSpeed = 1;

            NewDirection = null;
            NewSpeed = null;
            NewVelocityX = null;
            NewVelocityY = null;
        }

        /// <summary>
        /// Initializes this bullet with some behaviour.
        /// </summary>
        /// <param name="action">What this bullet will do.</param>
        /// <param name="isTopLevel">Whether this is a top level bullet (i.e., &lt;action label="top"/&gt;). Top level bullets should be invisible.</param>
        public void SetPattern(IAction action, bool isTopLevel = false)
        {
            var list = new List<IAction>();
            list.Add(action);
            SetPattern(list, isTopLevel);
        }

        /// <summary>
        /// Initializes this bullet with some behaviour.
        /// </summary>
        /// <param name="actions">What this bullet will do.</param>
        /// <param name="isTopLevel">Whether this is a top level bullet (i.e., &lt;action label="top"/&gt;). Top level bullets should be invisible.</param>
        public void SetPattern(List<IAction> actions, bool isTopLevel = false)
        {
            Actions = actions;
            IsTopLevel = isTopLevel;
            IsVanished = false;
        }

        /// <summary>
        /// Gets the angle in radians to point at the player.
        /// </summary>
        public float AimDirection
        {
            get
            {
                var raw = (float)Math.Atan2(BulletManager.PlayerX - X, -(BulletManager.PlayerY - Y));
                return MathHelper.NormalizeAngle(raw);
            }
        }

        /// <summary>
        /// Runs the associated actions for this bullet and updates position based on speed, direction, and velocities.
        /// This method should be called at 60 fps.
        /// </summary>
        public void Update()
        {
            if (IsVanished)
                return;

            // actions are run before bullets move
            Actions.ForEach(a => a.Run(this, new float[] { }));

            // apply queued changes if they exist
            if (NewSpeed != null)
            {
                Speed = NewSpeed.Value;
                NewSpeed = null;
            }
            if (NewDirection != null)
            {
                Direction = MathHelper.NormalizeAngle(NewDirection.Value);
                NewDirection = null;
            }
            if (NewVelocityX != null)
            {
                VelocityX = NewVelocityX.Value;
                NewVelocityX = null;
            }
            if (NewVelocityY != null)
            {
                VelocityY = NewVelocityY.Value;
                NewVelocityY = null;
            }
            
            // formulae are atypical because of non-standard coordinate system
			X += (float)(Math.Sin(Direction) * Speed) + VelocityX;
			Y += (float)(-Math.Cos(Direction) * Speed) + VelocityY;

            if (IsTopLevel && IsCompleted)
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
