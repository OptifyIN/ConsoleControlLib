using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleStatus
{
    public static class ConsoleStatus
    {
        static KeyStatus A { get; set; }

    }

    public class KeyStatus
    {
        bool Up { get; set; }
        bool Down { get; set; }
        bool Pressed { get; set; }
    }
}
