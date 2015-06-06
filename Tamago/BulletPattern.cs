using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents an entire BulletML document.
    /// </summary>
    public class BulletPattern
    {
        /// <summary>
        /// All top-level labelled &lt;action&gt; nodes in this pattern.
        /// </summary>
        internal Dictionary<string, ActionDef> Actions;

        /// <summary>
        /// All top-level labelled &lt;fire&gt; nodes in this pattern.
        /// </summary>
        internal Dictionary<string, FireDef> Fires;

        /// <summary>
        /// All top-level labelled &lt;bullet&gt; nodes in this pattern.
        /// </summary>
        internal Dictionary<string, BulletDef> Bullets;

        /// <summary>
        /// The latest version of Tamago's BulletML supported by this
        /// implementation.
        /// </summary>
        public const int LatestVersion = 1;

        /// <summary>
        /// The version of Tamago's BulletML to use when running this
        /// pattern. Used for backwards compatibility when changes would
        /// break existing patterns.
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// Parses a BulletML string to create a usable internal representation of it.
        /// </summary>
        /// <param name="xml">The BulletML string.</param>
        public BulletPattern(string xml)
        {
            if (xml == null) throw new ArgumentNullException("xml");

            try
            {
                XDocument doc = XDocument.Parse(xml);
                var root = doc.Root;
                if (root.Name.LocalName != "bulletml")
                    throw new ParseException("Root element must be <bulletml>.");

                Actions = ParseLabelledNodes(root, "action", n => new ActionDef(n, this));
                Fires = ParseLabelledNodes(root, "fire", n => new FireDef(n, this));
                Bullets = ParseLabelledNodes(root, "bullet", n => new BulletDef(n, this));

                var version = root.Attribute("version");
                int intValue;
                if (version != null && int.TryParse(version.Value, out intValue))
                {
                    Version = intValue;
                }
                else
                {
                    Version = LatestVersion;
                }
            }
            catch (XmlException e)
            {
                throw new ParseException("Could not parse XML.", e);
            }
        }

        /// <summary>
        /// Extracts labelled nodes. Only searches direct children of the given node.
        /// </summary>
        /// <param name="root">The node to search.</param>
        /// <param name="node">The name of the nodes to parse.</param>
        /// <param name="ctor">How to construct the internal representation given the XML node.</param>
        /// <returns>A dictionary mapping labels to T.</returns>
        private static Dictionary<string, T> ParseLabelledNodes<T>(XElement root, string node, Func<XElement, T> ctor) where T : ILabelled
        {
            try
            {
                return root.Elements(node)
                    .Select(n => ctor(n))
                    .ToDictionary(n => n.Label);
            }
            catch (ArgumentNullException e)
            {
                throw new ParseException("Top level <" + node + "> must be labelled.", e);
            }
            catch (ArgumentException e)
            {
                throw new ParseException("Top level <" + node + "> cannot share the same label.", e);
            }
        }

        public ActionDef CopyAction(string label)
        {
            return (ActionDef)Actions[label].Copy();
        }

        public FireDef CopyFire(string label)
        {
            return (FireDef)Fires[label].Copy();
        }

        public BulletDef CopyBullet(string label)
        {
            return Bullets[label];
        }
    }
}
