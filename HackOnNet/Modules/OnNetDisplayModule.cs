using Hacknet;
using HackOnNet.Screens;
using HackOnNet.Sessions.States;
using HackOnNet.Sessions.States.Irc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Modules
{
    class OnNetDisplayModule : OnCoreModule
    {
        int x;
        int y;

        private Hacknet.UIUtils.ScrollableTextRegion viewTextRegion;


        public OnNetDisplayModule(Rectangle location, UserScreen screen) : base(location, screen)
        {
            this.viewTextRegion = new Hacknet.UIUtils.ScrollableTextRegion(GuiData.spriteBatch.GraphicsDevice);
        }

        

        public enum DisplayState
        {
            NONE,
            SSH_SESSION,
            LS,
            VIEW,
            IRC,
            WEB
        }

        public DisplayState state = DisplayState.NONE;

        public override void Draw(float t)
        {
            base.Draw(t);
            try
            {
                this.doCommandModule();
            }
            catch (System.Exception ex)
            {
                string text = Utils.GenerateReportFromException(ex);
                System.Console.WriteLine(text);
                Utils.AppendToErrorFile(text);
            }
        }

        private void doCommandModule()
        {
            this.x = this.bounds.X + 5;
            this.y = this.bounds.Y + 5;

            if (state == DisplayState.NONE)
                doEmptyModule();
            else if (state == DisplayState.SSH_SESSION)
                doSSHModule();
            else if (state == DisplayState.LS)
                doLSModule();
            else if (state == DisplayState.IRC)
                doIRCModule();
            else if (state == DisplayState.VIEW)
                doViewModule();
            else if (state == DisplayState.WEB)
                doWebModule();
        }

        private void doEmptyModule()
        {
            this.spriteBatch.Draw(Utils.white, this.bounds, this.userScreen.displayModuleExtraLayerBackingColor);
            var tmpRect = new Rectangle();
            tmpRect.X = this.bounds.X + 2;
            tmpRect.Width = this.bounds.Width - 4;
            tmpRect.Y = this.bounds.Y + this.bounds.Height / 6 * 2;
            tmpRect.Height = this.bounds.Height / 3;
            this.spriteBatch.Draw(Utils.white, tmpRect, userScreen.indentBackgroundColor);
            Vector2 vector = GuiData.font.MeasureString(LocaleTerms.Loc("No Active Session"));
            Vector2 position = new Vector2((float)(tmpRect.X + this.bounds.Width / 2) - vector.X / 2f, (float)(this.bounds.Y + this.bounds.Height / 2 - 10));
            this.spriteBatch.DrawString(GuiData.font, LocaleTerms.Loc("No Active Session"), position, userScreen.subtleTextColor);
        }

        private void doSSHModule()
        {
            doConnectDisplay();
        }

        private void doConnectHeader()
        {
            this.x += 20;
            this.y += 5;
            this.spriteBatch.DrawString(GuiData.font, LocaleTerms.Loc("Connected to") + " ", new Vector2((float)(this.x + 160), (float)this.y), Color.White);
            this.y += 40;
            string text = userScreen.activeSession.serverName;
            Hacknet.Gui.TextItem.doFontLabel(new Vector2((float)(this.x + 160), (float)this.y), text, GuiData.font, new Color?(Color.White), (float)this.bounds.Width - 190f, 60f, false);
            this.y += 33;
            float num = Hacknet.Localization.LocaleActivator.ActiveLocaleIsCJK() ? 4f : 0f;
            this.spriteBatch.DrawString(GuiData.smallfont, "@  " + userScreen.activeSession.ip, new Vector2((float)(this.x + 160), (float)this.y + num), Color.White);
            this.y += 60;
            if (userScreen.activeSession.privilege != 3)
            {
                this.y -= 20;
                Rectangle _empty = Rectangle.Empty;
                _empty.X = this.bounds.X + 1;
                _empty.Y = this.y;
                _empty.Width = this.bounds.Width - 2;
                _empty.Height = 20;
                int priv = userScreen.activeSession.privilege;
                Color accountColor = userScreen.guestAccountColor;
                if (priv == 2)
                    accountColor = userScreen.userAccountColor;
                else if (priv == 1)
                    accountColor = userScreen.adminAccountColor;
                else if (priv == 0)
                    accountColor = userScreen.kernelAccountColor;
                this.spriteBatch.Draw(Utils.white, _empty, accountColor);

                var _text = LocaleTerms.Loc("You are "+userScreen.activeSession.GetRankName()+" on this System");
                Vector2 _vector = GuiData.UISmallfont.MeasureString(_text);

                var _pos = new Vector2((float)(_empty.X + _empty.Width / 2) - _vector.X / 2f, (float)_empty.Y);

                this.spriteBatch.DrawString(GuiData.UISmallfont, _text, _pos, Color.Black);

                if (this.bounds.Height > 500)
                {
                    this.y += 30;
                }
                else
                {
                    this.y += 12;
                }
            }

            Rectangle empty = Rectangle.Empty;
            empty.X = this.bounds.X + 1;
            empty.Y = this.y;
            empty.Width = this.bounds.Width - 2;
            empty.Height = 20;
            this.spriteBatch.Draw(Utils.white, empty, this.userScreen.highlightColor);

            string text2 = LocaleTerms.Loc("You are currently logged as : ") + userScreen.activeSession.accountName;
            Vector2 vector = GuiData.UISmallfont.MeasureString(text2);

            var pos = new Vector2((float)(empty.X + empty.Width / 2) - vector.X / 2f, (float)empty.Y);
            if (Hacknet.Localization.LocaleActivator.ActiveLocaleIsCJK())
            {
                pos.Y = pos.Y - 2f;
            }
            userScreen.postFXDrawActions = (Action)System.Delegate.Combine(userScreen.postFXDrawActions, new Action(delegate
            {
                this.spriteBatch.DrawString(GuiData.UISmallfont, text2, pos, Color.Black);
            }));
            if (this.bounds.Height > 500)
            {
                this.y += 40;
            }
            else
            {
                this.y += 12;
                
            }
        }

        private void doConnectDisplay()
        {
            this.doConnectHeader();

            int num = 0 + 6;
            int num2 = 40;
            int num3 = this.bounds.Height - (this.y - this.bounds.Y) - 20;
            num3 -= num * 5;
            if ((double)num3 / (double)num < (double)num2)
            {
                num2 = (int)((double)num3 / (double)num);
            }

            for (int i = 0; i < userScreen.activeSession.daemonButtons.Count; i++)
            {
                var daemonButton = userScreen.activeSession.daemonButtons[i];
                if (Hacknet.Gui.Button.doButton(29000 + i, this.x, this.y, 300, num2, daemonButton.Display, this.userScreen.highlightColor))
                {
                    this.userScreen.Execute(daemonButton.Command);
                }
                this.y += num2 + 5;
            }
            if (Hacknet.Gui.Button.doButton(300000, this.x, this.y, 300, num2, LocaleTerms.Loc("Login"), this.userScreen.highlightColor))
            {
                //Sets prefixcommand inside of OnNetTerminal.cs so that when you press the login button it asks for a username and password and does login (user) (pass) instead of using (user) and (pass) as a commmand.
                //You can prob use prefixcommand for more then just this.
                userScreen.terminal.currentLine = "";
                userScreen.terminal.writeLine("(Username) (Password)");
                userScreen.terminal.prefixcommand = "login";
                Hacknet.Gui.TextBox.cursorPosition = 0;
                Hacknet.Gui.TextBox.textDrawOffsetPosition = 0;
                userScreen.terminal.executionPreventionIsInteruptable = false;

                //VVV What i found when i got here VVV
                //this.userScreen.Execute("login");
            }
            this.y += num2 + 5;
            /*if (Hacknet.Gui.Button.doButton(300002, this.x, this.y, 300, num2, LocaleTerms.Loc("Probe System"), new Color?(this.userScreen.highlightColor)))
            {
                this.userScreen.Execute("probe");
            }
            this.y += num2 + 5;*/
            if (Hacknet.Gui.Button.doButton(300003, this.x, this.y, 300, num2, LocaleTerms.Loc("View Filesystem"), this.userScreen.highlightColor))
            {
                this.userScreen.Execute("ls");
            }
            this.y += num2 + 5;
            /*if (Hacknet.Gui.Button.doButton(300009, this.x, this.y, 300, num2, LocaleTerms.Loc("Scan Network"), this.userScreen.highlightColor))
            {
                this.userScreen.Execute("scan");
            }*/
            this.y = this.bounds.Y + this.bounds.Height - 30;
            if (Hacknet.Gui.Button.doButton(300012, this.x, this.y, 300, 20, LocaleTerms.Loc("Disconnect"), this.userScreen.lockedColor))
            {
                this.userScreen.Execute("dc");
                return;
            }
        }

        private void doLSModule()
        {
            this.x = 5;
            this.y = 5;
            int num = this.bounds.Width - 25;
            string text = this.userScreen.activeSession.serverName + " " + LocaleTerms.Loc("File System");
            Hacknet.Gui.TextItem.doFontLabel(new Vector2((float)(this.bounds.X + this.x), (float)(this.bounds.Y + this.y)), text, GuiData.font, new Color?(Color.White), (float)this.bounds.Width - 46f, 60f, false);
            if (Hacknet.Gui.Button.doButton(299999, this.bounds.X + (this.bounds.Width - 41), this.bounds.Y + 12, 27, 29, "<-", null))
            {
                Console.WriteLine(this.userScreen.activeSession.workingPath);
                if (this.userScreen.activeSession.workingPath != "/" && this.userScreen.activeSession.workingPath != "")
                {
                    this.userScreen.Execute("cd ..");
                    this.userScreen.Execute("ls");
                }
                else
                {
                    this.state = DisplayState.SSH_SESSION;
                }
            }
            this.y += 50;
            Rectangle dest = GuiData.tmpRect;
            dest.Width = this.bounds.Width;
            dest.X = this.bounds.X;
            dest.Y = this.bounds.Y + 55;
            dest.Height = this.bounds.Height - 57;

            Hacknet.Gui.TextItem.doFontLabel(new Vector2(dest.X, dest.Y), "Working Directory : "+(userScreen.activeSession.workingPath == "" ? "/" : userScreen.activeSession.workingPath), GuiData.smallfont, new Color?(Color.White), (float)this.bounds.Width - 46f, 60f, false);

            int ButtonHeight = (int)(GuiData.ActiveFontConfig.tinyFontCharHeight + 10f);

            dest.Y += 25;

            int width = dest.Width - 25;
            var sessionState = (LsState)userScreen.activeSession.GetState();
            for(int i = 0; i < sessionState.files.Count; i++)
            {
                if (sessionState.files[i].GetDisplayName() == "" || sessionState.files[i] == null)
                    continue;

                var activeFile = sessionState.files[i];

                //spriteBatch.Draw(Utils.white, new Rectangle((int)bounds.X+6, (int)dest.Y + 3 + (ButtonHeight + 5) * i, 5, (int)ButtonHeight), activeFile.hasWritePermission ? Color.White : Color.Black);
                if (Hacknet.Gui.Button.doButton(300000 + i, bounds.X + 5 + 5, dest.Y + 2 + (ButtonHeight+5)*i, width - 5, ButtonHeight, activeFile.GetDisplayName(), null))
                {
                    if(activeFile.IsFolder())
                    {
                        this.userScreen.Execute("cd " + activeFile.GetActualName());
                        this.userScreen.Execute("ls");
                    }
                    else
                    {
                        this.userScreen.Execute("view " + activeFile.GetActualName());

                    }
                }
            };
            //Hacknet.Gui.Button.DisableIfAnotherIsActive = true;
            //Hacknet.Gui.Button.DisableIfAnotherIsActive = false;
        }

        private void doViewModule()
        {
            if (Hacknet.Gui.Button.doButton(299999, this.bounds.X + (this.bounds.Width - 41), this.bounds.Y + 12, 27, 29, "<-", null))
            {
                this.userScreen.Execute("ls");
            }
            Rectangle rectangle = GuiData.tmpRect;
            rectangle.Width = this.bounds.Width;
            rectangle.X = this.bounds.X;
            rectangle.Y = this.bounds.Y + 1;
            rectangle.Height = this.bounds.Height - 2;

            string filename = ((ViewState)userScreen.activeSession.currentState).name;

            Hacknet.Gui.TextItem.doFontLabel(new Vector2((float)this.x, (float)(this.y + 3)), filename, GuiData.font, new Color?(Color.White), (float)(this.bounds.Width - 70), 3.40282347E+38f, false);
            int num = 55;
            Rectangle dest = new Rectangle(rectangle.X + 4, rectangle.Y + num, rectangle.Width - 6, rectangle.Height - num - 2);
            string content = ((ViewState)userScreen.activeSession.currentState).content;
            this.y += 70;
            string text2 = Utils.SuperSmartTwimForWidth(content, this.bounds.Width - 40, GuiData.tinyfont);
            this.viewTextRegion.Draw(dest, text2, this.spriteBatch);
        }

        private void doIRCModule()
        {
            var sb = spriteBatch;
            Rectangle dest = Utils.InsetRectangle(bounds, 2);
            Rectangle destinationRectangle = new Rectangle(dest.X, dest.Y - 1, 18, dest.Height + 2);
            sb.Draw(Utils.white, destinationRectangle, userScreen.moduleColorSolid);
            destinationRectangle.X += destinationRectangle.Width / 2;
            destinationRectangle.Width /= 2;
            sb.Draw(Utils.white, destinationRectangle, Color.Black * 0.2f);
            dest.X += 20;
            dest.Width -= 25;
            Rectangle rectangle = new Rectangle(dest.X + 4, dest.Y, dest.Width, 35);
            Hacknet.Gui.TextItem.doFontLabelToSize(rectangle, "Interweb Remote Com", GuiData.font, Color.White, true, true);
            int num = dest.Width / 4;
            int num2 = 22;
            if (Hacknet.Gui.Button.doButton(37849102, rectangle.X + rectangle.Width - 6 - num, rectangle.Y + rectangle.Height - rectangle.Height / 2 - num2 / 2, num, num2, LocaleTerms.Loc("Exit IRC"), userScreen.moduleColorSolid))
            {
                this.userScreen.Execute("daemon exit");
                this.state = DisplayState.SSH_SESSION;
            }
            rectangle.Y += rectangle.Height;
            rectangle.X -= 6;
            dest.Y += rectangle.Height;
            dest.Height -= rectangle.Height;
            rectangle.Height = 2;
            sb.Draw(Utils.white, rectangle, userScreen.moduleColorSolid);
            dest.Y += rectangle.Height + 2;
            dest.Height -= rectangle.Height + 2;
            dest.Height -= 6;
            PatternDrawer.draw(dest, 0.22f, Color.Black * 0.5f, /*flag ?*/ (userScreen.moduleColorSolid * 0.12f) /*: (Utils.AddativeRed * 0.2f)*/, sb, /*flag ? */PatternDrawer.thinStripe /*: PatternDrawer.warningStripe*/);
            dest.X += 2;
            dest.Width -= 4;
            dest.Height -= 4;
            //if (flag)
            //{
            var messages = ((IrcState)userScreen.activeSession.GetState()).GetMessages();

            int _num = (int)(GuiData.ActiveFontConfig.tinyFontCharHeight + 4f);
            int _num2 = 4;
            int _num3 = (int)((float)dest.Height / (float)_num);
            int _num4 = _num3;
            int _num5 = messages.Count - 1;
            int _num6 = 0;
            int y = dest.Y;
            while (_num4 > 0 && _num5 >= 0 && dest.Height - _num6 > _num)
            {
                //bool needsNewMessagesLineDraw = this.messagesAddedSinceLastView > 0 && this.messagesAddedSinceLastView < logsFromFile.Count && logsFromFile.Count - num5 == this.messagesAddedSinceLastView;
                _num4 -= this.DrawIRCMessage(messages[_num5], dest, _num, _num4, y/*, needsNewMessagesLineDraw,*/, out dest);
                dest.Y -= _num2;
                _num6 += _num2;
                _num5--;
            }
            if (_num5 <= -1 && _num4 > 1)
            {
                int _num7 = _num + 8;
                Rectangle _rectangle = new Rectangle(dest.X, dest.Y + dest.Height - _num7, dest.Width, _num7);
                SpriteFont tinyfont = GuiData.tinyfont;
                //string text = "--- " + LocaleTerms.Loc("Log Cleared by Administrator") + " ---";
                //Vector2 vector = tinyfont.MeasureString(text);
                //sb.DrawString(tinyfont, text, Utils.ClipVec2ForTextRendering(new Vector2((float)rectangle.X + (float)rectangle.Width / 2f - vector.X / 2f, (float)rectangle.Y + (float)rectangle.Height / 2f - vector.Y / 2f)), Color.Gray);
            }
            //}
            /*else
            {
                int num3 = dest.Height / 4;
                Rectangle rectangle2 = new Rectangle(dest.X - 4, dest.Y + dest.Height / 2 - num3 / 2, dest.Width + 6, num3);
                sb.Draw(Utils.white, rectangle2, this.userScreen.lockedColor);
                rectangle2.Height -= 35;
                Hacknet.Gui.TextItem.doCenteredFontLabel(rectangle2, LocaleTerms.Loc("Login To Server"), GuiData.font, Color.White, false);
                if (Hacknet.Gui.Button.doButton(84109551, rectangle2.X + rectangle2.Width / 2 - rectangle2.Width / 4, rectangle2.Y + rectangle2.Height - 32, rectangle2.Width / 2, 28, "Login", null))
                {
                    this.userScreen.Execute("login");
                }
            //}*/
        }

        private int DrawIRCMessage(IrcMessage message, Rectangle startingDest, int lineHeight, int linesRemaining, int yNotToPass, out Rectangle dest)
        {
            dest = startingDest;
            int num = 55;
            int num2 = 76;
            int num3 = 4;
            if (Settings.ActiveLocale != "en-us")
            {
                num2 = 78;
            }
            if (GuiData.ActiveFontConfig.name.ToLower() == "medium")
            {
                num2 = 92;
            }
            else if (GuiData.ActiveFontConfig.name.ToLower() == "large")
            {
                num2 = 115;
            }
            string text = "<" + message.author + ">";
            int num4 = (int)(GuiData.tinyfont.MeasureString(text).X + (float)num3);
            num2 = System.Math.Max(num2, (int)(GuiData.tinyfont.MeasureString(text).X + (float)num3));
            int width = dest.Width - (num + num3 + num2);
            string text2 = message.content;
            string[] array = new string[]
            {
                text2
            };
            /*if (!log.Message.StartsWith("!ATTACHMENT:"))
            {
                text2 = Utils.SuperSmartTwimForWidth(text2, width, GuiData.tinyfont);
                array = text2.Split(Utils.newlineDelim, System.StringSplitOptions.None);
            }*/
            Rectangle rectangle = new Rectangle(dest.X + num + num3 + num2, dest.Y, dest.Width - (num + num3 + num2), dest.Height);
            Rectangle dest2 = new Rectangle(dest.X, dest.Y, num + num2, dest.Height);
            Color color = Color.LightBlue;
            /*if (HighlightKeywords.ContainsKey(log.Author))
            {
                color = HighlightKeywords[log.Author];
            }*/
            Color defaultColor = Color.Lerp(Color.White, color, 0.22f);
            /*if (needsNewMessagesLineDraw)
            {
                int num5 = array.Length;
                Rectangle destinationRectangle = new Rectangle(dest.X, dest.Y + dest.Height - lineHeight * num5 + 1, dest.Width, 1);
                sb.Draw(Utils.white, destinationRectangle, Color.White * 0.5f);
            }*/
            int num6 = array.Length - 1;
            while (num6 >= 0 && linesRemaining > 0)
            {
                if (num6 == 0)
                {
                    this.DrawLine("[" + "tst"/*log.Timestamp*/ + "] ", dest2, spriteBatch, Color.White);
                    int x = dest2.X;
                    dest2.X = dest2.X + dest2.Width - num4;
                    this.DrawLine(text, dest2, spriteBatch, color);
                    dest2.X = x;
                }
                this.DrawLine(array[num6], rectangle, spriteBatch, defaultColor);
                dest.Height -= lineHeight;
                rectangle.Height = dest.Height;
                dest2.Height = dest.Height;
                linesRemaining--;
                if (dest.Y + dest.Height - 6 <= yNotToPass)
                {
                    //needsNewMessagesLineDraw = false;
                    break;
                }
                num6--;
            }
            Rectangle destinationRectangle2 = rectangle;
            destinationRectangle2.Width = 1;
            destinationRectangle2.X -= 5;
            destinationRectangle2.Height = lineHeight * array.Length + 4;
            destinationRectangle2.Y = rectangle.Y + rectangle.Height + 2;
            spriteBatch.Draw(Utils.white, destinationRectangle2, Color.White * 0.12f);
            return array.Length;
        }

        private void DrawLine(string line, Rectangle dest, SpriteBatch sb, Color defaultColor)
        {
            Vector2 vector = Utils.ClipVec2ForTextRendering(new Vector2((float)dest.X, (float)(dest.Y + dest.Height) - (GuiData.ActiveFontConfig.tinyFontCharHeight + 1f)));

            sb.DrawString(GuiData.tinyfont, line, vector, defaultColor);
        }

        private void doWebModule()
        {
            if (Hacknet.Gui.Button.doButton(299999, this.bounds.X + (this.bounds.Width - 41), this.bounds.Y + 12, 27, 29, "x", null))
            {
                this.state = DisplayState.SSH_SESSION;
            }
            Rectangle rectangle = GuiData.tmpRect;
            rectangle.Width = this.bounds.Width;
            rectangle.X = this.bounds.X;
            rectangle.Y = this.bounds.Y + 1;
            rectangle.Height = this.bounds.Height - 2;

            string serverName = userScreen.activeSession.serverName;

            Hacknet.Gui.TextItem.doFontLabel(new Vector2((float)this.x, (float)(this.y + 3)), serverName, GuiData.font, new Color?(Color.White), (float)(this.bounds.Width - 70), 3.40282347E+38f, false);
            int num = 55;
            Rectangle dest = new Rectangle(rectangle.X + 4, rectangle.Y + num, rectangle.Width - 6, rectangle.Height - num - 2);
            string content = ((WebState)userScreen.activeSession.currentState).content;
            this.y += 70;
            string text2 = Utils.SuperSmartTwimForWidth(content, this.bounds.Width - 40, GuiData.tinyfont);
            this.viewTextRegion.Draw(dest, text2, this.spriteBatch);
        }

    }
}
