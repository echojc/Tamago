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
        private Dictionary<string, ActionDef> Actions;

        public ActionDef TopLevelAction { get; private set; }

        public BulletPattern(string xml)
        {
            if (xml == null) throw new ArgumentNullException("xml");

            try
            {
                XDocument doc = XDocument.Parse(xml);
                var root = doc.Root;
                if (root.Name.LocalName != "bulletml")
                    throw new ParseException("Root element must be <bulletml>.");

                // extract all labelled actions
                var actions = (from node in root.XPathSelectElements("action")
                               select new ActionDef(node)).ToList();

                if (actions.Exists(a => a.Label == null))
                    throw new ParseException("Top level actions must be labelled.");

                var actionLabels = actions.Select(a => a.Label).Distinct().ToList();
                if (actionLabels.Count != actions.Count)
                    throw new ParseException("Actions cannot share the same label.");

                Actions = actions.ToDictionary(a => a.Label);
                if (!Actions.ContainsKey(TopLevelLabel))
                    throw new ParseException("Must have an action labelled '" + TopLevelLabel + "'.");

                TopLevelAction = Actions[TopLevelLabel];
            }
            catch (XmlException e)
            {
                throw new ParseException("Could not parse XML.", e);
            }
        }

        public ActionDef FindAction(string label)
        {
            return Actions.ContainsKey(label) ? Actions[label] : null;
        }
    }
}
