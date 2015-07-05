using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;repeat&gt; node.
    /// </summary>
    public class Repeat : ITask
    {
        private int timesRunCount = 0;

        /// <summary>
        /// The number of times the nested action has been completely run through.
        /// </summary>
        public Expression Times { get; private set; }

        /// <summary>
        /// The action to repeat.
        /// </summary>
        public IAction Action { get; private set; }

        /// <summary>
        /// True if the nested action has been completely run through <see cref="Times">Times</see> times.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// For cloning.
        /// </summary>
        private Repeat(IAction actionRef, Expression times)
        {
            Action = actionRef;
            Times = times;
            Reset();
        }

        /// <summary>
        /// Parses a &lt;repeat&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;repeat&gt; node.</param>
        /// <param name="pattern">The pattern this node belongs to.</param>
        public Repeat(XElement node, BulletPattern pattern)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (pattern == null) throw new ArgumentNullException("pattern");
            if (node.Name.LocalName != "repeat") throw new ArgumentException("node");

            var times = node.Element("times");
            if (times == null)
                throw new ParseException("<repeat> node requires a <times> node.");
            Times = new Expression(times.Value);

            var action = node.Element("action");
            var actionRef = node.Element("actionRef");

            if (action != null && actionRef != null)
                throw new ParseException("<repeat> node must only have one of <action> or <actionRef> nodes.");
            else if (action != null)
                Action = new ActionDef(action, pattern);
            else if (actionRef != null)
                Action = new ActionRef(actionRef, pattern);
            else
                throw new ParseException("<repeat> node requires an <action> or an <actionRef> node.");
        }

        /// <summary>
        /// Resets this task to its pre-run state.
        /// </summary>
        public void Reset()
        {
            IsCompleted = false;
            timesRunCount = 0;
        }

        /// <summary>
        /// Repeats the given action <see cref="Times">Times</see> times, waiting as necessary.
        /// </summary>
        /// <param name="bullet">The bullet to run the tasks against.</param>
        /// <param name="args">Values for params in expressions.</param>
        /// <param name="rest">Any other arguments for expressions.</param>
        /// <returns>True if no waiting is required, otherwise the result of the nested action.</returns>
        public bool Run(Bullet bullet, float[] args, Dictionary<string, float> rest)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            if (IsCompleted)
                return true;

            // must be rounded down
            int times = (int)Times.Evaluate(args, rest.GetValueOrDefault, bullet.BulletManager);

            while (!(IsCompleted = timesRunCount >= times))
            {
                // write current loop values into vars
                rest["times"] = times;
                rest["i"] = timesRunCount;

                var isDone = Action.Run(bullet, args, rest);

                // if the action waits, we also stop immediately
                if (!isDone)
                    return false;

                timesRunCount++;
                Action.Reset();
            }

            return true;
        }

        /// <summary>
        /// Copies this task and resets it.
        /// </summary>
        /// <returns>A reset copy of this task.</returns>
        public ITask Copy()
        {
            return new Repeat((IAction)Action.Copy(), Times);
        }
    }
}
