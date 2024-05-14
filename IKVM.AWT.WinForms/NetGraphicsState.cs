/*
    Copyright (C) 2002, 2004, 2005, 2006, 2007 Jeroen Frijters
    Copyright (C) 2006 Active Endpoints, Inc.
    Copyright (C) 2006 - 2014 Volker Berlin (i-net software)
    Copyright (C) 2011 Karsten Heinrich (i-net software)
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

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace IKVM.AWT.WinForms
{
    /// <summary>
    /// State to store/restore the state of a NetGraphics/Graphics object
    /// </summary>
    internal class NetGraphicsState
    {
        private Brush brush;
        private Pen pen;

        // Graphics State
        private Matrix Transform;
        private Region Clip;
        private SmoothingMode SmoothingMode;
        private PixelOffsetMode PixelOffsetMode;
        private TextRenderingHint TextRenderingHint;
        private InterpolationMode InterpolationMode;
        private CompositingMode CompositingMode;

        private bool savedGraphics = false;

        public NetGraphicsState()
        {
        }

        public NetGraphicsState(NetGraphics netG)
        {
            saveGraphics(netG);
        }

        public void saveGraphics(NetGraphics netG)
        {
            if (netG == null)
            {
                return;
            }
            if (netG.g != null)
            {
                Transform = netG.g.Transform;
                Clip = netG.g.Clip;
                SmoothingMode = netG.g.SmoothingMode;
                PixelOffsetMode = netG.g.PixelOffsetMode;
                TextRenderingHint = netG.g.TextRenderingHint;
                InterpolationMode = netG.g.InterpolationMode;
                CompositingMode = netG.g.CompositingMode;
                savedGraphics = true;
            }
            if (netG.pen != null && netG.brush != null)
            {
                pen = (Pen)netG.pen.Clone();
                brush = (Brush)netG.brush.Clone();
            }
        }

        public void restoreGraphics(NetGraphics netG)
        {
            if (netG == null)
            {
                return;
            }
            if (netG.g != null)
            {
                if (savedGraphics)
                {
                    netG.g.Transform = Transform;
                    netG.g.Clip = Clip;
                    netG.g.SmoothingMode = SmoothingMode;
                    netG.g.PixelOffsetMode = PixelOffsetMode;
                    netG.setTextRenderingHint(TextRenderingHint);
                    netG.g.InterpolationMode = InterpolationMode;
                    netG.g.CompositingMode = CompositingMode;
                }
                else
                {
                    // default values that Java used
                    netG.g.InterpolationMode = InterpolationMode.NearestNeighbor;
                }
            }
            if (pen != null && brush != null)
            {
                netG.pen = (Pen)pen.Clone();
                netG.brush = (Brush)brush.Clone();
            }
            else
            {
                netG.pen = new Pen(netG.color);
                netG.brush = new SolidBrush(netG.color);
                netG.setRenderingHint(java.awt.RenderingHints.KEY_TEXT_ANTIALIASING, java.awt.RenderingHints.VALUE_TEXT_ANTIALIAS_DEFAULT);
            }
        }
    }

}
