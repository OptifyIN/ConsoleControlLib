using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleControls
{
    public class CCButton : CCLabel
    {
        public CCButton()
        {
            Name = "Button";
            Text = "Button";
            Border = BorderStyle.FullBorder;

            OnHoverBackColor = BackColor;
            OnHoverBorderColor = BorderColor;
            OnHoverForeColor = ConsoleColor.Red;

            ModifyScheme(Text);
        }
    }
}
