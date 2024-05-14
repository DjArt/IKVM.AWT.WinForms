/*
 * Copyright 1996-2007 Sun Microsystems, Inc.  All Rights Reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.  Sun designates this
 * particular file as subject to the "Classpath" exception as provided
 * by Sun in the LICENSE file that accompanied this code.
 *
 * This code is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
 * version 2 for more details (a copy is included in the LICENSE file that
 * accompanied this code).
 *
 * You should have received a copy of the GNU General Public License version
 * 2 along with this work; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
 *
 * Please contact Sun Microsystems, Inc., 4150 Network Circle, Santa Clara,
 * CA 95054 USA or visit www.sun.com if you need additional information or
 * have any questions.
 */

/*
    Copyright (C) 2002, 2004-2009 Jeroen Frijters
    Copyright (C) 2006 Active Endpoints, Inc.
    Copyright (C) 2006-2013 Volker Berlin (i-net software)
    Copyright (C) 2010-2011 Karsten Heinrich (i-net software)
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
using System.Windows.Forms;

namespace IKVM.AWT.WinForms
{
    class NetCustomCursor : java.awt.Cursor
    {
        private Cursor cursor;
        public Cursor Cursor => cursor;

        internal NetCustomCursor(java.awt.Image cursorIm, java.awt.Point hotSpot, string name) // throws IndexOutOfBoundsException
            : base(name)
        {
            java.awt.Toolkit toolkit = java.awt.Toolkit.getDefaultToolkit();

            // Make sure image is fully loaded.
            java.awt.Component c = new java.awt.Canvas(); // for its imageUpdate method
            java.awt.MediaTracker tracker = new(c);
            tracker.addImage(cursorIm, 0);
            try
            {
                tracker.waitForAll();
            }
            catch (java.lang.InterruptedException)
            {
            }
            int width = cursorIm.getWidth(c);
            int height = cursorIm.getHeight(c);

            // Fix for bug 4212593 The Toolkit.createCustomCursor does not
            //                     check absence of the image of cursor
            // If the image is invalid, the cursor will be hidden (made completely
            // transparent). In this case, getBestCursorSize() will adjust negative w and h,
            // but we need to set the hotspot inside the image here.
            if (tracker.isErrorAny() || width < 0 || height < 0)
            {
                hotSpot.x = hotSpot.y = 0;
            }

            // Scale image to nearest supported size.
            java.awt.Dimension nativeSize = toolkit.getBestCursorSize(width, height);
            if (nativeSize.width != width || nativeSize.height != height)
            {
                cursorIm = cursorIm.getScaledInstance(nativeSize.width,
                                                  nativeSize.height,
                                                  java.awt.Image.SCALE_DEFAULT);
                width = nativeSize.width;
                height = nativeSize.height;
            }

            // Verify that the hotspot is within cursor bounds.
            if (hotSpot.x >= width || hotSpot.y >= height || hotSpot.x < 0 || hotSpot.y < 0)
            {
                throw new ArgumentException("invalid hotSpot");
            }

            Bitmap bitmap = J2C.ConvertImage(cursorIm);
            nint hIcon = bitmap.GetHicon();
            cursor = new Cursor(hIcon);
        }
    }

}
