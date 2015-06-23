using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago.Tests.Helpers
{
    public class TestManager : BulletManager
    {
        private float _x, _y, _rand, _rank;

        public const float TestRand = 0.1337f;
        public const float TestRank = 0.3246f;

        public TestManager()
            : base()
        {
            _rand = TestRand;
            _rank = TestRank;
        }

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
            get { return _rand; }
        }

        public override float Rank
        {
            get { return _rank; }
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

        public void SetRand(float rand)
        {
            _rand = rand;
        }

        public void SetRank(float rank)
        {
            _rank = rank;
        }

        public override void Update()
        {
            // spawned bullets run on the next frame
            // keep vanished bullets so we can check their final state
            // TODO: do this properly?
            for (int i = Bullets.Count - 1; i >= 0; i--)
            {
                var b = Bullets[i];
                if (!b.IsVanished)
                    b.Update();
            }
        }
    }
}
