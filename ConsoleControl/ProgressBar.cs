using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleControls
{
    public class CCProgressBar : ConsoleControl
    {
        private bool initalisated = false;

        internal char _fsc = '█';
        public char FullSectionCompleted { get { return _fsc; } set { _fsc = value; NeedModify = true; } }

        /*internal string _sh = "▉";
        public string SevenHeight { get { return _text; } set { _text = value; NeedModify = true; } }

        internal string _tq = "▊";
        public string ThreeQuarter { get { return _text; } set { _text = value; NeedModify = true; } }

        internal string _fh = "▋";
        public string FiveHeight { get { return _text; } set { _text = value; NeedModify = true; } }*/

        internal char _hb = '▌';
        public char HalfBlock { get { return _hb; } set { _hb = value; NeedModify = true; } }

        /*internal string _th = "▍";
        public string ThreeHeight { get { return _text; } set { _text = value; NeedModify = true; } }

        internal string _oq = "▎";
        public string OneQuarter { get { return _text; } set { _text = value; NeedModify = true; } }

        internal string _oh = "▏";
        public string OneHeight { get { return _text; } set { _text = value; NeedModify = true; } }*/

        private int _maxval;
        public int MaxValue { get { return _maxval; } set { _maxval = value; NeedModify = true; } }
        private float _val;
        public float Value { get { return _val; } set { 
                float i = value;
                if (i <= MaxValue)
                    _val = i;
                NeedModify = true; 
            } }
        private int _steps;
        public int Steps { get { return _steps; } set { _steps = value; NeedModify = true; } } //20 steps to go to 100 (number of char of the pb)(each block is 5)

        public CCProgressBar()
        {
            Name = "ProgressBar";
            Border = BorderStyle.FullBorder;
            TopLeftBorder = "╔";
            TopRightBorder = "╗";
            BottomLeftBorder = "╚";
            BottomRightBorder = "╝";
            LeftSideBorder = "║";
            RightSideBorder = "║";
            BottomSideBorder = "═";
            TopSideBorder = "═";

            ForeColor = ConsoleColor.Green;
            OnHoverBackColor = BackColor;
            OnHoverBorderColor = BorderColor;
            OnHoverForeColor = ForeColor;

            MaxValue = 100;
            Value = 0;
            Steps = 20;

            initalisated = true;
            ModifyScheme(Text);
        }

        public override void ModifyScheme(string text)
        {
            if (!initalisated)
                return;

            int blocksize = MaxValue / Steps;
            int hbtodraw = (int)Math.Round(Value * 2) / blocksize;
            int btodraw = (hbtodraw - (hbtodraw % 2)) / 2;
            bool drawhaflblock = (hbtodraw % 2) == 1;

            if (Border == BorderStyle.NoBorder)
            {
                DrawScheme = new CharInfoList[1];
                string s = new string(FullSectionCompleted, btodraw);
                if (drawhaflblock)
                    s += HalfBlock;
                s += new string(' ', Steps - (btodraw + (hbtodraw % 2)));
                DrawScheme[0] = new CharInfoList(Steps,s,Priority,ForeColor,BackColor);
                Witdth = Steps;
                Height = 1;
            }
            if(Border == BorderStyle.OnlySides)
            {
                DrawScheme = new CharInfoList[1];
                string s = new string(FullSectionCompleted, btodraw);
                if (drawhaflblock)
                    s += HalfBlock;
                s += new string(' ', Steps - (btodraw + (hbtodraw % 2)));
                DrawScheme[0] = new CharInfoList(Steps + 2, LeftSideBorder + s + RightSideBorder, Priority, ForeColor, BackColor);
                DrawScheme[0].CIList[0] = new CharInfo(LeftSideBorder.ToCharArray()[0], Priority, BackColor, BorderColor, true);
                DrawScheme[0].CIList[DrawScheme[0].CIList.Length - 1] = new CharInfo(RightSideBorder.ToCharArray()[0], Priority, BackColor, BorderColor, true);
                Witdth = Steps + 2;
                Height = 1;
            }
            if(Border == BorderStyle.FullBorder)
            {
                DrawScheme = new CharInfoList[3];
                string s = new string(FullSectionCompleted, btodraw);
                if (drawhaflblock)
                    s += HalfBlock;
                s += new string(' ', Steps - (btodraw + (hbtodraw % 2)));
                //Top
                DrawScheme[0] = new CharInfoList(Steps + 2, TopLeftBorder + new string(TopSideBorder.ToCharArray()[0], Steps) + TopRightBorder, Priority, BorderColor, BackColor,true);
                //Middle
                DrawScheme[1] = new CharInfoList(Steps + 2, LeftSideBorder + s + RightSideBorder, Priority, ForeColor, BackColor);
                DrawScheme[1].CIList[0] = new CharInfo(LeftSideBorder.ToCharArray()[0], Priority, BackColor, BorderColor, true);
                DrawScheme[1].CIList[DrawScheme[1].CIList.Length - 1] = new CharInfo(RightSideBorder.ToCharArray()[0], Priority, BackColor, BorderColor, true);
                //Bottom
                DrawScheme[2] = new CharInfoList(Steps + 2, BottomLeftBorder + new string(BottomSideBorder.ToCharArray()[0], Steps) + BottomRightBorder, Priority, BorderColor, BackColor,true);

                Witdth = Steps + 2;
                Height = 3;
            }
        }
    }
}