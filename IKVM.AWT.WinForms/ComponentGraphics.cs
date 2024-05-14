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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace IKVM.AWT.WinForms
{
    internal class ComponentGraphics : NetGraphics
    {
        private readonly Control control;

        internal ComponentGraphics(Control control, java.awt.Component target, java.awt.Color fgColor, java.awt.Color bgColor, java.awt.Font font)
            : base(control.CreateGraphics(), target, font, J2C.ConvertColor(fgColor), J2C.ConvertColor(bgColor))
        {
            this.control = control;
        }

        protected override SizeF GetSize()
        {
            return control.Size;
        }

        public override java.awt.Graphics create()
        {
            ComponentGraphics newGraphics = (ComponentGraphics)MemberwiseClone();
            newGraphics.init(control.CreateGraphics());
            return newGraphics;
        }

        private Point getPointToScreenImpl(Point point)
        {
            return control.PointToScreen(point);
        }

        private Point getPointToScreen(Point point)
        {
            return (Point)control.Invoke(new Converter<Point, Point>(getPointToScreenImpl), point);
        }

        public override void copyArea(int x, int y, int width, int height, int dx, int dy)
        {
            Matrix t = g.Transform;
            Point src = getPointToScreen(new Point(x + (int)t.OffsetX, y + (int)t.OffsetY));
            using Bitmap copy = new(width, height);
            using (Graphics gCopy = Graphics.FromImage(copy))
            {
                gCopy.CopyFromScreen(src, new Point(0, 0), new Size(width, height));
            }
            g.DrawImageUnscaled(copy, x + dx, y + dy);
        }

        public override void clip(java.awt.Shape shape)
        {
            if (shape == null)
            {
                // the API specification says that this will clear
                // the clip, but in fact the reference implementation throws a 
                // NullPointerException - see the following entry in the bug parade:
                // http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=6206189
                throw new java.lang.NullPointerException();
            }
            base.clip(shape);
        }
    }

}
