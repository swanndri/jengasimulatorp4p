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
    public abstract class UIComponent
    {
        protected Rectangle componentArea;
        protected String componentName;

        public UIComponent(Rectangle componentArea, String componentName)
        {
            this.componentArea = componentArea;
            this.componentName = componentName;
        }

        //Returns true if the touch point hits the UI component.
        abstract public Boolean processTouchPoint(TouchPoint p);

        abstract public void draw(GraphicsDevice g, SpriteBatch b);
    }
}
