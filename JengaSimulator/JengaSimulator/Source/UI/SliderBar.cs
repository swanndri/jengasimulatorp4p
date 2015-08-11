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
    public class SliderBar : UIComponent
    {
        private Texture2D defaultTexture, sliderTexture;        
        private Boolean verticalScroller;

        private Rectangle indicatorArea;
        private List<SliderListener> listeners;
        private float slideRatio;
        
        public SliderBar(Texture2D defaultTexture, Texture2D sliderTexture, Rectangle componentArea, String componentName, Boolean verticalScroller): 
            base(componentArea, componentName) 
        {
            this.defaultTexture = defaultTexture;
            this.sliderTexture = sliderTexture;
            this.verticalScroller = verticalScroller;

            //Initializations
            this.listeners = new List<SliderListener>();
            this.slideRatio = 0;
            this.indicatorArea = verticalScroller ?
                new Rectangle(componentArea.X, componentArea.Y - (defaultTexture.Width / 2), defaultTexture.Width, defaultTexture.Width) :
                new Rectangle(componentArea.X - (defaultTexture.Height / 2), componentArea.Y, defaultTexture.Height, defaultTexture.Height);

            updateListeners();
        }

        public void addSliderListener(SliderListener sliderListener)
        {
            listeners.Add(sliderListener);
        }

        public override bool processTouchPoint(TouchPoint p)
        {
            if (p != null)
            {
                if (componentArea.Contains(new Point((int)p.X, (int)p.Y)))
                {
                    if (verticalScroller)
                    {
                        float dFromTop;
                        dFromTop = (float)(p.Y - componentArea.Y);
                        slideRatio = dFromTop / componentArea.Height;

                        indicatorArea = new Rectangle(componentArea.X, (int)(slideRatio * componentArea.Height) + componentArea.Y - (defaultTexture.Width /2) , defaultTexture.Width, defaultTexture.Width);
                    }
                    else {
                        float dFromLeft;
                        dFromLeft = (float)(p.X - componentArea.X);
                        slideRatio = dFromLeft / componentArea.Width;
                        
                        indicatorArea = new Rectangle((int)(slideRatio * componentArea.Width) + componentArea.X - (defaultTexture.Height / 2), componentArea.Y, defaultTexture.Height, defaultTexture.Height);                    
                    }
                    updateListeners();
                    return true;
                }
            }
            return false;
        }

        private void updateListeners() {
            foreach (SliderListener sl in listeners)
            {
                sl.onSlide(this.componentName, slideRatio);
            }
        }

        public override void draw(GraphicsDevice g, SpriteBatch b)
        {
            b.Begin();            
            b.Draw(defaultTexture, componentArea, Color.White);
            b.Draw(sliderTexture, indicatorArea, Color.White);
            b.End();
            
            
            g.BlendState = BlendState.Opaque;
            g.DepthStencilState = DepthStencilState.Default;
        }
    }
}