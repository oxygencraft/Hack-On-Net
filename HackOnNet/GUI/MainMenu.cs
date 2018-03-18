using Hacknet;
using HackOnNet.Net;
using HackOnNet.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathfinder.Event;
using Pathfinder.GUI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.GUI
{
    internal static class MainMenu
    {
        public enum MenuState { OG_MENU, MAIN_MENU, LOGIN }

        public enum LoginState { MENU, LOGGED_IN, INVALID, UNAVAILABLE, LOGGED, LOGGING_IN }

        public static LoginState loginState = LoginState.MENU;
        private static MenuState currentState = MenuState.OG_MENU;

        private static Color CancelColor = new Color(125, 82, 82);
        private static string terminalString = "";
        private static string currentPrompt = "USERNAME :";
        private static int promptIndex = 0;
        private static List<string> History = new List<string>();
        private static List<string> PromptSequence = new List<string>();
        private static List<string> Answers = new List<string>();
        private static bool IsReady = false;
        private static bool IsLoginMode = true;
        private static bool InPasswordMode = false;
        private static bool CanReturnEnter = false;
        private static bool HasOverlayScreen = false;
        private static bool PreventAdvancing = false;
        public static Action RequestGoBack;

        private static string loginMessage = "";
        private static Hacknet.MainMenu bMenu;

        #region Buttons

        private static Button logIn = new Button(180, 240, 450, 50, "Continue to terminal", Color.LightGreen)
        { DrawFinish = (r) => { if (r.JustReleased) ChangeState(MenuState.LOGIN); } };

        private static Button confirmLogIn = new Button(180, 480, 300, 40, "Confirm", Color.LightGreen)
        {
            DrawFinish = (r) =>
            {
                if (r.JustReleased)
                {
                    MainMenu.StartGame(Answers[Answers.Count - 2], Answers[Answers.Count - 1]);
                }
            }
        };

        private static Button openHackOnNet = new Button(180, 655, 450, 50, "Open Multiplayer Mod", Color.AliceBlue)
        { DrawFinish = (r) => { if (r.JustReleased) ChangeState(MenuState.MAIN_MENU); } };

        private static Button returnButton = new Button(180, 535, 300, 28, "Return to Bootloader", Color.Gray)
        { DrawFinish = (r) => { if (r.JustReleased) ChangeState(MenuState.OG_MENU); } };

        #endregion Buttons

        #region Draw

        public static void DrawMainMenu(DrawMainMenuEvent e)
        {
            if (bMenu == null)
                bMenu = e.MainMenu;
            if (currentState == MenuState.OG_MENU)
            {
                openHackOnNet.Draw();
                return;
            }
            e.IsCancelled = true;
            if (currentState == MenuState.MAIN_MENU)
                DrawMain(e);
            else if (currentState == MenuState.LOGIN)
                DrawLogin(e);
        }

        private static void DrawMain(DrawMainMenuEvent e)
        {
            Rectangle dest = new Rectangle(180, 120, 340, 100);
            ResetForLogin();
            SpriteFont titleFont = (SpriteFont)e.MainMenu.GetType().GetField("titleFont", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(e.MainMenu);
            Color titleColor = (Color)e.MainMenu.GetType().GetField("titleColor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(e.MainMenu);

            Hacknet.Effects.FlickeringTextEffect.DrawLinedFlickeringText(dest, "HACK ON NET", 7f, 0.55f, titleFont, null, titleColor, 2);
            Hacknet.Gui.TextItem.doFontLabel(new Vector2(520f, 178f), "Hack On Net v0.1", GuiData.smallfont, new Color?(titleColor * 0.5f), 600f, 26f, false);

            logIn.Draw();

            if (Hacknet.Gui.Button.doButton(3, 180, 305, 450, 40, LocaleTerms.Loc("Settings"), Color.LightSkyBlue))
            {
                e.MainMenu.ScreenManager.AddScreen(new OptionsMenu(), new PlayerIndex?(e.MainMenu.ScreenManager.controllingPlayer));
            }

            if (Hacknet.Gui.Button.doButton(15, 180, 360, 450, 28, LocaleTerms.Loc("Back to Main Menu"), Color.Gray))
            {
                currentState = MenuState.OG_MENU;
            }
        }

        private static void DrawLogin(DrawMainMenuEvent e)
        {
            RequestGoBack += (Action)(() => ChangeState(MenuState.MAIN_MENU));
            Rectangle dest = new Rectangle(bMenu.ScreenManager.GraphicsDevice.Viewport.Width / 4, bMenu.ScreenManager.GraphicsDevice.Viewport.Height / 4, bMenu.ScreenManager.GraphicsDevice.Viewport.Width / 2, bMenu.ScreenManager.GraphicsDevice.Viewport.Height / 4);
            SpriteFont smallfont = GuiData.smallfont;
            int y1 = dest.Y + dest.Height + 12;
            int y2 = y1 + 10;
            int num1 = (int)((double)smallfont.MeasureString(currentPrompt).X + 4.0);
            int num2 = dest.Y + dest.Height - 18;
            int num3 = y2 - 60;
            float num4 = GuiData.ActiveFontConfig.tinyFontCharHeight + 8f;
            Vector2 position = new Vector2((float)dest.X, false ? (float)y2 : (float)(dest.Y + dest.Height - 20) - num4);

            GuiData.spriteBatch.Draw(Utils.white, new Rectangle(dest.X, y1, dest.Width / 2, 1), Utils.SlightlyDarkGray);
            GuiData.spriteBatch.DrawString(GuiData.UISmallfont, loginMessage, new Vector2(position.X, position.Y + 125), MainMenu.CancelColor);
            GuiData.spriteBatch.DrawString(smallfont, currentPrompt, new Vector2((float)dest.X, (float)num2), Color.White);

            if (IsReady)
            {
                if (!GuiData.getKeyboadState().IsKeyDown(Keys.Enter))
                    CanReturnEnter = true;
                if ((!HasOverlayScreen && (!IsLoginMode || Hacknet.Gui.Button.doButton(16392804, dest.X, y2, dest.Width / 3, 28, LocaleTerms.Loc("CONFIRM"), new Color?(Color.White))) || CanReturnEnter && Utils.keyPressed(GuiData.lastInput, Keys.Enter, new PlayerIndex?())) && !PreventAdvancing)
                {
                    if (IsLoginMode)
                    {
                        if (Answers.Count < 2)
                        {
                            ResetForLogin();
                        }
                        else
                        {
                            string username = Answers[Answers.Count - 2];
                            string password = Answers[Answers.Count - 1];
                            Hacknet.Gui.TextBox.MaskingText = false;
                            StartGame(username, password);
                        }
                    }
                    else
                    {
                        Hacknet.Gui.TextBox.MaskingText = false;
                        Console.Write("An Error has occured! Fixing now");
                        History.Clear();
                        currentPrompt = "";
                        ResetForLogin();
                        IsLoginMode = true;
                    }
                    PreventAdvancing = true;
                }
                y2 += 36;
            }
            else
            {
                Hacknet.Gui.TextBox.MaskingText = InPasswordMode;
                terminalString = Hacknet.Gui.TextBox.doTerminalTextField(16392802, dest.X + num1, num2 - 2, dest.Width, 20, 1, terminalString, GuiData.UISmallfont);
            }

            if (!HasOverlayScreen && Hacknet.Gui.TextBox.BoxWasActivated)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(currentPrompt);
                stringBuilder.Append(" ");
                string str1 = terminalString;
                if (InPasswordMode)
                {
                    string str2 = "";
                    for (int index = 0; index < str1.Length; ++index)
                        str2 += "*";
                    str1 = str2;
                }
                stringBuilder.Append(str1);
                History.Add(stringBuilder.ToString());
                Advance(terminalString);
                terminalString = "";
                Hacknet.Gui.TextBox.cursorPosition = 0;
                Hacknet.Gui.TextBox.BoxWasActivated = false;
            }
            if (!HasOverlayScreen && Hacknet.Gui.Button.doButton(16392806, dest.X, y2, dest.Width / 3, 22, LocaleTerms.Loc("CANCEL"), new Color?(CancelColor)) && RequestGoBack != null)
            {
                InPasswordMode = false;
                Hacknet.Gui.TextBox.MaskingText = false;
                RequestGoBack();
            }

            for (int index = History.Count - 1; index >= 0; --index)
            {
                GuiData.spriteBatch.DrawString(GuiData.UISmallfont, History[index], position, Color.White);
                position.Y -= num4 * (1f);
            }
        }

        #endregion Draw

        #region Functions

        async private static void StartGame(string username, string password)
        {
            UserScreen screen = new UserScreen();
            NetManager netManager = new NetManager(screen);
            netManager.Init();

            if (loginState == LoginState.UNAVAILABLE)
            {
                loginMessage = "The server is unavailable.";
                loginState = LoginState.MENU;
                return;
            }

            string hashedPassword = Hash(password);
            netManager.Login(username, hashedPassword);

            loginMessage = "Logging in...";

            loginState = LoginState.LOGGING_IN;

            for (int i = 0; i < 1000; i++)
            {
                await Task.Delay(10);
                if (loginState != LoginState.LOGGING_IN)
                    break;
            }
            if (loginState == LoginState.LOGGING_IN)
            {
                loginState = LoginState.MENU;
                loginMessage = "Login Timeout";
            }
            else if (loginState == LoginState.LOGGED)
            {
                loginMessage = "";
                currentState = MenuState.LOGIN;
                screen.netManager = netManager;
                screen.username = username;
                GuiData.hot = -1;
                InPasswordMode = false;
                Hacknet.Gui.TextBox.MaskingText = false;
                Hacknet.Gui.TextBox.cursorPosition = 0;
                bMenu.ScreenManager.AddScreen(screen, new PlayerIndex?(bMenu.ScreenManager.controllingPlayer));
            }
            else if (loginState == LoginState.INVALID)
            {
                loginMessage = "Invalid Username or Password.";
            }
            else if (loginState == LoginState.UNAVAILABLE)
            {
                loginMessage = "The server is unavailable.";
            }

            loginState = LoginState.MENU;
            ResetForLogin();
        }

        private static void ChangeState(MenuState state)
        {
            currentState = state;
        }

        private static string Hash(string input)
        {
            byte[] arrBytes = System.Text.Encoding.UTF8.GetBytes(input);
            SHA256Managed hashString = new SHA256Managed();
            string hex = "";

            var hashValue = hashString.ComputeHash(arrBytes);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        private static void Advance(string answer)
        {
            ++promptIndex;
            Answers.Add(answer);
            if (true)
            {
                if (promptIndex == 1)
                {
                    if (string.IsNullOrWhiteSpace(Answers[0]))
                    {
                        History.Add(" -- " + LocaleTerms.Loc("Username cannot be blank. Try Again") + " -- ");
                        promptIndex = 0;
                        Answers.Clear();
                    }
                    else if (Utils.StringContainsInvalidFilenameChars(Answers[0]))
                    {
                        History.Add(" -- " + LocaleTerms.Loc("Username contains invalid characters. Try Again") + " -- ");
                        promptIndex = 0;
                        Answers.Clear();
                    }
                }
                if (promptIndex == 3)
                {
                    if (string.IsNullOrWhiteSpace(answer))
                    {
                        History.Add(" -- " + LocaleTerms.Loc("Password Cannot be Blank! Try Again") + " -- ");
                        promptIndex = 1;
                        string answer1 = Answers[0];
                        Answers.Clear();
                        Answers.Add(answer1);
                    }
                    else if (Answers[1] != answer)
                    {
                        History.Add(" -- " + LocaleTerms.Loc("Password Mismatch! Try Again") + " -- ");
                        promptIndex = 1;
                        string answer1 = Answers[0];
                        Answers.Clear();
                        Answers.Add(answer1);
                    }
                }
                InPasswordMode = promptIndex == 1 || promptIndex == 2;
            }
            if (promptIndex >= PromptSequence.Count)
            {
                if (true)
                {
                    currentPrompt = LocaleTerms.Loc("READY - PRESS ENTER TO CONFIRM");
                    IsReady = true;
                }
            }
            else
                currentPrompt = PromptSequence[promptIndex];
        }

        public static void ResetForLogin()
        {
            promptIndex = 0;
            IsReady = false;
            PromptSequence.Clear();
            PromptSequence.Add(LocaleTerms.Loc("USERNAME") + " :");
            PromptSequence.Add(LocaleTerms.Loc("PASSWORD") + " :");
            History.Clear();
            History.Add("-- " + LocaleTerms.Loc("HackOnNet Login") + " --");
            currentPrompt = PromptSequence[promptIndex];
            IsLoginMode = true;
            PreventAdvancing = false;
            terminalString = "";
            Hacknet.Gui.TextBox.cursorPosition = 0;
        }

        public static void SetLoginStatus(string status)
        {
            loginMessage = status;
        }

        #endregion Functions
    }
}