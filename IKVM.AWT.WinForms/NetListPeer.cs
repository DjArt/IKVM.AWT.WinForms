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

namespace IKVM.AWT.WinForms
{
    sealed class NetListPeer : NetComponentPeer<java.awt.List, ListBox>, java.awt.peer.ListPeer
    {
        internal NetListPeer(java.awt.List target) : base(target)
        {
            control.IntegralHeight = false;
            setMultipleMode(target.isMultipleMode());
            for (int i = 0; i < target.getItemCount(); i++)
            {
                add(target.getItem(i), i);
                if (target.isSelected(i))
                {
                    select(i);
                }
            }
            makeVisible(target.getVisibleIndex());
        }

        public void add(string item, int index)
        {
            NetToolkit.Invoke(delegate { control.Items.Insert(index, item); });
        }

        public void addItem(string item, int index)
        {
            add(item, index);
        }

        public void clear()
        {
            NetToolkit.Invoke(delegate { control.Items.Clear(); });
        }

        public void delItems(int start_index, int end_index)
        {
            NetToolkit.Invoke(delegate
            {
                for (int i = start_index; i < end_index; i++)
                {
                    control.Items.RemoveAt(start_index);
                }
            });
        }

        public void deselect(int index)
        {
            NetToolkit.Invoke(delegate { control.SelectedIndices.Remove(index); });
        }

        public int[] getSelectedIndexes()
        {
            return NetToolkit.Invoke(delegate
            {
                ListBox.SelectedIndexCollection sic = control.SelectedIndices;
                int[] indexes = new int[sic.Count];
                sic.CopyTo(indexes, 0);
                return indexes;
            });
        }

        public void makeVisible(int index)
        {
            NetToolkit.Invoke(delegate { control.TopIndex = index; });
        }

        public java.awt.Dimension minimumSize(int s)
        {
            return getMinimumSize(s);
        }

        public java.awt.Dimension preferredSize(int s)
        {
            return getPreferredSize(s);
        }

        public void removeAll()
        {
            clear();
        }

        public void select(int index)
        {
            NetToolkit.Invoke(delegate { control.SelectedIndices.Add(index); });
        }

        public void setMultipleMode(bool multi)
        {
            NetToolkit.Invoke(delegate { control.SelectionMode = multi ? SelectionMode.MultiSimple : SelectionMode.One; });
        }

        public void setMultipleSelections(bool multi)
        {
            setMultipleMode(multi);
        }

        public java.awt.Dimension getPreferredSize(int s)
        {
            return getMinimumSize(s);
        }

        public java.awt.Dimension getMinimumSize(int s)
        {
            return new java.awt.Dimension(100, 100);
        }

        protected override ListBox CreateControl()
        {
            return new ListBox();
        }
    }

}
