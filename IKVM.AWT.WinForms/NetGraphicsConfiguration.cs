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
using System.Windows.Forms;

using java.awt.image;

namespace IKVM.AWT.WinForms
{
    sealed class NetGraphicsConfiguration : java.awt.GraphicsConfiguration
    {
        internal readonly Screen screen;

        public NetGraphicsConfiguration(Screen screen)
        {
            this.screen = screen;
        }

        public override BufferedImage createCompatibleImage(int width, int height, int transparency)
        {
            return transparency switch
            {
                java.awt.Transparency.__Fields.OPAQUE => new BufferedImage(width, height, BufferedImage.TYPE_INT_RGB),
                java.awt.Transparency.__Fields.BITMASK => new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB_PRE),
                java.awt.Transparency.__Fields.TRANSLUCENT => new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB),
                _ => throw new java.lang.IllegalArgumentException("transparency:" + transparency),
            };
        }

        public override BufferedImage createCompatibleImage(int width, int height)
        {
            return new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB);
        }

        public override VolatileImage createCompatibleVolatileImage(int param1, int param2, java.awt.ImageCapabilities param3)
        {
            throw new NotImplementedException();
        }

        public override VolatileImage createCompatibleVolatileImage(int width, int height)
        {
            return new NetVolatileImage(width, height);
        }

        public override java.awt.Rectangle getBounds()
        {
            System.Drawing.Rectangle bounds = screen.Bounds;
            return new java.awt.Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public override java.awt.BufferCapabilities getBufferCapabilities()
        {
            throw new NotImplementedException();
        }

        public override ColorModel getColorModel(int transparency)
        {
            return transparency switch
            {
                java.awt.Transparency.__Fields.TRANSLUCENT => ColorModel.getRGBdefault(),
                _ => null
            };
        }

        public override ColorModel getColorModel()
        {
            //we return the default ColorModel because this produce the fewest problems with convertions
            return ColorModel.getRGBdefault();
        }

        public override java.awt.geom.AffineTransform getDefaultTransform()
        {
            return new java.awt.geom.AffineTransform();
        }

        public override java.awt.GraphicsDevice getDevice()
        {
            return new NetGraphicsDevice(screen);
        }

        public override java.awt.ImageCapabilities getImageCapabilities()
        {
            throw new NotImplementedException();
        }

        public override java.awt.geom.AffineTransform getNormalizingTransform()
        {
            throw new NotImplementedException();
        }

        public override VolatileImage createCompatibleVolatileImage(int width, int height, int transparency)
        {
            return new NetVolatileImage(width, height);
        }

        public override VolatileImage createCompatibleVolatileImage(int width, int height, java.awt.ImageCapabilities caps, int transparency)
        {
            return new NetVolatileImage(width, height);
        }

        public override bool isTranslucencyCapable()
        {
            return true;
        }
    }

}
