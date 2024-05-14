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
using System.Globalization;
using System.Windows.Forms;

using java.awt.image;
using java.util;

namespace IKVM.AWT.WinForms
{
    public class NetGraphicsEnvironment : sun.java2d.SunGraphicsEnvironment
    {
        public override bool isDisplayLocal()
        {
            return true;
        }

        // Create a bitmap with the dimensions of the argument image. Then
        // create a graphics objects from the bitmap. All paint operations will
        // then paint the bitmap.
        public override java.awt.Graphics2D createGraphics(BufferedImage bi)
        {
            return new BitmapGraphics(bi.getBitmap(), bi);
        }

        public override java.awt.Font[] getAllFonts()
        {
#if WINFX  
            System.Collections.Generic.ICollection<Typeface> typefaces = System.Windows.Media.Fonts.SystemTypefaces;
            java.awt.Font[] fonts = new java.awt.Font[typefaces.Count];
            int i = 0;
            foreach (Typeface face in typefaces)
            {
                FontFamily family = face.FontFamily;
                fonts[i++] = new java.awt.Font(family.GetName(0), face.Style, 1);
            }
#else
            string[] names = getAvailableFontFamilyNames();
            java.awt.Font[] fonts = new java.awt.Font[names.Length];
            for (int i = 0; i < fonts.Length; i++)
            {
                fonts[i] = new java.awt.Font(names[i], 0, 1);
            }
            return fonts;
#endif
        }

        public override string[] getAvailableFontFamilyNames()
        {
            int language = CultureInfo.CurrentCulture.LCID;
            return getAvailableFontFamilyNames(language);
        }

        public override string[] getAvailableFontFamilyNames(Locale locale)
        {
            int language = CultureInfo.GetCultureInfo(locale.toString()).LCID;
            return getAvailableFontFamilyNames(language);
        }

        private string[] getAvailableFontFamilyNames(int language)
        {
            FontFamily[] families = FontFamily.Families;
            string[] results = new string[families.Length + 5];
            int i = 0;
            for (; i < families.Length; i++)
            {
                results[i] = families[i].GetName(language);
            }
            results[i++] = "Dialog";
            results[i++] = "DialogInput";
            results[i++] = "Serif";
            results[i++] = "SansSerif";
            results[i++] = "Monospaced";
            Array.Sort(results);
            return results;
        }

        public override java.awt.GraphicsDevice getDefaultScreenDevice()
        {
            return new NetGraphicsDevice(Screen.PrimaryScreen);
        }

        public override java.awt.GraphicsDevice[] getScreenDevices()
        {
            Screen[] screens = Screen.AllScreens;
            NetGraphicsDevice[] devices = new NetGraphicsDevice[screens.Length];
            for (int i = 0; i < screens.Length; i++)
            {
                devices[i] = new NetGraphicsDevice(screens[i]);
            }
            return devices;
        }
    }

}
