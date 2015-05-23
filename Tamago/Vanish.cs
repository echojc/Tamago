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
        /// For cloning.
        /// </summary>
        private Vanish()
        {
            Reset();
        }

        /// <summary>
        /// Parses a &lt;vanish&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;vanish&gt; node.</param>
        public Vanish(XElement node)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (node.Name.LocalName != "vanish") throw new ArgumentException("node");
        }

        /// <summary>
        /// Resets this task to its pre-run state.
        /// </summary>
        public void Reset()
        {
            IsCompleted = false;
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

            if (IsCompleted)
                return true;

            bullet.Vanish();

            IsCompleted = true;
            return true;
        }

        /// <summary>
        /// Copies this task and resets it.
        /// </summary>
        /// <returns>A reset copy of this task.</returns>
        public Task Copy()
        {
            return new Vanish();
        }
    }
}
