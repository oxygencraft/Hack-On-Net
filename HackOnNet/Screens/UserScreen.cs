using HackLinksCommon;
using Hacknet;
using HackOnNet.DotNetCompatibility;
using HackOnNet.FileSystem;
using HackOnNet.Modules;
using HackOnNet.Modules.Overlays;
using HackOnNet.Net;
using HackOnNet.Sessions;
using HackOnNet.Sessions.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HackOnNet.Modules.OnNetDisplayModule;

namespace HackOnNet.Screens
{
    class UserScreen : Hacknet.GameScreen
    {
        private GameTime lastGameTime;
        public Action postFXDrawActions;

        public ContentManager content;

        private Texture2D scanLines;
        private Color scanlinesColor = new Color(255, 255, 255, 15);

        private Texture2D cross;

        private Rectangle fullscreen;

        private Rectangle topBar;
        private Color topBarColor = new Color(0, 139, 199, 255);
        private Color topBarTextColor = new Color(126, 126, 126, 100);
        private Color topBarIconsColor = Color.White;

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

        public Color defaultHighlightColor = new Color(0, 139, 199, 255);
        public Color warningColor = Color.Red;

        public Color guestAccountColor = new Color(119,119,119);
        public Color userAccountColor = new Color(0, 139, 199, 255);
        public Color adminAccountColor = new Color(242, 160, 0);
        public Color kernelAccountColor = new Color(255, 80, 80);

        public SoundEffect beep;

        public OnNetTerminal terminal;
        public OnNetworkMap netMap;
        public OnNetDisplayModule display;
        public OnNetRamModule ram;

        public List<Overlay> overlays = new List<Overlay>();

        private System.Collections.Generic.List<OnModule> modules;
        private MessageBoxScreen ExitToMenuMessageBox;

        public static float WARNING_FLASH_TIME = 2f;
        public float warningFlashTimer = 0f;

        public NetManager netManager;

        public string username;
        public string homeIP = "none";

        public Session activeSession;

        public override void LoadContent()
        {
            this.content = base.ScreenManager.Game.Content;

            scanLines = this.content.Load<Texture2D>("ScanLines");
            fullscreen = new Rectangle(0, 0, base.ScreenManager.GraphicsDevice.Viewport.Width, base.ScreenManager.GraphicsDevice.Viewport.Height);

            this.topBar = new Rectangle(0, 0, base.ScreenManager.GraphicsDevice.Viewport.Width, OS.TOP_BAR_HEIGHT - 1);
            this.cross = this.content.Load<Texture2D>("Cross");

            this.beep = this.content.Load<SoundEffect>("SFX/beep");

            this.modules = new System.Collections.Generic.List<OnModule>();
            Viewport viewport = base.ScreenManager.GraphicsDevice.Viewport;
            int mODULE_WIDTH = RamModule.MODULE_WIDTH;
            int num2 = 205;
            int num3 = (int)((double)(viewport.Width - mODULE_WIDTH - 6) * 0.44420000000000004);
            int num4 = (int)((double)(viewport.Width - mODULE_WIDTH - 6) * 0.5558);
            int height = viewport.Height - num2 - OS.TOP_BAR_HEIGHT - 6;
            this.terminal = new OnNetTerminal(new Rectangle(viewport.Width - 2 - num3, OS.TOP_BAR_HEIGHT, num3, viewport.Height - OS.TOP_BAR_HEIGHT - 2), this);
            this.terminal.name = "TERMINAL";
            this.terminal.LoadContent();
            this.netMap = new OnNetworkMap(new Rectangle(mODULE_WIDTH + 4, viewport.Height - num2 - 2, num4 - 1, num2), this);
            this.netMap.name = "netMap v1.72";
            this.netMap.LoadContent();
            this.modules.Add(this.netMap);
            this.display = new OnNetDisplayModule(new Rectangle(mODULE_WIDTH + 4, OS.TOP_BAR_HEIGHT, num4 - 2, height), this);
            this.display.name = "DISPLAY";
            this.display.LoadContent();
            this.modules.Add(this.display);
            this.ram = new OnNetRamModule(new Rectangle(2, OS.TOP_BAR_HEIGHT, mODULE_WIDTH, 800 - (OS.TOP_BAR_HEIGHT + 2) + RamModule.contentStartOffset), this);
            this.ram.name = "RAM";
            this.ram.LoadContent();
            this.modules.Add(this.ram);



            this.modules.Add(terminal);
        }

        public override void UnloadContent()
        {
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            float time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (this.warningFlashTimer > 0f)
            {
                this.warningFlashTimer -= time;
                if (this.warningFlashTimer <= 0f)
                {
                    this.highlightColor = this.defaultHighlightColor;
                }
                else
                {
                    this.highlightColor = Color.Lerp(this.defaultHighlightColor, this.warningColor, this.warningFlashTimer / OS.WARNING_FLASH_TIME);
                    this.moduleColorSolid = Color.Lerp(this.moduleColorSolidDefault, this.warningColor, this.warningFlashTimer / OS.WARNING_FLASH_TIME);
                }
            }

            foreach (Overlay overlay in overlays)
                overlay.Update(gameTime.ElapsedGameTime.TotalSeconds);
        }

