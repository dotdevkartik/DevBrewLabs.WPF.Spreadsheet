using System;
using System.Collections.Generic;

namespace DevBrewLabs.Spreadsheet.Drawing
{
    public struct DrawingColor : IEquatable<DrawingColor>
    {
        private static Dictionary<DrawingKnownColor, DrawingColor> _colorsCache;

        static DrawingColor()
        {
            _colorsCache = new Dictionary<DrawingKnownColor, DrawingColor>();
        }

        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public DrawingColor(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public static DrawingColor FromArgb(byte a, byte r, byte g, byte b)
        {
            return new DrawingColor(a, r, g, b);
        }

        public bool Equals(DrawingColor color2)
        {
            return this.A == color2.A && this.R == color2.R && this.G == color2.G && this.B == color2.B;
        }

        public static bool operator ==(DrawingColor color1, DrawingColor color2)
        {
            return color1.Equals(color2);
        }

        public static bool operator !=(DrawingColor color1, DrawingColor color2)
        {
            return !color1.Equals(color2);
        }

        public override bool Equals(object obj)
        {
            return obj is DrawingColor && Equals((DrawingColor)obj);
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();
        }

        #region Colors
        public static DrawingColor Transparent
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Transparent, out color))
                {
                    color = FromArgb(0, 255, 255, 255);
                    _colorsCache.Add(DrawingKnownColor.Transparent, color);
                }

