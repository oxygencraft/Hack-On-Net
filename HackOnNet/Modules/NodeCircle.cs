using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Modules
{
    class NodeCircle
    {
        public string ip = "";

        Color color;
        public Vector2 position;

        public NodeCircle(string ip, Vector2 pos)
        {
            position = pos;
            this.ip = ip;
        }
    }
}
