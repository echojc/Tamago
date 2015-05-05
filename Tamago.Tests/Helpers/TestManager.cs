using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago.Tests.Helpers
{
    public class TestManager : IBulletManager
    {
        public float PlayerX { get; set; }
        public float PlayerY { get; set; }

        public List<Bullet> Bullets = new List<Bullet>();

        public void SetPlayerPosition(float x, float y)
        {
            PlayerX = x;
            PlayerY = y;
        }

        public void Update()
        {
            for (int i = 0; i < Bullets.Count; i++)
                Bullets[i].Update();
        }

        public Bullet CreateBullet()
        {
            var b = new Bullet(this);
            Bullets.Add(b);
            return b;
        }
    }
}
