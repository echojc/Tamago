﻿using System;
using System.Xml.Linq;

namespace Tamago
{
    /// <summary>
    /// Represents an &lt;accel&gt; node.
    /// </summary>
    public class Accel : Task
    {
        private bool isFirstRun = true;
        private float initialVelocityX;
        private float targetVelocityX;
        private float initialVelocityY;
        private float targetVelocityY;
        private int framesRunCount = 0;

        /// <summary>
        /// The horizontal velocity to change to.
        /// </summary>
        public Speed? VelocityX { get; private set; }

        /// <summary>
        /// The vertical velocity to change to.
        /// </summary>
        public Speed? VelocityY { get; private set; }

        /// <summary>
        /// The number of frames to animate the change over.
        /// </summary>
        public int Term { get; private set; }

        /// <summary>
        /// True if this has been run <code>Term</code> times or more.
        /// </summary>
        public bool IsCompleted
        {
            get { return framesRunCount >= Term; }
        }

        /// <summary>
        /// Parses an &lt;accel&gt; node into an object representation.
        /// </summary>
        /// <param name="node">The &lt;accel&gt; node.</param>
        public Accel(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var term = node.Element("term");
            if (term == null)
                throw new ParseException("<accel> node requires a <term> node.");
            Term = (int)float.Parse(term.Value);

            var horizontal = node.Element("horizontal");
            if (horizontal != null)
                VelocityX = new Speed(horizontal);

            var vertical = node.Element("vertical");
            if (vertical != null)
                VelocityY = new Speed(vertical);
        }

        /// <summary>
        /// Sets the X/Y velocities of the bullet for the next frame.
        /// </summary>
        /// <remarks>
        /// This updates <see cref="Bullet.NewVelocityX"/> and
        /// <see cref="Bullet.NewVelocityY"/> which copies over to
        /// <see cref="Bullet.VelocityX"/> and <see cref="Bullet.VelocityY"/>
        /// after all tasks are run. This is done to prevent multiple
        /// &lt;accel&gt;s from stacking their effects. Note that both
        /// &lt;horizontal&gt; and &lt;vertical&gt; default to null which do
        /// not apply any effect to the bullet.
        /// </remarks>
        /// <param name="bullet">The bullet to change the velocities of.</param>
        /// <returns>True always</returns>
        public bool Run(Bullet bullet)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            if (!isFirstRun && IsCompleted)
                return true;

            if (isFirstRun)
            {
                isFirstRun = false;
                initialVelocityX = bullet.VelocityX;
                initialVelocityY = bullet.VelocityY;

                if (VelocityX != null)
                {
                    Speed x = VelocityX.Value;
                    switch (x.Type)
                    {
                        case SpeedType.Relative:
                            targetVelocityX = bullet.VelocityX + x.Value;
                            break;
                        case SpeedType.Sequence:
                            targetVelocityX = bullet.VelocityX + (x.Value * Math.Max(0, Term));
                            break;
                        case SpeedType.Absolute:
                        default:
                            targetVelocityX = x.Value;
                            break;
                    }
                }

                if (VelocityY != null)
                {
                    Speed y = VelocityY.Value;
                    switch (y.Type)
                    {
                        case SpeedType.Relative:
                            targetVelocityY = bullet.VelocityY + y.Value;
                            break;
                        case SpeedType.Sequence:
                            targetVelocityY = bullet.VelocityY + (y.Value * Math.Max(0, Term));
                            break;
                        case SpeedType.Absolute:
                        default:
                            targetVelocityY = y.Value;
                            break;
                    }
                }
            }

            framesRunCount++;
            var ratio = Term <= 0 ? 1 : (float)framesRunCount / Term;

            if (VelocityX != null)
                bullet.NewVelocityX = initialVelocityX + (targetVelocityX - initialVelocityX) * ratio;

            if (VelocityY != null)
                bullet.NewVelocityY = initialVelocityY + (targetVelocityY - initialVelocityY) * ratio;

            return true;
        }
    }
}
