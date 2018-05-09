using System;
using System.IO;

namespace HackOnNet.DiscordRP {
    class RPHandler {
        private DiscordRpc.RichPresence presence = new DiscordRpc.RichPresence();
        DiscordRpc.EventHandlers handlers;

        private const string clientId = "428716603719417867";

        public enum State { OGMenu, MPLogIn, MainMenu, InGame, Error, Start };
        private State currentState = State.Start;

        private bool isEnabled = false;
        private bool firstRun = true;

        private void Initialize() {
            handlers = new DiscordRpc.EventHandlers();
            DiscordRpc.Initialize(clientId, ref handlers, true, null);
            isEnabled = true;
        }
        public void UpdatePresence(string details, string largeImageKey) {
            if (isEnabled) {
                presence.details = details;
                presence.largeImageKey = largeImageKey;

                DiscordRpc.UpdatePresence(presence);
            }
        }
        public static void Shutdown() {
            DiscordRpc.ClearPresence();
            DiscordRpc.Shutdown();
        }

        public void PresencePresetSet(State e) {
            if(firstRun) {
                string DLLLocation = Environment.CurrentDirectory + @"\Mods\discord-rpc.dll";
                DLLLocation = DLLLocation.Replace(@"\\", @"\");
                if (File.Exists(DLLLocation)) Initialize();
                firstRun = false;
            }
            if(isEnabled) {
                switch (e) {
                    case State.OGMenu:
                        if (currentState != State.OGMenu) {
                            presence.details = "OG Hacknet";
                            presence.largeImageKey = "oghn";
                            DiscordRpc.UpdatePresence(presence);
                            currentState = State.OGMenu;
                            break;
                        } else break;
                    case State.MainMenu:
                        if (currentState != State.MainMenu) {
                            presence.details = "Main Menu";
                            presence.largeImageKey = "logo";
                            DiscordRpc.UpdatePresence(presence);
                            currentState = State.MainMenu;
                            break;
                        } else break;
                    case State.MPLogIn:
                        if (currentState != State.MPLogIn) {
                            presence.details = "Logging in";
                            presence.largeImageKey = "logo";
                            DiscordRpc.UpdatePresence(presence);
                            currentState = State.MPLogIn;
                            break;
                        } else break;
                    case State.InGame:
                        if (currentState != State.InGame) {
                            presence.details = "In Game";
                            presence.largeImageKey = "logo";
                            DiscordRpc.UpdatePresence(presence);
                            currentState = State.InGame;
                            break;
                        } else break;
                    default:
                        Console.Error.WriteLine("Case for e was not found, e = " + e);
                        Console.WriteLine("How the hell did you get here, go back and fix your shit dumbass");
                        if (currentState != State.Error) {
                            presence.details = "Error";
                            presence.largeImageKey = "logo";
                            DiscordRpc.UpdatePresence(presence);
                            currentState = State.Error;
                            break;
                        } else break;
                }
            }

        }
    }
}
