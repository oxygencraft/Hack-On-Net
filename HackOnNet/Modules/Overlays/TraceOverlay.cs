using HackOnNet.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Modules.Overlays
{
    class TraceOverlay : Overlay
    {
        float progress = 100;
        float speed = 0;
        float timeAtMax = 0;
        float beepTimer = 0;

        public Color timerColor = new Color(170, 0, 0);

        private static SpriteFont font;

        public TraceOverlay(SpriteBatch batch, Rectangle rect, UserScreen screen, float progress, float speed) : base(batch, rect, screen)
        {
            this.progress = progress;
            this.speed = speed;
            
            if(TraceOverlay.font == null)
            {
                TraceOverlay.font = screen.content.Load<SpriteFont>("Kremlin");
                TraceOverlay.font.Spacing = 11f;
            }
        }

        public void Force(float newProg, float newSpd)
        {
            progress = newProg;
            speed = newSpd;
        }

        public override void Launch()
        {
            screen.Flash(true);
        }

        public override void Update(double dT)
        {
            this.progress -= speed * (float)dT;
            this.beepTimer -= (float)dT;
            if (progress >= 99.95)
                timeAtMax += (float)dT;
            else
                timeAtMax = 0;
            if (timeAtMax > 3)
                Destroy();
            if (progress < 0)
                progress = 0;
            if (progress > 100)
                progress = 100;
            if((int)progress % 10 == 1 && beepTimer < 0)
            {
                this.beepTimer = 2f;
                screen.Flash(true);
            }
        }

        public override void Draw()
        {
            string text = (progress).ToString("00.00");
            Vector2 vector = TraceOverlay.font.MeasureString(text);
            Vector2 position = new Vector2(10f, (float)spriteBatch.GraphicsDevice.Viewport.Height - vector.Y);
            spriteBatch.DrawString(TraceOverlay.font, text, position, this.timerColor);
            position.Y -= 25f;
            spriteBatch.DrawString(TraceOverlay.font, "TRACE :", position, this.timerColor, 0f, Vector2.Zero, new Vector2(0.3f), SpriteEffects.None, 0.5f);
        }

        public override void Destroy()
        {
            base.Destroy();
            this.screen.traceOverlay = null;
        }
    }
}
