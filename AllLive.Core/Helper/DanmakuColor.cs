using System;
using System.Collections.Generic;
using System.Text;

namespace AllLive.Core.Helper
{
    public class DanmakuColor
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; } = 255;
       
        public DanmakuColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
        public DanmakuColor(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        public DanmakuColor(string color)
        {
            if (color.StartsWith("#"))
            {
                color = color.Substring(1);
            }
            if (color.Length == 6)
            {
                R = Convert.ToByte(color.Substring(0, 2), 16);
                G = Convert.ToByte(color.Substring(2, 2), 16);
                B = Convert.ToByte(color.Substring(4, 2), 16);
            }
            else if (color.Length == 8)
            {
                R = Convert.ToByte(color.Substring(0, 2), 16);
                G = Convert.ToByte(color.Substring(2, 2), 16);
                B = Convert.ToByte(color.Substring(4, 2), 16);
                A = Convert.ToByte(color.Substring(6, 2), 16);
            }
        }

        public DanmakuColor(int decColor)
        {
            var color = decColor.ToString("X2");
            if (color.Length == 4)
            {
                color = "00" + color;
            }
            if (color.Length == 6)
            {
                R = Convert.ToByte(color.Substring(0, 2), 16);
                G = Convert.ToByte(color.Substring(2, 2), 16);
                B = Convert.ToByte(color.Substring(4, 2), 16);
            }
            else if (color.Length == 8)
            {
                R = Convert.ToByte(color.Substring(0, 2), 16);
                G = Convert.ToByte(color.Substring(2, 2), 16);
                B = Convert.ToByte(color.Substring(4, 2), 16);
                A = Convert.ToByte(color.Substring(6, 2), 16);
            }

        }

        public override string ToString()
        {
            if (A == 255)
            {
                return $"#{R:X2}{G:X2}{B:X2}";
            }
            else
            {
                return $"#{R:X2}{G:X2}{B:X2}{A:X2}";
            }
        }
       
        public static DanmakuColor White { get; } = new DanmakuColor(255, 255, 255);
        public static DanmakuColor Black { get; } = new DanmakuColor(0, 0, 0);
        public static DanmakuColor Red { get; } = new DanmakuColor(255, 0, 0);
        public static DanmakuColor Green { get; } = new DanmakuColor(0, 255, 0);
        public static DanmakuColor Blue { get; } = new DanmakuColor(0, 0, 255);

        public static DanmakuColor FromArgb(byte a, byte r, byte g, byte b)
        {
            return new DanmakuColor(r, g, b, a);
        }

        public static DanmakuColor FromRgb(byte r, byte g, byte b)
        {
            return new DanmakuColor(r, g, b);
        }

    }
}
