using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.DiscordRP {
    class RPHandler {
        public static DiscordRpc.RichPresence presence = new DiscordRpc.RichPresence();
        static DiscordRpc.EventHandlers handlers;

        const string clientId = "428716603719417867";

        public static void Initialize() {
            handlers = new DiscordRpc.EventHandlers();
            DiscordRpc.Initialize(clientId, ref handlers, true, null);
        }
        public static void UpdatePresence(string details, string largeImageKey) {
            presence.details = details;
            presence.largeImageKey = largeImageKey;

            DiscordRpc.UpdatePresence(presence);
        } 
        public static void Shutdown() {
            DiscordRpc.ClearPresence();
            DiscordRpc.Shutdown();
        }
    }
}