        public override void Draw(GameTime gameTime)
        {
            try
            {
                float t = (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (this.lastGameTime == null)
                {
                    this.lastGameTime = gameTime;
                }

                PostProcessor.begin();
                GuiData.startDraw();
                try
                {
                    Overlay onlyOverlay = null;
                    foreach (Overlay overlay in this.overlays)
                        if (overlay.PreventsDrawing())
                        {
                            onlyOverlay = overlay;
                            break;
                        }
                    if(onlyOverlay == null)
                    {
                        this.drawBackground();
                        this.drawModules(gameTime);
                        SFX.Draw(GuiData.spriteBatch);
                        foreach (Overlay overlay in this.overlays)
                            overlay.Draw();
                    }
                    else
                    {
                        onlyOverlay.Draw();
                    }
                }
                catch (System.Exception ex)
                {
                    Utils.AppendToErrorFile(Utils.GenerateReportFromException(ex) + "\r\n\r\n");
                }
                GuiData.endDraw();
                PostProcessor.end();
                GuiData.startDraw();
                if (this.postFXDrawActions != null)
                {
                    this.postFXDrawActions();
                    this.postFXDrawActions = null;
                }
                this.drawScanlines();
                GuiData.endDraw();
            }
            catch (System.Exception ex)
            {
                Utils.AppendToErrorFile(Utils.GenerateReportFromException(ex));
            }
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
            GuiData.doInput(input);
        }

        public void drawScanlines()
        {
            if (PostProcessor.scanlinesEnabled)
            {
                Vector2 position = new Vector2(0f, 0f);
                while (position.X < (float)base.ScreenManager.GraphicsDevice.Viewport.Width)
                {
                    while (position.Y < (float)base.ScreenManager.GraphicsDevice.Viewport.Height)
                    {
                        GuiData.spriteBatch.Draw(this.scanLines, position, this.scanlinesColor);
                        position.Y += (float)this.scanLines.Height;
                    }
                    position.Y = 0f;
                    position.X += (float)this.scanLines.Width;
                }
            }
        }

        public void HandleFX(string[] messages)
        {
            if(messages[0] == "traceOver")
            {
                Overlay overlay = new TerminationOverlay(this.ScreenManager.SpriteBatch, this.fullscreen, this);
                this.overlays.Add(overlay);
                overlay.Launch();
            }
            else if(messages[0] == "warnBlink")
                Flash(true);
        }

        public void Flash(bool doBeep)
        {
            this.warningFlashTimer = UserScreen.WARNING_FLASH_TIME;
            if(doBeep)
                beep.Play(0.5f, 0f, 0f);
        }

        public void drawBackground()
        {
            //ThemeManager.drawBackgroundImage(GuiData.spriteBatch, this.fullscreen);
            GuiData.spriteBatch.GraphicsDevice.Clear(Color.Black);
        }

        public void drawModules(GameTime gameTime)
        {
            Vector2 zero = Vector2.Zero;
            GuiData.spriteBatch.Draw(Utils.white, this.topBar, this.topBarColor);
            float t = (float)gameTime.ElapsedGameTime.TotalSeconds;
            try
            {
                Vector2 vector = GuiData.UITinyfont.MeasureString("Location: "+(activeSession == null ? "Not Connected" : activeSession.ip));
                zero.X = (float)this.topBar.Width - vector.X/* - (float)this.mailicon.getWidth()*/;
                zero.Y -= 3f;
                GuiData.spriteBatch.DrawString(GuiData.UITinyfont, "Location: " + (activeSession == null ? "Not Connected" : activeSession.ip), zero, this.topBarTextColor);
                if (GuiData.ActiveFontConfig.tinyFontCharHeight * 2f <= (float)this.topBar.Height)
                {
                    string text = LocaleTerms.Loc("Home IP:") + " " + homeIP + " ";
                    zero.Y += (float)(this.topBar.Height / 2);
                    vector = GuiData.UITinyfont.MeasureString(text);
                    zero.X = (float)this.topBar.Width - vector.X/* - (float)this.mailicon.getWidth()*/;
                    GuiData.spriteBatch.DrawString(GuiData.UITinyfont, text, zero, this.topBarTextColor);
                }
                zero.Y = 0f;
            }
            catch (System.Exception)
            {
            }
            zero.X = 110f;
            if (Hacknet.Gui.Button.doButton(3827178, 3, 0, 20, this.topBar.Height - 1, "", new Color?(this.topBarIconsColor), this.cross))
            {
                this.ExitToMenuMessageBox = new MessageBoxScreen("Logout of your\nCurrent Session?" + "\n", false, true);
                this.ExitToMenuMessageBox.OverrideAcceptedText = LocaleTerms.Loc("Exit to Menu");
                this.ExitToMenuMessageBox.Accepted += new System.EventHandler<PlayerIndexEventArgs>(this.quitGame);
                base.ScreenManager.AddScreen(this.ExitToMenuMessageBox);
            }
            else
            {
                zero.X = 2f;
            }
            zero.Y = 1f;
            string text2 = string.Concat((int)(1.0 / gameTime.ElapsedGameTime.TotalSeconds + 0.5));
            GuiData.spriteBatch.DrawString(GuiData.UITinyfont, text2, zero, this.topBarTextColor);
            zero.Y = 0f;

            //this.mailicon.Draw();

            /*int num = this.ram.bounds.Height + this.topBar.Height + 16;
            if (num < this.fullscreen.Height && this.ram.visible)
            {
                this.audioVisualizer.Draw(new Rectangle(this.ram.bounds.X, num + 1, this.ram.bounds.Width - 2, this.fullscreen.Height - num - 4), GuiData.spriteBatch);
            }*/
            for (int i = 0; i < this.modules.Count; i++)
            {
                try
                {
                    if (this.modules[i].visible)
                    {
                        this.modules[i].PreDrawStep();
                        this.modules[i].Draw(t);
                        this.modules[i].PostDrawStep();
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            /*if (this.ram.visible)
            {
                for (int i = 0; i < this.exes.Count; i++)
                {
                    this.exes[i].Draw(t);
                }
            }*/
            //this.IncConnectionOverlay.Draw(this.fullscreen, GuiData.spriteBatch);
            //this.traceTracker.Draw(GuiData.spriteBatch);
        }

        public void Write(string text)
        {
            this.terminal.writeLine(text);
        }

        public void HandleKernel(string[] command)
        {
            if(command[0] == "connect")
            {
                if(command[1] == "fail")
                {
                    Write("Connection failed.");
                    return;
                }
                int privilege;
                int.TryParse(command[3], out privilege);
                this.netMap.DiscoverNode(command[2]);
                string currentIP;
                currentIP = command[2];
                display.state = DisplayState.SSH_SESSION;
                Write("Connected to: " + command[2]);
                activeSession = new Session(currentIP, privilege);
            }
            else if(command[0] == "disconnect")
            {
                activeSession = null;
                display.state = DisplayState.NONE;
            }
            else if(command[0] == "ls")
            {
                display.state = DisplayState.NONE;
                activeSession.workingPath = command[1];
                if(activeSession.GetState().GetStateType() != SessionState.StateType.LS)
                {
                    activeSession.SetState(new LsState(activeSession));
                }
                var sessionState = (LsState)activeSession.GetState();
                sessionState.files.Clear();
                for(int i = 2; i < command.Length; i += 3)
                {
                    if (command[i] == "")
                        continue;
                    sessionState.files.Add(new LsFileEntry(command.Skip(i).Take(3).ToArray()));
                }
                display.state = DisplayState.LS;
            }
            else if(command[0] == "cd")
            {
                activeSession.workingPath = command[1];
            }
            else if(command[0] == "login")
            {
                activeSession.privilege = int.Parse(command[1]);
                activeSession.accountName = command[2];
            }
            else if(command[0] == "state")
            {
                if(command[1] == "irc")
                {
                    if(command[2] == "join")
                    {
                        activeSession.SetState(new IrcState(activeSession));
                        display.state = DisplayState.IRC;
                    }
                    else if(command[2] == "messg")
                    {
                        for(int i = 3; i < command.Length; i += 2)
                        {
                            string author = command[i];
                            string message = command[i + 1];

                            ((IrcState)activeSession.GetState()).AddMessage(author, message);
                        }
                        
                    }
                }
                if(command[1] == "view")
                {
                    string fileName = command[2];
                    string fileContent = command[3];

                    activeSession.SetState(new ViewState(activeSession, fileName, fileContent));
                    display.state = DisplayState.VIEW;
                }
            }
            else if(command[0] == "node")
            {
                if (activeSession != null)
                    activeSession.SetNodeInfo(command);
            }
        }
        
        public void Execute(string command)
        {
            try
            {
                netManager.Send(NetUtil.PacketType.COMND, command);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void quitGame(object sender, PlayerIndexEventArgs e)
        {
            base.ExitScreen();
            MainMenu.resetOS();
            base.ScreenManager.AddScreen(new MainMenu());
        }

        public void quitGame(object sender, string cause)
        {
            base.ExitScreen();
            MainMenu.resetOS();
            GUI.MainMenu.SetLoginStatus(cause);
            base.ScreenManager.AddScreen(new MainMenu());
        }

    }
}
