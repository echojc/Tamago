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
    public interface Task
    {
        /// <summary>
        /// True if this task has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Runs this task on the given bullet. This should be called at 60 fps.
        /// </summary>
        /// <param name="bullet">The bullet to run this task on.</param>
        /// <returns>True if following tasks should be run, otherwise false.</returns>
        bool Run(Bullet bullet);
    }
}