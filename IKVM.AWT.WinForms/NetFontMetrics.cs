/*
  Copyright (C) 2002, 2004, 2005, 2006, 2007 Jeroen Frijters
  Copyright (C) 2006 Active Endpoints, Inc.
  Copyright (C) 2006, 2007, 2009 - 2011 Volker Berlin
  Copyright (C) 2023-2024 Dj Art.

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net 

*/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

#if WINFX
using System.Windows;
using System.Windows.Media;
#endif

namespace IKVM.AWT.WinForms
{
    class NetFontMetrics : java.awt.FontMetrics
    {
        private static readonly Bitmap defaultbitmap = new Bitmap(1, 1);
        [ThreadStatic]
        private static Graphics threadLocalDefaultGraphics;

        private static Graphics GetDefaultGraphics()
        {
            Graphics g = threadLocalDefaultGraphics;
            if (g == null)
            {
                g = threadLocalDefaultGraphics = Graphics.FromImage(defaultbitmap);
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.None;
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            }
            return g;
        }

        public NetFontMetrics(java.awt.Font font) : base(font)
        {
        }

        private Font GetNetFont()
        {
            return font.getNetFont();
        }

        public override int getHeight()
        {
            return GetNetFont().Height;
        }

        public override int getLeading()
        {
            return (int)Math.Round(GetLeadingFloat());
        }

        public override int getMaxAdvance()
        {
            // HACK very lame
            return charWidth('M');
        }

        public override int charWidth(char ch)
        {
            // HACK we average 20 characters to decrease the influence of the pre/post spacing
            return stringWidth(new string(ch, 20)) / 20;
        }

        public override int charsWidth(char[] data, int off, int len)
        {
            return stringWidth(new string(data, off, len));
        }

        public override int getAscent()
        {
            return (int)Math.Round(GetAscentFloat());
        }

        public override int getDescent()
        {
            return (int)Math.Round(GetDescentFloat());
        }

        public override int stringWidth(string s)
        {
            return (int)Math.Round(GetStringWidth(s, GetDefaultGraphics()));
        }

        public float GetAscentFloat()
        {
            Font f = GetNetFont();
            int ascent = f.FontFamily.GetCellAscent(f.Style);
            return f.Size * ascent / f.FontFamily.GetEmHeight(f.Style);
        }

        public float GetDescentFloat()
        {
            Font f = GetNetFont();
            int descent = f.FontFamily.GetCellDescent(f.Style);
            return f.Size * descent / f.FontFamily.GetEmHeight(f.Style);
        }

        public float GetLeadingFloat()
        {
            float leading = getHeight() - (GetAscentFloat() + GetDescentFloat());
            return Math.Max(0.0f, leading);
        }

        internal float GetStringWidth(string aString, Graphics g)
        {
            if (aString.Length == 0)
            {
                return 0;
            }
            // System.Windows.Forms.TextRenderer#MeasureText seems to large
            // Graphics#MeasureString is many faster but work only correct with TextRenderingHint.AntiAlias
            bool rounding;
            StringFormat format;
            switch (g.TextRenderingHint)
            {
                // Fractional metrics
                case TextRenderingHint.AntiAlias:
                case TextRenderingHint.SingleBitPerPixel:
                    // this very mystic, if a StringFormat extends from GenericTypographic then the metric are different but like Java with fractional metrics
                    format = new StringFormat(StringFormat.GenericTypographic);
                    rounding = false;
                    break;
                default:
                    format = new StringFormat();
                    rounding = true;
                    break;
            }

            format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox;
            format.Trimming = StringTrimming.None;
            format.SetMeasurableCharacterRanges([new CharacterRange(0, aString.Length)]);
            Region[] regions = g.MeasureCharacterRanges(aString, GetNetFont(), new RectangleF(0, 0, int.MaxValue, int.MaxValue), format);
            SizeF size = regions[0].GetBounds(g).Size;
            regions[0].Dispose();
            //with Arial 9.0 and only one character under Vista .NET does not round it, that we rounding manualy
            return rounding ? (int)Math.Round(size.Width) : size.Width;
        }

        internal java.awt.geom.Rectangle2D GetStringBounds(string aString, Graphics g)
        {
            Font netFont = GetNetFont();
            FontFamily family = netFont.FontFamily;
            FontStyle style = netFont.Style;
            float factor = netFont.Size / family.GetEmHeight(style);
            float height = family.GetLineSpacing(style) * factor;
            float descent = family.GetCellDescent(style) * factor;
            float ascent = family.GetCellAscent(style) * factor;
            float leading = height - ascent - descent;

            return new java.awt.geom.Rectangle2D.Float(0, -ascent - leading / 2, GetStringWidth(aString, g), height);
        }

        public override java.awt.geom.Rectangle2D getStringBounds(string aString, java.awt.Graphics gr)
        {
            return gr switch
            {
                NetGraphics netG => GetStringBounds(aString, netG.g),
                _ => GetStringBounds(aString, GetDefaultGraphics())
            };
        }
    }

}
