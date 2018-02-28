using Hacknet;
using HackOnNet.Screens;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Modules
{
    class OnNetRamModule : OnCoreModule
    {
        public static int contentStartOffset = 16;

        public static int MODULE_WIDTH = 252;

        public static Color USED_RAM_COLOR = new Color(60, 60, 67);

        public static float FLASH_TIME = 3f;

        private string infoString = "";

        private Vector2 infoStringPos;

        private Rectangle infoBar;

        private Rectangle infoBarUsedRam;

        private float OutOfMemoryFlashTime = 0f;

        public OnNetRamModule(Rectangle location, UserScreen screen) : base(location, screen)
		{
        }

        public override void LoadContent()
        {
            base.LoadContent();
            this.infoBar = new Rectangle(this.bounds.X + 1, this.bounds.Y + 1, this.bounds.Width - 2, RamModule.contentStartOffset);
            this.infoBarUsedRam = new Rectangle(this.bounds.X + 1, this.bounds.Y + 1, this.bounds.Width - 2, RamModule.contentStartOffset);
            this.infoStringPos = new Vector2((float)this.infoBar.X, (float)this.infoBar.Y);
        }

        public override void Update(float t)
        {
            base.Update(t);
            this.infoBar = new Rectangle(this.bounds.X + 1, this.bounds.Y + 1, this.bounds.Width - 2, RamModule.contentStartOffset);
            this.infoString = string.Concat(new object[]
            {
                "USED RAM: ",
                "0",
                "mb / ",
                "infinite",
                "mb"
            });
            this.infoBarUsedRam = new Rectangle(this.bounds.X + 1, this.bounds.Y + 1, this.bounds.Width - 2, RamModule.contentStartOffset);
            if (this.OutOfMemoryFlashTime > 0f)
            {
                this.OutOfMemoryFlashTime -= t;
            }
        }

        public void FlashMemoryWarning()
        {

        }

        public override void Draw(float t)
        {
            base.Draw(t);
            this.spriteBatch.Draw(Utils.white, this.infoBar, this.userScreen.indentBackgroundColor);
            this.infoBarUsedRam.Width = (int)((float)this.infoBar.Width * (1f - 800 - (OS.TOP_BAR_HEIGHT + 2) / (float)800 - (OS.TOP_BAR_HEIGHT + 2)));
            this.spriteBatch.Draw(Utils.white, this.infoBarUsedRam, RamModule.USED_RAM_COLOR);
            this.spriteBatch.DrawString(GuiData.detailfont, "hello", new Vector2((float)this.infoBar.X, (float)this.infoBar.Y), Color.White);
            if (this.OutOfMemoryFlashTime > 0f)
            {
                float scale = System.Math.Min(1f, this.OutOfMemoryFlashTime);
                float amount = System.Math.Max(0f, this.OutOfMemoryFlashTime - (RamModule.FLASH_TIME - 1f));
                Color patternColor = Color.Lerp(this.userScreen.lockedColor, Utils.AddativeRed, amount) * scale;
                PatternDrawer.draw(this.bounds, 0f, Color.Transparent, patternColor, this.spriteBatch, PatternDrawer.errorTile);
                int num = 40;
                Rectangle rectangle = new Rectangle(this.bounds.X, this.bounds.Y + this.bounds.Height - num - 1, this.bounds.Width, num);
                this.spriteBatch.Draw(Utils.white, Utils.InsetRectangle(rectangle, 4), Color.Black * 0.75f);
                rectangle.X--;
                string text = " ^ " + LocaleTerms.Loc("INSUFFICIENT MEMORY") + " ^ ";
                Hacknet.Gui.TextItem.doFontLabelToSize(rectangle, text, GuiData.font, Color.Black * scale, false, false);
                rectangle.X += 2;
                Hacknet.Gui.TextItem.doFontLabelToSize(rectangle, text, GuiData.font, Color.Black * scale, false, false);
                rectangle.X--;
                Hacknet.Gui.TextItem.doFontLabelToSize(rectangle, text, GuiData.font, Color.White * scale, false, false);
            }
        }

        public virtual void drawOutline()
        {
            Rectangle bounds = this.bounds;
            this.spriteBatch.Draw(Utils.white, bounds, this.userScreen.outlineColor);
            bounds.X++;
            bounds.Y++;
            bounds.Width -= 2;
            bounds.Height -= 2;
            this.spriteBatch.Draw(Utils.white, bounds, this.userScreen.darkBackgroundColor);
        }
    }
}
