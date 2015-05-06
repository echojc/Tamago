using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Tamago.Tests
{
    [TestFixture]
    public class SpeedTest
    {
        [Test]
        public void ThrowsArgumentNullIfNodeToConstructFromIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Speed(null));
        }

        [Test]
        public void DefaultsTypeToAbsolute()
        {
            var node = XElement.Parse(@"
              <speed>2</speed> 
            ");

            var speed = new Speed(node);
            Assert.AreEqual(SpeedType.Absolute, speed.Type);
        }

        [Test]
        public void ParsesMagnitude()
        {
            var node = XElement.Parse(@"
              <speed>2.4</speed> 
            ");

            var speed = new Speed(node);
            Assert.AreEqual(2.4f, speed.Value);
        }

        [Test]
        public void ParsesTypeAbsolute()
        {
            var node = XElement.Parse(@"
              <speed type=""absolute"">2.4</speed> 
            ");

            var speed = new Speed(node);
            Assert.AreEqual(SpeedType.Absolute, speed.Type);
        }

        [Test]
        public void ParsesTypeRelative()
        {
            var node = XElement.Parse(@"
              <speed type=""relative"">2.4</speed> 
            ");

            var speed = new Speed(node);
            Assert.AreEqual(SpeedType.Relative, speed.Type);
        }

        [Test]
        public void ParsesTypeSequence()
        {
            var node = XElement.Parse(@"
              <speed type=""sequence"">2.4</speed> 
            ");

            var speed = new Speed(node);
            Assert.AreEqual(SpeedType.Sequence, speed.Type);
        }

        [Test]
        public void Boilerplate()
        {
            var a = new Speed(SpeedType.Absolute, 1.23f);
            var b = new Speed(SpeedType.Relative, 1.23f);
            var c = new Speed(SpeedType.Relative, 2.34f);
            var d = new Speed(SpeedType.Relative, 2.34f);

            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(b, c);
            Assert.AreNotEqual(a, d);
            Assert.AreEqual(c, d);

            Assert.True(a != b);
            Assert.True(b != c);
            Assert.True(a != d);
            Assert.True(c == d);

            var set = new HashSet<Speed>();
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
