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
    class Overlay
    {
        protected SpriteBatch spriteBatch;
        protected UserScreen screen;

        protected Rectangle displayRect;

        public Overlay(SpriteBatch batch, Rectangle displayRect, UserScreen userScreen)
        {
            spriteBatch = batch;
            screen = userScreen;
            this.displayRect = displayRect;
        }

        public virtual void Launch()
        {

        }

        public virtual bool PreventsDrawing()
        {
            return false;
        }

        public virtual bool IsOnTop()
        {
            return true;
        }

        public virtual void Update(double dT)
        {

        }

        public virtual void Draw()
        {

        }
    }
}
