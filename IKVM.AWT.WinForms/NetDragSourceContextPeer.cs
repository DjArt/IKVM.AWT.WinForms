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
using java.util;
using ikvm.runtime;

namespace IKVM.AWT.WinForms
{
    internal class NetDragSourceContextPeer : sun.awt.dnd.SunDragSourceContextPeer
    {
        private static readonly NetDragSourceContextPeer theInstance = new(null);
        private bool dragStart = false;

        private NetDragSourceContextPeer(java.awt.dnd.DragGestureEvent dge) : base(dge)
        {
        }

        public static NetDragSourceContextPeer createDragSourceContextPeer(java.awt.dnd.DragGestureEvent dge)
        {
            theInstance.setTrigger(dge);
            return theInstance;
        }

        public override void startSecondaryEventLoop()
        {
            //NetToolkit.startSecondaryEventLoop();
        }

        public override void quitSecondaryEventLoop()
        {
            //NetToolkit.quitSecondaryEventLoop();
        }

        internal static new java.awt.dnd.DragSourceContext getDragSourceContext()
        {
            return theInstance.getDragSourceContextCore();
        }

        internal static NetDragSourceContextPeer getInstance()
        {
            return theInstance;
        }

        internal java.awt.dnd.DragSourceContext getDragSourceContextCore()
        {
            return base.getDragSourceContext();
        }

        internal new void dragDropFinished(bool success, int operations, int x, int y)
        {
            if (dragStart)
            {
                java.awt.EventQueue.invokeLater(Delegates.toRunnable(delegate
                {
                    base.dragDropFinished(success, operations, x, y);
                }));
            }
            dragStart = false;
        }

        protected override void startDrag(java.awt.datatransfer.Transferable trans, long[] formats, Map formatMap)
        {
            dragStart = true;

            createDragSource(getTrigger().getComponent(),
                             trans,
                             getTrigger().getTriggerEvent(),
                             getTrigger().getSourceAsDragGestureRecognizer().getSourceActions(),
                             formats,
                             formatMap);
            sun.awt.dnd.SunDropTargetContextPeer.setCurrentJVMLocalSourceTransferable(trans);
        }

        private long createDragSource(java.awt.Component component,
                                 java.awt.datatransfer.Transferable transferable,
                                 java.awt.@event.InputEvent nativeTrigger,
                                 int actions,
                                 long[] formats,
                                 Map formatMap)
        {
            java.awt.Component controlOwner = component;

            while (controlOwner != null && (controlOwner.getPeer() == null || controlOwner.getPeer() is sun.awt.NullComponentPeer))
            {
                controlOwner = controlOwner.getParent();
            }

            if (controlOwner?.getPeer() is NetComponentPeer peer)
            {
                peer.performedDragDropEffects = DragDropEffects.None;
                Control control = peer.Control;
                if (control != null)
                {
                    java.awt.dnd.DragSource dragSource = getTrigger().getDragSource();
                    IDataObject data = NetDataTransferer.getInstanceImpl().getDataObject(transferable, sun.awt.datatransfer.DataTransferer.adaptFlavorMap(dragSource.getFlavorMap()));
                    NetToolkit.BeginInvoke(delegate
                    {
                        DragDropEffects effects = control.DoDragDrop(data, DragDropEffects.All);
                        if (effects == DragDropEffects.None && peer.performedDragDropEffects != DragDropEffects.None)
                        {
                            effects = peer.performedDragDropEffects;
                        }
                        peer.performedDragDropEffects = DragDropEffects.None;
                        dragDropFinished(effects != DragDropEffects.None, NetComponentPeer.getAction(effects), Control.MousePosition.X, Control.MousePosition.Y);
                    });
                }
            }

            return 0;
        }

        protected override void setNativeCursor(long nativeCtxt, java.awt.Cursor c, int cType)
        {

        }
    }

}
