using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static ConsoleLib.NativeMethods;

namespace ConsoleControls
{
    public class ConsoleUI
    {
        private bool _continue = false;
        private Thread DrawThread;

        public int DrawTime = 0;

        public List<ConsoleControl> Controls { get; set; } = new List<ConsoleControl>();

        public bool ForceModify = false;

        public ConsoleColor BackColor { get; set; } = ConsoleColor.Black;

        private ConsoleControl[,] CharToControl = new ConsoleControl[Console.WindowWidth, Console.WindowHeight];

        //Events
        public event EventHandler ConsoleUICreated;
        public event EventHandler ConsoleUIReseted;
        public event EventHandler ConsoleUIDestroyed;
        public event EventHandler BeforeDraw;
        public event EventHandler AfterDraw;

        public void ReadyConsole()
        {
            _continue = true;
            Console.CursorVisible = false;
            Console.BufferHeight = Console.WindowHeight;
            ConsoleWindow.QuickEditMode(false);
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;

            IntPtr inHandle = GetStdHandle(STD_INPUT_HANDLE);
            uint mode = 0;
            GetConsoleMode(inHandle, ref mode);
            mode |= ENABLE_MOUSE_INPUT; //enable
            SetConsoleMode(inHandle, mode);
            ConsoleLib.ConsoleListener.Start();
            ConsoleLib.ConsoleListener.MouseEvent += ConsoleListener_MouseEvent;

            DrawThread = new Thread(Draw);
            DrawThread.Start();
            //Call created event
            ConsoleUICreated?.Invoke(this, EventArgs.Empty);
        }

        int mouseX = 0;
        public int MouseX { get { return mouseX; } }
        int mouseY = 0;
        public int MouseY { get { return mouseY; } }
        bool leftpressed = false;
        public bool MouseLeftPressed { get { return leftpressed; } }
        private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
        {
            mouseX = r.dwMousePosition.X;
            mouseY = r.dwMousePosition.Y;
            leftpressed = r.dwButtonState == 0x001;
        }

        public void ResetConsole()
        {
            _continue = false;
            DrawThread.Interrupt();
            //reset code
            _continue = true;
            DrawThread.Start();
            //Call reset event
            ConsoleUIReseted?.Invoke(this, EventArgs.Empty);
        }

        public void ReturnToNormalConsole()
        {
            _continue = false;
            Console.CursorVisible = true;
            Console.BufferHeight = 5001;
            ConsoleWindow.QuickEditMode(true);
            ConsoleLib.ConsoleListener.Stop();
            ConsoleLib.ConsoleListener.MouseEvent -= ConsoleListener_MouseEvent;

            DrawThread.Interrupt();
            //Call destroy event
            ConsoleUIDestroyed?.Invoke(this, EventArgs.Empty);
        }

        ////////
        private CharInfo[,] ciprev = new CharInfo[Console.WindowWidth, Console.WindowHeight];
        private ConsoleControl mousehover = null;
        private ConsoleControl lastcclp = null;
        private bool lcUnpressed = true;

