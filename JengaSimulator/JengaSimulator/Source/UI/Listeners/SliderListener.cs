using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JengaSimulator.Source.UI
{
    public interface SliderListener
    {
        void onSlide(String sliderName, float slideRatio);
    }
}
