using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Tamago
{
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
