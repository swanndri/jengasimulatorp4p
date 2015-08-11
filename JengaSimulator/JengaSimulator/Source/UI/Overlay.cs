using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Surface;
using Microsoft.Surface.Core;

namespace JengaSimulator.Source.UI
{
    public class Overlay : DrawableGameComponent
    {
        SpriteBatch b;
        List<UIComponent> componentList;
        
        public Overlay(Game game)
            : base(game)
        {
            componentList = new List<UIComponent>();
            b = new SpriteBatch(Game.GraphicsDevice);
        }

        public void addUIComponent(UIComponent component) {
            componentList.Add(component);
        }

        public Boolean checkHitUI(TouchPoint p) {
            foreach (UIComponent c in componentList)
            {
                c.processTouchPoint(p);
            }
            return false;
        }

        public override void Draw(GameTime gameTime )
        {
            foreach (UIComponent c in componentList){
                c.draw(Game.GraphicsDevice, b);
            }
        }

    }	
}
