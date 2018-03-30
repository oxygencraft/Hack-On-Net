using HackOnNet.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Modules.Overlays
{
    class TerminationOverlay : Overlay
    {
        Rectangle fullscreen;

        Dictionary<float, string> farewellMessages = new Dictionary<float, string>() {
            { 0.35f,    "Farewell, agent." },
            { 0.5f,     "Your efforts were appreciated." },
            { 0.55f,    "Disappointing."},
            { 0.6f,     "We are closing our doors to you." },
            { 0.85f,    "A cleaner has been sent your direction."},
            { 0.99f,    "What a mess you made. Farewell."},
            { 1f,       "You betrayed our trust. You will suffer the consequences."}
        };

        private SoundEffect spinDownSound;

        string chosenStr = "Farewell, agent.";


        public TerminationOverlay(SpriteBatch batch, Rectangle displayRect, UserScreen screen) : base(batch, displayRect, screen)
        {
            fullscreen = this.displayRect;

            Random rnd = new Random();
            float choice = (float)rnd.NextDouble();
            this.spinDownSound = screen.content.Load<SoundEffect>("Music/Ambient/spiral_gauge_down");

            foreach (KeyValuePair<float, string> message in farewellMessages)
            {
                if (message.Key > choice)
                {
                    chosenStr = message.Value;
                    break;
                }
            }
        }

        private float time = 0f;

        public override void Launch()
        {
            Hacknet.MusicManager.playSongImmediatley("Music/Ambient/dark_drone_008");
            this.spinDownSound.Play();
        }

        public override bool PreventsDrawing()
        {
            return true;
        }

        public override void Update(double dT)
        {
            time += (float)dT;
        }

        public override void Draw()
        {
            Hacknet.Gui.TextItem.DrawShadow = false;

            this.spriteBatch.Draw(Hacknet.Utils.white, this.fullscreen, Color.Black);
            Rectangle destinationRectangle = default(Rectangle);
            destinationRectangle.X = this.fullscreen.X + 2;
            destinationRectangle.Width = this.fullscreen.Width - 4;
            destinationRectangle.Y = this.fullscreen.Y + this.fullscreen.Height / 6 * 2;
            destinationRectangle.Height = this.fullscreen.Height / 3;
            this.spriteBatch.Draw(Hacknet.Utils.white, destinationRectangle, this.screen.indentBackgroundColor);
            Vector2 vector = Hacknet.GuiData.titlefont.MeasureString("TERMINATED");
            Vector2 position = new Vector2((float)(destinationRectangle.X + this.fullscreen.Width / 2) - vector.X / 2f, (float)(this.fullscreen.Y + this.fullscreen.Height / 2 - 50));
            this.spriteBatch.DrawString(Hacknet.GuiData.titlefont, "TERMINATED", position, this.screen.subtleTextColor);
            Vector2 pos = new Vector2(200f, (float)(destinationRectangle.Y + destinationRectangle.Height + 20));
            pos = this.DrawFlashInString(Hacknet.LocaleTerms.Loc("Your terminal has been terminated"), pos, 4f, 0.2f, false, 0.2f);
            pos = this.DrawFlashInString(Hacknet.LocaleTerms.Loc("All of your assets have been seized"), pos, 6f, 0.2f, false, 0.2f);
            pos = this.DrawFlashInString(Hacknet.LocaleTerms.Loc(chosenStr), pos, 10f, 0.2f, false, 0.8f);
            pos = this.DrawFlashInString(Hacknet.LocaleTerms.Loc("Remote connection shutting down"), pos, 12f, 0.2f, true, 0.8f);
        }

        private Vector2 DrawFlashInString(string text, Vector2 pos, float offset, float transitionInTime = 0.2f, bool hasDots = false, float dotsDelayer = 0.2f)
        {
            Vector2 value = new Vector2(40f, 0f);
            if (this.time >= offset)
            {
                float num = System.Math.Min((this.time - offset) / transitionInTime, 1f);
                Vector2 position = pos + value * (1f - Hacknet.Utils.QuadraticOutCurve(num));
                string text2 = "";
                if (hasDots)
                {
                    float num2 = this.time - offset;
                    while (num2 > 0f && text2.Length < 5)
                    {
                        num2 -= dotsDelayer;
                        text2 += ".";
                    }
                }
                float scale = 0.5f;
                float num3 = 17f;
                if (Hacknet.Localization.LocaleActivator.ActiveLocaleIsCJK())
                {
                    scale = 0.7f;
                    num3 = 22f;
                }
                this.spriteBatch.DrawString(Hacknet.GuiData.font, text + text2, position, Color.White * num, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.4f);
                pos.Y += num3;
            }
            return pos;
        }
    }
}
