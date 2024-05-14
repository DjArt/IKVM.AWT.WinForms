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

using sun.awt;

namespace IKVM.AWT.WinForms
{
    abstract class NetComponentPeer : java.awt.peer.ComponentPeer
    {
        internal bool eraseBackground = true;

        public abstract void applyShape(sun.java2d.pipe.Region r);
        public abstract bool canDetermineObscurity();
        public abstract int checkImage(java.awt.Image i1, int i2, int i3, java.awt.image.ImageObserver io);
        public abstract void coalescePaintEvent(java.awt.@event.PaintEvent pe);
        public abstract void createBuffers(int i, java.awt.BufferCapabilities bc);
        public abstract java.awt.Image createImage(int i1, int i2);
        public abstract java.awt.Image createImage(java.awt.image.ImageProducer ip);
        public abstract java.awt.image.VolatileImage createVolatileImage(int i1, int i2);
        public abstract void destroyBuffers();
        public abstract void disable();
        public abstract void dispose();
        public abstract void enable();
        public abstract void flip(java.awt.BufferCapabilities.FlipContents bcfc);
        public abstract java.awt.Image getBackBuffer();
        public abstract java.awt.Rectangle getBounds();
        public abstract java.awt.image.ColorModel getColorModel();
        public abstract java.awt.FontMetrics getFontMetrics(java.awt.Font f);
        public abstract java.awt.Graphics getGraphics();
        public abstract java.awt.GraphicsConfiguration getGraphicsConfiguration();
        public abstract java.awt.Point getLocationOnScreen();
        public abstract java.awt.Dimension getMinimumSize();
        public abstract java.awt.Dimension getPreferredSize();
        public abstract java.awt.Toolkit getToolkit();
        public abstract void handleEvent(java.awt.AWTEvent awte);
        public abstract bool handlesWheelScrolling();
        public abstract void hide();
        public abstract bool isFocusable();
        public abstract bool isObscured();
        public abstract bool isReparentSupported();
        public abstract void layout();
        public abstract java.awt.Dimension minimumSize();
        public abstract void paint(java.awt.Graphics g);
        public abstract java.awt.Dimension preferredSize();
        public abstract bool prepareImage(java.awt.Image i1, int i2, int i3, java.awt.image.ImageObserver io);
        public abstract void print(java.awt.Graphics g);
        public abstract void repaint(long l, int i1, int i2, int i3, int i4);
        public abstract void reparent(java.awt.peer.ContainerPeer cp);
        public abstract bool requestFocus(java.awt.Component c, bool b1, bool b2, long l, CausedFocusEvent.Cause cfec);
        public abstract void reshape(int i1, int i2, int i3, int i4);
        public abstract void setBackground(java.awt.Color c);
        public abstract void setBounds(int i1, int i2, int i3, int i4, int i5);
        public abstract void setEnabled(bool b);
        public abstract void setFont(java.awt.Font f);
        public abstract void setForeground(java.awt.Color c);
        public abstract void setVisible(bool b);
        public abstract void show();
        public abstract void updateCursorImmediately();
        public abstract void flip(int x1, int y1, int x2, int y2, java.awt.BufferCapabilities.FlipContents flipAction);
        public abstract void setZOrder(java.awt.peer.ComponentPeer above);
        public abstract bool updateGraphicsData(java.awt.GraphicsConfiguration gc);

        internal DragDropEffects performedDragDropEffects = DragDropEffects.None;

        internal abstract Control Control { get; }
        internal abstract java.awt.Component Target { get; }

        internal abstract int getInsetsLeft();
        internal abstract int getInsetsTop();

        internal static int getAction(DragDropEffects effects)
        {
            int actions = java.awt.dnd.DnDConstants.ACTION_NONE;
            switch (effects)
            {
                case DragDropEffects.None:
                    actions = java.awt.dnd.DnDConstants.ACTION_NONE;
                    break;
                case DragDropEffects.Copy:
                    actions = java.awt.dnd.DnDConstants.ACTION_COPY;
                    break;
                case DragDropEffects.Move:
                    actions = java.awt.dnd.DnDConstants.ACTION_MOVE;
                    break;
                case DragDropEffects.Move | DragDropEffects.Copy:
                    actions = java.awt.dnd.DnDConstants.ACTION_COPY_OR_MOVE;
                    break;
                case DragDropEffects.Link:
                    actions = java.awt.dnd.DnDConstants.ACTION_LINK;
                    break;
            }
            return actions;
        }

        internal static int GetMouseEventModifiers(MouseEventArgs ev)
        {
            int modifiers = GetModifiers(Control.ModifierKeys);
            //Which button was pressed or released, because it can only one that it is a switch
            MouseButtons button = ev.Button;
            switch (button)
            {
                case MouseButtons.Left:
                    modifiers |= java.awt.@event.InputEvent.BUTTON1_MASK;
                    break;
                case MouseButtons.Middle:
                    modifiers |= java.awt.@event.InputEvent.BUTTON2_MASK;
                    break;
                case MouseButtons.Right:
                    modifiers |= java.awt.@event.InputEvent.BUTTON3_MASK;
                    break;
            }
            return modifiers;
        }

        internal static int GetModifiers(Keys keys)
        {
            int modifiers = 0;
            if ((keys & Keys.Shift) != 0)
            {
                modifiers |= java.awt.@event.InputEvent.SHIFT_DOWN_MASK;
            }
            switch (keys & (Keys.Control | Keys.Alt))
            {
                case Keys.Control:
                    modifiers |= java.awt.@event.InputEvent.CTRL_DOWN_MASK;
                    break;
                case Keys.Alt:
                    modifiers |= java.awt.@event.InputEvent.ALT_DOWN_MASK;
                    break;
                case Keys.Control | Keys.Alt:
                    modifiers |= java.awt.@event.InputEvent.ALT_GRAPH_DOWN_MASK;
                    break;
            }
            if ((Control.MouseButtons & MouseButtons.Left) != 0)
            {
                modifiers |= java.awt.@event.InputEvent.BUTTON1_DOWN_MASK;
            }
            if ((Control.MouseButtons & MouseButtons.Middle) != 0)
            {
                modifiers |= java.awt.@event.InputEvent.BUTTON2_DOWN_MASK;
            }
            if ((Control.MouseButtons & MouseButtons.Right) != 0)
            {
                modifiers |= java.awt.@event.InputEvent.BUTTON3_DOWN_MASK;
            }
            return modifiers;
        }

        internal static int GetButton(MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButtons.Left))
            {
                return java.awt.@event.MouseEvent.BUTTON1;
            }
            else if (e.Button.HasFlag(MouseButtons.Middle))
            {
                return java.awt.@event.MouseEvent.BUTTON2;
            }
            else if (e.Button.HasFlag(MouseButtons.Right))
            {
                return java.awt.@event.MouseEvent.BUTTON3;
            }
            else
            {
                return java.awt.@event.MouseEvent.NOBUTTON;
            }
        }

        internal static NetComponentPeer FromControl(Control control)
        {
            return (NetComponentPeer)control.Tag;
        }
    }

}
