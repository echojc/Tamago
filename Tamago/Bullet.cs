using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago
{
    public class Bullet
    {
        private static float[] EmptyArray = new float[0];
        private static Dictionary<string, float> EmptyDictionary = new Dictionary<string, float>();

        public IBulletManager BulletManager;

        public List<IAction> Actions { get; protected set; }
        public float[] Params { get; protected set; }
        public Dictionary<string, float> Rest { get; protected set; }
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
        /// Sets the expression parameters for actions encapsulated by this bullet.
        /// </summary>
        /// <param name="args">The parameters to use.</param>
        /// <param name="rest">The other parameters to use.</param>
        public void SetParams(float[] args, Dictionary<string, float> rest)
        {
            if (args == null) throw new ArgumentNullException("args");
            if (rest == null) throw new ArgumentNullException("rest");

            Params = new float[args.Length];
            Array.Copy(args, Params, args.Length);
            Rest = new Dictionary<string, float>(rest);
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
        /// 
        /// This method should be called at 60 fps.
        /// 
        /// When overriding, it is recommended that <code>base.Update()</code> should be called before any custom code is executed.
        /// </summary>
        public virtual void Update()
        {
            if (IsVanished)
                return;

            // actions are run before bullets move
            Actions.ForEach(a => a.Run(this, Params ?? EmptyArray, Rest ?? EmptyDictionary));

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

        #region Boilerplate

        public override string ToString()
        {
            return string.Format("Bullet [X={0} Y={1} Direction={2} Speed={3}",
                X, Y, Direction, Speed);
        }

        #endregion
    }
}
