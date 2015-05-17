using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents a &lt;repeat&gt; node.
    /// </summary>
    public class Repeat : Task
    {
        private int timesRunCount = 0;

        /// <summary>
        /// The number of times the nested action has been completely run through.
        /// </summary>
        public Expression Times { get; private set; }

        /// <summary>
        /// The action to repeat.
        /// </summary>
        public ActionDef Action { get; private set; }

        /// <summary>
        /// True if the nested action has been completely run through <see cref="Times">Times</see> times.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// For cloning.
        /// </summary>
        private Repeat(ActionDef actionRef, Expression times)
        {
            Action = actionRef;
            Times = times;
            Reset();
        }

        /// <summary>
        /// Parses a &lt;repeat&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;repeat&gt; node.</param>
        public Repeat(XElement node)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (node.Name.LocalName != "repeat") throw new ArgumentException("node");

            var times = node.Element("times");
            if (times == null)
                throw new ParseException("<repeat> node requires a <times> node.");
            Times = new Expression(times.Value);

            var action = node.Element("action");
            if (action == null)
                throw new ParseException("<repeat> node requires an <action> node.");
            Action = new ActionDef(action);

            Reset();
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
        /// <returns>True if no waiting is required, otherwise the result of the nested action.</returns>
        public bool Run(Bullet bullet)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            if (IsCompleted)
                return true;

            // must be rounded down
            int times = (int)Times.Evaluate();

            while (!(IsCompleted = timesRunCount >= times))
            {
                var isDone = Action.Run(bullet);

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
        public Task Copy()
        {
            return new Repeat((ActionDef)Action.Copy(), Times);
        }
    }
}
