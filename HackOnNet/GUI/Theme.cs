using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.GUI
{
    class Theme
    {
        public Color topBarColor = new Color(0, 139, 199, 255);

        public Color topBarTextColor = new Color(126, 126, 126, 100);

        public Color moduleColorSolid = new Color(50, 59, 90, 255);
        public Color displayModuleExtraLayerBackingColor = new Color(0, 0, 0, 0);
        public Color moduleColorSolidDefault = new Color(50, 59, 90, 255);
        public Color terminalTextColor = new Color(213, 245, 255);
        public Color moduleColorStrong = new Color(14, 28, 40, 80);
        public Color highlightColor = new Color(0, 139, 199, 255);
        public Color netmapToolTipColor = new Color(213, 245, 255, 0);
        public Color netmapToolTipBackground = new Color(0, 0, 0, 150);
        public Color moduleColorBacking = new Color(5, 6, 7, 10);
        public Color semiTransText = new Color(120, 120, 120, 0);
        public Color indentBackgroundColor = new Color(12, 12, 12);
        public Color outlineColor = new Color(68, 68, 68);
        public Color lockedColor = new Color(65, 16, 16, 200);
        public Color darkBackgroundColor = new Color(8, 8, 8);
        public Color subtleTextColor = new Color(90, 90, 90);

        public static string Serialize(Theme theme)
        {
            return JsonConvert.SerializeObject(theme);
        }

        public static Theme Deserialize(string theme)
        {
            return JsonConvert.DeserializeObject<Theme>(theme);
        }
    }
}
