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
using System.IO;
using System.Threading;
using System.Windows.Forms;

using java.util;

using sun.awt;

namespace IKVM.AWT.WinForms
{
    /// <summary>
    /// also WFileDialogPeer extends from WWindowPeer
    /// </summary>
    class NetFileDialogPeer : NetWindowPeer, java.awt.peer.FileDialogPeer
    {
        internal NetFileDialogPeer(java.awt.FileDialog dialog, bool isFocusableWindow, bool isAlwaysOnTop)
            : base(dialog, isFocusableWindow, isAlwaysOnTop)
        {
        }

        public void setDirectory(string str)
        {
        }

        public void setFile(string str)
        {
        }

        public void setFilenameFilter(java.io.FilenameFilter ff)
        {
        }

        public void setResizable(bool b)
        {
        }

        public void setTitle(string str)
        {
        }

        public override void show()
        {
            java.awt.FileDialog dialog = (java.awt.FileDialog)target;
            if (dialog.getMode() != java.awt.FileDialog.LOAD)
            {
                throw new NotImplementedException();
            }
            Thread t = new((ThreadStart)delegate
            {
                using OpenFileDialog dlg = new();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    dialog.setFile(Path.GetFileName(dlg.FileName));
                    dialog.setDirectory(Path.GetDirectoryName(dlg.FileName) + java.io.File.separator);
                    dialog.hide();
                }
                else
                {
                    dialog.setFile(null);
                    dialog.hide();
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        public void blockWindows(List toBlock)
        {
            // code copies from sun.awt.windows.WFileDialogPeer.java
            for (Iterator it = toBlock.iterator(); it.hasNext();)
            {
                java.awt.Window w = (java.awt.Window)it.next();
                java.awt.peer.WindowPeer wp = (java.awt.peer.WindowPeer)AWTAccessor.getComponentAccessor().getPeer(w);
                wp?.setModalBlocked((java.awt.Dialog)target, true);
            }
        }
    }

}
