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
        public string topBarColor = "0, 139, 199, 255";
        public string topBarTextColor = "126, 126, 126, 100";

        public string moduleColorSolid = "50, 59, 90, 255";
        public string displayModuleExtraLayerBackingColor = "0, 0, 0, 0";
        public string moduleColorSolidDefault = "50, 59, 90, 255";
        public string terminalTextColor = "213, 245, 255";
        public string moduleColorStrong = "14, 28, 40, 80";
        public string highlightColor = "0, 139, 199, 255";
        public string netmapToolTipColor = "213, 245, 255, 0";
        public string netmapToolTipBackground = "0, 0, 0, 150";
        public string moduleColorBacking = "5, 6, 7, 10";
        public string semiTransText = "120, 120, 120, 0";
        public string indentBackgroundColor = "12, 12, 12";
        public string outlineColor = "68, 68, 68";
        public string lockedColor = "65, 16, 16, 200";
        public string darkBackgroundColor = "8, 8, 8";
        public string subtleTextColor = "90, 90, 90";

        public static string Serialize(Theme theme)
        {
            return JsonConvert.SerializeObject(theme);
        }

        public static Theme Deserialize(string theme)
        {
            return JsonConvert.DeserializeObject<Theme>(theme);
        }

        public static Color StringToColour(string colour)
        {
            string[] stringRbga = colour.Split(',');
            int[] rbga = new int[4];
            for (int i = 0; i < stringRbga.Length; i++)
            {
                rbga[i] = Convert.ToInt32(stringRbga[i]);
            }
            if (rbga.Length == 4)
                return new Color(rbga[0], rbga[1], rbga[2], rbga[3]);
            else
                return new Color(rbga[0], rbga[1], rbga[2]);
        }
    }
}
