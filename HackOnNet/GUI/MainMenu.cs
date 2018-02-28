using Hacknet;
using HackOnNet.Net;
using HackOnNet.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Event;
using Pathfinder.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HackOnNet.GUI
{
    static class MainMenu
    {
        enum MenuState
        {
            MAIN_MENU,
            LOGIN
        }
        public enum LoginState
        {
            MENU,
            LOGGING_IN,
            LOGGED,
            INVALID,
            UNAVAILABLE
        }

        public static LoginState loginState = LoginState.MENU;

        private static string username = "";
        private static string password = "";
        private static string loginMessage = "";

        private static Hacknet.MainMenu baseMenu;

        private static MenuState currentState = MenuState.MAIN_MENU;

        private static Button logIn = new Button(180, 240, 450, 50, "Enter your terminal", Color.LightGreen)
        { DrawFinish = (r) => { if (r.JustReleased) ChangeState(MenuState.LOGIN); } };

        private static Button confirmLogIn = new Button(180, 480, 300, 40, "Confirm", Color.LightGreen)
        { DrawFinish = (r) => {
            if (r.JustReleased)
            {
                MainMenu.StartGame();
            }
        } };

        private static Button returnButton = new Button(180, 535, 300, 28, "Return to Bootloader", Color.Gray)
        { DrawFinish = (r) => { if (r.JustReleased) ChangeState(MenuState.MAIN_MENU); } };

        public static void DrawMainMenu(DrawMainMenuEvent e)
        {
            e.IsCancelled = true;

            if (baseMenu == null)
                baseMenu = e.MainMenu;

            if (currentState == MenuState.MAIN_MENU)
                DrawMain(e);
            else if (currentState == MenuState.LOGIN)
                DrawLogin(e);

        }

        private static void DrawMain(DrawMainMenuEvent e)
        {
            Rectangle dest = new Rectangle(180, 120, 340, 100);

            SpriteFont titleFont = (SpriteFont)e.MainMenu.GetType().GetField("titleFont", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(e.MainMenu);
            Color titleColor = (Color)e.MainMenu.GetType().GetField("titleColor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(e.MainMenu);

            Hacknet.Effects.FlickeringTextEffect.DrawLinedFlickeringText(dest, "HACK ON NET", 7f, 0.55f, titleFont, null, titleColor, 2);
            Hacknet.Gui.TextItem.doFontLabel(new Vector2(520f, 178f), "Hack On Net v0.1", GuiData.smallfont, new Color?(titleColor * 0.5f), 600f, 26f, false);

            logIn.Draw();

            if (Hacknet.Gui.Button.doButton(3, 180, 305, 450, 40, LocaleTerms.Loc("Settings"), Color.LightSkyBlue))
            {
                e.MainMenu.ScreenManager.AddScreen(new OptionsMenu(), new PlayerIndex?(e.MainMenu.ScreenManager.controllingPlayer));
            }

            if (Hacknet.Gui.Button.doButton(15, 180, 360, 450, 28, LocaleTerms.Loc("Exit"), Color.Gray))
            {
                MusicManager.stop();
                Game1.threadsExiting = true;
                Game1.getSingleton().Exit();
            }
        }

        private static void DrawLogin(DrawMainMenuEvent e)
        {

            GuiData.spriteBatch.DrawString(GuiData.font, "Enter your credentials.", new Vector2(180, 305), Color.White);
            GuiData.spriteBatch.DrawString(GuiData.smallfont, "Login : ", new Vector2(219, 380), Color.White);
            GuiData.spriteBatch.DrawString(GuiData.smallfont, "Password : ", new Vector2(190, 420), Color.White);

            GuiData.spriteBatch.DrawString(GuiData.smallfont, loginMessage, new Vector2(190, 575), Color.White);


            username = Hacknet.Gui.TextBox.doTextBox(16392802, 290, 380, 200, 1, username, GuiData.UISmallfont);
            Hacknet.Gui.TextBox.MaskingText = true;
            password = Hacknet.Gui.TextBox.doTextBox(16392803, 290, 420, 200, 1, password, GuiData.UISmallfont);
            Hacknet.Gui.TextBox.MaskingText = false;
            confirmLogIn.Draw();
            returnButton.Draw();
        }

        private static void ChangeState(MenuState state)
        {
            currentState = state;
        }

        async private static void StartGame()
        {
            
            UserScreen screen = new UserScreen();
            NetManager netManager = new NetManager(screen);
            netManager.Init();

            if(loginState == LoginState.UNAVAILABLE)
            {
                loginMessage = "The server is unavailable.";
                loginState = LoginState.MENU;
                return;
            }

            string hashedPassword = Hash(password);
            netManager.Login(username, hashedPassword);

            loginMessage = "Logging in...";

            loginState = LoginState.LOGGING_IN;

            for(int i = 0; i < 1000; i++)
            {
                await Task.Delay(10);
                if(loginState != LoginState.LOGGING_IN)
                    break;
            }
            if(loginState == LoginState.LOGGING_IN)
            {
                loginState = LoginState.MENU;
                loginMessage = "Login Timeout";
            }
            else if(loginState == LoginState.LOGGED)
            {
                loginMessage = "";
                currentState = MenuState.LOGIN;
                screen.netManager = netManager;
                screen.username = username;
                GuiData.hot = -1;
                baseMenu.ScreenManager.AddScreen(screen, new PlayerIndex?(baseMenu.ScreenManager.controllingPlayer));
            }
            else if(loginState == LoginState.INVALID)
            {
                loginMessage = "Invalid Username or Password.";
            }
            else if(loginState == LoginState.UNAVAILABLE)
            {
                loginMessage = "The server is unavailable.";
            }

            loginState = LoginState.MENU;
        }

        private static string Hash(string input)
        {
            var hash = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }
    }
}
