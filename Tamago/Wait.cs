using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;wait&gt; node.
    /// </summary>
    public class Wait : ITask
    {
        private int framesRunCount = 0;

        /// <summary>
        /// How many frames to wait for.
        /// </summary>
        public Expression Duration { get; private set; }

        /// <summary>
        /// True if we have waited <see cref="Duration">Duration</see> number of frames.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// For cloning.
        /// </summary>
        private Wait(Expression duration)
        {
            Duration = duration;
            Reset();
        }

        /// <summary>
        /// Parses a &lt;wait&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;wait&gt; node.</param>
        public Wait(XElement node)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (node.Name.LocalName != "wait") throw new ArgumentException("node");

            var duration = node.Value;
            if (duration == string.Empty)
                throw new ParseException("wait node without duration");

            Duration = new Expression(duration);
        }

        /// <summary>
        /// Resets this task to its pre-run state.
        /// </summary>
        public void Reset()
        {
            framesRunCount = 0;
            IsCompleted = false;
        }

        /// <summary>
        /// Prevents further task execution until <see cref="Duration">Duration</see> frames have passed.
        /// </summary>
        /// <param name="bullet">The bullet doing the waiting.</param>
        /// <param name="args">Values for params in expressions.</param>
        /// <param name="manager">BulletManager for <see cref="Rand"/> and <see cref="Rank"/> in expressions.</param>
        /// <returns>True if <see cref="Duration">Duration</see> frames have passed, otherwise false</returns>
        public bool Run(Bullet bullet, float[] args)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            if (IsCompleted)
                return true;

            // must be rounded down
            int duration = (int)Duration.Evaluate(args, bullet.BulletManager);

            framesRunCount++;
            IsCompleted = framesRunCount >= duration;
            return framesRunCount > duration;
        }

        /// <summary>
        /// Copies this task and resets it.
        /// </summary>
        /// <returns>A reset copy of this task.</returns>
        public ITask Copy()
        {
            return new Wait(Duration);
        }
    }
}