        private void Draw()
        {
            while (_continue)
            {
                Stopwatch st = new Stopwatch();
                st.Start();

                CharInfo[,] ci = new CharInfo[Console.WindowWidth, Console.WindowHeight];

                //Call before draw event
                BeforeDraw?.Invoke(this, EventArgs.Empty);
                //Modify controls
                foreach (ConsoleControl cc in Controls)
                {
                    if (ForceModify)
                    {
                        cc.ModifyScheme(cc.Text);
                        continue;
                    }
                    if (cc.NeedModify)
                    {
                        cc.ModifyScheme(cc.Text);
                        cc.NeedModify = false;
                    }
                }
                //Register chars
                foreach (ConsoleControl cc in Controls)
                {
                    if (cc.Visible == false)
                        continue;
                    for (int y = 0; y < cc.DrawScheme.Length; y++)
                    {
                    prev:;
                        CharInfoList cil = cc.DrawScheme[y];
                        if (cil == null)
                            goto prev;
                        for (int x = 0; x < cil.CIList.Length; x++)
                        {
                            CharInfo c = cil.CIList[x];
                            if (cc.Enabled)
                            {
                                if (ci[cc.Left + x, cc.Top + y] == null)
                                {
                                    if (cc.hovering && !c.IsBorder)
                                        ci[cc.Left + x, cc.Top + y] = new CharInfo(c.Char, cc.Priority, cc.OnHoverBackColor, cc.OnHoverForeColor);
                                    else if (cc.hovering && c.IsBorder)
                                        ci[cc.Left + x, cc.Top + y] = new CharInfo(c.Char, cc.Priority, cc.OnHoverBackColor, cc.OnHoverBorderColor);
                                    else
                                        ci[cc.Left + x, cc.Top + y] = c;
                                    CharToControl[cc.Left + x, cc.Top + y] = cc;
                                }
                                else
                                {
                                    if (ci[cc.Left + x, cc.Top + y].Priority > cc.Priority)
                                    {
                                        if (cc.hovering && !c.IsBorder)
                                            ci[cc.Left + x, cc.Top + y] = new CharInfo(c.Char, cc.Priority, cc.OnHoverBackColor, cc.OnHoverForeColor);
                                        else if (cc.hovering && c.IsBorder)
                                            ci[cc.Left + x, cc.Top + y] = new CharInfo(c.Char, cc.Priority, cc.OnHoverBackColor, cc.OnHoverBorderColor);
                                        else
                                            ci[cc.Left + x, cc.Top + y] = c;
                                        CharToControl[cc.Left + x, cc.Top + y] = cc;
                                    }
                                }
                            }
                            else
                            {
                                if (ci[cc.Left + x, cc.Top + y] == null)
                                {
                                    ci[cc.Left + x, cc.Top + y] = new CharInfo(c.Char, Int32.MaxValue, cc.BackColor, cc.DisabledColor);
                                    CharToControl[cc.Left + x, cc.Top + y] = cc;
                                }
                            }
                        }
                    }
                }

                //trigger onhover/click events
                ConsoleControl cco = CharToControl[mouseX, mouseY];
                if (cco != null)
                {
                    cco.MouseEnterInvoke(cco);

                    if (leftpressed  && lcUnpressed)
                    {
                        cco.ClickInvoke(cco);
                        lastcclp = cco;
                        lcUnpressed = false;
                    }
                    else if(!leftpressed && !lcUnpressed)
                    {
                        lcUnpressed = true;
                    }
                    
                    if(mousehover != null && cco != mousehover)
                    {
                        mousehover.MouseLeaveInvoke(mousehover);
                    }
                    mousehover = cco;
                    ForceModify = true;
                }
                else
                {
                    if (mousehover != null)
                    {
                        mousehover.MouseLeaveInvoke(mousehover);
                        mousehover = null;
                        ForceModify = true;
                    }
                    if (lastcclp != null)
                        lastcclp = null;
                }

                //DisplayChar
                for (int x = 0; x < Console.WindowWidth; x++)
                    for (int y = 0; y < Console.WindowHeight; y++)
                    {

                       if (ci[x, y] == null && ciprev[x, y] != null)
                        {
                            Console.SetCursorPosition(x, y);
                            Console.BackgroundColor = BackColor;
                            Console.Write(" ");
                            continue;
                        }
                        if (ci[x, y] == null)
                            continue;

                        if (ciprev[x, y] != null)
                            if (ci[x, y].Char == ciprev[x, y].Char && ForceModify == false && ci[x, y].BackColor == ciprev[x, y].BackColor && ci[x, y].ForeColor == ciprev[x, y].ForeColor)
                                continue;

                        Console.SetCursorPosition(x, y);
                        Console.ForegroundColor = ci[x, y].ForeColor;
                        Console.BackgroundColor = ci[x, y].BackColor;
                        Console.Write(ci[x, y].Char);
                    }

                //Call after draw event
                AfterDraw?.Invoke(this, EventArgs.Empty);

                ForceModify = false;
                ciprev = ci;
                st.Stop();
                DrawTime = (int)st.ElapsedMilliseconds;
            }
        }
    }

    public class CharInfo
    {
        public char Char = ' ';
        public int Priority = 0;
        public ConsoleColor BackColor = ConsoleColor.Black;
        public ConsoleColor ForeColor = ConsoleColor.White;
        public bool IsBorder;

        public CharInfo(char c, int p, ConsoleColor b, ConsoleColor f, bool isborder = false)
        {
            Char = c;
            Priority = p;
            BackColor = b;
            ForeColor = f;
            IsBorder = isborder;
        }
    }

