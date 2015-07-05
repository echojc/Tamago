using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents an &lt;actionRef&gt; node.
    /// </summary>
    public class ActionRef : IAction
    {
        private BulletPattern _pattern;
        private ActionDef _action;
        private Expression[] _params;

        /// <summary>
        /// The underlying action. Resolution is delayed until the first time this property is read.
        /// </summary>
        public ActionDef Action
        {
            get
            {
                if (_action == null)
                    _action = (ActionDef)_pattern.CopyAction(Label);
                return _action;
            }
        }

        /// <summary>
        /// The parameters for nested expressions.
        /// </summary>
        public ReadOnlyCollection<Expression> Params
        {
            get
            {
                return Array.AsReadOnly(_params);
            }
        }

        /// <summary>
        /// For cloning.
        /// </summary>
        private ActionRef(string label, BulletPattern pattern, Expression[] args)
        {
            Label = label;
            _pattern = pattern;
            _params = args;
        }

        /// <summary>
        /// Parses an &lt;actionRef&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;actionRef&gt; node.</param>
        /// <param name="pattern">The pattern this node belongs to.</param>
        public ActionRef(XElement node, BulletPattern pattern)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (pattern == null) throw new ArgumentNullException("pattern");
            if (node.Name.LocalName != "actionRef") throw new ArgumentException("node");

            var label = node.Attribute("label");
            if (label == null)
                throw new ParseException("<actionRef> node requires a label.");
            Label = label.Value;

            var args = node.Elements("param");
            _params = args.Select(p => new Expression(p.Value)).ToArray();

            _pattern = pattern;
        }

        /// <summary>
        /// A read-only view of all tasks the underlying action performs.
        /// </summary>
        /// <remarks>
        /// You can still force underlying tasks to run but that's not recommended.
        /// </remarks>
        public IList<ITask> Tasks
        {
            get { return Action.Tasks; }
        }

        /// <summary>
        /// The name of the action to resolve to.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// True if the underlying action has completed.
        /// </summary>
        public bool IsCompleted
        {
            get { return Action.IsCompleted; }
        }

        /// <summary>
        /// Resets the underlying action.
        /// </summary>
        public void Reset()
        {
            Action.Reset();
        }

        /// <summary>
        /// Runs the underlying action. Parameters are evaluated with the given arguments and passed on.
        /// </summary>
        /// <param name="bullet">The bullet to run the underlying tasks against.</param>
        /// <param name="args">Values for params in expressions.</param>
        /// <param name="rest">Any other arguments for expressions.</param>
        /// <returns>True if no waiting is required, otherwise the result of any nested &lt;wait&gt; nodes</returns>
        public bool Run(Bullet bullet, float[] args, Dictionary<string, float> rest)
        {
            float[] newArgs = new float[_params.Length];
            for (int i = 0; i < newArgs.Length; i++)
                newArgs[i] = _params[i].Evaluate(args, rest.GetValueOrDefault, bullet.BulletManager);

            return Action.Run(bullet, newArgs, new Dictionary<string, float>());
        }

        /// <summary>
        /// Copies this action reference with the underlying action unresolved.
        /// </summary>
        /// <returns>An action reference that has not been resolved.</returns>
        public ITask Copy()
        {
            return new ActionRef(Label, _pattern, _params);
        }
    }
}
