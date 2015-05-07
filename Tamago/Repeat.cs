using System;
using System.Xml.Linq;

namespace Tamago
{
    public class Repeat : Task
    {
        public ActionRef Action { get; private set; }

        public int Times { get; private set; }

        public bool IsCompleted { get; private set; }

        public Repeat(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var times = node.Element("times");
            if (times == null)
                throw new ParseException("<repeat> node requires a <times> node.");
            Times = (int)float.Parse(times.Value);

            var action = node.Element("action");
            if (action == null)
                throw new ParseException("<repeat> node requires an <action> node.");
            Action = new ActionRef(action);

            Reset();
        }

        public void Reset()
        {
            IsCompleted = false;
        }

        public bool Run(Bullet bullet)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            if (IsCompleted)
                return true;

            for (int i = 0; i < Times; i++)
            {
                Action.Run(bullet);
                Action.Reset();
            }

            IsCompleted = true;
            return true;
        }
    }
}
