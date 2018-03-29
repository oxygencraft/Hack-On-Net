using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Graphics
{
    static class AssetBank
    {
        static SoundEffect spinDownSound;


        public static void LoadBank(ContentManager content)
        {
            spinDownSound = content.Load<SoundEffect>("Music/Ambient/spiral_gauge_down");
        }
    }
}
