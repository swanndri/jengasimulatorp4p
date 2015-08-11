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
    public class Button : UIComponent
    {
        private Texture2D defaultTexture;
        private Boolean buttonDown;

        private List<ButtonListener> listeners;

        public Button(Texture2D defaultTexture, Rectangle componentArea, String componentName) :
            base(componentArea, componentName)
        {
            this.defaultTexture = defaultTexture;

            //Initialization
            this.buttonDown = false;
            this.listeners = new List<ButtonListener>();
        }

        public void addButtonListener(ButtonListener buttonListener) {
            listeners.Add(buttonListener);
        }

        public override bool processTouchPoint(TouchPoint p)
        {
            if (p == null)
            {
                buttonDown = false;
                return false;
            }
            if (componentArea.Contains(new Point((int)p.CenterX, (int)p.CenterY)))
            {
                if (buttonDown == false)
                {
                    foreach (ButtonListener bl in listeners)
                    {
                        bl.onButtonDown(componentName);
                    }
                }
                buttonDown = true;
                return true;
            }            
            return false;
        }

        public override void draw(GraphicsDevice g, SpriteBatch b)
        {
            b.Begin();            
            b.Draw(defaultTexture, componentArea, Color.White);
            b.End();
                        
            g.BlendState = BlendState.Opaque;
            g.DepthStencilState = DepthStencilState.Default;
        }
    }
}