    public class CharInfoList
    {
        public CharInfo[] CIList;
        public CharInfoList(int lenght) => CIList = new CharInfo[lenght];
        public CharInfoList(int lenght, string text, int priority, ConsoleColor Fore, ConsoleColor Back, bool isborder = false)
        {
            CIList = new CharInfo[lenght];
            foreach (char c in text)
            {
                Add(new CharInfo(c,priority,Back,Fore,isborder));
            }
        }
        public void Add(CharInfo ci)
        {
            for (int i = 0; i < CIList.Length; i++)
            {
                if (CIList[i] != null)
                    continue;
                else
                {
                    CIList[i] = ci;
                    break;
                }
            }
        }
        public void FillBlanks()
        {
            for (int i = 0; i < CIList.Length; i++)
            {
                if (CIList[i] != null)
                    continue;
                else
                {
                    CIList[i] = new CharInfo(' ', 0, ConsoleColor.Black, ConsoleColor.Black);
                }
            }
        }
    }

    public static class ConsoleWindow
    {
        private static class NativeFunctions
        {
            public enum StdHandle : int
            {
                STD_INPUT_HANDLE = -10,
                STD_OUTPUT_HANDLE = -11,
                STD_ERROR_HANDLE = -12,
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr GetStdHandle(int nStdHandle); //returns Handle

            public enum ConsoleMode : uint
            {
                ENABLE_ECHO_INPUT = 0x0004,
                ENABLE_EXTENDED_FLAGS = 0x0080,
                ENABLE_INSERT_MODE = 0x0020,
                ENABLE_LINE_INPUT = 0x0002,
                ENABLE_MOUSE_INPUT = 0x0010,
                ENABLE_PROCESSED_INPUT = 0x0001,
                ENABLE_QUICK_EDIT_MODE = 0x0040,
                ENABLE_WINDOW_INPUT = 0x0008,
                ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200,

                //screen buffer handle
                ENABLE_PROCESSED_OUTPUT = 0x0001,
                ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
                ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
                DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
                ENABLE_LVB_GRID_WORLDWIDE = 0x0010
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        }

        public static void QuickEditMode(bool Enable)
        {
            //QuickEdit lets the user select text in the console window with the mouse, to copy to the windows clipboard.
            //But selecting text stops the console process (e.g. unzipping). This may not be always wanted.
            IntPtr consoleHandle = NativeFunctions.GetStdHandle((int)NativeFunctions.StdHandle.STD_INPUT_HANDLE);
            UInt32 consoleMode;

            NativeFunctions.GetConsoleMode(consoleHandle, out consoleMode);
            if (Enable)
                consoleMode |= ((uint)NativeFunctions.ConsoleMode.ENABLE_QUICK_EDIT_MODE);
            else
                consoleMode &= ~((uint)NativeFunctions.ConsoleMode.ENABLE_QUICK_EDIT_MODE);

            consoleMode |= ((uint)NativeFunctions.ConsoleMode.ENABLE_EXTENDED_FLAGS);

            NativeFunctions.SetConsoleMode(consoleHandle, consoleMode);
        }
    }
}
namespace ConsoleLib
{
    public static class ConsoleListener
    {
        public static event ConsoleMouseEvent MouseEvent;

        public static event ConsoleKeyEvent KeyEvent;

        public static event ConsoleWindowBufferSizeEvent WindowBufferSizeEvent;

        private static bool Run = false;


        public static void Start()
        {
            if (!Run)
            {
                Run = true;
                IntPtr handleIn = GetStdHandle(STD_INPUT_HANDLE);
                new Thread(() =>
                {
                    while (true)
                    {
                        uint numRead = 0;
                        INPUT_RECORD[] record = new INPUT_RECORD[1];
                        record[0] = new INPUT_RECORD();
                        ReadConsoleInput(handleIn, record, 1, ref numRead);
                        if (Run)
                            switch (record[0].EventType)
                            {
                                case INPUT_RECORD.MOUSE_EVENT:
                                    MouseEvent?.Invoke(record[0].MouseEvent);
                                    break;
                                case INPUT_RECORD.KEY_EVENT:
                                    KeyEvent?.Invoke(record[0].KeyEvent);
                                    break;
                                case INPUT_RECORD.WINDOW_BUFFER_SIZE_EVENT:
                                    WindowBufferSizeEvent?.Invoke(record[0].WindowBufferSizeEvent);
                                    break;
                            }
                        else
                        {
                            uint numWritten = 0;
                            WriteConsoleInput(handleIn, record, 1, ref numWritten);
                            return;
                        }
                    }
                }).Start();
            }
        }

        public static void Stop() => Run = false;


        public delegate void ConsoleMouseEvent(MOUSE_EVENT_RECORD r);

        public delegate void ConsoleKeyEvent(KEY_EVENT_RECORD r);

