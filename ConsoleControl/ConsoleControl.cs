using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleControls
{
    public abstract class ConsoleControl //Base class for all controls
    {
        //Properties
        internal string _text = "Control";
        public string Text { get { return _text; } set { _text = value; NeedModify = true; } }
        public string Name { get; set; } = "ConsoleControl1";
        public int Left { get; set; } = 0;
        public int Top { get; set; } = 0;
        public int Witdth { get; set; } = 0;
        public int Height { get; set; } = 0;
        public object Tag { get; set; } = null;
        public ConsoleColor BackColor { get; set; } = ConsoleColor.Black;
        public ConsoleColor ForeColor { get; set; } = ConsoleColor.White;
        public ConsoleColor BorderColor { get; set; } = ConsoleColor.White;
        public ConsoleColor OnHoverBackColor { get; set; } = ConsoleColor.White;
        public ConsoleColor OnHoverForeColor { get; set; } = ConsoleColor.Black;
        public ConsoleColor OnHoverBorderColor { get; set; } = ConsoleColor.Black;
        internal bool hovering = false;
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public ConsoleColor DisabledColor { get; set; } = ConsoleColor.Gray; 
        public int Priority { get; set; } = 0; //A control with a priority of 0 will be drawn on top of all. A priority of 2 mean that it will be under the 0 and 1 but over the rest

        internal BorderStyle _border;
        public BorderStyle Border { get { return _border; } set { _border = value; NeedModify = true; } }
        public bool NeedModify = false;
        public CharInfoList[] DrawScheme { get; set; } //Ex [0] : ╔════╗    ┌────┐    
                                                       //   [1] : ║Text║ or │Text│ or [Text] or Text. Note that the first two are 6*3 and the third is 6*1 and the last is 4*1
                                                       //   [2] : ╚════╝    └────┘    
                                                       //Do not set this, the control will do it itself
        #region BorderChar
        private string _tlb;
        public string TopLeftBorder { get { return _tlb; } set { _tlb = value; NeedModify = true; } }

        private string _trb;
        public string TopRightBorder { get { return _trb; } set { _trb = value; NeedModify = true; } }

        private string _blb;
        public string BottomLeftBorder { get { return _blb; } set { _blb = value; NeedModify = true; } }

        private string _brb;
        public string BottomRightBorder { get { return _brb; } set { _brb = value; NeedModify = true; } }

        private string _lsb;
        public string LeftSideBorder { get { return _lsb; } set { _lsb = value; NeedModify = true; } }

        private string _rsb;
        public string RightSideBorder { get { return _rsb; } set { _rsb = value; NeedModify = true; } }

        private string _bs;
        public string BottomSideBorder { get { return _bs; } set { _bs = value; NeedModify = true; } }

        private string _us;
        public string TopSideBorder { get { return _us; } set { _us = value; NeedModify = true; } }
        #endregion


        //Events
        public event EventHandler Click;
        public void ClickInvoke(object sender)
        {
            if(((ConsoleControl)sender).Enabled)
                Click?.Invoke(sender, EventArgs.Empty);
        }

        public event EventHandler MouseEnter;
        public void MouseEnterInvoke(object sender)
        {
            if (((ConsoleControl)sender).Enabled)
            {
                hovering = true;
                MouseEnter?.Invoke(sender, EventArgs.Empty);
            }
        }

        public event EventHandler MouseLeave;
        public void MouseLeaveInvoke(object sender)
        {
            if (((ConsoleControl)sender).Enabled)
            {
                hovering = false;
                MouseLeave?.Invoke(sender, EventArgs.Empty);
            }
        }

        //Methods
        public abstract void ModifyScheme(string text);
    }
    public enum BorderStyle
    {
        NoBorder = 0,
        FullBorder = 1,
        OnlySides = 2
    }
}
