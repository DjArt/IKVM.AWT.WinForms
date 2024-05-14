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
    internal class NetDropTargetContextPeer : sun.awt.dnd.SunDropTargetContextPeer
    {
        private IDataObject data;

        internal static NetDropTargetContextPeer getNetDropTargetContextPeer()
        {
            return new NetDropTargetContextPeer();
        }

        internal int handleEnterMessage(java.awt.Component component, int x, int y, int dropAction, int actions, long[] formats, long nativeCtxt)
        {
            return postDropTargetEvent(component, x, y, dropAction, actions, formats, nativeCtxt, java.awt.@event.MouseEvent.MOUSE_ENTERED, DISPATCH_SYNC);
        }

        internal void handleExitMessage(java.awt.Component component, long nativeCtxt)
        {
            postDropTargetEvent(component, 0, 0, java.awt.dnd.DnDConstants.ACTION_NONE, java.awt.dnd.DnDConstants.ACTION_NONE, null, nativeCtxt, java.awt.@event.MouseEvent.MOUSE_EXITED, DISPATCH_SYNC);
        }

        internal int handleMotionMessage(java.awt.Component component, int x, int y, int dropAction, int actions, long[] formats, long nativeCtxt)
        {
            return postDropTargetEvent(component, x, y, dropAction, actions, formats, nativeCtxt, java.awt.@event.MouseEvent.MOUSE_DRAGGED, DISPATCH_SYNC);
        }

        internal void handleDropMessage(java.awt.Component component, int x, int y, int dropAction, int actions, long[] formats, long nativeCtxt, IDataObject data)
        {
            this.data = data;
            postDropTargetEvent(component, x, y, dropAction, actions, formats, nativeCtxt, sun.awt.dnd.SunDropTargetEvent.MOUSE_DROPPED, !DISPATCH_SYNC);
        }

        internal new int postDropTargetEvent(java.awt.Component component, int x, int y, int dropAction, int actions, long[] formats, long nativeCtxt, int eventID, bool dispatchType)
        {
            NetComponentPeer peer = (NetComponentPeer)component.getPeer();
            Control control = peer.Control;
            Point screenPt = new(x, y);
            Point clientPt = control.PointToClient(screenPt);
            return base.postDropTargetEvent(component, clientPt.X, clientPt.Y, dropAction, actions, formats, nativeCtxt, eventID, dispatchType);
        }

        protected override void doDropDone(bool success, int dropAction, bool isLocal)
        {
            // Don't do anything as .NET framework already handle the message pump
        }

        public override bool isDataFlavorSupported(java.awt.datatransfer.DataFlavor df)
        {
            return isTransferableJVMLocal()
                 ? base.isDataFlavorSupported(df)
                 : base.isDataFlavorSupported(df);
        }

        public override object getTransferData(java.awt.datatransfer.DataFlavor df)
        {
            return isTransferableJVMLocal()
                 ? base.getTransferData(df)
                 : new NetClipboardTransferable(data).getTransferData(df);
        }

        protected override object getNativeData(long l)
        {
            throw new NotImplementedException();
        }
    }

}