        public delegate void ConsoleWindowBufferSizeEvent(WINDOW_BUFFER_SIZE_RECORD r);

    }


    public static class NativeMethods
    {
        public struct COORD
        {
            public short X;
            public short Y;

            public COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT_RECORD
        {
            public const ushort KEY_EVENT = 0x0001,
                MOUSE_EVENT = 0x0002,
                WINDOW_BUFFER_SIZE_EVENT = 0x0004; //more

            [FieldOffset(0)]
            public ushort EventType;
            [FieldOffset(4)]
            public KEY_EVENT_RECORD KeyEvent;
            [FieldOffset(4)]
            public MOUSE_EVENT_RECORD MouseEvent;
            [FieldOffset(4)]
            public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
            /*
            and:
             MENU_EVENT_RECORD MenuEvent;
             FOCUS_EVENT_RECORD FocusEvent;
             */
        }

        public struct MOUSE_EVENT_RECORD
        {
            public COORD dwMousePosition;

            public const uint FROM_LEFT_1ST_BUTTON_PRESSED = 0x0001,
                FROM_LEFT_2ND_BUTTON_PRESSED = 0x0004,
                FROM_LEFT_3RD_BUTTON_PRESSED = 0x0008,
                FROM_LEFT_4TH_BUTTON_PRESSED = 0x0010,
                RIGHTMOST_BUTTON_PRESSED = 0x0002;
            public uint dwButtonState;

            public const int CAPSLOCK_ON = 0x0080,
                ENHANCED_KEY = 0x0100,
                LEFT_ALT_PRESSED = 0x0002,
                LEFT_CTRL_PRESSED = 0x0008,
                NUMLOCK_ON = 0x0020,
                RIGHT_ALT_PRESSED = 0x0001,
                RIGHT_CTRL_PRESSED = 0x0004,
                SCROLLLOCK_ON = 0x0040,
                SHIFT_PRESSED = 0x0010;
            public uint dwControlKeyState;

            public const int DOUBLE_CLICK = 0x0002,
                MOUSE_HWHEELED = 0x0008,
                MOUSE_MOVED = 0x0001,
                MOUSE_WHEELED = 0x0004;
            public uint dwEventFlags;
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        public struct KEY_EVENT_RECORD
        {
            [FieldOffset(0)]
            public bool bKeyDown;
            [FieldOffset(4)]
            public ushort wRepeatCount;
            [FieldOffset(6)]
            public ushort wVirtualKeyCode;
            [FieldOffset(8)]
            public ushort wVirtualScanCode;
            [FieldOffset(10)]
            public char UnicodeChar;
            [FieldOffset(10)]
            public byte AsciiChar;

            public const int CAPSLOCK_ON = 0x0080,
                ENHANCED_KEY = 0x0100,
                LEFT_ALT_PRESSED = 0x0002,
                LEFT_CTRL_PRESSED = 0x0008,
                NUMLOCK_ON = 0x0020,
                RIGHT_ALT_PRESSED = 0x0001,
                RIGHT_CTRL_PRESSED = 0x0004,
                SCROLLLOCK_ON = 0x0040,
                SHIFT_PRESSED = 0x0010;
            [FieldOffset(12)]
            public uint dwControlKeyState;
        }

        public struct WINDOW_BUFFER_SIZE_RECORD
        {
            public COORD dwSize;
        }

        public const uint STD_INPUT_HANDLE = unchecked((uint)-10),
            STD_OUTPUT_HANDLE = unchecked((uint)-11),
            STD_ERROR_HANDLE = unchecked((uint)-12);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(uint nStdHandle);


        public const uint ENABLE_MOUSE_INPUT = 0x0010,
            ENABLE_QUICK_EDIT_MODE = 0x0040,
            ENABLE_EXTENDED_FLAGS = 0x0080,
            ENABLE_ECHO_INPUT = 0x0004,
            ENABLE_WINDOW_INPUT = 0x0008; //more

        [DllImportAttribute("kernel32.dll")]
        public static extern bool GetConsoleMode(IntPtr hConsoleInput, ref uint lpMode);

        [DllImportAttribute("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleInput, uint dwMode);


        [DllImportAttribute("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool ReadConsoleInput(IntPtr hConsoleInput, [Out] INPUT_RECORD[] lpBuffer, uint nLength, ref uint lpNumberOfEventsRead);

        [DllImportAttribute("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool WriteConsoleInput(IntPtr hConsoleInput, INPUT_RECORD[] lpBuffer, uint nLength, ref uint lpNumberOfEventsWritten);

    }
}
