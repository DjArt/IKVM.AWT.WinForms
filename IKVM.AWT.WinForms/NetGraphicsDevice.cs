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

using System.Windows.Forms;

namespace IKVM.AWT.WinForms
{
    class NetGraphicsDevice : java.awt.GraphicsDevice
    {
        internal readonly Screen screen;

        internal NetGraphicsDevice(Screen screen)
        {
            this.screen = screen;
        }

        public override java.awt.GraphicsConfiguration[] getConfigurations()
        {
            Screen[] screens = Screen.AllScreens;
            NetGraphicsConfiguration[] configs = new NetGraphicsConfiguration[screens.Length];
            for (int i = 0; i < screens.Length; i++)
            {
                configs[i] = new NetGraphicsConfiguration(screens[i]);
            }
            return configs;
        }

        public override java.awt.GraphicsConfiguration getDefaultConfiguration()
        {
            return new NetGraphicsConfiguration(screen);
        }

        public override string getIDstring()
        {
            return screen.DeviceName;
        }

        public override int getType()
        {
            return TYPE_RASTER_SCREEN;
        }
    }

}
