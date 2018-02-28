using Hacknet;
using HackOnNet.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Modules
{
    class OnModule
    {
        public static int PANEL_HEIGHT = 15;

        public Rectangle bounds;

        public SpriteBatch spriteBatch;

        public string name = "Unknown";

        public UserScreen userScreen;

        public bool visible = true;

        private static Rectangle tmpRect;

        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
            set
            {
                this.bounds = value;
                this.bounds.Y = this.bounds.Y + Module.PANEL_HEIGHT;
                this.bounds.Height = this.bounds.Height - Module.PANEL_HEIGHT;
            }
        }

        public OnModule(Rectangle location, UserScreen screen)
        {
            location.Y += OnModule.PANEL_HEIGHT;
            location.Height -= OnModule.PANEL_HEIGHT;
            this.bounds = location;
            this.userScreen = screen;
            this.spriteBatch = this.userScreen.ScreenManager.SpriteBatch;
        }

        public virtual void LoadContent()
        {
        }

        public virtual void Update(float t)
        {
        }

        public virtual void PreDrawStep()
        {
        }

        public virtual void Draw(float t)
        {
            this.drawFrame();
        }

        public virtual void PostDrawStep()
        {
        }

        public void drawFrame()
        {
            OnModule.tmpRect = this.bounds;
            OnModule.tmpRect.Y = OnModule.tmpRect.Y - OnModule.PANEL_HEIGHT;
            OnModule.tmpRect.Height = OnModule.tmpRect.Height + OnModule.PANEL_HEIGHT;
            this.spriteBatch.Draw(Utils.white, OnModule.tmpRect, this.userScreen.moduleColorBacking);
            Hacknet.Gui.RenderedRectangle.doRectangleOutline(OnModule.tmpRect.X, OnModule.tmpRect.Y, OnModule.tmpRect.Width, OnModule.tmpRect.Height, 1, new Color?(this.userScreen.moduleColorSolid));
            OnModule.tmpRect.Height = OnModule.PANEL_HEIGHT;
            this.spriteBatch.Draw(Utils.white, OnModule.tmpRect, this.userScreen.moduleColorStrong);
            this.spriteBatch.DrawString(GuiData.detailfont, this.name, new Vector2((float)(OnModule.tmpRect.X + 2), (float)(OnModule.tmpRect.Y + 2)), this.userScreen.semiTransText);
            OnModule.tmpRect = this.bounds;
            OnModule.tmpRect.Y = OnModule.tmpRect.Y - OnModule.PANEL_HEIGHT;
            OnModule.tmpRect.Height = OnModule.tmpRect.Height + OnModule.PANEL_HEIGHT;
            Hacknet.Gui.RenderedRectangle.doRectangleOutline(OnModule.tmpRect.X, OnModule.tmpRect.Y, OnModule.tmpRect.Width, OnModule.tmpRect.Height, 1, new Color?(this.userScreen.moduleColorSolid));
        }
}
}
