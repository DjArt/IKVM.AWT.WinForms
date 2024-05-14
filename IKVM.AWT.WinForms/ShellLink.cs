/*
    Copyright (C) 2007, 2008, 2010 Jeroen Frijters
    Copyright (C) 2009 - 2012 Volker Berlin (i-net software)
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
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace IKVM.AWT.WinForms
{
    [System.Security.SecurityCritical]
    class ShellLink : IDisposable
    {
        [ComImport]
        [Guid("0000010B-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IPersistFile
        {
            [PreserveSig]
            void GetClassID(out Guid pClassID);
            [PreserveSig]
            void IsDirty();
            [PreserveSig]
            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
            [PreserveSig]
            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
            [PreserveSig]
            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            [PreserveSig]
            void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
        }

        [ComImport]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, nint pfd, uint fFlags);
            void GetIDList(out nint ppidl);
            void SetIDList(nint pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short pwHotkey);
            void GetShowCmd(out uint piShowCmd);
            void SetShowCmd(uint piShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            /// <summary>Attempts to find the target of a Shell link, even if it has been moved or renamed.</summary>
			void Resolve(nint hWnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [Guid("00021401-0000-0000-C000-000000000046")]
        [ClassInterface(ClassInterfaceType.None)]
        [ComImport]
        private class CShellLink { }

        [Flags]
        public enum EShowWindowFlags : uint
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10
        }

        private IShellLinkW linkW = (IShellLinkW)new CShellLink();

        [System.Security.SecuritySafeCritical]
        public void Dispose()
        {
            if (linkW != null)
            {
                Marshal.ReleaseComObject(linkW);
                linkW = null;
            }
        }

        public void SetPath(string path)
        {
            linkW.SetPath(path);
        }

        public void SetDescription(string description)
        {
            linkW.SetDescription(description);
        }

        public void SetWorkingDirectory(string dir)
        {
            linkW.SetWorkingDirectory(dir);
        }

        public void SetArguments(string args)
        {
            linkW.SetArguments(args);
        }

        public void SetShowCmd(EShowWindowFlags cmd)
        {
            linkW.SetShowCmd((uint)cmd);
        }

        public void Save(string linkFile)
        {
            ((IPersistFile)linkW).Save(linkFile, true);
        }

        public void Load(string linkFile)
        {
            ((IPersistFile)linkW).Load(linkFile, 0);
        }

        public string GetArguments()
        {
            StringBuilder sb = new(512);
            linkW.GetArguments(sb, sb.Capacity);
            return sb.ToString();
        }

        public void Resolve()
        {
            linkW.Resolve(nint.Zero, 0);
        }

        public nint GetIDList()
        {
            linkW.GetIDList(out nint ppidl);
            return ppidl;
        }

        public string GetPath()
        {
            StringBuilder sb = new(512);
            linkW.GetPath(sb, sb.Capacity, nint.Zero, 0);
            return sb.ToString();
        }
    }
}
