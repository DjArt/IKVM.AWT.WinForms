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

using java.util;

namespace IKVM.AWT.WinForms
{
    class NetInputMethodDescriptor : java.awt.im.spi.InputMethodDescriptor
    {
        public java.awt.im.spi.InputMethod createInputMethod()
        {
            throw new NotImplementedException();
        }

        public Locale[] getAvailableLocales()
        {
            // TODO Feature with .NET 3.0 available
            //IEnumerable languages = System.Windows.Input.InputLanguageManager.AvailableInputLanguages;
            // as a hack we return the default locale
            return [Locale.getDefault()];
        }

        public string getInputMethodDisplayName(Locale inputLocale, Locale displayLanguage)
        {
            // copied from WInputMethodDescriptor

            // We ignore the input locale.
            // When displaying for the default locale, rely on the localized AWT properties;
            // for any other locale, fall back to English.
            string name = "System Input Methods";
            if (Locale.getDefault().equals(displayLanguage))
            {
                name = java.awt.Toolkit.getProperty("AWT.HostInputMethodDisplayName", name);
            }
            return name;
        }

        public java.awt.Image getInputMethodIcon(Locale l)
        {
            //WInputMethodDescriptor return also ever null
            return null;
        }

        public bool hasDynamicLocaleList()
        {
            // Java return also true
            return true;
        }
    }

}
