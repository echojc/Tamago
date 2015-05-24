using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago
{
    /// <summary>
    /// Used internally to generically parse top level labelled nodes.
    /// </summary>
    public interface ILabelled
    {
        /// <summary>
        /// Name to refer to this node.
        /// </summary>
        string Label { get; }
    }
}
