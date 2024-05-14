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

using System.Windows.Forms;

using ikvm.runtime;

namespace IKVM.AWT.WinForms
{
    sealed class NetKeyboardFocusManagerPeer : java.awt.peer.KeyboardFocusManagerPeer
    {
        private static java.lang.reflect.Method m_removeLastFocusRequest;

        public void clearGlobalFocusOwner(java.awt.Window activeWindow)
        {
        }

        public java.awt.Component getCurrentFocusOwner()
        {
            return getNativeFocusOwner();
        }

        public void setCurrentFocusOwner(java.awt.Component component)
        {
        }

        public java.awt.Window getCurrentFocusedWindow()
        {
            return getNativeFocusedWindow();
        }

        public void setCurrentFocusedWindow(java.awt.Window w)
        {
        }

        private static java.awt.Component getNativeFocusOwner()
        {
            return NetToolkit.Invoke(delegate
            {
                if (Form.ActiveForm is UndecoratedForm form)
                {
                    Control control = form.ActiveControl;
                    while (control is ContainerControl container)
                    {
                        control = container.ActiveControl;
                    }
                    NetComponentPeer peer;
                    if (control == null)
                    {
                        peer = NetComponentPeer.FromControl(form);
                    }
                    else
                    {
                        while ((peer = NetComponentPeer.FromControl(form)) == null)
                        {
                            control = control.Parent;
                        }
                    }
                    return peer.Target;
                }
                return null;
            });
        }

        private static java.awt.Window getNativeFocusedWindow()
        {
            return NetToolkit.Invoke(delegate
            {
                Form form = Form.ActiveForm;
                if (form != null)
                {
                    NetComponentPeer peer = NetComponentPeer.FromControl(form);
                    if (peer != null)
                    {
                        return (java.awt.Window)peer.Target;
                    }
                }
                return null;
            });
        }

        public static void removeLastFocusRequest(java.awt.Component heavyweight)
        {
            try
            {
                if (m_removeLastFocusRequest == null)
                {
                    java.security.AccessController.doPrivileged(Delegates.toPrivilegedAction(delegate
                    {
                        java.lang.Class keyboardFocusManagerCls = typeof(java.awt.KeyboardFocusManager);
                        java.lang.reflect.Method method = keyboardFocusManagerCls.getDeclaredMethod(
                            "removeLastFocusRequest",
                            typeof(java.awt.Component));
                        method.setAccessible(true);
                        m_removeLastFocusRequest = method;
                        return null;
                    }));
                }
                m_removeLastFocusRequest.invoke(null, [heavyweight]);
            }
            catch (java.lang.reflect.InvocationTargetException ite)
            {
                ite.printStackTrace();
            }
            catch (java.lang.IllegalAccessException ex)
            {
                ex.printStackTrace();
            }
        }
    }

}
