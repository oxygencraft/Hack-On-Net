using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HackOnNet.GUI
{
    static class TerminalStringParser
    {
        public static void DrawFormatString(this SpriteBatch spriteBatch, string text, Vector2 pos)
        {
            string[] stringPieces = text.Split(new string[] { "{{" }, StringSplitOptions.None);
            Dictionary<string, Color> colors = new Dictionary<string, Color>()
            {
                { "blue", new Color(65, 135, 210)},
                { "white", new Color(213, 245, 255)},
                { "red", Color.Red},
                { "yellow", Color.Yellow},
                { "green", Color.Green},
            };

            Vector2 startPosition = pos;
            Vector2 offset = Vector2.Zero;
            SpriteFont font = GuiData.tinyfont;
             
            for (int x = 0; x < stringPieces.Length; x++)
            {
                var piece = stringPieces[x].Split(new string[] { "}}" }, StringSplitOptions.None);
                string printText = piece[0];
                Color color = colors["white"];
                if(piece.Length == 2)
                {
                    printText = piece[1];
                    bool parsedCustom = false;
                    if(piece[0].StartsWith("$"))
                    {
                        if(piece.Length == 7)
                        {
                            uint cR, cG, cB;
                            bool f = uint.TryParse(piece[0].Substring(1, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out cR);
                            if(f)
                            {
                                f = uint.TryParse(piece[0].Substring(3, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out cG);
                                if(f)
                                {
                                    f = uint.TryParse(piece[0].Substring(5, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out cB);
                                    if(f)
                                    {
                                        parsedCustom = true;
                                        color = new Color(cR, cG, cB);
                                    }
                                }
                            }
                        }
                    }
                    if(!parsedCustom)
                    {
                        if (colors.ContainsKey(piece[0]))
                            color = colors[piece[0]];
                    }
                    
                }
                spriteBatch.DrawString(font, printText, startPosition + offset, color);
                offset.X += font.MeasureString(printText).X;
            }
        }

        public static string RemoveFormatting(this string text)
        {
            return Regex.Replace(text, @"\{\{[^}]+\}\}", "");
        }
    }
}
