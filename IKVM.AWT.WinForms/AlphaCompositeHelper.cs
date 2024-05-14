/*
  Copyright (C) 2010 Volker Berlin (i-net software)
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
using System.Drawing.Imaging;

namespace IKVM.AWT.WinForms
{
    internal sealed class AlphaCompositeHelper : CompositeHelper
    {
        private readonly float alpha;

        /// <summary>
        /// Create a AlphaCompositeHelper
        /// </summary>
        /// <param name="alpha">a value in the range from 0.0 to 1.0</param>
        internal AlphaCompositeHelper(float alpha)
        {
            this.alpha = alpha;
            ColorMatrix matrix = new()
            {
                Matrix33 = alpha
            };
            GetImageAttributes().SetColorMatrix(matrix);
        }

        internal override int GetArgb(java.awt.Color color)
        {
            uint argb = (uint)color.getRGB();
            uint newAlpha = (uint)((0xff000000 & argb) * alpha + 0x800000);
            uint newArgb = 0xff000000 & newAlpha | 0xffffff & argb;
            return (int)newArgb;
        }

        internal override int ToArgb(Color color)
        {
            uint argb = (uint)color.ToArgb();
            uint newAlpha = (uint)((0xff000000 & argb) / alpha + 0x800000);
            uint newArgb = 0xff000000 & newAlpha | 0xffffff & argb;
            return (int)newArgb;
        }
    }

}
