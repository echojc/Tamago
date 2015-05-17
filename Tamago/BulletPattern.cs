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

        public static BulletPattern ParseString(string xml)
        {
            if (xml == null) throw new ArgumentNullException("xml");

            try
            {
                XDocument doc = XDocument.Parse(xml);
                var root = doc.Root;
                if (root.Name.LocalName != "bulletml")
                    throw new ParseException("Root element must be <bulletml>.");

                // extract all labelled nodes
                var actions = (from node in root.XPathSelectElements("action")
                               select new ActionRef(node)).ToList();

                if (actions.Exists(a => a.Label == null))
                    throw new ParseException("Top level actions must be labelled.");

                return new BulletPattern(actions);
            }
            catch (XmlException e)
            {
                throw new ParseException("Could not parse XML.", e);
            }
        }
    }
}
