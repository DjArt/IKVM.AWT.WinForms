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

using ikvm.runtime;

using sun.awt;

namespace IKVM.AWT.WinForms
{
    class NetTrayIconPeer : java.awt.peer.TrayIconPeer
    {
        internal const int TRAY_ICON_WIDTH = 16;
        internal const int TRAY_ICON_HEIGHT = 16;
        internal const int TRAY_ICON_MASK_SIZE = TRAY_ICON_WIDTH * TRAY_ICON_HEIGHT / 8;

        private java.awt.TrayIcon target;
        private NotifyIcon notifyIcon;
        private java.awt.Frame popupParent = new("PopupMessageWindow");
        private java.awt.PopupMenu popup;
        private bool disposed;
        private bool isPopupMenu;

        internal NetTrayIconPeer(java.awt.TrayIcon target)
        {
            this.target = target;
            popupParent.addNotify();
            create();
            updateImage();
        }

        public void displayMessage(string caption, string text, string messageType)
        {
            ToolTipIcon icon = ToolTipIcon.None;
            switch (messageType)
            {
                case "ERROR":
                    icon = ToolTipIcon.Error;
                    break;
                case "WARNING":
                    icon = ToolTipIcon.Warning;
                    break;
                case "INFO":
                    icon = ToolTipIcon.Info;
                    break;
                case "NONE":
                    icon = ToolTipIcon.None;
                    break;
            }
            NetToolkit.BeginInvoke(delegate
            {
                notifyIcon.ShowBalloonTip(10000, caption, text, icon);
            });
        }

        private void create()
        {
            NetToolkit.Invoke(delegate
            {
                notifyIcon = new NotifyIcon();
                hookEvents();
                notifyIcon.Visible = true;
            });
        }

        public void dispose()
        {
            bool callDisposed = true;
            lock (this)
            {
                if (disposed)
                    callDisposed = false;
                disposed = true;
            }
            if (callDisposed)
                disposeImpl();
        }

        protected void disposeImpl()
        {
            popupParent?.dispose();
            NetToolkit.targetDisposedPeer(target, this);
            NetToolkit.BeginInvoke(nativeDispose);
        }

        private void hookEvents()
        {
            notifyIcon.MouseClick += new MouseEventHandler(OnClick);
            notifyIcon.MouseDoubleClick += new MouseEventHandler(OnDoubleClick);
            notifyIcon.MouseDown += new MouseEventHandler(OnMouseDown);
            notifyIcon.MouseUp += new MouseEventHandler(OnMouseUp);
            notifyIcon.MouseMove += new MouseEventHandler(OnMouseMove);
        }

        private void unhookEvents()
        {
            notifyIcon.MouseClick -= new MouseEventHandler(OnClick);
            notifyIcon.MouseDoubleClick -= new MouseEventHandler(OnDoubleClick);
            notifyIcon.MouseDown -= new MouseEventHandler(OnMouseDown);
            notifyIcon.MouseUp -= new MouseEventHandler(OnMouseUp);
            notifyIcon.MouseMove -= new MouseEventHandler(OnMouseMove);
        }

        internal void postEvent(java.awt.AWTEvent evt)
        {
            SunToolkit.postEvent(SunToolkit.targetToAppContext(target), evt);
        }

        private void postMouseEvent(MouseEventArgs ev, int id, int clicks)
        {
            long when = java.lang.System.currentTimeMillis();
            int modifiers = NetComponentPeer.GetMouseEventModifiers(ev);
            int button = NetComponentPeer.GetButton(ev);
            int clickCount = clicks;
            int x = Control.MousePosition.X;
            int y = Control.MousePosition.Y;
            bool isPopup = isPopupMenu;
            java.awt.EventQueue.invokeLater(Delegates.toRunnable(delegate
            {
                java.awt.Component fake = new java.awt.TextField();
                java.awt.@event.MouseEvent mouseEvent = new(fake, id, when, modifiers, x, y, clickCount, isPopup, button);
                mouseEvent.setSource(target);
                postEvent(mouseEvent);
            }));
            isPopupMenu = false;
        }