                return color;
            }
        }

        public static DrawingColor AliceBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.AliceBlue, out color))
                {
                    color = FromArgb(255, 240, 248, 255);
                    _colorsCache.Add(DrawingKnownColor.AliceBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor AntiqueWhite
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.AntiqueWhite, out color))
                {
                    color = FromArgb(255, 250, 235, 215);
                    _colorsCache.Add(DrawingKnownColor.AntiqueWhite, color);
                }

                return color;
            }
        }

        public static DrawingColor Aqua
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Aqua, out color))
                {
                    color = FromArgb(255, 0, 255, 255);
                    _colorsCache.Add(DrawingKnownColor.Aqua, color);
                }

                return color;
            }
        }

        public static DrawingColor Aquamarine
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Aquamarine, out color))
                {
                    color = FromArgb(255, 127, 255, 212);
                    _colorsCache.Add(DrawingKnownColor.Aquamarine, color);
                }

                return color;
            }
        }

        public static DrawingColor Azure
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Azure, out color))
                {
                    color = FromArgb(255, 240, 255, 255);
                    _colorsCache.Add(DrawingKnownColor.Azure, color);
                }

                return color;
            }
        }

        public static DrawingColor Beige
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Beige, out color))
                {
                    color = FromArgb(255, 245, 245, 220);
                    _colorsCache.Add(DrawingKnownColor.Beige, color);
                }

                return color;
            }
        }

        public static DrawingColor Bisque
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Bisque, out color))
                {
                    color = FromArgb(255, 255, 228, 196);
                    _colorsCache.Add(DrawingKnownColor.Bisque, color);
                }

                return color;
            }
        }

        public static DrawingColor Black
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Black, out color))
                {
                    color = FromArgb(255, 0, 0, 0);
                    _colorsCache.Add(DrawingKnownColor.Black, color);
                }

                return color;
            }
        }

        public static DrawingColor BlanchedAlmond
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.BlanchedAlmond, out color))
                {
                    color = FromArgb(255, 255, 235, 205);
                    _colorsCache.Add(DrawingKnownColor.BlanchedAlmond, color);
                }

                return color;
            }
        }

        public static DrawingColor Blue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Blue, out color))
                {
                    color = FromArgb(255, 0, 0, 255);
                    _colorsCache.Add(DrawingKnownColor.Blue, color);
                }

                return color;
            }
        }

        public static DrawingColor BlueViolet
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.BlueViolet, out color))
                {
                    color = FromArgb(255, 138, 43, 226);
                    _colorsCache.Add(DrawingKnownColor.BlueViolet, color);
                }

                return color;
            }
        }

        public static DrawingColor Brown
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Brown, out color))
                {
                    color = FromArgb(255, 165, 42, 42);
                    _colorsCache.Add(DrawingKnownColor.Brown, color);
                }

                return color;
            }
        }

        public static DrawingColor BurlyWood
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.BurlyWood, out color))
                {
                    color = FromArgb(255, 222, 184, 135);
                    _colorsCache.Add(DrawingKnownColor.BurlyWood, color);
                }

                return color;
            }
        }

        public static DrawingColor CadetBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.CadetBlue, out color))
                {
                    color = FromArgb(255, 95, 158, 160);
                    _colorsCache.Add(DrawingKnownColor.CadetBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor Chartreuse
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Chartreuse, out color))
                {
                    color = FromArgb(255, 127, 255, 0);
                    _colorsCache.Add(DrawingKnownColor.Chartreuse, color);
                }

                return color;
            }
        }

        public static DrawingColor Chocolate
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Chocolate, out color))
                {
                    color = FromArgb(255, 210, 105, 30);
                    _colorsCache.Add(DrawingKnownColor.Chocolate, color);
                }

                return color;
            }
        }

        public static DrawingColor Coral
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Coral, out color))
                {
                    color = FromArgb(255, 255, 127, 80);
                    _colorsCache.Add(DrawingKnownColor.Coral, color);
                }

                return color;
            }
        }

        public static DrawingColor CornflowerBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.CornflowerBlue, out color))
                {
                    color = FromArgb(255, 100, 149, 237);
                    _colorsCache.Add(DrawingKnownColor.CornflowerBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor Cornsilk
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Cornsilk, out color))
                {
                    color = FromArgb(255, 255, 248, 220);
                    _colorsCache.Add(DrawingKnownColor.Cornsilk, color);
                }

                return color;
            }
        }

        public static DrawingColor Crimson
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Crimson, out color))
                {
                    color = FromArgb(255, 220, 20, 60);
                    _colorsCache.Add(DrawingKnownColor.Crimson, color);
                }

                return color;
            }
        }

        public static DrawingColor Cyan
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Cyan, out color))
                {
                    color = FromArgb(255, 0, 255, 255);
                    _colorsCache.Add(DrawingKnownColor.Cyan, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkBlue, out color))
                {
                    color = FromArgb(255, 0, 0, 139);
                    _colorsCache.Add(DrawingKnownColor.DarkBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkCyan
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkCyan, out color))
                {
                    color = FromArgb(255, 0, 139, 139);
                    _colorsCache.Add(DrawingKnownColor.DarkCyan, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkGoldenrod
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkGoldenrod, out color))
                {
                    color = FromArgb(255, 184, 134, 11);
                    _colorsCache.Add(DrawingKnownColor.DarkGoldenrod, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkGray
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkGray, out color))
                {
                    color = FromArgb(255, 169, 169, 169);
                    _colorsCache.Add(DrawingKnownColor.DarkGray, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkGreen, out color))
                {
                    color = FromArgb(255, 0, 100, 0);
                    _colorsCache.Add(DrawingKnownColor.DarkGreen, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkKhaki
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkKhaki, out color))
                {
                    color = FromArgb(255, 189, 183, 107);
                    _colorsCache.Add(DrawingKnownColor.DarkKhaki, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkMagenta
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkMagenta, out color))
                {
                    color = FromArgb(255, 139, 0, 139);
                    _colorsCache.Add(DrawingKnownColor.DarkMagenta, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkOliveGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkOliveGreen, out color))
                {
                    color = FromArgb(255, 85, 107, 47);
                    _colorsCache.Add(DrawingKnownColor.DarkOliveGreen, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkOrange
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkOrange, out color))
                {
                    color = FromArgb(255, 255, 140, 0);
                    _colorsCache.Add(DrawingKnownColor.DarkOrange, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkOrchid
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkOrchid, out color))
                {
                    color = FromArgb(255, 153, 50, 204);
                    _colorsCache.Add(DrawingKnownColor.DarkOrchid, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkRed
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkRed, out color))
                {
                    color = FromArgb(255, 139, 0, 0);
                    _colorsCache.Add(DrawingKnownColor.DarkRed, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkSalmon
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkSalmon, out color))
                {
                    color = FromArgb(255, 233, 150, 122);
                    _colorsCache.Add(DrawingKnownColor.DarkSalmon, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkSeaGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkSeaGreen, out color))
                {
                    color = FromArgb(255, 143, 188, 139);
                    _colorsCache.Add(DrawingKnownColor.DarkSeaGreen, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkSlateBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkSlateBlue, out color))
                {
                    color = FromArgb(255, 72, 61, 139);
                    _colorsCache.Add(DrawingKnownColor.DarkSlateBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkSlateGray
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkSlateGray, out color))
                {
                    color = FromArgb(255, 47, 79, 79);
                    _colorsCache.Add(DrawingKnownColor.DarkSlateGray, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkTurquoise
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkTurquoise, out color))
                {
                    color = FromArgb(255, 0, 206, 209);
                    _colorsCache.Add(DrawingKnownColor.DarkTurquoise, color);
                }

                return color;
            }
        }

        public static DrawingColor DarkViolet
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DarkViolet, out color))
                {
                    color = FromArgb(255, 148, 0, 211);
                    _colorsCache.Add(DrawingKnownColor.DarkViolet, color);
                }

                return color;
            }
        }

        public static DrawingColor DeepPink
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DeepPink, out color))
                {
                    color = FromArgb(255, 255, 20, 147);
                    _colorsCache.Add(DrawingKnownColor.DeepPink, color);
                }

                return color;
            }
        }

        public static DrawingColor DeepSkyBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DeepSkyBlue, out color))
                {
                    color = FromArgb(255, 0, 191, 255);
                    _colorsCache.Add(DrawingKnownColor.DeepSkyBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor DimGray
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DimGray, out color))
                {
                    color = FromArgb(255, 105, 105, 105);
                    _colorsCache.Add(DrawingKnownColor.DimGray, color);
                }

                return color;
            }
        }

        public static DrawingColor DodgerBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.DodgerBlue, out color))
                {
                    color = FromArgb(255, 30, 144, 255);
                    _colorsCache.Add(DrawingKnownColor.DodgerBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor Firebrick
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Firebrick, out color))
                {
                    color = FromArgb(255, 178, 34, 34);
                    _colorsCache.Add(DrawingKnownColor.Firebrick, color);
                }

                return color;
            }
        }

        public static DrawingColor FloralWhite
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.FloralWhite, out color))
                {
                    color = FromArgb(255, 255, 250, 240);
                    _colorsCache.Add(DrawingKnownColor.FloralWhite, color);
                }

                return color;
            }
        }

        public static DrawingColor ForestGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.ForestGreen, out color))
                {
                    color = FromArgb(255, 34, 139, 34);
                    _colorsCache.Add(DrawingKnownColor.ForestGreen, color);
                }

                return color;
            }
        }

        public static DrawingColor Fuchsia
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Fuchsia, out color))
                {
                    color = FromArgb(255, 255, 0, 255);
                    _colorsCache.Add(DrawingKnownColor.Fuchsia, color);
                }

                return color;
            }
        }

        public static DrawingColor Gainsboro
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Gainsboro, out color))
                {
                    color = FromArgb(255, 220, 220, 220);
                    _colorsCache.Add(DrawingKnownColor.Gainsboro, color);
                }

                return color;
            }
        }

        public static DrawingColor GhostWhite
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.GhostWhite, out color))
                {
                    color = FromArgb(255, 248, 248, 255);
                    _colorsCache.Add(DrawingKnownColor.GhostWhite, color);
                }

                return color;
            }
        }

        public static DrawingColor Gold
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Gold, out color))
                {
                    color = FromArgb(255, 255, 215, 0);
                    _colorsCache.Add(DrawingKnownColor.Gold, color);
                }

                return color;
            }
        }

        public static DrawingColor Goldenrod
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Goldenrod, out color))
                {
                    color = FromArgb(255, 218, 165, 32);
                    _colorsCache.Add(DrawingKnownColor.Goldenrod, color);
                }

                return color;
            }
        }

        public static DrawingColor Gray
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Gray, out color))
                {
                    color = FromArgb(255, 128, 128, 128);
                    _colorsCache.Add(DrawingKnownColor.Gray, color);
                }

                return color;
            }
        }

        public static DrawingColor Green
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Green, out color))
                {
                    color = FromArgb(255, 0, 128, 0);
                    _colorsCache.Add(DrawingKnownColor.Green, color);
                }

                return color;
            }
        }

        public static DrawingColor GreenYellow
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.GreenYellow, out color))
                {
                    color = FromArgb(255, 173, 255, 47);
                    _colorsCache.Add(DrawingKnownColor.GreenYellow, color);
                }

                return color;
            }
        }

        public static DrawingColor Honeydew
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Honeydew, out color))
                {
                    color = FromArgb(255, 240, 255, 240);
                    _colorsCache.Add(DrawingKnownColor.Honeydew, color);
                }

                return color;
            }
        }

        public static DrawingColor HotPink
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.HotPink, out color))
                {
                    color = FromArgb(255, 255, 105, 180);
                    _colorsCache.Add(DrawingKnownColor.HotPink, color);
                }

                return color;
            }
        }

        public static DrawingColor IndianRed
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.IndianRed, out color))
                {
                    color = FromArgb(255, 205, 92, 92);
                    _colorsCache.Add(DrawingKnownColor.IndianRed, color);
                }

                return color;
            }
        }

        public static DrawingColor Indigo
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Indigo, out color))
                {
                    color = FromArgb(255, 75, 0, 130);
                    _colorsCache.Add(DrawingKnownColor.Indigo, color);
                }

                return color;
            }
        }

        public static DrawingColor Ivory
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Ivory, out color))
                {
                    color = FromArgb(255, 255, 255, 240);
                    _colorsCache.Add(DrawingKnownColor.Ivory, color);
                }

                return color;
            }
        }

        public static DrawingColor Khaki
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Khaki, out color))
                {
                    color = FromArgb(255, 240, 230, 140);
                    _colorsCache.Add(DrawingKnownColor.Khaki, color);
                }

                return color;
            }
        }

        public static DrawingColor Lavender
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Lavender, out color))
                {
                    color = FromArgb(255, 230, 230, 250);
                    _colorsCache.Add(DrawingKnownColor.Lavender, color);
                }

                return color;
            }
        }

        public static DrawingColor LavenderBlush
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LavenderBlush, out color))
                {
                    color = FromArgb(255, 255, 240, 245);
                    _colorsCache.Add(DrawingKnownColor.LavenderBlush, color);
                }

                return color;
            }
        }

        public static DrawingColor LawnGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LawnGreen, out color))
                {
                    color = FromArgb(255, 124, 252, 0);
                    _colorsCache.Add(DrawingKnownColor.LawnGreen, color);
                }

                return color;
            }
        }

        public static DrawingColor LemonChiffon
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LemonChiffon, out color))
                {
                    color = FromArgb(255, 255, 250, 205);
                    _colorsCache.Add(DrawingKnownColor.LemonChiffon, color);
                }

                return color;
            }
        }

        public static DrawingColor LightBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LightBlue, out color))
                {
                    color = FromArgb(255, 173, 216, 230);
                    _colorsCache.Add(DrawingKnownColor.LightBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor LightCoral
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LightCoral, out color))
                {
                    color = FromArgb(255, 240, 128, 128);
                    _colorsCache.Add(DrawingKnownColor.LightCoral, color);
                }

                return color;
            }
        }

        public static DrawingColor LightCyan
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LightCyan, out color))
                {
                    color = FromArgb(255, 224, 255, 255);
                    _colorsCache.Add(DrawingKnownColor.LightCyan, color);
                }

                return color;
            }
        }

        public static DrawingColor LightGoldenrodYellow
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LightGoldenrodYellow, out color))
                {
                    color = FromArgb(255, 250, 250, 210);
                    _colorsCache.Add(DrawingKnownColor.LightGoldenrodYellow, color);
                }

                return color;
            }
        }

        public static DrawingColor LightGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LightGreen, out color))
                {
                    color = FromArgb(255, 144, 238, 144);
                    _colorsCache.Add(DrawingKnownColor.LightGreen, color);
                }

                return color;
            }
        }

        public static DrawingColor LightGray
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LightGray, out color))
                {
                    color = FromArgb(255, 211, 211, 211);
                    _colorsCache.Add(DrawingKnownColor.LightGray, color);
                }

                return color;
            }
        }

        public static DrawingColor LightPink
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LightPink, out color))
                {
                    color = FromArgb(255, 255, 182, 193);
                    _colorsCache.Add(DrawingKnownColor.LightPink, color);
                }

                return color;
            }
        }

        public static DrawingColor LightSalmon
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LightSalmon, out color))
                {
                    color = FromArgb(255, 255, 160, 122);
                    _colorsCache.Add(DrawingKnownColor.LightSalmon, color);
                }

                return color;
            }
        }

        public static DrawingColor LightSeaGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LightSeaGreen, out color))
                {
                    color = FromArgb(255, 32, 178, 170);
                    _colorsCache.Add(DrawingKnownColor.LightSeaGreen, color);
                }

                return color;
            }
        }

        public static DrawingColor LightSkyBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LightSkyBlue, out color))
                {
                    color = FromArgb(255, 135, 206, 250);
                    _colorsCache.Add(DrawingKnownColor.LightSkyBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor LightSlateGray
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LightSlateGray, out color))
                {
                    color = FromArgb(255, 119, 136, 153);
                    _colorsCache.Add(DrawingKnownColor.LightSlateGray, color);
                }

                return color;
            }
        }

        public static DrawingColor LightSteelBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LightSteelBlue, out color))
                {
                    color = FromArgb(255, 176, 196, 222);
                    _colorsCache.Add(DrawingKnownColor.LightSteelBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor LightYellow
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LightYellow, out color))
                {
                    color = FromArgb(255, 255, 255, 224);
                    _colorsCache.Add(DrawingKnownColor.LightYellow, color);
                }

                return color;
            }
        }

        public static DrawingColor Lime
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Lime, out color))
                {
                    color = FromArgb(255, 0, 255, 0);
                    _colorsCache.Add(DrawingKnownColor.Lime, color);
                }

                return color;
            }
        }

        public static DrawingColor LimeGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.LimeGreen, out color))
                {
                    color = FromArgb(255, 50, 205, 50);
                    _colorsCache.Add(DrawingKnownColor.LimeGreen, color);
                }

                return color;
            }
        }

        public static DrawingColor Linen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Linen, out color))
                {
                    color = FromArgb(255, 250, 240, 230);
                    _colorsCache.Add(DrawingKnownColor.Linen, color);
                }

                return color;
            }
        }

        public static DrawingColor Magenta
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Magenta, out color))
                {
                    color = FromArgb(255, 255, 0, 255);
                    _colorsCache.Add(DrawingKnownColor.Magenta, color);
                }

                return color;
            }
        }

        public static DrawingColor Maroon
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Maroon, out color))
                {
                    color = FromArgb(255, 128, 0, 0);
                    _colorsCache.Add(DrawingKnownColor.Maroon, color);
                }

                return color;
            }
        }

        public static DrawingColor MediumAquamarine
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.MediumAquamarine, out color))
                {
                    color = FromArgb(255, 102, 205, 170);
                    _colorsCache.Add(DrawingKnownColor.MediumAquamarine, color);
                }

                return color;
            }
        }

        public static DrawingColor MediumBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.MediumBlue, out color))
                {
                    color = FromArgb(255, 0, 0, 205);
                    _colorsCache.Add(DrawingKnownColor.MediumBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor MediumOrchid
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.MediumOrchid, out color))
                {
                    color = FromArgb(255, 186, 85, 211);
                    _colorsCache.Add(DrawingKnownColor.MediumOrchid, color);
                }

                return color;
            }
        }

        public static DrawingColor MediumPurple
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.MediumPurple, out color))
                {
                    color = FromArgb(255, 147, 112, 219);
                    _colorsCache.Add(DrawingKnownColor.MediumPurple, color);
                }

                return color;
            }
        }

        public static DrawingColor MediumSeaGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.MediumSeaGreen, out color))
                {
                    color = FromArgb(255, 60, 179, 113);
                    _colorsCache.Add(DrawingKnownColor.MediumSeaGreen, color);
                }

                return color;
            }
        }

        public static DrawingColor MediumSlateBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.MediumSlateBlue, out color))
                {
                    color = FromArgb(255, 123, 104, 238);
                    _colorsCache.Add(DrawingKnownColor.MediumSlateBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor MediumSpringGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.MediumSpringGreen, out color))
                {
                    color = FromArgb(255, 0, 250, 154);
                    _colorsCache.Add(DrawingKnownColor.MediumSpringGreen, color);
                }

                return color;
            }
        }

        public static DrawingColor MediumTurquoise
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.MediumTurquoise, out color))
                {
                    color = FromArgb(255, 72, 209, 204);
                    _colorsCache.Add(DrawingKnownColor.MediumTurquoise, color);
                }

                return color;
            }
        }

        public static DrawingColor MediumVioletRed
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.MediumVioletRed, out color))
                {
                    color = FromArgb(255, 199, 21, 133);
                    _colorsCache.Add(DrawingKnownColor.MediumVioletRed, color);
                }

                return color;
            }
        }

        public static DrawingColor MidnightBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.MidnightBlue, out color))
                {
                    color = FromArgb(255, 25, 25, 112);
                    _colorsCache.Add(DrawingKnownColor.MidnightBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor MintCream
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.MintCream, out color))
                {
                    color = FromArgb(255, 245, 255, 250);
                    _colorsCache.Add(DrawingKnownColor.MintCream, color);
                }

                return color;
            }
        }

        public static DrawingColor MistyRose
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.MistyRose, out color))
                {
                    color = FromArgb(255, 255, 228, 225);
                    _colorsCache.Add(DrawingKnownColor.MistyRose, color);
                }

                return color;
            }
        }

        public static DrawingColor Moccasin
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Moccasin, out color))
                {
                    color = FromArgb(255, 255, 228, 181);
                    _colorsCache.Add(DrawingKnownColor.Moccasin, color);
                }

                return color;
            }
        }

        public static DrawingColor NavajoWhite
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.NavajoWhite, out color))
                {
                    color = FromArgb(255, 255, 222, 173);
                    _colorsCache.Add(DrawingKnownColor.NavajoWhite, color);
                }

                return color;
            }
        }

        public static DrawingColor Navy
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Navy, out color))
                {
                    color = FromArgb(255, 0, 0, 128);
                    _colorsCache.Add(DrawingKnownColor.Navy, color);
                }

                return color;
            }
        }

        public static DrawingColor OldLace
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.OldLace, out color))
                {
                    color = FromArgb(255, 253, 245, 230);
                    _colorsCache.Add(DrawingKnownColor.OldLace, color);
                }

                return color;
            }
        }

        public static DrawingColor Olive
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Olive, out color))
                {
                    color = FromArgb(255, 128, 128, 0);
                    _colorsCache.Add(DrawingKnownColor.Olive, color);
                }

                return color;
            }
        }

        public static DrawingColor OliveDrab
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.OliveDrab, out color))
                {
                    color = FromArgb(255, 107, 142, 35);
                    _colorsCache.Add(DrawingKnownColor.OliveDrab, color);
                }

                return color;
            }
        }

        public static DrawingColor Orange
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Orange, out color))
                {
                    color = FromArgb(255, 255, 165, 0);
                    _colorsCache.Add(DrawingKnownColor.Orange, color);
                }

                return color;
            }
        }

        public static DrawingColor OrangeRed
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.OrangeRed, out color))
                {
                    color = FromArgb(255, 255, 69, 0);
                    _colorsCache.Add(DrawingKnownColor.OrangeRed, color);
                }

                return color;
            }
        }

        public static DrawingColor Orchid
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Orchid, out color))
                {
                    color = FromArgb(255, 218, 112, 214);
                    _colorsCache.Add(DrawingKnownColor.Orchid, color);
                }

                return color;
            }
        }

        public static DrawingColor PaleGoldenrod
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.PaleGoldenrod, out color))
                {
                    color = FromArgb(255, 238, 232, 170);
                    _colorsCache.Add(DrawingKnownColor.PaleGoldenrod, color);
                }

                return color;
            }
        }

        public static DrawingColor PaleGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.PaleGreen, out color))
                {
                    color = FromArgb(255, 152, 251, 152);
                    _colorsCache.Add(DrawingKnownColor.PaleGreen, color);
                }

                return color;
            }
        }

        public static DrawingColor PaleTurquoise
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.PaleTurquoise, out color))
                {
                    color = FromArgb(255, 175, 238, 238);
                    _colorsCache.Add(DrawingKnownColor.PaleTurquoise, color);
                }

                return color;
            }
        }

        public static DrawingColor PaleVioletRed
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.PaleVioletRed, out color))
                {
                    color = FromArgb(255, 219, 112, 147);
                    _colorsCache.Add(DrawingKnownColor.PaleVioletRed, color);
                }

                return color;
            }
        }

        public static DrawingColor PapayaWhip
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.PapayaWhip, out color))
                {
                    color = FromArgb(255, 255, 239, 213);
                    _colorsCache.Add(DrawingKnownColor.PapayaWhip, color);
                }

                return color;
            }
        }

        public static DrawingColor PeachPuff
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.PeachPuff, out color))
                {
                    color = FromArgb(255, 255, 218, 185);
                    _colorsCache.Add(DrawingKnownColor.PeachPuff, color);
                }

                return color;
            }
        }

        public static DrawingColor Peru
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Peru, out color))
                {
                    color = FromArgb(255, 205, 133, 63);
                    _colorsCache.Add(DrawingKnownColor.Peru, color);
                }

                return color;
            }
        }

        public static DrawingColor Pink
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Pink, out color))
                {
                    color = FromArgb(255, 255, 192, 203);
                    _colorsCache.Add(DrawingKnownColor.Pink, color);
                }

                return color;
            }
        }

        public static DrawingColor Plum
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Plum, out color))
                {
                    color = FromArgb(255, 221, 160, 221);
                    _colorsCache.Add(DrawingKnownColor.Plum, color);
                }

                return color;
            }
        }

        public static DrawingColor PowderBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.PowderBlue, out color))
                {
                    color = FromArgb(255, 176, 224, 230);
                    _colorsCache.Add(DrawingKnownColor.PowderBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor Purple
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Purple, out color))
                {
                    color = FromArgb(255, 128, 0, 128);
                    _colorsCache.Add(DrawingKnownColor.Purple, color);
                }

                return color;
            }
        }

        public static DrawingColor Red
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Red, out color))
                {
                    color = FromArgb(255, 255, 0, 0);
                    _colorsCache.Add(DrawingKnownColor.Red, color);
                }

                return color;
            }
        }

        public static DrawingColor RosyBrown
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.RosyBrown, out color))
                {
                    color = FromArgb(255, 188, 143, 143);
                    _colorsCache.Add(DrawingKnownColor.RosyBrown, color);
                }

                return color;
            }
        }

        public static DrawingColor RoyalBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.RoyalBlue, out color))
                {
                    color = FromArgb(255, 65, 105, 225);
                    _colorsCache.Add(DrawingKnownColor.RoyalBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor SaddleBrown
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.SaddleBrown, out color))
                {
                    color = FromArgb(255, 139, 69, 19);
                    _colorsCache.Add(DrawingKnownColor.SaddleBrown, color);
                }

                return color;
            }
        }

        public static DrawingColor Salmon
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Salmon, out color))
                {
                    color = FromArgb(255, 250, 128, 114);
                    _colorsCache.Add(DrawingKnownColor.Salmon, color);
                }

                return color;
            }
        }

        public static DrawingColor SandyBrown
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.SandyBrown, out color))
                {
                    color = FromArgb(255, 244, 164, 96);
                    _colorsCache.Add(DrawingKnownColor.SandyBrown, color);
                }

                return color;
            }
        }

        public static DrawingColor SeaGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.SeaGreen, out color))
                {
                    color = FromArgb(255, 46, 139, 87);
                    _colorsCache.Add(DrawingKnownColor.SeaGreen, color);
                }

                return color;
            }
        }

        public static DrawingColor SeaShell
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.SeaShell, out color))
                {
                    color = FromArgb(255, 255, 245, 238);
                    _colorsCache.Add(DrawingKnownColor.SeaShell, color);
                }

                return color;
            }
        }

        public static DrawingColor Sienna
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Sienna, out color))
                {
                    color = FromArgb(255, 160, 82, 45);
                    _colorsCache.Add(DrawingKnownColor.Sienna, color);
                }

                return color;
            }
        }

        public static DrawingColor Silver
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Silver, out color))
                {
                    color = FromArgb(255, 192, 192, 192);
                    _colorsCache.Add(DrawingKnownColor.Silver, color);
                }

                return color;
            }
        }

        public static DrawingColor SkyBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.SkyBlue, out color))
                {
                    color = FromArgb(255, 135, 206, 235);
                    _colorsCache.Add(DrawingKnownColor.SkyBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor SlateBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.SlateBlue, out color))
                {
                    color = FromArgb(255, 106, 90, 205);
                    _colorsCache.Add(DrawingKnownColor.SlateBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor SlateGray
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.SlateGray, out color))
                {
                    color = FromArgb(255, 112, 128, 144);
                    _colorsCache.Add(DrawingKnownColor.SlateGray, color);
                }

                return color;
            }
        }

        public static DrawingColor Snow
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Snow, out color))
                {
                    color = FromArgb(255, 255, 250, 250);
                    _colorsCache.Add(DrawingKnownColor.Snow, color);
                }

                return color;
            }
        }

        public static DrawingColor SpringGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.SpringGreen, out color))
                {
                    color = FromArgb(255, 0, 255, 127);
                    _colorsCache.Add(DrawingKnownColor.SpringGreen, color);
                }

                return color;
            }
        }

        public static DrawingColor SteelBlue
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.SteelBlue, out color))
                {
                    color = FromArgb(255, 70, 130, 180);
                    _colorsCache.Add(DrawingKnownColor.SteelBlue, color);
                }

                return color;
            }
        }

        public static DrawingColor Tan
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Tan, out color))
                {
                    color = FromArgb(255, 210, 180, 140);
                    _colorsCache.Add(DrawingKnownColor.Tan, color);
                }

                return color;
            }
        }

        public static DrawingColor Teal
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Teal, out color))
                {
                    color = FromArgb(255, 0, 128, 128);
                    _colorsCache.Add(DrawingKnownColor.Teal, color);
                }

                return color;
            }
        }

        public static DrawingColor Thistle
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Thistle, out color))
                {
                    color = FromArgb(255, 216, 191, 216);
                    _colorsCache.Add(DrawingKnownColor.Thistle, color);
                }

                return color;
            }
        }

        public static DrawingColor Tomato
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Tomato, out color))
                {
                    color = FromArgb(255, 255, 99, 71);
                    _colorsCache.Add(DrawingKnownColor.Tomato, color);
                }

                return color;
            }
        }

        public static DrawingColor Turquoise
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Turquoise, out color))
                {
                    color = FromArgb(255, 64, 224, 208);
                    _colorsCache.Add(DrawingKnownColor.Turquoise, color);
                }

                return color;
            }
        }

        public static DrawingColor Violet
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Violet, out color))
                {
                    color = FromArgb(255, 238, 130, 238);
                    _colorsCache.Add(DrawingKnownColor.Violet, color);
                }

                return color;
            }
        }

        public static DrawingColor Wheat
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Wheat, out color))
                {
                    color = FromArgb(255, 245, 222, 179);
                    _colorsCache.Add(DrawingKnownColor.Wheat, color);
                }

                return color;
            }
        }

        public static DrawingColor White
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.White, out color))
                {
                    color = FromArgb(255, 255, 255, 255);
                    _colorsCache.Add(DrawingKnownColor.White, color);
                }

                return color;
            }
        }

        public static DrawingColor WhiteSmoke
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.WhiteSmoke, out color))
                {
                    color = FromArgb(255, 245, 245, 245);
                    _colorsCache.Add(DrawingKnownColor.WhiteSmoke, color);
                }

                return color;
            }
        }

        public static DrawingColor Yellow
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.Yellow, out color))
                {
                    color = FromArgb(255, 255, 255, 0);
                    _colorsCache.Add(DrawingKnownColor.Yellow, color);
                }

                return color;
            }
        }

        public static DrawingColor YellowGreen
        {
            get
            {
                DrawingColor color;
                if (!_colorsCache.TryGetValue(DrawingKnownColor.YellowGreen, out color))
                {
                    color = FromArgb(255, 154, 205, 50);
                    _colorsCache.Add(DrawingKnownColor.YellowGreen, color);
                }

                return color;
            }
        }
        #endregion
    }
}
