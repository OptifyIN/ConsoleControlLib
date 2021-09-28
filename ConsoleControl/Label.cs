using System;
using System.Collections.Generic;

namespace ConsoleControls
{
    public class CCLabel : ConsoleControl
    {
        private bool initalisated = false;
        /// <summary>
        /// Work only with Border.FullBorder. Will Not count the border of the label (eg : ForcedWidth = 10 mean a 12 Width label)
        /// </summary>
        public int ForcedWitdth { get; set; } = -1;
        /// <summary>
        ///  Work only with Border.FullBorder. Will Not count the border of the label (eg : ForcedHeight = 2 mean a 4 Height label)
        /// </summary>
        public int ForcedHeight { get; set; } = -1;

        public CCLabel()
        {
            Name = "Label";
            Text = "Label";
            Border = BorderStyle.NoBorder;
            TopLeftBorder = "╔";
            TopRightBorder = "╗";
            BottomLeftBorder = "╚";
            BottomRightBorder = "╝";
            LeftSideBorder = "║";
            RightSideBorder = "║";
            BottomSideBorder = "═";
            TopSideBorder = "═";

            OnHoverBackColor = BackColor;
            OnHoverBorderColor = BorderColor;
            OnHoverForeColor = ForeColor;

            initalisated = true;
            ModifyScheme(Text);
        }

        public override void ModifyScheme(string text)
        {
            if (!initalisated)
                return;

            initalisated = false;
            Text = text;
            initalisated = true;

            string[] split = text.Split(Environment.NewLine);
            int textLines = split.Length;

            if (Border == BorderStyle.FullBorder)
            {
                int maxchar = 0;
                DrawScheme = new CharInfoList[textLines + 2];

                foreach (string s in split)
                {
                    if (s.Length > maxchar)
                        maxchar = s.Length;
                }

                if (ForcedWitdth >= 0)
                {
                    maxchar = ForcedWitdth;
                }

                for (int i = 0; i < split.Length; i++)
                {
                    string s = split[i];
                    DrawScheme[i + 1] = new CharInfoList(maxchar + 2);

                    DrawScheme[i + 1].Add(new CharInfo(' ', 0, ConsoleColor.Black, ConsoleColor.Black));

                    foreach (char c in s)
                        DrawScheme[i + 1].Add(new CharInfo(c, Priority, BackColor, ForeColor));

                    DrawScheme[i + 1].FillBlanks();
                }
                for (int i = 1; i < DrawScheme.Length - 1; i++)
                {
                    DrawScheme[i].CIList[0] = new CharInfo(LeftSideBorder.ToCharArray()[0], Priority, BackColor, BorderColor, true);
                    DrawScheme[i].CIList[DrawScheme[i].CIList.Length - 1] = new CharInfo(RightSideBorder.ToCharArray()[0], Priority, BackColor, BorderColor, true);
                }

                DrawScheme[0] = new CharInfoList(maxchar + 2, TopLeftBorder + new string(TopSideBorder.ToCharArray()[0], maxchar) + TopRightBorder, Priority, BorderColor, BackColor, true);

                DrawScheme[DrawScheme.Length - 1] = new CharInfoList(maxchar + 2, BottomLeftBorder + new string(BottomSideBorder.ToCharArray()[0], maxchar) + BottomRightBorder, Priority, BorderColor, BackColor, true);

                if (ForcedHeight > 0)
                {
                    CharInfoList top = DrawScheme[0];
                    CharInfoList bottom = DrawScheme[DrawScheme.Length - 1];
                    List<CharInfoList> list = new List<CharInfoList>();
                    for (int i = 1; i < DrawScheme.Length - 1; i++)
                    {
                        list.Add(DrawScheme[i]);
                    }
                    if (list.Count < ForcedHeight)
                        goto suitefh;
                    DrawScheme = new CharInfoList[ForcedHeight + 2];
                    for (int i = 0; i < ForcedHeight; i++)
                    {
                        DrawScheme[i + 1] = list[i];
                    }
                    DrawScheme[0] = top;
                    DrawScheme[DrawScheme.Length - 1] = bottom;
                }
            suitefh:;

                Height = DrawScheme.Length;
                Witdth = maxchar + 2;
            }
            if (Border == BorderStyle.NoBorder)
            {
                DrawScheme = new CharInfoList[textLines];
                for (int i = 0; i < split.Length; i++)
                {
                    string s = split[i];
                    DrawScheme[i] = new CharInfoList(s.Length, s, Priority, ForeColor, BackColor);
                }

                int maxchar = 0;
                for (int i = 0; i < split.Length; i++)
                {
                    string s = split[i];
                    if (s.Length > maxchar)
                        maxchar = s.Length;
                }
                Witdth = maxchar;
                Height = DrawScheme.Length;
            }
            if (Border == BorderStyle.OnlySides)
            {
                DrawScheme = new CharInfoList[textLines];

                int maxchar = 0;
                for (int i = 0; i < split.Length; i++)
                {
                    string s = split[i];
                    if (s.Length > maxchar)
                        maxchar = s.Length;
                }

                for (int i = 0; i < split.Length; i++)
                {
                    string s = split[i];
                    DrawScheme[i] = new CharInfoList(maxchar, LeftSideBorder + s + RightSideBorder, Priority, ForeColor, BackColor);
                    DrawScheme[i].CIList[0] = new CharInfo(LeftSideBorder.ToCharArray()[0], Priority, BackColor, BorderColor, true);
                    DrawScheme[i].CIList[DrawScheme[i].CIList.Length - 1] = new CharInfo(RightSideBorder.ToCharArray()[0], Priority, BackColor, BorderColor, true);
                }


                Witdth = maxchar + 2;
                Height = DrawScheme.Length;
            }
        }
    }
}
