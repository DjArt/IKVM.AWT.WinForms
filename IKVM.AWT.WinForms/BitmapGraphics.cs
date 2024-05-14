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

using java.awt.image;

namespace IKVM.AWT.WinForms
{
    internal class BitmapGraphics : NetGraphics
    {
        private readonly Bitmap bitmap;
        private readonly BufferedImage image;

        internal BitmapGraphics(Bitmap bitmap, object destination, java.awt.Font font, Color fgcolor, Color bgcolor)
            : base(createGraphics(bitmap), destination, font, fgcolor, bgcolor)
        {
            this.bitmap = bitmap;
            image = destination as BufferedImage;
        }

        internal BitmapGraphics(Bitmap bitmap, object destination)
            : this(bitmap, destination, null, Color.White, Color.Black)
        {
        }

        internal override Graphics g
        {
            get
            {
                image?.getBitmap();
                return base.g;
            }
        }

        protected override SizeF GetSize()
        {
            return bitmap.Size;
        }

        private static Graphics createGraphics(Bitmap bitmap)
        {
            // lock to prevent the exception
            // System.InvalidOperationException: Object is currently in use elsewhere
            lock (bitmap)
            {
                return Graphics.FromImage(bitmap);
            }
        }

        public override java.awt.Graphics create()
        {
            BitmapGraphics newGraphics = (BitmapGraphics)MemberwiseClone();
            newGraphics.init(createGraphics(bitmap));
            return newGraphics;
        }

        public override void copyArea(int x, int y, int width, int height, int dx, int dy)
        {
            using Bitmap copy = new(width, height);
            using (Graphics gCopy = Graphics.FromImage(copy))
            {
                gCopy.DrawImage(bitmap, new Rectangle(0, 0, width, height), x, y, width, height, GraphicsUnit.Pixel);
            }
            g.DrawImageUnscaled(copy, x + dx, y + dy);
        }
    }

}
