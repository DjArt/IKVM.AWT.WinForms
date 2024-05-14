/*
  Copyright (C) 2002, 2004, 2005, 2006 Jeroen Frijters
  Copyright (C) 2006 Active Endpoints, Inc.
  Copyright (C) 2006 - 2013 Volker Berlin (i-net software)
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

using java.awt.image;

namespace IKVM.AWT.WinForms
{
    class NoImage : java.awt.Image
    {
        private sun.awt.image.InputStreamImageSource source;

        internal NoImage(sun.awt.image.InputStreamImageSource source)
        {
            this.source = source;
        }

        public override int getWidth(ImageObserver observer)
        {
            observer?.imageUpdate(this, ImageObserver.__Fields.ERROR | ImageObserver.__Fields.ABORT, 0, 0, -1, -1);
            return -1;
        }

        public override int getHeight(ImageObserver observer)
        {
            observer?.imageUpdate(this, ImageObserver.__Fields.ERROR | ImageObserver.__Fields.ABORT, 0, 0, -1, -1);
            return -1;
        }

        public override ImageProducer getSource()
        {
            return source;
        }

        public override java.awt.Graphics getGraphics()
        {
            // TODO throw java.lang.IllegalAccessError: getGraphics() only valid for images created with createImage(w, h)
            return null;
        }

        public override object getProperty(string name, ImageObserver observer)
        {
            observer?.imageUpdate(this, ImageObserver.__Fields.ERROR | ImageObserver.__Fields.ABORT, 0, 0, -1, -1);
            return null;
        }

        public override void flush()
        {
        }
    }
}