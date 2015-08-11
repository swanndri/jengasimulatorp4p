using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JengaSimulator.Source.UI
{
    public interface ButtonListener
    {
        void onButtonDown(String buttonName);
    }
}