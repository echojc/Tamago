using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;wait&gt; node.
    /// </summary>
    public class Wait : Task
    {
        private int framesRunCount = 0;

        /// <summary>
        /// How many frames to wait for.
        /// </summary>
        public int Duration { get; private set; }

        /// <summary>
        /// True if we have waited <code>Duration</code> number of frames.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Parses a &lt;wait&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;wait&gt; node.</param>
        public Wait(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var duration = node.Value;
            if (duration == string.Empty)
                throw new ParseException("wait node without duration");

            Duration = (int)decimal.Parse(duration);
        }

        /// <summary>
        /// Prevents further task execution until <code>Duration</code> frames have passed.
        /// </summary>
        /// <param name="bullet">The bullet doing the waiting.</param>
        /// <returns>True if this has been run 
        public bool Run(Bullet bullet)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            if (IsCompleted)
                return true;

            framesRunCount++;
            IsCompleted = framesRunCount >= Duration;

            return false;
        }
    }
}
