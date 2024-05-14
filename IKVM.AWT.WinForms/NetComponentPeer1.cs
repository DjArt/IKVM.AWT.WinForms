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
    abstract class NetComponentPeer<T, C> : NetComponentPeer
        where T : java.awt.Component
        where C : Control
    {
        protected static readonly java.awt.Font defaultFont = new(java.awt.Font.DIALOG, java.awt.Font.PLAIN, 12);
        internal readonly T target;
        internal readonly C control;
        private bool isMouseClick;
        private bool isDoubleClick;
        private bool isPopupMenu;
        private int oldWidth = -1;
        private int oldHeight = -1;
        private bool sm_suppressFocusAndActivation;
        //private bool m_callbacksEnabled;
        //private int m_validationNestCount;
        private int serialNum = 0;
        private bool isLayouting = false;
        private bool paintPending = false;
        private RepaintArea paintArea;
        private java.awt.Font font;
        private java.awt.Color foreground;
        private java.awt.Color background;
        private volatile bool disposed;
        private NetDropTargetContextPeer dropTargetPeer;

        internal override Control Control => control;

        internal override java.awt.Component Target => target;

        public NetComponentPeer(T target)
        {
            this.target = target;
            paintArea = new RepaintArea();
            // A window has an owner, but it does NOT have a container. 
            // Component getNativeContainer() was changed in 8.2 so it returns null for Window
            // We have to use getParent() instead
            //java.awt.Container parent = SunToolkit.getNativeContainer(target);
            java.awt.Component parent = SunToolkit.getHeavyweightComponent(target.getParent());
            NetComponentPeer parentPeer = (NetComponentPeer)NetToolkit.targetToPeer(parent);
            control = Create(parentPeer);
            // fix for 5088782: check if window object is created successfully
            //checkCreation();
            //this.winGraphicsConfig = (NetGraphicsConfiguration)getGraphicsConfiguration();
            /*
			this.surfaceData =
				winGraphicsConfig.createSurfaceData(this, numBackBuffers);
			 */
            initialize();
            start();  // Initialize enable/disable state, turn on callbacks
        }

        protected virtual void initialize()
        {
            if (target.isVisible())
            {
                show();  // the wnd starts hidden
            }
            java.awt.Color fg = target.getForeground();
            if (fg != null)
            {
                setForeground(fg);
            }
            // Set background color in C++, to avoid inheriting a parent's color.
            java.awt.Font f = target.getFont();
            if (f != null)
            {
                setFont(f);
            }
            if (!target.isEnabled())
            {
                disable();
            }
            java.awt.Rectangle r = target.getBounds();
            setBounds(r.x, r.y, r.width, r.height, java.awt.peer.ComponentPeer.__Fields.SET_BOUNDS);

            // this is from initialize() in WCanvasPeer.java
            eraseBackground = !SunToolkit.getSunAwtNoerasebackground();
            if (!PaintEventDispatcher.getPaintEventDispatcher().shouldDoNativeBackgroundErase(target))
            {
                eraseBackground = false;
            }
        }

        void start()
        {
            NetToolkit.BeginInvoke(delegate
            {
                hookEvents();
                // JDK native code also disables the window here, but since that is already done in initialize(),
                // I don't see the point
                EnableCallbacks(true);
                control.Invalidate();
                control.Update();
            });
        }

        void EnableCallbacks(bool enabled)
        {
            //m_callbacksEnabled = enabled;
        }

        private C Create(NetComponentPeer parent)
        {
            C control = CreateControl();
            control.Tag = this;
            if (parent != null)
            {
                if (control is Form form)
                {
                    form.Owner = parent.Control.FindForm();
                }
                else
                {
                    control.Parent = parent.Control;
                }
            }
            NetToolkit.CreateNative(control);
            return control;
        }

        protected abstract C CreateControl();

        void pShow()
        {
            NetToolkit.BeginInvoke(delegate { control.Visible = true; });
        }

        void Enable(bool enable)
        {
            sm_suppressFocusAndActivation = true;
            control.Enabled = enable;
            sm_suppressFocusAndActivation = false;
        }

        internal virtual void hookEvents()
        {
            // TODO we really only should hook these events when they are needed...
            control.KeyDown += new KeyEventHandler(OnKeyDown);
            control.KeyUp += new KeyEventHandler(OnKeyUp);
            control.KeyPress += new KeyPressEventHandler(OnKeyPress);
            control.MouseMove += new MouseEventHandler(OnMouseMove);
            control.MouseDown += new MouseEventHandler(OnMouseDown);
            control.MouseWheel += new MouseEventHandler(OnMouseWheel);
            control.Click += new EventHandler(OnClick);
            control.DoubleClick += new EventHandler(OnDoubleClick);
            control.MouseUp += new MouseEventHandler(OnMouseUp);
            control.MouseEnter += new EventHandler(OnMouseEnter);
            control.MouseLeave += new EventHandler(OnMouseLeave);
            control.GotFocus += new EventHandler(OnGotFocus);
            control.LostFocus += new EventHandler(OnLostFocus);
            //control.Leave += new EventHandler(OnBoundsChanged);
            control.Paint += new PaintEventHandler(OnPaint);
#if NETFRAMEWORK
            control.ContextMenu = new ContextMenu();
            control.ContextMenu.Popup += new EventHandler(OnPopupMenu);
#endif
            control.AllowDrop = true;
            control.DragDrop += new DragEventHandler(OnDragDrop);
            control.DragOver += new DragEventHandler(OnDragOver);
            control.DragLeave += new EventHandler(OnDragLeave);
            control.DragEnter += new DragEventHandler(OnDragEnter);
            control.QueryContinueDrag += new QueryContinueDragEventHandler(OnQueryContinueDrag);
        }

        internal virtual void unhookEvents()
        {
            control.KeyDown -= new KeyEventHandler(OnKeyDown);
            control.KeyUp -= new KeyEventHandler(OnKeyUp);
            control.KeyPress -= new KeyPressEventHandler(OnKeyPress);
            control.MouseMove -= new MouseEventHandler(OnMouseMove);
            control.MouseDown -= new MouseEventHandler(OnMouseDown);
            control.MouseWheel -= new MouseEventHandler(OnMouseWheel);
            control.Click -= new EventHandler(OnClick);
            control.DoubleClick -= new EventHandler(OnDoubleClick);
            control.MouseUp -= new MouseEventHandler(OnMouseUp);
            control.MouseEnter -= new EventHandler(OnMouseEnter);
            control.MouseLeave -= new EventHandler(OnMouseLeave);
            control.GotFocus -= new EventHandler(OnGotFocus);
            control.LostFocus -= new EventHandler(OnLostFocus);
            //control.Leave -= new EventHandler(OnBoundsChanged);
            control.Paint -= new PaintEventHandler(OnPaint);
            control.DragDrop -= new DragEventHandler(OnDragDrop);
            control.DragOver -= new DragEventHandler(OnDragOver);
            control.DragLeave -= new EventHandler(OnDragLeave);
            control.DragEnter -= new DragEventHandler(OnDragEnter);
#if NETFRAMEWORK
            if (control.ContextMenu != null)
                control.ContextMenu.Popup -= new EventHandler(OnPopupMenu);
#endif
        }

        protected void SendEvent(java.awt.AWTEvent evt)
        {
            postEvent(evt);
        }

        /// <summary>
        /// Get the left insets of the .NET Window.
        /// In .NET the coordinate of a window start on the most left, top point with 0,0
        /// In Java the most left, top point with 0,0 is in the detail area of the window.
        /// In all not Windows Component this return ever 0.
        /// </summary>
        /// <returns></returns>
		internal override int getInsetsLeft()
        {
            return 0;
        }

        /// <summary>
        /// Get the top insets of the .NET Window.
        /// In .NET the coordinate of a window start on the most left, top point with 0,0
        /// In Java the most left, top point with 0,0 is in the detail area of the window.
        /// In all not Windows Component this return ever 0.
        /// </summary>
        /// <returns></returns>
		internal override int getInsetsTop()
        {
            return 0;
        }


        /// <summary>
        /// .NET calculates the offset relative to the detail area.
        /// Java uses the top left point of a window.
        /// That means we must compensate the coordinate of a component
        /// if the parent is a window, frame or dialog.
        /// </summary>
        /// <returns>The offset of the details area in the parent</returns>
        private Point getParentOffset()
        {
            if (target is not java.awt.Window)
            {
                java.awt.Container parent = target.getParent();
                if (parent?.getPeer() is NetComponentPeer peer)
                {
                    return new Point(peer.getInsetsLeft(), peer.getInsetsTop());
                }
            }
            return new Point();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            //CheckFontSmoothingSettings(GetHWnd());
            /* Set draw state */
            //SetDrawState(GetDrawState() | JAWT_LOCK_CLIP_CHANGED);
            WmPaint(e.Graphics, e.ClipRectangle);
        }

        private void WmPaint(Graphics g, Rectangle r)
        {
            handlePaint(r.X + getInsetsLeft(), r.Y + getInsetsTop(), r.Width, r.Height);
        }

        /* Invoke a paint() method call on the target, without clearing the
		 * damaged area.  This is normally called by a native control after
		 * it has painted itself.
		 *
		 * NOTE: This is called on the privileged toolkit thread. Do not
		 *       call directly into user code using this thread!
		 */
        private void handlePaint(int x, int y, int w, int h)
        {
            postPaintIfNecessary(x, y, w, h);
        }

        private void postPaintIfNecessary(int x, int y, int w, int h)
        {
            if (!AWTAccessor.getComponentAccessor().getIgnoreRepaint(target))
            {
                java.awt.@event.PaintEvent evt = PaintEventDispatcher.getPaintEventDispatcher().createPaintEvent(target, x, y, w, h);
                if (evt != null)
                {
                    postEvent(evt);
                }
            }
        }

        private static int MapKeyCode(Keys key)
        {
            switch (key)
            {
                case Keys.Delete:
                    return java.awt.@event.KeyEvent.VK_DELETE;

                case Keys.Enter:
                    return java.awt.@event.KeyEvent.VK_ENTER;

                default:
                    return (int)key;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            long when = java.lang.System.currentTimeMillis();
            int modifiers = GetModifiers(e.Modifiers);
            int keyCode = MapKeyCode(e.KeyCode);
            // TODO set keyChar
            char keyChar = ' ';
            int keyLocation = java.awt.@event.KeyEvent.KEY_LOCATION_STANDARD;
            java.awt.EventQueue.invokeLater(Delegates.toRunnable(delegate
            {
                postEvent(new java.awt.@event.KeyEvent(target, java.awt.@event.KeyEvent.KEY_PRESSED, when, modifiers, keyCode, keyChar, keyLocation));
            }));
        }

        private void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            IDataObject obj = e.Data;
            long[] formats = NetDataTransferer.getInstanceImpl().getClipboardFormatCodes(obj.GetFormats());
            dropTargetPeer = NetDropTargetContextPeer.getNetDropTargetContextPeer();
            int actions = dropTargetPeer.handleEnterMessage(target, e.X, e.Y, getDropAction(e.AllowedEffect, e.KeyState), getAction(e.AllowedEffect), formats, 0);
            e.Effect = getDragDropEffects(actions);
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            IDataObject obj = e.Data;
            long[] formats = NetDataTransferer.getInstanceImpl().getClipboardFormatCodes(obj.GetFormats());
            dropTargetPeer = NetDropTargetContextPeer.getNetDropTargetContextPeer();
            int actions = dropTargetPeer.handleMotionMessage(target, e.X, e.Y, getDropAction(e.AllowedEffect, e.KeyState), getAction(e.AllowedEffect), formats, 0);
            e.Effect = getDragDropEffects(actions);
        }

        private void OnDragLeave(object sender, EventArgs e)
        {
            dropTargetPeer?.handleExitMessage(target, 0);
            dropTargetPeer = null;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            IDataObject obj = e.Data;
            long[] formats = NetDataTransferer.getInstanceImpl().getClipboardFormatCodes(obj.GetFormats());
            int actions = getAction(e.Effect);
            dropTargetPeer?.handleDropMessage(target, e.X, e.Y, getAction(e.Effect), getAction(e.AllowedEffect), formats, 0, e.Data);
            NetDragSourceContextPeer.getInstance().dragDropFinished(true, actions, e.X, e.Y);
            performedDragDropEffects = e.Effect;
            dropTargetPeer = null;
        }

        private static DragDropEffects getDragDropEffects(int actions)
        {
            switch (actions)
            {
                case java.awt.dnd.DnDConstants.ACTION_COPY:
                    return DragDropEffects.Copy;
                case java.awt.dnd.DnDConstants.ACTION_MOVE:
                    return DragDropEffects.Move;
                case java.awt.dnd.DnDConstants.ACTION_COPY_OR_MOVE:
                    return DragDropEffects.Move | DragDropEffects.Copy;
                case java.awt.dnd.DnDConstants.ACTION_LINK:
                    return DragDropEffects.Link;
                default:
                    return DragDropEffects.None;
            }
        }

        private static int getDropAction(DragDropEffects effects, int keyState)
        {
            int ret = java.awt.dnd.DnDConstants.ACTION_NONE;
            const int MK_CONTROL = 0x8;
            const int MK_SHIFT = 0x4;
            //            const int WM_MOUSEWHEEL = 0x20A;
            //            const int MK_LBUTTON = 0x1;
            //            const int MK_MBUTTON = 0x10;
            //            const int MK_RBUTTON = 0x2;
            //            const int MK_XBUTTON1 = 0x20;
            //            const int MK_XBUTTON2 = 0x40;
            switch (keyState & (MK_CONTROL | MK_SHIFT))
            {
                case MK_CONTROL:
                    ret = (effects & DragDropEffects.Copy) == DragDropEffects.Copy
                        ? java.awt.dnd.DnDConstants.ACTION_COPY
                        : java.awt.dnd.DnDConstants.ACTION_NONE;
                    break;

                case MK_CONTROL | MK_SHIFT:
                    ret = (effects & DragDropEffects.Link) == DragDropEffects.Link
                        ? java.awt.dnd.DnDConstants.ACTION_LINK
                        : java.awt.dnd.DnDConstants.ACTION_NONE;
                    break;

                case MK_SHIFT:
                    ret = (effects & DragDropEffects.Move) == DragDropEffects.Move
                        ? java.awt.dnd.DnDConstants.ACTION_MOVE
                        : java.awt.dnd.DnDConstants.ACTION_NONE;
                    break;

                default:
                    if ((effects & DragDropEffects.Move) == DragDropEffects.Move)
                    {
                        ret = java.awt.dnd.DnDConstants.ACTION_MOVE;
                    }
                    else if ((effects & DragDropEffects.Copy) == DragDropEffects.Copy)
                    {
                        ret = java.awt.dnd.DnDConstants.ACTION_COPY;
                    }
                    else if ((effects & DragDropEffects.Link) == DragDropEffects.Link)
                    {
                        ret = java.awt.dnd.DnDConstants.ACTION_LINK;
                    }
                    break;
            }

            return ret;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            long when = java.lang.System.currentTimeMillis();
            int modifiers = GetModifiers(e.Modifiers);
            int keyCode = MapKeyCode(e.KeyCode);
            // TODO set keyChar
            char keyChar = ' ';
            int keyLocation = java.awt.@event.KeyEvent.KEY_LOCATION_STANDARD;
            java.awt.EventQueue.invokeLater(Delegates.toRunnable(delegate
            {
                postEvent(new java.awt.@event.KeyEvent(target, java.awt.@event.KeyEvent.KEY_RELEASED, when, modifiers, keyCode, keyChar, keyLocation));
            }));
        }

        protected virtual void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            long when = java.lang.System.currentTimeMillis();
            int modifiers = GetModifiers(Control.ModifierKeys);
            int keyCode = java.awt.@event.KeyEvent.VK_UNDEFINED;
            char keyChar = e.KeyChar;
            int keyLocation = java.awt.@event.KeyEvent.KEY_LOCATION_UNKNOWN;
            java.awt.EventQueue.invokeLater(Delegates.toRunnable(delegate
            {
                postEvent(new java.awt.@event.KeyEvent(target, java.awt.@event.KeyEvent.KEY_TYPED, when, modifiers, keyCode, keyChar, keyLocation));
            }));
        }

        private void postMouseEvent(MouseEventArgs ev, int id, int clicks)
        {
            long when = java.lang.System.currentTimeMillis();
            int modifiers = GetMouseEventModifiers(ev);
            int button = GetButton(ev);
            int clickCount = clicks;
            int x = ev.X + getInsetsLeft(); //The Inset correctur is needed for Window and extended classes
            int y = ev.Y + getInsetsTop();
            bool isPopup = isPopupMenu;
            java.awt.EventQueue.invokeLater(Delegates.toRunnable(delegate
            {
                postEvent(new java.awt.@event.MouseEvent(target, id, when, modifiers, x, y, clickCount, isPopup, button));
            }));
            isPopupMenu = false;
        }

        private void postMouseEvent(EventArgs ev, int id)
        {
            long when = java.lang.System.currentTimeMillis();
            int modifiers = GetModifiers(Control.ModifierKeys);
            int button = 0;
            int clickCount = 0;
            int x = Control.MousePosition.X - control.Location.X;
            int y = Control.MousePosition.Y - control.Location.Y;
            bool isPopup = isPopupMenu;
            java.awt.EventQueue.invokeLater(Delegates.toRunnable(delegate
            {
                postEvent(new java.awt.@event.MouseEvent(target, id, when, modifiers, x, y, clickCount, isPopup, button));
            }));
            isPopupMenu = false;
        }

        private void postMouseWheelEvent(EventArgs ev, int id, int delta)
        {
            long when = java.lang.System.currentTimeMillis();
            int modifiers = GetModifiers(Control.ModifierKeys);
            int scrollAmount = -delta * SystemInformation.MouseWheelScrollLines / 120;
            int clickCount = 0;
            int x = Control.MousePosition.X - control.Location.X;
            int y = Control.MousePosition.Y - control.Location.Y;
            bool isPopup = isPopupMenu;
            java.awt.EventQueue.invokeLater(Delegates.toRunnable(delegate
            {
                postEvent(new java.awt.@event.MouseWheelEvent(target, id, when, modifiers, x, y, clickCount, isPopup, java.awt.@event.MouseWheelEvent.WHEEL_UNIT_SCROLL, scrollAmount, scrollAmount));
            }));
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

        protected virtual void OnMouseDown(object sender, MouseEventArgs ev)
        {
            isMouseClick = false;
            isDoubleClick = false;
            isPopupMenu = false;
            postMouseEvent(ev, java.awt.@event.MouseEvent.MOUSE_PRESSED, ev.Clicks);
        }

        private void OnMouseWheel(object sender, MouseEventArgs ev)
        {
            postMouseWheelEvent(ev, java.awt.@event.MouseEvent.MOUSE_WHEEL, ev.Delta);
        }

        private void OnClick(object sender, EventArgs ev)
        {
            isMouseClick = true;
        }

        private void OnDoubleClick(object sender, EventArgs ev)
        {
            isDoubleClick = true;
        }

        private void OnMouseUp(object sender, MouseEventArgs ev)
        {
            postMouseEvent(ev, java.awt.@event.MouseEvent.MOUSE_RELEASED, ev.Clicks);
            if (isMouseClick || isDoubleClick) // there can only be an Click OR an DoubleClick event - both count as click here
            {
                //We make our own mouse click event because the event order is different in .NET
                //in .NET the click occured before MouseUp
                int clicks = ev.Clicks;
                if (isDoubleClick)
                {
                    clicks = 2;
                }
                postMouseEvent(ev, java.awt.@event.MouseEvent.MOUSE_CLICKED, clicks);
            }
            isMouseClick = false;
        }

        private void OnMouseEnter(object sender, EventArgs ev)
        {
            postMouseEvent(ev, java.awt.@event.MouseEvent.MOUSE_ENTERED);
        }

        private void OnMouseLeave(object sender, EventArgs ev)
        {
            postMouseEvent(ev, java.awt.@event.MouseEvent.MOUSE_EXITED);
        }

        private void OnGotFocus(object sender, EventArgs e)
        {
            if (sm_suppressFocusAndActivation)
            {
                return;
            }
            java.awt.EventQueue.invokeLater(Delegates.toRunnable(delegate
            {
                postEvent(new java.awt.@event.FocusEvent(target, java.awt.@event.FocusEvent.FOCUS_GAINED));
            }));
        }

        private void OnLostFocus(object sender, EventArgs e)
        {
            java.awt.EventQueue.invokeLater(Delegates.toRunnable(delegate
            {
                postEvent(new java.awt.@event.FocusEvent(target, java.awt.@event.FocusEvent.FOCUS_LOST));
            }));
        }

        /*
		 * Called from native code (on Toolkit thread) in order to
		 * dynamically layout the Container during resizing
		 */
        internal void dynamicallyLayoutContainer()
        {
            // If we got the WM_SIZING, this must be a Container, right?
            // In fact, it must be the top-level Container.
            //if (log.isLoggable(Level.FINE)) {
            //    java.awt.Container parent = NetToolkit.getNativeContainer((java.awt.Component)target);
            //    if (parent != null) {
            //        log.log(Level.FINE, "Assertion (parent == null) failed");
            //    }
            //}
            java.awt.Container cont = (java.awt.Container)(object)target;

            SunToolkit.executeOnEventHandlerThread(cont, Delegates.toRunnable(delegate
            {
                // Discarding old paint events doesn't seem to be necessary.
                cont.invalidate();
                cont.validate();

                //if (surfaceData instanceof OGLSurfaceData) {
                //    // 6290245: When OGL is enabled, it is necessary to
                //    // replace the SurfaceData for each dynamic layout
                //    // request so that the OGL viewport stays in sync
                //    // with the window bounds.
                //    try {
                //        replaceSurfaceData();
                //    } catch (InvalidPipeException e) {
                //        // REMIND: this is unlikely to occur for OGL, but
                //        // what do we do if surface creation fails?
                //    }
                //}

                // Forcing a paint here doesn't seem to be necessary.
                // paintDamagedAreaImmediately();
            }));
        }

        /*
		 * Paints any portion of the component that needs updating
		 * before the call returns (similar to the Win32 API UpdateWindow)
		 */
        internal void paintDamagedAreaImmediately()
        {
            // force Windows to send any pending WM_PAINT events so
            // the damage area is updated on the Java side
            updateWindow();
            // make sure paint events are transferred to main event queue
            // for coalescing
            SunToolkit.flushPendingEvents();
            // paint the damaged area
            paintArea.paint(target, shouldClearRectBeforePaint());
        }

        private void updateWindow()
        {
            NetToolkit.BeginInvoke(delegate
            {
                control.Update();
            });
        }

        /* override and return false on components that DO NOT require
		   a clearRect() before painting (i.e. native components) */
        public virtual bool shouldClearRectBeforePaint()
        {
            return true;
        }

        private void OnPopupMenu(object sender, EventArgs ev)
        {
            isPopupMenu = true;
        }

        /*
		 * Post an event. Queue it for execution by the callback thread.
		 */
        internal void postEvent(java.awt.AWTEvent evt)
        {
            SunToolkit.postEvent(SunToolkit.targetToAppContext(target), evt);
        }

        // Routines to support deferred window positioning.
        public void beginLayout()
        {
            // Skip all painting till endLayout
            isLayouting = true;
        }

        public void endLayout()
        {
            if (!paintArea.isEmpty() && !paintPending && !target.getIgnoreRepaint())
            {
                // if not waiting for native painting repaint damaged area
                postEvent(new java.awt.@event.PaintEvent(target, java.awt.@event.PaintEvent.PAINT, new java.awt.Rectangle()));
            }
            isLayouting = false;
        }

        public void beginValidate()
        {
            //    Invoke(delegate
            //    {
            //        if (m_validationNestCount == 0)
            //        {
            //            m_hdwp = BeginDeferWindowPos();
            //        }
            //        m_validationNestCount++;
            //    });
        }

        public void endValidate()
        {
            //    Invoke(delegate
            //    {
            //    m_validationNestCount--;
            //    if (m_validationNestCount == 0) {
            //        // if this call to EndValidate is not nested inside another
            //        // Begin/EndValidate pair, end deferred window positioning
            //        ::EndDeferWindowPos(m_hdwp);
            //        m_hdwp = NULL;
            //    }
            //    });
        }

        // Returns true if we are inside begin/endLayout and
        // are waiting for native painting
        public bool isPaintPending()
        {
            return paintPending && isLayouting;
        }

        public override int checkImage(java.awt.Image img, int width, int height, java.awt.image.ImageObserver ob)
        {
            return getToolkit().checkImage(img, width, height, ob);
        }

        public override java.awt.Image createImage(java.awt.image.ImageProducer prod)
        {
            return new sun.awt.image.ToolkitImage(prod);
        }

        public override java.awt.Image createImage(int width, int height)
        {
            return new java.awt.image.BufferedImage(width, height, java.awt.image.BufferedImage.TYPE_INT_ARGB);
        }

        public override void disable()
        {
            NetToolkit.BeginInvoke(delegate { Enable(false); });
        }

        public override void dispose()
        {
            bool callDisposed = true;
            lock (this)
            {
                if (disposed)
                    callDisposed = false;
                disposed = true;
            }
            if (callDisposed)
            {
                disposeImpl();
            }
        }

        protected virtual void disposeImpl()
        {
            NetToolkit.targetDisposedPeer(target, this);
            NetToolkit.Invoke(nativeDispose);
        }

        protected virtual void nativeDispose()
        {
            unhookEvents();
            control.Dispose();
        }

        public override void enable()
        {
            NetToolkit.BeginInvoke(delegate { Enable(true); });
        }

        public override java.awt.image.ColorModel getColorModel()
        {
            //we return the default ColorModel because this causes the least problems with conversions
            return java.awt.image.ColorModel.getRGBdefault();
        }

        public override java.awt.FontMetrics getFontMetrics(java.awt.Font f)
        {
            return new NetFontMetrics(f);
        }

        public override java.awt.Graphics getGraphics()
        {
            if (!control.IsDisposed)
            {
                /* Fix for bug 4746122. Color and Font shouldn't be null */
                java.awt.Color bgColor = background;
                bgColor ??= java.awt.SystemColor.window;
                java.awt.Color fgColor = foreground;
                fgColor ??= java.awt.SystemColor.windowText;
                java.awt.Font font = this.font;
                font ??= defaultFont;
                return new ComponentGraphics(control, target, fgColor, bgColor, font);
            }
            return null;
        }

        public override java.awt.Point getLocationOnScreen()
        {
            return NetToolkit.Invoke(delegate
            {
                Point p = new(0 - getInsetsLeft(), 0 - getInsetsTop());
                p = control.PointToScreen(p);
                return new java.awt.Point(p.X, p.Y);
            });
        }

        public override java.awt.Dimension getMinimumSize()
        {
            return target.getSize();
        }

        public override java.awt.Dimension getPreferredSize()
        {
            return getMinimumSize();
        }

        public override java.awt.Toolkit getToolkit()
        {
            return java.awt.Toolkit.getDefaultToolkit();
        }

        // returns true if the event has been handled and shouldn't be propagated
        // though handleEvent method chain - e.g. WTextFieldPeer returns true
        // on handling '\n' to prevent it from being passed to native code
        public virtual bool handleJavaKeyEvent(java.awt.@event.KeyEvent e) { return false; }

        private void nativeHandleEvent(java.awt.AWTEvent e)
        {
            // TODO arrghh!! code from void AwtComponent::_NativeHandleEvent(void *param) in awt_Component.cpp should be here
        }

        public override void handleEvent(java.awt.AWTEvent e)
        {
            int id = e.getID();

            if (target.isEnabled() && e is java.awt.@event.KeyEvent @event && !@event.isConsumed())
            {
                if (handleJavaKeyEvent(@event))
                {
                    return;
                }
            }

            switch (id)
            {
                case java.awt.@event.PaintEvent.PAINT:
                    // Got native painting
                    paintPending = false;
                    // Fallthrough to next statement
                    goto case java.awt.@event.PaintEvent.UPDATE;
                case java.awt.@event.PaintEvent.UPDATE:
                    // Skip all painting while layouting and all UPDATEs
                    // while waiting for native paint
                    if (!isLayouting && !paintPending)
                    {
                        paintArea.paint(target, shouldClearRectBeforePaint());
                    }
                    return;
                default:
                    break;
            }

            // Call the native code
            nativeHandleEvent(e);
        }

        public override void hide()
        {
            NetToolkit.BeginInvoke(delegate { control.Visible = false; });
        }

        public bool isFocusTraversable()
        {
            return true;
        }

        public override java.awt.Dimension minimumSize()
        {
            return getMinimumSize();
        }

        public override java.awt.Dimension preferredSize()
        {
            return getPreferredSize();
        }

        public override void paint(java.awt.Graphics graphics)
        {
            target.paint(graphics);
        }

        public override bool prepareImage(java.awt.Image img, int width, int height, java.awt.image.ImageObserver ob)
        {
            return getToolkit().prepareImage(img, width, height, ob);
        }

        public override void print(java.awt.Graphics graphics)
        {
            throw new NotImplementedException();
        }

        public override void repaint(long tm, int x, int y, int width, int height)
        {
        }

        public void requestFocus()
        {
            NetToolkit.Invoke(control.Focus);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request">the component for which the focus is requested</param>
        /// <param name="temporary">indicates if the focus change is temporary (true) or permanent (false)</param>
        /// <param name="allowWindowFocus">indicates if it's allowed to change window focus</param>
        /// <param name="time">the timestamp</param>
        /// <returns></returns>
		public bool requestFocus(java.awt.Component request, bool temporary, bool allowWindowFocus, long time)
        {
            if (!getEnabled() || !getVisible())
            {
                return false;
            }
            postEvent(new java.awt.@event.FocusEvent(request, java.awt.@event.FocusEvent.FOCUS_GAINED, temporary, target));
            return true;
        }

        public override void reshape(int x, int y, int width, int height)
        {
            NetToolkit.BeginInvoke(delegate
            {
                Form window = control.FindForm();
                java.awt.Insets insets = window is MyForm form ? form.peerInsets : new java.awt.Insets(0, 0, 0, 0);
                control.SetBounds(x - insets.left, y - insets.top, width, height);
                //If the .NET control does not accept the new bounds (minimum size, maximum size) 
                //then we need to reflect the real bounds on the .NET site to the Java site
                Rectangle bounds = control.Bounds;
                if (bounds.X + insets.left != x || bounds.Y + insets.top != y)
                {
                    AWTAccessor.getComponentAccessor().setLocation(target, bounds.X + insets.left, bounds.Y + insets.top);
                }
                if (bounds.Width != width || bounds.Height != height)
                {
                    AWTAccessor.getComponentAccessor().setSize(target, bounds.Width, bounds.Height);
                }
            });
        }

        public override void setBackground(java.awt.Color color)
        {
            lock (this)
            {
                background = color;
                NetToolkit.BeginInvoke(delegate { control.BackColor = J2C.ConvertColor(color); });
            }
        }

        private void reshapeNoCheck(int x, int y, int width, int height)
        {
            NetToolkit.BeginInvoke(delegate { control.SetBounds(x, y, width, height); });
        }

        public override void setBounds(int x, int y, int width, int height, int op)
        {
            // Should set paintPending before reahape to prevent
            // thread race between paint events
            // Native components do redraw after resize
            paintPending = width != oldWidth || height != oldHeight;

            if ((op & java.awt.peer.ComponentPeer.__Fields.NO_EMBEDDED_CHECK) != 0)
            {
                reshapeNoCheck(x, y, width, height);
            }
            else
            {
                reshape(x, y, width, height);
            }
            if (width != oldWidth || height != oldHeight)
            {
                // Only recreate surfaceData if this setBounds is called
                // for a resize; a simple move should not trigger a recreation
                try
                {
                    //replaceSurfaceData();
                }
                catch (sun.java2d.InvalidPipeException)
                {
                    // REMIND : what do we do if our surface creation failed?
                }
                oldWidth = width;
                oldHeight = height;
            }

            serialNum++;
        }

        private void setCursorImpl(java.awt.Cursor cursor)
        {
            if (cursor is NetCustomCursor ncc)
            {
                control.Cursor = ncc.Cursor;
                return;
            }
            switch (cursor.getType())
            {
                case java.awt.Cursor.WAIT_CURSOR:
                    control.Cursor = Cursors.WaitCursor;
                    break;
                case java.awt.Cursor.DEFAULT_CURSOR:
                    control.Cursor = Cursors.Default;
                    break;
                case java.awt.Cursor.HAND_CURSOR:
                    control.Cursor = Cursors.Hand;
                    break;
                case java.awt.Cursor.CROSSHAIR_CURSOR:
                    control.Cursor = Cursors.Cross;
                    break;
                case java.awt.Cursor.W_RESIZE_CURSOR:
                case java.awt.Cursor.E_RESIZE_CURSOR:
                    control.Cursor = Cursors.SizeWE;
                    break;
                case java.awt.Cursor.MOVE_CURSOR:
                    control.Cursor = Cursors.SizeAll;
                    break;
                case java.awt.Cursor.N_RESIZE_CURSOR:
                case java.awt.Cursor.S_RESIZE_CURSOR:
                    control.Cursor = Cursors.SizeNS;
                    break;
                case java.awt.Cursor.NE_RESIZE_CURSOR:
                case java.awt.Cursor.SW_RESIZE_CURSOR:
                    control.Cursor = Cursors.SizeNESW;
                    break;
                case java.awt.Cursor.NW_RESIZE_CURSOR:
                case java.awt.Cursor.SE_RESIZE_CURSOR:
                    control.Cursor = Cursors.SizeNWSE;
                    break;
                case java.awt.Cursor.TEXT_CURSOR:
                    control.Cursor = Cursors.IBeam;
                    break;
                default:
                    Console.WriteLine("setCursor not implement for: " + cursor);
                    break;
            }
        }

        public void setCursor(java.awt.Cursor cursor)
        {
            NetToolkit.Invoke(setCursorImpl, cursor);
        }

        public bool getEnabled()
        {
            return NetToolkit.Invoke(delegate { return control.Enabled; });
        }

        public bool getFocused()
        {
            return NetToolkit.Invoke(delegate { return control.Focused; });
        }

        public bool getVisible()
        {
            return NetToolkit.Invoke(delegate { return control.Visible; });
        }

        public override void setEnabled(bool enabled)
        {
            if (enabled)
            {
                enable();
            }
            else
            {
                disable();
            }
        }

        public override void setFont(java.awt.Font font)
        {
            lock (this)
            {
                this.font = font;
                NetToolkit.BeginInvoke(delegate { control.Font = font.getNetFont(); });
            }
        }

        public override void setForeground(java.awt.Color color)
        {
            lock (this)
            {
                foreground = color;
                NetToolkit.BeginInvoke(delegate { control.ForeColor = J2C.ConvertColor(color); });
            }
        }

        public override void setVisible(bool visible)
        {
            if (visible)
            {
                show();
            }
            else
            {
                hide();
            }
        }

        public override void show()
        {
            java.awt.Dimension s = target.getSize();
            oldHeight = s.height;
            oldWidth = s.width;
            pShow();
        }

        /*
		 * Return the GraphicsConfiguration associated with this peer, either
		 * the locally stored winGraphicsConfig, or that of the target Component.
		 */
        public override java.awt.GraphicsConfiguration getGraphicsConfiguration()
        {
            // we don't need a treelock here, since
            // Component.getGraphicsConfiguration() gets it itself.
            return target.getGraphicsConfiguration();
        }

        public void setEventMask(long mask)
        {
            //Console.WriteLine("NOTE: NetComponentPeer.setEventMask not implemented");
        }

        public override bool isObscured()
        {
            // should never be called because we return false from canDetermineObscurity()
            return true;
        }

        public override bool canDetermineObscurity()
        {
            // JDK returns true here and uses GetClipBox to determine if the window is partially obscured,
            // this is an optimization for scrolling in javax.swing.JViewport, since there appears to be
            // no managed equivalent of GetClipBox, we'll simply return false and forgo the optimization.
            return false;
        }

        public override void coalescePaintEvent(java.awt.@event.PaintEvent e)
        {
            java.awt.Rectangle r = e.getUpdateRect();
            if (e is not sun.awt.@event.IgnorePaintEvent)
            {
                paintArea.add(r, e.getID());
            }
        }

        public override void updateCursorImmediately()
        {
        }

        public override java.awt.image.VolatileImage createVolatileImage(int width, int height)
        {
            return new NetVolatileImage(target, width, height);
        }

        public override bool handlesWheelScrolling()
        {
            return true;
        }

        public override void createBuffers(int x, java.awt.BufferCapabilities capabilities)
        {
            throw new NotImplementedException();
        }

        public override java.awt.Image getBackBuffer()
        {
            throw new NotImplementedException();
        }

        public override void flip(java.awt.BufferCapabilities.FlipContents contents)
        {
            throw new NotImplementedException();
        }

        public override void destroyBuffers()
        {
            throw new NotImplementedException();
        }

        public override bool isFocusable()
        {
            return false;
        }

        protected bool isDisposed()
        {
            return disposed;
        }

        public override java.awt.Rectangle getBounds()
        {
            return target.getBounds();
        }

        public override void reparent(java.awt.peer.ContainerPeer parent)
        {
            throw new NotImplementedException();
        }

        public override bool isReparentSupported()
        {
            return false;
        }

        // Do nothing for heavyweight implementation
        public override void layout()
        {
        }

        public override void applyShape(sun.java2d.pipe.Region shape)
        {
            NetToolkit.BeginInvoke(ApplyShapeImpl, shape);
        }

        private void ApplyShapeImpl(sun.java2d.pipe.Region shape)
        {
            control.Region = J2C.ConvertRegion(shape);
        }

        //copied form KeyboardFocusManager
        private const int SNFH_FAILURE = 0;
        private const int SNFH_SUCCESS_HANDLED = 1;
        private const int SNFH_SUCCESS_PROCEED = 2;

        private static java.lang.reflect.Method shouldNativelyFocusHeavyweight;
        private static java.lang.reflect.Method processSynchronousLightweightTransfer;
        private static java.lang.reflect.Method removeLastFocusRequest;

        public override bool requestFocus(java.awt.Component lightweightChild, bool temporary, bool focusedWindowChangeAllowed, long time, CausedFocusEvent.Cause cause)
        {
            // this is a interpretation of the code in WComponentPeer.java and awt_component.cpp
            try
            {
                if (processSynchronousLightweightTransfer == null)
                {
                    java.security.AccessController.doPrivileged(Delegates.toPrivilegedAction(delegate
                    {
                        java.lang.Class keyboardFocusManagerCls = typeof(java.awt.KeyboardFocusManager);
                        java.lang.reflect.Method method = keyboardFocusManagerCls.getDeclaredMethod(
                            "processSynchronousLightweightTransfer",
                            typeof(java.awt.Component),
                            typeof(java.awt.Component),
                            java.lang.Boolean.TYPE,
                            java.lang.Boolean.TYPE,
                            java.lang.Long.TYPE);
                        method.setAccessible(true);
                        processSynchronousLightweightTransfer = method;
                        return null;
                    }));
                }
                processSynchronousLightweightTransfer.invoke(
                    null,
                    target,
                    lightweightChild,
                    java.lang.Boolean.valueOf(temporary),
                    java.lang.Boolean.valueOf(focusedWindowChangeAllowed),
                    java.lang.Long.valueOf(time));
            }
            catch
            {
                return true;
            }
            if (shouldNativelyFocusHeavyweight == null)
            {
                java.security.AccessController.doPrivileged(Delegates.toPrivilegedAction(delegate
                {
                    java.lang.Class keyboardFocusManagerCls = typeof(java.awt.KeyboardFocusManager);
                    java.lang.reflect.Method method = keyboardFocusManagerCls.getDeclaredMethod(
                        "shouldNativelyFocusHeavyweight",
                        typeof(java.awt.Component),
                        typeof(java.awt.Component),
                        java.lang.Boolean.TYPE,
                        java.lang.Boolean.TYPE,
                        java.lang.Long.TYPE,
                        typeof(CausedFocusEvent.Cause));
                    method.setAccessible(true);
                    shouldNativelyFocusHeavyweight = method;
                    return null;
                }));
            }
            int retval = ((java.lang.Integer)shouldNativelyFocusHeavyweight.invoke(
                null,
                target,
                lightweightChild,
                java.lang.Boolean.valueOf(temporary),
                java.lang.Boolean.valueOf(focusedWindowChangeAllowed),
                java.lang.Long.valueOf(time),
                cause)).intValue();
            if (retval == SNFH_SUCCESS_HANDLED)
            {
                return true;
            }
            else if (retval == SNFH_SUCCESS_PROCEED)
            {
                if (getFocused())
                {
                    return true;
                }
                if (removeLastFocusRequest == null)
                {
                    java.security.AccessController.doPrivileged(Delegates.toPrivilegedAction(delegate
                    {
                        java.lang.Class keyboardFocusManagerCls = typeof(java.awt.KeyboardFocusManager);
                        java.lang.reflect.Method method = keyboardFocusManagerCls.getDeclaredMethod(
                            "removeLastFocusRequest",
                            typeof(java.awt.Component));
                        method.setAccessible(true);
                        removeLastFocusRequest = method;
                        return null;
                    }));
                }
                removeLastFocusRequest.invoke(null, target);
            }
            //SNFH_FAILURE
            return false;
        }

        /**
         * Move the back buffer to the front buffer.
         *
         * @param x1 the area to be flipped, upper left X coordinate
         * @param y1 the area to be flipped, upper left Y coordinate
         * @param x2 the area to be flipped, lower right X coordinate
         * @param y2 the area to be flipped, lower right Y coordinate
         * @param flipAction the flip action to perform
         *
         * @see Component.FlipBufferStrategy#flip
         */
        public override void flip(int x1, int y1, int x2, int y2, java.awt.BufferCapabilities.FlipContents flipAction)
        {
            throw new ikvm.@internal.NotYetImplementedError();
        }

        /**
         * Lowers this component at the bottom of the above HW peer. If the above parameter
         * is null then the method places this component at the top of the Z-order.
         */
        public override void setZOrder(java.awt.peer.ComponentPeer above)
        {
            Control.ControlCollection controls = control.Controls;
            if (!controls.Contains(control))
            {
                // Control was not added to any window. Occur if you call addNotify without
                return;
            }
            if (above == null)
            {
                controls.SetChildIndex(control, 0);
            }
            else
            {
                NetComponentPeer<T, C> netPeer = (NetComponentPeer<T, C>)above;
                controls.SetChildIndex(control, controls.GetChildIndex(netPeer.control));
            }
        }

        /**
         * Updates internal data structures related to the component's GC.
         *
         * @return if the peer needs to be recreated for the changes to take effect
         * @since 1.7
         */
        public override bool updateGraphicsData(java.awt.GraphicsConfiguration gc)
        {
            throw new ikvm.@internal.NotYetImplementedError();
        }
    }
}
