using Pathfinder.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet
{
    public class HackOnNetMod : Pathfinder.ModManager.IMod
    {
        public string Identifier => "HackOnNet";

        public void Load()
        {
            EventManager.RegisterListener<DrawMainMenuEvent>(GUI.MainMenu.DrawMainMenu);
            EventManager.RegisterListener<DrawMainMenuButtonsEvent>(GUI.MainMenu.DrawHackOnNetButton);
        }

        public void LoadContent()
        {
            
        }

        public void Unload()
        {
            
        }
    }
}
