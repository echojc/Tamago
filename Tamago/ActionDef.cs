using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents an &lt;action&gt; node.
    /// </summary>
    public class ActionDef : IAction
    {
        private BulletPattern _pattern;
        private List<ITask> _tasks;

        /// <summary>
        /// A read-only view of all tasks this action performs.
        /// </summary>
        /// <remarks>
        /// You can still force underlying tasks to run but that's not recommended.
        /// </remarks>
        public IList<ITask> Tasks
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

        private static ActionDef _default = new ActionDef(new List<ITask>(), null, null);

        /// <summary>
        /// A static instance for a no-op action.
        /// </summary>
        public static ActionDef Default
        {
            get { return _default; }
        }

        /// <summary>
        /// For cloning.
        /// </summary>
        private ActionDef(List<ITask> tasks, string label, BulletPattern pattern)
        {
            _pattern = pattern;
            _tasks = tasks;
            Label = label;
            Reset();
        }

        /// <summary>
        /// Parses an &lt;action&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;action&gt; node.</param>
        /// <param name="pattern">The pattern this node belongs to.</param>
        public ActionDef(XElement node, BulletPattern pattern)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (pattern == null) throw new ArgumentNullException("pattern");
            if (node.Name.LocalName != "action") throw new ArgumentException("node");

            _pattern = pattern;

            var label = node.Attribute("label");
            if (label != null)
                Label = label.Value;

            _tasks = new List<ITask>();
            foreach (var child in node.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "accel":
                        _tasks.Add(new Accel(child));
                        break;
                    case "action":
                        _tasks.Add(new ActionDef(child, _pattern));
                        break;
                    case "actionRef":
                        _tasks.Add(new ActionRef(child, _pattern));
                        break;
                    case "changeDirection":
                        _tasks.Add(new ChangeDirection(child));
                        break;
                    case "changeSpeed":
                        _tasks.Add(new ChangeSpeed(child));
                        break;
                    case "fire":
                        _tasks.Add(new FireDef(child, _pattern));
                        break;
                    case "fireRef":
                        _tasks.Add(new FireRef(child, _pattern));
                        break;
                    case "repeat":
                        _tasks.Add(new Repeat(child, _pattern));
                        break;
                    case "vanish":
                        _tasks.Add(new Vanish(child));
                        break;
                    case "wait":
                        _tasks.Add(new Wait(child));
                        break;
                }
            }
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
        /// <param name="args">Values for params in expressions.</param>
        /// <param name="manager">BulletManager for <see cref="Rand"/> and <see cref="Rank"/> in expressions.</param>
        /// <returns>True if no waiting is required, otherwise the result of any nested &lt;wait&gt; nodes</returns>
        public bool Run(Bullet bullet, float[] args)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            if (IsCompleted)
                return true;

            for (int i = 0; i < _tasks.Count; i++)
            {
                var isDone = _tasks[i].Run(bullet, args);

                if (!isDone)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Copies this task and resets it.
        /// </summary>
        /// <returns>A reset copy of this task.</returns>
        public ITask Copy()
        {
            List<ITask> copies = new List<ITask>();
            foreach (ITask t in _tasks)
                copies.Add(t.Copy());

            return new ActionDef(copies, Label, _pattern);
        }
    }
}
