using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Tamago
{
    public class ActionRef : Task
    {
        private List<Task> _tasks = new List<Task>();

        /// <summary>
        /// A read-only view of all tasks this action performs.
        /// </summary>
        /// <remarks>
        /// You can still force underlying tasks to run but that's not recommended.
        /// </remarks>
        public IList<Task> Tasks
        {
            get { return _tasks.AsReadOnly(); }
        }

        /// <summary>
        /// The name of this action.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// True if every nested task has completed.
        /// </summary>
        public bool IsCompleted
        {
            get { return _tasks.TrueForAll(t => t.IsCompleted); }
        }

        private static ActionRef _default = new ActionRef(XElement.Parse("<action/>"));

        /// <summary>
        /// A static instance for a no-op action.
        /// </summary>
        public static ActionRef Default
        {
            get { return _default; }
        }


        /// <summary>
        /// Creates a new representation of an &lt;action&gt; node.
        /// </summary>
        /// <param name="node">The node this instance should base itself on. If null, creates an empty action (equivalent to &lt;action/&gt;).</param>
        public ActionRef(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var label = node.Attribute("label");
            if (label != null)
                Label = label.Value;

            foreach (var child in node.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "fire":
                        _tasks.Add(new FireRef(child));
                        break;
                }
            }
        }

        public bool Run(Bullet bullet)
        {
            for (int i = 0; i < _tasks.Count; i++)
            {
                _tasks[i].Run(bullet);
            }

            // TODO
            return true;
        }
    }
}
