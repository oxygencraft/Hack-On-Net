using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hacknet;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace HackOnNet.Music
{
    class MusicControl
    {
        public static void initialize() {
            MusicManager.playSong();
        }
    }
}
