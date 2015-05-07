using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;changeSpeed&gt; node.
    /// </summary>
    public class ChangeSpeed : Task
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
        public int Term { get; private set; }

        /// <summary>
        /// True if this has been run <code>Term</code> times or more.
        /// </summary>
        public bool IsCompleted
        {
            get { return framesRunCount >= Term; }
        }

        /// <summary>
        /// Parses a &lt;changeSpeed&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;changeSpeed&gt; node.</param>
        public ChangeSpeed(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var speed = node.Element("speed");
            if (speed == null)
                throw new ParseException("<changeSpeed> node requires a <speed> node.");
            Speed = new Speed(speed);

            var term = node.Element("term");
            if (term == null)
                throw new ParseException("<changeSpeed> node requires a <term> node.");
            Term = (int)float.Parse(term.Value);

            Reset();
        }

        /// <summary>
        /// Resets this task to its pre-run state.
        /// </summary>
        public void Reset()
        {
            isFirstRun = true;
            framesRunCount = 0;
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
        /// <returns>True always</returns>
        public bool Run(Bullet bullet)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            if (!isFirstRun && IsCompleted)
                return true;
            
            if (isFirstRun)
            {
                isFirstRun = false;
                initialSpeed = bullet.Speed;

                switch (Speed.Type)
                {
                    case SpeedType.Relative:
                        targetSpeed = bullet.Speed + Speed.Value;
                        break;
                    case SpeedType.Sequence:
                        targetSpeed = bullet.Speed + (Speed.Value * Math.Max(0, Term));
                        break;
                    case SpeedType.Absolute:
                    default:
                        targetSpeed = Speed.Value;
                        break;
                }
            }

            framesRunCount++;
            var ratio = Term <= 0 ? 1 : (float)framesRunCount / Term;
            bullet.NewSpeed = initialSpeed + (targetSpeed - initialSpeed) * ratio;

            return true;
        }
    }
}
