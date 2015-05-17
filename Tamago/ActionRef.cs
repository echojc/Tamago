using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents an &lt;action&gt; node.
    /// </summary>
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
        /// Parses an &lt;action&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;action&gt; node.</param>
        public ActionRef(XElement node)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (node.Name.LocalName != "action") throw new ArgumentException("node");

            var label = node.Attribute("label");
            if (label != null)
                Label = label.Value;

            foreach (var child in node.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "accel":
                        _tasks.Add(new Accel(child));
                        break;
                    case "action":
                        _tasks.Add(new ActionRef(child));
                        break;
                    case "changeDirection":
                        _tasks.Add(new ChangeDirection(child));
                        break;
                    case "changeSpeed":
                        _tasks.Add(new ChangeSpeed(child));
                        break;
                    case "fire":
                        _tasks.Add(new FireRef(child));
                        break;
                    case "repeat":
                        _tasks.Add(new Repeat(child));
                        break;
                    case "vanish":
                        _tasks.Add(new Vanish(child));
                        break;
                    case "wait":
                        _tasks.Add(new Wait(child));
                        break;
                }
            }

            Reset();
        }

        /// <summary>
        /// Resets all nested tasks to their pre-run state.
        /// </summary>
        public void Reset()
        {
            _tasks.ForEach(t => t.Reset());
        }

        /// <summary>
        /// Runs the nested tasks in order.
        /// </summary>
        /// <param name="bullet">The bullet to run the tasks against.</param>
        /// <returns>True if no waiting is required, otherwise the result of any nested &lt;wait&gt; nodes</returns>
        public bool Run(Bullet bullet)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            if (IsCompleted)
                return true;

            for (int i = 0; i < _tasks.Count; i++)
            {
                var isDone = _tasks[i].Run(bullet);

                if (!isDone)
                    return false;
            }

            return true;
        }
    }
}
