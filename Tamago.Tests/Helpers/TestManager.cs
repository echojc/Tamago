using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago.Tests.Helpers
{
    public class TestManager : BulletManager
    {
        private float _x, _y;

        public const float TestRand = 0.1337f;
        public const float TestRank = 0.3246f;

        public override float PlayerX
        {
            get { return _x; }
        }

        public override float PlayerY
        {
            get { return _y; }
        }

        public override float Rand
        {
            get { return TestRand; }
        }

        public override float Rank
        {
            get { return TestRank; }
        }

        public new List<Bullet> Bullets
        {
            get { return base.Bullets; }
        }

        public void SetPlayerPosition(float x, float y)
        {
            _x = x;
            _y = y;
        }
    }
}
