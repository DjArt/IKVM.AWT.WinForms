﻿/*
  Copyright (C) 2009 Volker Berlin (i-net software)
  Copyright (C) 2010 Karsten Heinrich (i-net software)
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

using Microsoft.Win32.SafeHandles;

using System.Runtime.InteropServices;

namespace IKVM.AWT.WinForms.Printing
{
    [System.Security.SecurityCritical]
    sealed partial class SafePrinterHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [LibraryImport("winspool.drv")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ClosePrinter(nint hPrinter);

        private SafePrinterHandle()
            : base(true)
        {
        }

        [System.Security.SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return ClosePrinter(handle);
        }
    }
}