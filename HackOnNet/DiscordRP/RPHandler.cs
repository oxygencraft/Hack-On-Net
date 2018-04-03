using System;

namespace HackOnNet.DiscordRP {
    class RPHandler {
        public static DiscordRpc.RichPresence presence = new DiscordRpc.RichPresence();
        static DiscordRpc.EventHandlers handlers;

        const string clientId = "428716603719417867";

        public enum State { OGMenu, MPLogIn, MainMenu, InGame, Error, Start };
        public static State currentState = State.Start;

        public static bool isEnabled = false;

        public static void Initialize() {
            handlers = new DiscordRpc.EventHandlers();
            DiscordRpc.Initialize(clientId, ref handlers, true, null);
            isEnabled = true;
        }
        public static void UpdatePresence(string details, string largeImageKey) {
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

        public static void PresencePresetSet(State e) {
            switch (e) {
                case State.OGMenu:
                    if (currentState != State.OGMenu && isEnabled) {
                        presence.details = "OG Hacknet";
                        presence.largeImageKey = "oghn";
                        DiscordRpc.UpdatePresence(presence);
                        currentState = State.OGMenu;
                        break;
                    } else break;
                case State.MainMenu:
                    if (currentState != State.MainMenu && isEnabled) {
                        presence.details = "Main Menu";
                        presence.largeImageKey = "logo";
                        DiscordRpc.UpdatePresence(presence);
                        currentState = State.MainMenu;
                        break;
                    } else break;
                case State.MPLogIn:
                    if (currentState != State.MPLogIn && isEnabled) {
                        presence.details = "Logging in";
                        presence.largeImageKey = "logo";
                        DiscordRpc.UpdatePresence(presence);
                        currentState = State.MPLogIn;
                        break;
                    } else break;
                case State.InGame:
                    if (currentState != State.InGame && isEnabled) {
                        presence.details = "In Game";
                        presence.largeImageKey = "logo";
                        DiscordRpc.UpdatePresence(presence);
                        currentState = State.InGame;
                        break;
                    } else break;
                default:
                    Console.Error.WriteLine("Case for e was not found, e = " + e);
                    Console.WriteLine("How the hell did you get here, go back and fix your shit dumbass");
                    if (currentState != State.Error && isEnabled) {
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
