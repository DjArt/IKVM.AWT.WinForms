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
using System.Windows.Forms;

namespace IKVM.AWT.WinForms
{
    sealed class NetTextAreaPeer : NetComponentPeer<java.awt.TextArea, RichTextBox>, java.awt.peer.TextAreaPeer
    {
        public NetTextAreaPeer(java.awt.TextArea textArea)
            : base(textArea)
        {
            control.ReadOnly = !target.isEditable();
            control.WordWrap = false;
            control.ScrollBars = RichTextBoxScrollBars.Both;
            control.Multiline = true;
            control.AutoSize = false;
            control.Text = target.getText();
        }

        public override bool isFocusable()
        {
            return true;
        }

        public int getSelectionEnd()
        {
            return NetToolkit.Invoke(delegate { return control.SelectionStart + control.SelectionLength; });
        }

        public int getSelectionStart()
        {
            return NetToolkit.Invoke(delegate { return control.SelectionStart; });
        }

        public string getText()
        {
            return NetToolkit.Invoke(delegate { return control.Text; });
        }

        public void setText(string text)
        {
            NetToolkit.Invoke(delegate { control.Text = text; });
        }

        public void select(int start_pos, int end_pos)
        {
            throw new NotImplementedException();
        }

        public void setEditable(bool editable)
        {
            throw new NotImplementedException();
        }

        public int getCaretPosition()
        {
            return getSelectionStart();
        }

        private void setCaretPositionImpl(int pos)
        {
            control.SelectionStart = pos;
            control.SelectionLength = 0;
        }

        public void setCaretPosition(int pos)
        {
            NetToolkit.Invoke(setCaretPositionImpl, pos);
        }

        public void insert(string text, int pos)
        {
            NetToolkit.Invoke(delegate { control.Text = control.Text.Insert(pos, text); });
        }

        public void insertText(string text, int pos)
        {
            insert(text, pos);
        }

        public override java.awt.Dimension getMinimumSize()
        {
            return getMinimumSize(10, 60);
        }

        public java.awt.Dimension minimumSize(int rows, int cols)
        {
            return getMinimumSize(rows, cols);
        }

        public java.awt.Dimension getMinimumSize(int rows, int cols)
        {
            java.awt.FontMetrics fm = getFontMetrics(target.getFont());
            return new java.awt.Dimension(fm.charWidth('0') * cols + 20, fm.getHeight() * rows + 20);
        }

        public java.awt.Dimension preferredSize(int rows, int cols)
        {
            return getPreferredSize(rows, cols);
        }

        public java.awt.Dimension getPreferredSize(int rows, int cols)
        {
            return getMinimumSize(rows, cols);
        }

        public void replaceRange(string text, int start_pos, int end_pos)
        {
            NetToolkit.Invoke(delegate { control.Text = control.Text.Substring(0, start_pos) + text + control.Text.Substring(end_pos); });
        }

        public void replaceText(string text, int start_pos, int end_pos)
        {
            replaceRange(text, start_pos, end_pos);
        }

        public java.awt.im.InputMethodRequests getInputMethodRequests()
        {
            throw new NotImplementedException();
        }

        protected sealed override RichTextBox CreateControl()
        {
            return new RichTextBox();
        }
    }

}
