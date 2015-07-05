using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;changeDirection&gt; node.
    /// </summary>
    public class ChangeDirection : ITask
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
        public Expression Term { get; private set; }

        /// <summary>
        /// True if this has been run <see cref="Term">Term</see> times or more.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// For cloning.
        /// </summary>
        private ChangeDirection(Direction direction, Expression term)
        {
            Direction = direction;
            Term = term;
            Reset();
        }

        /// <summary>
        /// Parses a &lt;changeDirection&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;changeDirection&gt; node.</param>
        public ChangeDirection(XElement node)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (node.Name.LocalName != "changeDirection") throw new ArgumentException("node");

            var direction = node.Element("direction");
            if (direction == null)
                throw new ParseException("<changeDirection> node requires a <direction> node.");
            Direction = new Direction(direction);

            var term = node.Element("term");
            if (term == null)
                throw new ParseException("<changeDirection> node requires a <term> node.");
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
        /// Sets the direction of the bullet for the next frame.
        /// </summary>
        /// <remarks>
        /// This updates <see cref="Bullet.NewDirection"/> which copies over to
        /// <see cref="Bullet.Direction"/> after all tasks are run. This is done
        /// to prevent multiple &lt;changeDirection&gt;s from stacking their effects.
        /// </remarks>
        /// <param name="bullet">The bullet to change the direction of.</param>
        /// <param name="args">Values for params in expressions.</param>
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
                initialDirection = bullet.Direction;

                var direction = MathHelper.ToRadians(Direction.Value.Evaluate(args, rest.GetValueOrDefault, bullet.BulletManager));
                switch (Direction.Type)
                {
                    case DirectionType.Relative:
                        targetDirection = bullet.Direction + direction;
                        break;
                    case DirectionType.Sequence:
                        // we cache the sequence value 
                        // this can't be interpolated because we normalise the target value
                        targetDirection = direction;
                        break;
                    case DirectionType.Absolute:
                        targetDirection = direction;
                        break;
                    case DirectionType.Aim:
                    default:
                        targetDirection = bullet.AimDirection + direction;
                        break;
                }

                // denormalise target so we can lerp
                // this also guarantees shortest path
                targetDirection = initialDirection + MathHelper.NormalizeAngle(targetDirection - initialDirection);
            }

            framesRunCount++;
            if (Direction.Type == DirectionType.Sequence)
            {
                // we cached the per-frame delta in targetDirection in the first run block
                // this gets around normalisation of the target value
                // we also don't run if term <= 0 as per the specs
                if (term > 0)
                    bullet.NewDirection = MathHelper.NormalizeAngle(bullet.Direction + targetDirection);
            }
            else
            {
                var ratio = term <= 0 ? 1 : (float)framesRunCount / term;
                bullet.NewDirection = initialDirection + (targetDirection - initialDirection) * ratio;
            }
            IsCompleted = framesRunCount >= term;

            return true;
        }

        /// <summary>
        /// Copies this task and resets it.
        /// </summary>
        /// <returns>A reset copy of this task.</returns>
        public ITask Copy()
        {
            return new ChangeDirection(Direction, Term);
        }
    }
}
