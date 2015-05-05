using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Tamago
{
    public class ParseException : Exception
    {
        public ParseException(string message)
            : base(message)
        { }

        public ParseException(string message, Exception inner)
            : base(message, inner)
        { }
    }

    public class BulletPattern
    {
        public const string TopLevelLabel = "top";

        public ActionRef TopLevelAction { get; private set; }
        private List<ActionRef> Actions;

        private BulletPattern(List<ActionRef> actions)
        {
            Actions = actions;
            TopLevelAction = actions.Find(a => a.Label == TopLevelLabel);
        }

        // TODO rethrow xmlexceptions
        public static BulletPattern ParseString(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            var root = doc.Root;

            // extract all labelled nodes
            var actions = (from node in root.XPathSelectElements("//action[@label]")
                           select new ActionRef(node)).ToList();

            return new BulletPattern(actions);
        }
    }

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

        public void Run(Bullet bullet)
        {
            for (int i = 0; i < _tasks.Count; i++)
            {
                _tasks[i].Run(bullet);
            }
        }
    }

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
        void Run(Bullet bullet);
    }

}
