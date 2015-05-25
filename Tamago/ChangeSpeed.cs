using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;changeSpeed&gt; node.
    /// </summary>
    public class ChangeSpeed : ITask
    {
        private float initialSpeed;
        private float targetSpeed;

        private bool isFirstRun = true;
        private int framesRunCount = 0;

        /// <summary>
        /// The speed to change to.
        /// </summary>
        public Speed Speed { get; private set; }

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
        private ChangeSpeed(Speed speed, Expression term)
        {
            Speed = speed;
            Term = term;
            Reset();
        }

        /// <summary>
        /// Parses a &lt;changeSpeed&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;changeSpeed&gt; node.</param>
        public ChangeSpeed(XElement node)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (node.Name.LocalName != "changeSpeed") throw new ArgumentException("node");

            var speed = node.Element("speed");
            if (speed == null)
                throw new ParseException("<changeSpeed> node requires a <speed> node.");
            Speed = new Speed(speed);

            var term = node.Element("term");
            if (term == null)
                throw new ParseException("<changeSpeed> node requires a <term> node.");
            Term = new Expression(term.Value);
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
        /// Sets the speed of the bullet for the next frame.
        /// </summary>
        /// <remarks>
        /// This updates <see cref="Bullet.NewSpeed"/> which copies over to
        /// <see cref="Bullet.Speed"/> after all tasks are run. This is done
        /// to prevent multiple &lt;changeSpeed&gt;s from stacking their effects.
        /// </remarks>
        /// <param name="bullet">The bullet to change the speed of.</param>
        /// <param name="args">Values for params in expressions.</param>
        /// <param name="manager">BulletManager for <see cref="Rand"/> and <see cref="Rank"/> in expressions.</param>
        /// <returns>True always</returns>
        public bool Run(Bullet bullet, float[] args)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            if (!isFirstRun && IsCompleted)
                return true;

            // must be rounded down
            int term = (int)Term.Evaluate(args, bullet.BulletManager);
            
            if (isFirstRun)
            {
                isFirstRun = false;
                initialSpeed = bullet.Speed;

                var speed = Speed.Value.Evaluate(args, bullet.BulletManager);
                switch (Speed.Type)
                {
                    case SpeedType.Relative:
                        targetSpeed = bullet.Speed + speed;
                        break;
                    case SpeedType.Sequence:
                        targetSpeed = bullet.Speed + (speed * Math.Max(0, term));
                        break;
                    case SpeedType.Absolute:
                    default:
                        targetSpeed = speed;
                        break;
                }
            }

            framesRunCount++;
            var ratio = term <= 0 ? 1 : (float)framesRunCount / term;
            bullet.NewSpeed = initialSpeed + (targetSpeed - initialSpeed) * ratio;

            IsCompleted = framesRunCount >= term;
            return true;
        }

        /// <summary>
        /// Copies this task and resets it.
        /// </summary>
        /// <returns>A reset copy of this task.</returns>
        public ITask Copy()
        {
            return new ChangeSpeed(Speed, Term);
        }
    }
}
