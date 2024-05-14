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

using ikvm.runtime;

using sun.awt;

using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace IKVM.AWT.WinForms
{
    class NetWindowPeer : NetContainerPeer<java.awt.Window, Form>, java.awt.peer.WindowPeer
    {
        // we can't use NetDialogPeer as blocker may be an instance of NetPrintDialogPeer that
        // extends NetWindowPeer, not NetDialogPeer
        private NetWindowPeer modalBlocker;
        private bool modalSavedEnabled;

        private static NetWindowPeer grabbedWindow;

        public NetWindowPeer(java.awt.Window window, bool isFocusableWindow, bool isAlwaysOnTop) : base(window)
        {
            //form.Shown += new EventHandler(OnOpened); Will already post in java.awt.Window.show()
            control.Closing += new CancelEventHandler(OnClosing);
            control.Closed += new EventHandler(OnClosed);
            control.Activated += new EventHandler(OnActivated);
            control.Deactivate += new EventHandler(OnDeactivate);
            control.SizeChanged += new EventHandler(OnSizeChanged);
            control.Resize += new EventHandler(OnResize);
            control.Move += new EventHandler(OnMove);
            ((UndecoratedForm)control).SetWindowState(isFocusableWindow, isAlwaysOnTop);
        }

        protected override void initialize()
        {
            base.initialize();
            updateIconImages();
            if (target.getBackground() == null)
            {
                AWTAccessor.getComponentAccessor().setBackground(target, target is java.awt.Dialog ? java.awt.SystemColor.control : java.awt.SystemColor.window);
            }
            control.BackColor = J2C.ConvertColor(target.getBackground());
            if (target.getForeground() == null)
            {
                target.setForeground(java.awt.SystemColor.windowText);
            }
            if (target.getFont() == null)
            {
                //target.setFont(defaultFont);
                //HACK: Sun is calling setFont(Font) here and this is calling firePropertyChange("font", oldFont, newFont)
                //but this produce a deadlock with getTreeLock() because the creating of the peer is already in this synchronized
                java.security.AccessController.doPrivileged(Delegates.toPrivilegedAction(delegate
                {
                    java.lang.Class component = typeof(java.awt.Component);
                    java.lang.reflect.Field field = component.getDeclaredField("font");
                    field.setAccessible(true);
                    field.set(target, defaultFont);
                    java.lang.reflect.Method method = component.getDeclaredMethod("firePropertyChange", typeof(java.lang.String), typeof(java.lang.Object), typeof(java.lang.Object));
                    method.setAccessible(true);
                    method.invoke(target, "font", null, defaultFont);
                    return null;
                }));
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            // WmSizing
            SendComponentEvent(java.awt.@event.ComponentEvent.COMPONENT_RESIZED);
            dynamicallyLayoutContainer();
        }

        private void OnMove(object sender, EventArgs e)
        {
            // WmMove
            AWTAccessor.getComponentAccessor().setLocation(target, control.Left, control.Top);
            SendComponentEvent(java.awt.@event.ComponentEvent.COMPONENT_MOVED);
        }

        /*
		 * Although this function sends ComponentEvents, it needs to be defined
		 * here because only top-level windows need to have move and resize
		 * events fired from native code.  All contained windows have these events
		 * fired from common Java code.
		 */
        private void SendComponentEvent(int eventId)
        {
            SendEvent(new java.awt.@event.ComponentEvent(target, eventId));
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            // WmSize
            typeof(java.awt.Component).GetField("width", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, control.Width);
            typeof(java.awt.Component).GetField("height", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, control.Height);
            SendComponentEvent(java.awt.@event.ComponentEvent.COMPONENT_RESIZED);
        }

        private void OnOpened(object sender, EventArgs e)
        {
            postEvent(new java.awt.@event.WindowEvent(target, java.awt.@event.WindowEvent.WINDOW_OPENED));
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            postEvent(new java.awt.@event.WindowEvent(target, java.awt.@event.WindowEvent.WINDOW_CLOSING));
        }

        private void OnClosed(object sender, EventArgs e)
        {
            postEvent(new java.awt.@event.WindowEvent(target, java.awt.@event.WindowEvent.WINDOW_CLOSED));
        }

        private const int WA_ACTIVE = 1;
        private const int WA_INACTIVE = 2;

        private void OnActivated(object sender, EventArgs e)
        {
            WmActivate(WA_ACTIVE, control.WindowState == FormWindowState.Minimized, null);
        }

        private void OnDeactivate(object sender, EventArgs e)
        {
            WmActivate(WA_INACTIVE, control.WindowState == FormWindowState.Minimized, null);
        }

        private void WmActivate(int nState, bool fMinimized, Control opposite)
        {
            int type;

            if (nState != WA_INACTIVE)
            {
                type = java.awt.@event.WindowEvent.WINDOW_GAINED_FOCUS;
            }
            else
            {
                if (grabbedWindow != null && !grabbedWindow.IsOneOfOwnersOf(this))
                {
                    grabbedWindow.Ungrab(true);
                }
                type = java.awt.@event.WindowEvent.WINDOW_LOST_FOCUS;
            }

            SendWindowEvent(type, opposite);
        }

        private void SendWindowEvent(int id, Control opposite) { SendWindowEvent(id, opposite, 0, 0); }

        private void SendWindowEvent(int id, Control opposite, int oldState, int newState)
        {
            java.awt.AWTEvent evt = new java.awt.@event.WindowEvent(target, id, null);

            if (id == java.awt.@event.WindowEvent.WINDOW_GAINED_FOCUS || id == java.awt.@event.WindowEvent.WINDOW_LOST_FOCUS)
            {
                Type type = typeof(java.awt.Component).Assembly.GetType("java.awt.SequencedEvent");
                ConstructorInfo cons = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, [typeof(java.awt.AWTEvent)], null);
                evt = (java.awt.AWTEvent)cons.Invoke([evt]);
            }

            SendEvent(evt);
        }

        public override java.awt.Graphics getGraphics()
        {
            java.awt.Graphics g = base.getGraphics();
            java.awt.Insets insets = getInsets();
            g.translate(-insets.left, -insets.top);
            g.setClip(insets.left, insets.top, control.ClientRectangle.Width, control.ClientRectangle.Height);
            return g;
        }

        public override bool shouldClearRectBeforePaint()
        {
            // clearing the window before repainting causes the controls to "flicker" on screen
            return false;
        }

        /// <summary>
        /// Set the border style of the window and recalc the insets
        /// </summary>
        /// <param name="style">the new style</param>
        protected void setFormBorderStyle(FormBorderStyle style)
        {
            NetToolkit.BeginInvoke(delegate
            {
                control.FormBorderStyle = style;
                //Calculate the Insets one time
                //This is many faster because there no thread change is needed.
                CalcInsetsImpl();
            });
        }

        protected void CalcInsetsImpl()
        {
            Rectangle client = control.ClientRectangle;
            if (client.Height == 0)
            {
                // HACK for .NET bug if form has the minimum size then ClientRectangle is not recalulate
                // if the FormBorderStyle is changed
                Size size = control.Size;
                size.Height++;
                control.Size = size;
                size.Height--;
                control.Size = size;
                client = control.ClientRectangle;
            }
            Rectangle r = control.RectangleToScreen(client);
            int x = r.Location.X - control.Location.X;
            int y = r.Location.Y - control.Location.Y;
            // only modify this instance, since it's shared by the control-peers of this form
            _insets.top = y;
            _insets.left = x;
            _insets.bottom = control.Height - client.Height - y;
#if NETFRAMEWORK
            if (control.Menu != null)
            {
                _insets.bottom += SystemInformation.MenuHeight;
            }
#endif
            _insets.right = control.Width - client.Width - x;
        }

        public override void reshape(int x, int y, int width, int height)
        {
            NetToolkit.BeginInvoke(delegate
            {
                control.SetBounds(x, y, width, height);
                //If the .NET control does not accept the new bounds (minimum size, maximum size) 
                //then we need to reflect the real bounds on the .NET site to the Java site
                Rectangle bounds = control.Bounds;
                if (bounds.X != x || bounds.Y != y)
                {
                    AWTAccessor.getComponentAccessor().setLocation(target, bounds.X, bounds.Y);
                }
                if (bounds.Width != width || bounds.Height != height)
                {
                    AWTAccessor.getComponentAccessor().setSize(target, bounds.Width, bounds.Height);
                }
            });
        }

        public void toBack()
        {
            NetToolkit.BeginInvoke(control.SendToBack);
        }

        public void toFront()
        {
            NetToolkit.BeginInvoke(control.Activate);
        }

        public bool requestWindowFocus()
        {
            return NetToolkit.Invoke(control.Focus);
        }

        public void updateAlwaysOnTopState()
        {
            // The .NET property TopMost does not work with a not focusable Window
            // that we need to set the window flags directly. To reduce double code
            // we call updateFocusableWindowState().
            updateFocusableWindowState();
        }

        public bool isModalBlocked()
        {
            return modalBlocker != null;
        }

        public void setModalBlocked(java.awt.Dialog dialog, bool blocked)
        {
            lock (target.getTreeLock()) // State lock should always be after awtLock
            {
                // use NetWindowPeer instead of NetDialogPeer because of FileDialogs and PrintDialogs
                NetWindowPeer blockerPeer = (NetWindowPeer)dialog.getPeer();
                if (blocked)
                {
                    modalBlocker = blockerPeer;
                    modalSavedEnabled = control.Enabled;
                    disable();
                }
                else
                {
                    modalBlocker = null;
                    if (modalSavedEnabled)
                    {
                        enable();
                    }
                    else
                    {
                        disable();
                    }
                }
            }
        }

        public void updateFocusableWindowState()
        {
            ((UndecoratedForm)control).SetWindowState(target.isFocusableWindow(), target.isAlwaysOnTop());
        }

        public void updateIconImages()
        {
            java.util.List imageList = target.getIconImages();
            Icon icon;
            if (imageList == null || imageList.size() == 0)
            {
                icon = null;
            }
            else
            {
                IconFactory factory = new();
                icon = factory.CreateIcon(imageList, SystemInformation.IconSize);
            }
            NetToolkit.BeginInvoke(delegate
               {
                   control.Icon = icon;
               });
        }

        public void updateMinimumSize()
        {
            java.awt.Dimension dim = target.getMinimumSize();
            NetToolkit.BeginInvoke(delegate
            {
                control.MinimumSize = new Size(dim.width, dim.height);
            });
        }

        /**
         * Sets the level of opacity for the window.
         *
         * @see Window#setOpacity(float)
         */
        public void setOpacity(float opacity)
        {
            throw new ikvm.@internal.NotYetImplementedError();
        }

        /**
         * Enables the per-pixel alpha support for the window.
         *
         * @see Window#setBackground(Color)
         */
        public void setOpaque(bool isOpaque)
        {
            throw new ikvm.@internal.NotYetImplementedError();
        }


        /**
         * Updates the native part of non-opaque window.
         *
         * @see Window#setBackground(Color)
         */
        public void updateWindow()
        {
            throw new ikvm.@internal.NotYetImplementedError();
        }


        /**
         * Instructs the peer to update the position of the security warning.
         */
        public void repositionSecurityWarning()
        {
            throw new ikvm.@internal.NotYetImplementedError();
        }



        protected override Form CreateControl()
        {
            return new UndecoratedForm();
        }

        protected override void OnMouseDown(object sender, MouseEventArgs ev)
        {
            if (grabbedWindow != null && !grabbedWindow.IsOneOfOwnersOf(this))
            {
                grabbedWindow.Ungrab(true);
            }
            base.OnMouseDown(sender, ev);
        }

        internal void Grab()
        {
            //copy from file awt_Windows.cpp
            grabbedWindow?.Ungrab(true);
            grabbedWindow = this;
            if (Form.ActiveForm == null)
            {
                Ungrab(true);
            }
            else if (control != Form.ActiveForm)
            {
                toFront();
            }
        }

        internal void Ungrab(bool doPost)
        {
            //copy from file awt_Windows.cpp
            if (grabbedWindow == this)
            {
                if (doPost)
                {
                    SendEvent(new UngrabEvent(target));
                }
                grabbedWindow = null;
            }
        }

        private bool IsOneOfOwnersOf(NetWindowPeer window)
        {
            while (window != null)
            {
                if (window == this)
                {
                    return true;
                }
                java.awt.Container parent = window.target.getParent();
                window = parent == null
                       ? null
                       : (NetWindowPeer)parent.getPeer();
            }
            return false;
        }
    }

}
