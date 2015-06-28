using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents an &lt;accel&gt; node.
    /// </summary>
    public class Accel : ITask
    {
        private float initialVelocityX;
        private float targetVelocityX;
        private float initialVelocityY;
        private float targetVelocityY;

        private bool isFirstRun = true;
        private int framesRunCount = 0;

        /// <summary>
        /// The horizontal velocity to change to.
        /// </summary>
        public Speed? VelocityX { get; private set; }

        /// <summary>
        /// The vertical velocity to change to.
        /// </summary>
        public Speed? VelocityY { get; private set; }

        /// <summary>
        /// The number of frames to animate the change over.
        /// </summary>
        public Expression Term { get; private set; }

        /// <summary>
        /// True if this has been run <see cref="Term">Term</see> times or more.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// For cloning.
        /// </summary>
        private Accel(Speed? velocityX, Speed? velocityY, Expression term)
        {
            VelocityX = velocityX;
            VelocityY = velocityY;
            Term = term;
            Reset();
        }

        /// <summary>
        /// Parses an &lt;accel&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;accel&gt; node.</param>
        public Accel(XElement node)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (node.Name.LocalName != "accel") throw new ArgumentException("node");

            var term = node.Element("term");
            if (term == null)
                throw new ParseException("<accel> node requires a <term> node.");
            Term = new Expression(term.Value);

            var horizontal = node.Element("horizontal");
            if (horizontal != null)
                VelocityX = new Speed(horizontal);

            var vertical = node.Element("vertical");
            if (vertical != null)
                VelocityY = new Speed(vertical);
        }

        /// <summary>
        /// Resets this task to its pre-run state.
        /// </summary>
        public void Reset()
        {
            isFirstRun = true;
            framesRunCount = 0;
            IsCompleted = false;
        }

        /// <summary>
        /// Sets the X/Y velocities of the bullet for the next frame.
        /// </summary>
        /// <remarks>
        /// This updates <see cref="Bullet.NewVelocityX"/> and
        /// <see cref="Bullet.NewVelocityY"/> which copies over to
        /// <see cref="Bullet.VelocityX"/> and <see cref="Bullet.VelocityY"/>
        /// after all tasks are run. This is done to prevent multiple
        /// &lt;accel&gt;s from stacking their effects. Note that both
        /// &lt;horizontal&gt; and &lt;vertical&gt; default to null which do
        /// not apply any effect to the bullet.
        /// </remarks>
        /// <param name="bullet">The bullet to change the velocities of.</param>
        /// <param name="args">Values for params in expressions.</param>
        /// <param name="manager">BulletManager for <see cref="Rand"/> and <see cref="Rank"/> in expressions.</param>
        /// <param name="rest">Any other arguments for expressions.</param>
        /// <returns>True always</returns>
        public bool Run(Bullet bullet, float[] args, Dictionary<string, float> rest)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            if (!isFirstRun && IsCompleted)
                return true;

            // must be rounded down
            int term = (int)Term.Evaluate(args, rest.GetValueOrDefault, bullet.BulletManager);

            if (isFirstRun)
            {
                isFirstRun = false;
                initialVelocityX = bullet.VelocityX;
                initialVelocityY = bullet.VelocityY;

                if (VelocityX != null)
                {
                    Speed x = VelocityX.Value;
                    var xvalue = x.Value.Evaluate(args, rest.GetValueOrDefault, bullet.BulletManager);
                    switch (x.Type)
                    {
                        case SpeedType.Relative:
                            targetVelocityX = bullet.VelocityX + xvalue;
                            break;
                        case SpeedType.Sequence:
                            targetVelocityX = bullet.VelocityX + (xvalue * Math.Max(0, term));
                            break;
                        case SpeedType.Absolute:
                        default:
                            targetVelocityX = xvalue;
                            break;
                    }
                }

                if (VelocityY != null)
                {
                    Speed y = VelocityY.Value;
                    var yvalue = y.Value.Evaluate(args, rest.GetValueOrDefault, bullet.BulletManager);
                    switch (y.Type)
                    {
                        case SpeedType.Relative:
                            targetVelocityY = bullet.VelocityY + yvalue;
                            break;
                        case SpeedType.Sequence:
                            targetVelocityY = bullet.VelocityY + (yvalue * Math.Max(0, term));
                            break;
                        case SpeedType.Absolute:
                        default:
                            targetVelocityY = yvalue;
                            break;
                    }
                }
            }

            framesRunCount++;
            var ratio = term <= 0 ? 1 : (float)framesRunCount / term;

            if (VelocityX != null)
                bullet.NewVelocityX = initialVelocityX + (targetVelocityX - initialVelocityX) * ratio;

            if (VelocityY != null)
                bullet.NewVelocityY = initialVelocityY + (targetVelocityY - initialVelocityY) * ratio;

            IsCompleted = framesRunCount >= term;
            return true;
        }

        /// <summary>
        /// Copies this task and resets it.
        /// </summary>
        /// <returns>A reset copy of this task.</returns>
        public ITask Copy()
        {
            return new Accel(VelocityX, VelocityY, Term);
        }
    }
}
