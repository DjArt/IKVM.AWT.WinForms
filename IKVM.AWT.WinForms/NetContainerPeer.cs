﻿/*
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
using System.Windows.Forms;

namespace IKVM.AWT.WinForms
{
    class NetContainerPeer<T, C> : NetComponentPeer<T, C>, java.awt.peer.ContainerPeer
        where T : java.awt.Container
        where C : Control
    {
        /// <summary>
        /// The native insets of the .NET Window
        /// </summary>
		protected java.awt.Insets _insets = new(0, 0, 0, 0);

        public NetContainerPeer(java.awt.Container awtcontainer)
            : base((T)awtcontainer)
        {
        }

        internal override int getInsetsLeft()
        {
            return _insets.left; ;
        }

        internal override int getInsetsTop()
        {
            return _insets.top;
        }

        public java.awt.Insets insets()
        {
            return getInsets();
        }

        public java.awt.Insets getInsets()
        {
            return _insets;
        }

        public bool isRestackSupported()
        {
            return false;
        }

        public void cancelPendingPaint(int x, int y, int width, int height)
        {
            throw new NotImplementedException();
        }

        public void restack()
        {
            throw new NotImplementedException();
        }

        protected override C CreateControl()
        {
            throw new NotImplementedException();
        }
    }

}