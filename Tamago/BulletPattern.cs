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
}
