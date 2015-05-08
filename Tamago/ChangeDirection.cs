using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;changeDirection&gt; node.
    /// </summary>
    public class ChangeDirection : Task
    {
        private float initialDirection;
        private float targetDirection;

        private bool isFirstRun = true;
        private int framesRunCount = 0;

        /// <summary>
        /// The direction to change to.
        /// </summary>
        public Direction Direction { get; private set; }

        /// <summary>
        /// The number of frames to animate the change over.
        /// </summary>
        public int Term { get; private set; }

        /// <summary>
        /// True if this has been run <see cref="Term">Term</see> times or more.
        /// </summary>
        public bool IsCompleted
        {
            get { return framesRunCount >= Term; }
        }

        /// <summary>
        /// Parses a &lt;changeDirection&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;changeDirection&gt; node.</param>
        public ChangeDirection(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var direction = node.Element("direction");
            if (direction == null)
                throw new ParseException("<changeDirection> node requires a <direction> node.");
            Direction = new Direction(direction);

            var term = node.Element("term");
            if (term == null)
                throw new ParseException("<changeDirection> node requires a <term> node.");
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
        /// Sets the direction of the bullet for the next frame.
        /// </summary>
        /// <remarks>
        /// This updates <see cref="Bullet.NewDirection"/> which copies over to
        /// <see cref="Bullet.Direction"/> after all tasks are run. This is done
        /// to prevent multiple &lt;changeDirection&gt;s from stacking their effects.
        /// </remarks>
        /// <param name="bullet">The bullet to change the direction of.</param>
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
                initialDirection = bullet.Direction;

                switch (Direction.Type)
                {
                    case DirectionType.Relative:
                        targetDirection = bullet.Direction + Direction.Value;
                        break;
                    case DirectionType.Sequence:
                        targetDirection = bullet.Direction + (Direction.Value * Math.Max(0, Term));
                        break;
                    case DirectionType.Absolute:
                        targetDirection = Direction.Value;
                        break;
                    case DirectionType.Aim:
                    default:
                        targetDirection = bullet.AimDirection + Direction.Value;
                        break;
                }
            }

            framesRunCount++;
            var ratio = Term <= 0 ? 1 : (float)framesRunCount / Term;
            bullet.NewDirection = initialDirection + (targetDirection - initialDirection) * ratio;

            return true;
        }
    }
}
