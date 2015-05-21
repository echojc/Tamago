using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago
{
    /// <summary>
    /// Used for subclassing &lt;action&gt; and &lt;actionRef&gt; nodes.
    /// </summary>
    public interface IAction : Task
    {
        /// <summary>
        /// A read-only view of all tasks this action performs.
        /// </summary>
        /// <remarks>
        /// You can still force underlying tasks to run but that's not recommended.
        /// </remarks>
        IList<Task> Tasks { get; }

        /// <summary>
        /// The name of this action.
        /// </summary>
        string Label { get; }
    }
}
