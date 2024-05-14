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
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace IKVM.AWT.WinForms
{
    internal class CompositeHelper
    {

        private readonly ImageAttributes imageAttributes = new();

        /// <summary>
        /// Create a default CompositeHelper. Is used from Create only.
        /// </summary>
        protected CompositeHelper()
        {
        }

        internal static CompositeHelper Create(java.awt.Composite comp, Graphics graphics)
        {
            if (comp is java.awt.AlphaComposite alphaComp)
            {
                float alpha = alphaComp.getAlpha();
                switch (alphaComp.getRule())
                {
                    case java.awt.AlphaComposite.CLEAR:
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        return new ClearCompositeHelper();
                    case java.awt.AlphaComposite.SRC:
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        break;
                    case java.awt.AlphaComposite.SRC_OVER:
                        graphics.CompositingMode = CompositingMode.SourceOver;
                        break;
                    case java.awt.AlphaComposite.DST:
                        graphics.CompositingMode = CompositingMode.SourceOver;
                        alpha = 0.0F;
                        break;
                    default:
                        graphics.CompositingMode = CompositingMode.SourceOver;
                        Console.Error.WriteLine("AlphaComposite with Rule " + alphaComp.getRule() + " not supported.");
                        break;
                }
                return alpha == 1.0 ? new CompositeHelper() : new AlphaCompositeHelper(alpha);
            }
            else
            {
                graphics.CompositingMode = CompositingMode.SourceOver;
                Console.Error.WriteLine("Composite not supported: " + comp.GetType().FullName);
                return new CompositeHelper();
            }
        }

        internal virtual int GetArgb(java.awt.Color color)
        {
            return color.getRGB();
        }

        internal virtual Color GetColor(java.awt.Color color)
        {
            return color == null ? Color.Empty : Color.FromArgb(GetArgb(color));
        }

        internal virtual int ToArgb(Color color)
        {
            return color.ToArgb();
        }

        internal virtual java.awt.Color GetColor(Color color)
        {
            return color == Color.Empty ? null : new java.awt.Color(ToArgb(color), true);
        }

        /// <summary>
        /// Get the ImageAttributes instance. Does not change it bcause it is not a copy.
        /// </summary>
        /// <returns></returns>
        internal virtual ImageAttributes GetImageAttributes()
        {
            return imageAttributes;
        }
    }

}
