using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;vanish&gt; node.
    /// </summary>
    public class Vanish : Task
    {
        /// <summary>
        /// True if this task has been successfully run.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Parses a &lt;vanish&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;vanish&gt; node.</param>
        public Vanish(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");
        }

        /// <summary>
        /// Vanishes the given bullet.
        /// </summary>
        /// <param name="bullet">The bullet to vanish.</param>
        /// <returns>True always</returns>
        public bool Run(Bullet bullet)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            bullet.Vanish();

            IsCompleted = true;
            return true;
        }
    }
}