        private void postMouseEvent(EventArgs ev, int id)
        {
            long when = java.lang.System.currentTimeMillis();
            int modifiers = NetComponentPeer.GetModifiers(Control.ModifierKeys);
            int button = 0;
            int clickCount = 0;
            int x = Control.MousePosition.X;
            int y = Control.MousePosition.Y;
            bool isPopup = isPopupMenu;
            java.awt.EventQueue.invokeLater(Delegates.toRunnable(delegate
            {
                java.awt.Component fake = new java.awt.TextField();
                java.awt.@event.MouseEvent mouseEvent = new(fake, id, when, modifiers, x, y, clickCount, isPopup, button);
                mouseEvent.setSource(target);
                postEvent(mouseEvent);
            }));
            isPopupMenu = false;
        }

        protected virtual void OnMouseDown(object sender, MouseEventArgs ev)
        {
            isPopupMenu = false;
            postMouseEvent(ev, java.awt.@event.MouseEvent.MOUSE_PRESSED, ev.Clicks);
        }

        private void OnClick(object sender, MouseEventArgs ev)
        {
            postMouseEvent(ev, java.awt.@event.MouseEvent.MOUSE_CLICKED, ev.Clicks);
        }

        private void OnDoubleClick(object sender, MouseEventArgs ev)
        {
            postMouseEvent(ev, java.awt.@event.MouseEvent.MOUSE_CLICKED, ev.Clicks);
            long when = java.lang.System.currentTimeMillis();
            int modifiers = NetComponentPeer.GetModifiers(Control.ModifierKeys);
            postEvent(new java.awt.@event.ActionEvent(target, java.awt.@event.ActionEvent.ACTION_PERFORMED, target.getActionCommand(), when, modifiers));
        }

        private void OnMouseUp(object sender, MouseEventArgs ev)
        {
            postMouseEvent(ev, java.awt.@event.MouseEvent.MOUSE_RELEASED, ev.Clicks);
        }

        private void OnMouseEnter(object sender, EventArgs ev)
        {
            postMouseEvent(ev, java.awt.@event.MouseEvent.MOUSE_ENTERED);
        }

        private void OnMouseLeave(object sender, EventArgs ev)
        {
            postMouseEvent(ev, java.awt.@event.MouseEvent.MOUSE_EXITED);
        }

        protected virtual void OnMouseMove(object sender, MouseEventArgs ev)
        {
            if ((ev.Button & (MouseButtons.Left | MouseButtons.Right)) != 0)
            {
                postMouseEvent(ev, java.awt.@event.MouseEvent.MOUSE_DRAGGED, ev.Clicks);
            }
            else
            {
                postMouseEvent(ev, java.awt.@event.MouseEvent.MOUSE_MOVED, ev.Clicks);
            }
        }

        private void nativeDispose()
        {
            if (notifyIcon != null)
            {
                unhookEvents();
                notifyIcon.Dispose();
            }
        }

        public void setToolTip(string str)
        {
            NetToolkit.BeginInvoke(delegate { notifyIcon.Text = str; });
        }

        protected bool isDisposed()
        {
            return disposed;
        }

        public void showPopupMenu(int x, int y)
        {
            if (isDisposed())
                return;
            java.lang.Runnable runnable = Delegates.toRunnable(delegate
            {
                java.awt.PopupMenu newPopup = target.getPopupMenu();
                if (popup != newPopup)
                {
                    if (popup != null)
                    {
                        popupParent.remove(popup);
                    }
                    if (newPopup != null)
                    {
                        popupParent.add(newPopup);
                    }
                    popup = newPopup;
                }
                if (popup != null)
                {
                    ((NetPopupMenuPeer)popup.getPeer()).show(popupParent, new java.awt.Point(x, y));
                }
            });
            SunToolkit.executeOnEventHandlerThread(target, runnable);
        }

        public void updateImage()
        {
            java.awt.Image image = target.getImage();
            if (image != null)
            {
                updateNativeImage(image);
            }
        }

        private void updateNativeImage(java.awt.Image image)
        {
            lock (this)
            {
                if (isDisposed())
                    return;
                bool autosize = target.isImageAutoSize();

                using Bitmap bitmap = getNativeImage(image, autosize);
                nint hicon = bitmap.GetHicon();
                Icon icon = Icon.FromHandle(hicon);
                notifyIcon.Icon = icon;
            }
        }

        private Bitmap getNativeImage(java.awt.Image image, bool autosize)
        {
            if (image is NoImage)
            {
                return new Bitmap(TRAY_ICON_WIDTH, TRAY_ICON_HEIGHT);
            }
            else
            {
                Image netImage = J2C.ConvertImage(image);
                return autosize ? new Bitmap(netImage, TRAY_ICON_WIDTH, TRAY_ICON_HEIGHT) : new Bitmap(netImage);
            }
        }
    }

}
