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
    class OnCoreModule : OnModule
    {
        private static Texture2D LockSprite;

        public bool inputLocked = false;

        private bool guiInputLockStatus = false;

        public OnCoreModule(Rectangle location, UserScreen screen) : base(location, screen)
		{
        }

        public override void LoadContent()
        {
            base.LoadContent();
            if (OnCoreModule.LockSprite == null)
            {
                OnCoreModule.LockSprite = this.userScreen.content.Load<Texture2D>("Lock");
            }
        }

        public override void PreDrawStep()
        {
            base.PreDrawStep();
            if (this.inputLocked)
            {
                this.guiInputLockStatus = GuiData.blockingInput;
                GuiData.blockingInput = true;
            }
        }

        public override void PostDrawStep()
        {
            base.PostDrawStep();
            if (this.inputLocked)
            {
                GuiData.blockingInput = false;
                GuiData.blockingInput = this.guiInputLockStatus;
                Rectangle bounds = this.bounds;
                if (bounds.Contains(GuiData.getMousePoint()))
                {
                    GuiData.spriteBatch.Draw(Utils.white, bounds, Color.Gray * 0.5f);
                    Vector2 position = new Vector2((float)(bounds.X + bounds.Width / 2 - OnCoreModule.LockSprite.Width / 2), (float)(bounds.Y + bounds.Height / 2 - OnCoreModule.LockSprite.Height / 2));
                    GuiData.spriteBatch.Draw(OnCoreModule.LockSprite, position, Color.White);
                }
            }
        }
    }
}
