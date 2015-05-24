using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago
{
    /// <summary>
    /// Extended by any node that can be inside an &lt;action&gt; node.
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// True if this task has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Place this task back into its pre-run state.
        /// Used primarily for &lt;repeat&gt; nodes.
        /// </summary>
        void Reset();

        /// <summary>
        /// Runs this task on the given bullet. This should be called at 60 fps.
        /// </summary>
        /// <param name="bullet">The bullet to run this task on.</param>
        /// <returns>True if following tasks should be run, otherwise false.</returns>
        bool Run(Bullet bullet);

        /// <summary>
        /// Copies this task and resets it.
        /// </summary>
        /// <returns>A reset copy of this task.</returns>
        ITask Copy();
    }
}