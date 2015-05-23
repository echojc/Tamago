using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Tamago
{
    public class BulletPattern
    {
        public const string TopLevelLabel = "top";
        private Dictionary<string, ActionDef> Actions;
        private Dictionary<string, FireDef> Fires;

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

                ParseActions(root);
                ParseFires(root);

                // find top
                if (!Actions.ContainsKey(TopLevelLabel))
                    throw new ParseException("Must have an action labelled '" + TopLevelLabel + "'.");

                TopLevelAction = Actions[TopLevelLabel];
            }
            catch (XmlException e)
            {
                throw new ParseException("Could not parse XML.", e);
            }
        }

        private void ParseActions(XElement root)
        {
            var actions = (from node in root.Elements("action")
                           select new ActionDef(node, this)).ToList();

            if (actions.Exists(a => a.Label == null))
                throw new ParseException("Top level actions must be labelled.");

            var actionLabels = actions.Select(a => a.Label).Distinct().ToList();
            if (actionLabels.Count != actions.Count)
                throw new ParseException("Actions cannot share the same label.");

            Actions = actions.ToDictionary(a => a.Label);
        }

        private void ParseFires(XElement root)
        {
            var fires = (from node in root.Elements("fire")
                         select new FireDef(node, this)).ToList();

            if (fires.Exists(f => f.Label == null))
                throw new ParseException("Top level fires must be labelled.");

            var fireLabels = fires.Select(f => f.Label).Distinct().ToList();
            if (fireLabels.Count != fires.Count)
                throw new ParseException("Fires cannot share the same label.");

            Fires = fires.ToDictionary(f => f.Label);
        }

        /// <summary>
        /// Looks for the action with the given label.
        /// </summary>
        /// <param name="label">The action to find.</param>
        /// <exception cref="KeyNotFoundException">If action does not exist.</exception>
        public ActionDef FindAction(string label)
        {
            return Actions[label];
        }

        /// <summary>
        /// Looks for the fire with the given label.
        /// </summary>
        /// <param name="label">The fire to find.</param>
        /// <exception cref="KeyNotFoundException">If fire does not exist.</exception>
        public FireDef FindFire(string label)
        {
            return Fires[label];
        }
    }
}
