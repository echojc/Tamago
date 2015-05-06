using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class DirectionTest
    {
        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Direction(null));
        }

        [Test]
        public void DefaultsTypeToAim()
        {
            var node = XElement.Parse(@"
              <direction>2</direction> 
            ");

            var direction = new Direction(node);
            Assert.AreEqual(DirectionType.Aim, direction.Type);
        }

        [Test]
        public void ParsesMagnitudeAsDegrees()
        {
            var node = XElement.Parse(@"
              <direction>30</direction> 
            ");

            var direction = new Direction(node);
            Assert.AreEqual((float)(Math.PI / 6), direction.Value, 0.00001f);
        }

        [Test]
        public void ParsesTypeAim()
        {
            var node = XElement.Parse(@"
              <direction type=""aim"">2.4</direction> 
            ");

            var direction = new Direction(node);
            Assert.AreEqual(DirectionType.Aim, direction.Type);
        }

        [Test]
        public void ParsesTypeAbsolute()
        {
            var node = XElement.Parse(@"
              <direction type=""absolute"">2.4</direction> 
            ");

            var direction = new Direction(node);
            Assert.AreEqual(DirectionType.Absolute, direction.Type);
        }

        [Test]
        public void ParsesTypeRelative()
        {
            var node = XElement.Parse(@"
              <direction type=""relative"">2.4</direction> 
            ");

            var direction = new Direction(node);
            Assert.AreEqual(DirectionType.Relative, direction.Type);
        }

        [Test]
        public void ParsesTypeSequence()
        {
            var node = XElement.Parse(@"
              <direction type=""sequence"">2.4</direction> 
            ");

            var direction = new Direction(node);
            Assert.AreEqual(DirectionType.Sequence, direction.Type);
        }

        [Test]
        public void Boilerplate()
        {
            var a = new Direction(DirectionType.Absolute, 1.23f);
            var b = new Direction(DirectionType.Relative, 1.23f);
            var c = new Direction(DirectionType.Relative, 2.34f);
            var d = new Direction(DirectionType.Relative, 2.34f);

            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(b, c);
            Assert.AreNotEqual(a, d);
            Assert.AreEqual(c, d);

            Assert.True(a != b);
            Assert.True(b != c);
            Assert.True(a != d);
            Assert.True(c == d);

            var set = new HashSet<Direction>();
            set.Add(a);
            set.Add(c);

            Assert.AreEqual(2, set.Count);
            Assert.True(set.Contains(a));
            Assert.False(set.Contains(b));
            Assert.True(set.Contains(c));
            Assert.True(set.Contains(d));

            set.Add(d);
            Assert.AreEqual(2, set.Count);
        }
    }
}
