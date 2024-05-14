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
using System.ComponentModel;

namespace IKVM.AWT.WinForms
{
    sealed class NetFramePeer : NetWindowPeer, java.awt.peer.FramePeer
    {
        public NetFramePeer(java.awt.Frame frame, bool isFocusableWindow, bool isAlwaysOnTop)
            : base(frame, isFocusableWindow, isAlwaysOnTop)
        {
        }

        protected override void initialize()
        {
            base.initialize();
            java.awt.Frame target = (java.awt.Frame)this.target;

            if (target.getTitle() != null)
            {
                setTitle(target.getTitle());
            }
            setResizable(target.isResizable());
            setState(target.getExtendedState());
        }

        public void setMenuBar(java.awt.MenuBar mb)
        {
            if (mb == null)
            {
                NetToolkit.Invoke(delegate
                {
#if NETFRAMEWORK
                    control.Menu = null;
#endif
                    CalcInsetsImpl();
                });
            }
            else
            {
                mb.addNotify();
                NetToolkit.Invoke(delegate
                {
#if NETFRAMEWORK
                    //control.Menu = ((NetMenuBarPeer)mb.getPeer()).menu;
#endif
                    CalcInsetsImpl();
                });
            }
        }

        public void setResizable(bool resizable)
        {
            if (((java.awt.Frame)target).isUndecorated())
            {
                setFormBorderStyle(FormBorderStyle.None);
            }
            else
            {
                if (resizable)
                {
                    setFormBorderStyle(FormBorderStyle.Sizable);
                }
                else
                {
                    setFormBorderStyle(FormBorderStyle.FixedSingle);
                }
            }
        }

        public void setTitle(string title)
        {
            NetToolkit.BeginInvoke(delegate { control.Text = title; });
        }

        public int getState()
        {
            Form f = control;
            FormWindowState state = f.WindowState;
            switch (state)
            {
                case FormWindowState.Normal:
                    return java.awt.Frame.NORMAL;
                case FormWindowState.Maximized:
                    return java.awt.Frame.MAXIMIZED_BOTH;
                case FormWindowState.Minimized:
                    return java.awt.Frame.ICONIFIED;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public void setState(int state)
        {
            NetToolkit.BeginInvoke(delegate
               {
                   MyForm form = (MyForm)control;
                   switch (state)
                   {
                       case java.awt.Frame.NORMAL:
                           form.WindowState = FormWindowState.Normal;
                           break;
                       case java.awt.Frame.MAXIMIZED_BOTH:
                           form.WindowState = FormWindowState.Maximized;
                           break;
                       case java.awt.Frame.ICONIFIED:
                           form.WindowState = FormWindowState.Minimized;
                           break;
                   }
               });
        }

        public void setMaximizedBounds(java.awt.Rectangle rect)
        {
            ((MyForm)control).setMaximizedBounds(rect);
        }

        public void setBoundsPrivate(int x, int y, int width, int height)
        {
            NetToolkit.Invoke(delegate { control.Bounds = new Rectangle(x, y, width, height); });
        }

        public java.awt.Rectangle getBoundsPrivate()
        {
            throw new NotImplementedException();
        }

        protected override Form CreateControl()
        {
            return new MyForm(_insets);
        }

        public void emulateActivation(bool b)
        {
            throw new NotImplementedException();
        }
    }

}
