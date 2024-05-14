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

using ikvm.awt;
using ikvm.runtime;

using IKVM.AWT.WinForms.Printing;

using java.net;
using java.util;

using sun.awt;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

using MethodInvoker = System.Windows.Forms.MethodInvoker;

namespace IKVM.AWT.WinForms
{
    public sealed class NetToolkit : SunToolkit, IkvmToolkit, KeyboardFocusManagerPeerProvider
    {
        private int resolution;
        private NetClipboard clipboard;
        private bool eventQueueSynchronizationContext;

        protected override java.awt.EventQueue getSystemEventQueueImpl()
        {
            java.awt.EventQueue eq = base.getSystemEventQueueImpl();
            if (!eventQueueSynchronizationContext)
            {
                InstallEventQueueSynchronizationContext(eq);
            }
            return eq;
        }

        private void InstallEventQueueSynchronizationContext(java.awt.EventQueue eq)
        {
            bool install;
            lock (this)
            {
                install = !eventQueueSynchronizationContext;
                eventQueueSynchronizationContext = true;
            }
            if (install)
            {
                eq.postEvent(new java.awt.@event.InvocationEvent(this, Delegates.toRunnable(EventQueueSynchronizationContext.Install), null, true));
            }
        }

        internal static void CreateNative(Control control)
        {
            control.CreateControl();
            // HACK I have no idea why this line is necessary...
            nint p = control.Handle;
            if (p == nint.Zero)
            {
                // shut up compiler warning
            }
        }

        public NetToolkit()
        {
        }

        /// <summary>
        /// Run on a win 32 system
        /// </summary>
        /// <returns></returns>
        internal static bool isWin32()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32Windows;
        }

        protected override void loadSystemColors(int[] systemColors)
        {
            // initialize all colors to purple to make the ones we might have missed stand out
            for (int i = 0; i < systemColors.Length; i++)
            {
                systemColors[i] = Color.Purple.ToArgb();
            }
            systemColors[java.awt.SystemColor.DESKTOP] = SystemColors.Desktop.ToArgb();
            systemColors[java.awt.SystemColor.ACTIVE_CAPTION] = SystemColors.ActiveCaption.ToArgb();
            systemColors[java.awt.SystemColor.ACTIVE_CAPTION_TEXT] = SystemColors.ActiveCaptionText.ToArgb();
            systemColors[java.awt.SystemColor.ACTIVE_CAPTION_BORDER] = SystemColors.ActiveBorder.ToArgb();
            systemColors[java.awt.SystemColor.INACTIVE_CAPTION] = SystemColors.InactiveCaption.ToArgb();
            systemColors[java.awt.SystemColor.INACTIVE_CAPTION_TEXT] = SystemColors.InactiveCaptionText.ToArgb();
            systemColors[java.awt.SystemColor.INACTIVE_CAPTION_BORDER] = SystemColors.InactiveBorder.ToArgb();
            systemColors[java.awt.SystemColor.WINDOW] = SystemColors.Window.ToArgb();
            systemColors[java.awt.SystemColor.WINDOW_BORDER] = SystemColors.WindowFrame.ToArgb();
            systemColors[java.awt.SystemColor.WINDOW_TEXT] = SystemColors.WindowText.ToArgb();
            systemColors[java.awt.SystemColor.MENU] = SystemColors.Menu.ToArgb();
            systemColors[java.awt.SystemColor.MENU_TEXT] = SystemColors.MenuText.ToArgb();
            systemColors[java.awt.SystemColor.TEXT] = SystemColors.Window.ToArgb();
            systemColors[java.awt.SystemColor.TEXT_TEXT] = SystemColors.WindowText.ToArgb();
            systemColors[java.awt.SystemColor.TEXT_HIGHLIGHT] = SystemColors.Highlight.ToArgb();
            systemColors[java.awt.SystemColor.TEXT_HIGHLIGHT_TEXT] = SystemColors.HighlightText.ToArgb();
            systemColors[java.awt.SystemColor.TEXT_INACTIVE_TEXT] = SystemColors.GrayText.ToArgb();
            systemColors[java.awt.SystemColor.CONTROL] = SystemColors.Control.ToArgb();
            systemColors[java.awt.SystemColor.CONTROL_TEXT] = SystemColors.ControlText.ToArgb();
            systemColors[java.awt.SystemColor.CONTROL_HIGHLIGHT] = SystemColors.ControlLight.ToArgb();
            systemColors[java.awt.SystemColor.CONTROL_LT_HIGHLIGHT] = SystemColors.ControlLightLight.ToArgb();
            systemColors[java.awt.SystemColor.CONTROL_SHADOW] = SystemColors.ControlDark.ToArgb();
            systemColors[java.awt.SystemColor.CONTROL_DK_SHADOW] = SystemColors.ControlDarkDark.ToArgb();
            systemColors[java.awt.SystemColor.SCROLLBAR] = SystemColors.ScrollBar.ToArgb();
            systemColors[java.awt.SystemColor.INFO] = SystemColors.Info.ToArgb();
            systemColors[java.awt.SystemColor.INFO_TEXT] = SystemColors.InfoText.ToArgb();
        }

        public override java.awt.peer.ButtonPeer createButton(java.awt.Button target)
        {
            java.awt.peer.ButtonPeer peer = Invoke(delegate { return new NetButtonPeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        // MONOBUG mcs refuses to override these two methods, so we disable them when building with mcs
        // (since AWT isn't supported anyway)
#if !__MonoCS__
        public override java.awt.peer.CanvasPeer createCanvas(java.awt.Canvas target)
        {
            java.awt.peer.CanvasPeer peer = Invoke(delegate { return new NetCanvasPeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.PanelPeer createPanel(java.awt.Panel target)
        {
            java.awt.peer.PanelPeer peer = Invoke(delegate { return new NetPanelPeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }
#endif

        public override java.awt.peer.TextFieldPeer createTextField(java.awt.TextField target)
        {
            java.awt.peer.TextFieldPeer peer = Invoke(delegate { return new NetTextFieldPeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.LabelPeer createLabel(java.awt.Label target)
        {
            java.awt.peer.LabelPeer peer = Invoke(delegate { return new NetLabelPeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.ListPeer createList(java.awt.List target)
        {
            java.awt.peer.ListPeer peer = Invoke(delegate { return new NetListPeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.CheckboxPeer createCheckbox(java.awt.Checkbox target)
        {
            java.awt.peer.CheckboxPeer peer = Invoke(delegate { return new NetCheckboxPeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.ScrollbarPeer createScrollbar(java.awt.Scrollbar target)
        {
            java.awt.peer.ScrollbarPeer peer = Invoke(delegate { return new NetScrollbarPeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.ScrollPanePeer createScrollPane(java.awt.ScrollPane target)
        {
            java.awt.peer.ScrollPanePeer peer = Invoke(delegate { return new NetScrollPanePeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.TextAreaPeer createTextArea(java.awt.TextArea target)
        {
            java.awt.peer.TextAreaPeer peer = Invoke(delegate { return new NetTextAreaPeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.ChoicePeer createChoice(java.awt.Choice target)
        {
            java.awt.peer.ChoicePeer peer = Invoke(delegate { return new NetChoicePeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.FramePeer createFrame(java.awt.Frame target)
        {
            bool isFocusableWindow = target.isFocusableWindow();
            bool isAlwaysOnTop = target.isAlwaysOnTop();
            java.awt.peer.FramePeer peer = Invoke(delegate { return new NetFramePeer(target, isFocusableWindow, isAlwaysOnTop); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.WindowPeer createWindow(java.awt.Window target)
        {
            bool isFocusableWindow = target.isFocusableWindow();
            bool isAlwaysOnTop = target.isAlwaysOnTop();
            java.awt.peer.WindowPeer peer = Invoke(delegate { return new NetWindowPeer(target, isFocusableWindow, isAlwaysOnTop); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.DialogPeer createDialog(java.awt.Dialog target)
        {
            bool isFocusableWindow = target.isFocusableWindow();
            bool isAlwaysOnTop = target.isAlwaysOnTop();
            java.awt.peer.DialogPeer peer = Invoke(delegate { return new NetDialogPeer(target, isFocusableWindow, isAlwaysOnTop); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.MenuBarPeer createMenuBar(java.awt.MenuBar target)
        {
            // we need to force peer creation of the sub menus here, because we're
            // transitioning to the UI thread to do the rest of the work and there
            // we cannot acquire the AWT tree lock (because it is owned by the current thread)
            for (int i = 0; i < target.getMenuCount(); i++)
            {
                target.getMenu(i).addNotify();
            }
            java.awt.Menu help = target.getHelpMenu();
            help?.addNotify();
            java.awt.peer.MenuBarPeer peer = Invoke(delegate { return new NetMenuBarPeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.MenuPeer createMenu(java.awt.Menu target)
        {
            for (int i = 0; i < target.getItemCount(); i++)
            {
                target.getItem(i).addNotify();
            }
            java.awt.peer.MenuPeer peer = Invoke(delegate { return new NetMenuPeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.PopupMenuPeer createPopupMenu(java.awt.PopupMenu target)
        {
            for (int i = 0; i < target.getItemCount(); i++)
            {
                target.getItem(i).addNotify();
            }
            java.awt.peer.PopupMenuPeer peer = Invoke(delegate { return new NetPopupMenuPeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.MenuItemPeer createMenuItem(java.awt.MenuItem target)
        {
            java.awt.peer.MenuItemPeer peer = Invoke(delegate { return new NetMenuItemPeer(target); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.FileDialogPeer createFileDialog(java.awt.FileDialog target)
        {
            bool isFocusableWindow = target.isFocusableWindow();
            bool isAlwaysOnTop = target.isAlwaysOnTop();
            java.awt.peer.FileDialogPeer peer = Invoke(delegate { return new NetFileDialogPeer(target, isFocusableWindow, isAlwaysOnTop); });
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.CheckboxMenuItemPeer createCheckboxMenuItem(java.awt.CheckboxMenuItem target)
        {
            return new NetCheckboxMenuItemPeer(target);
        }

        public override java.awt.peer.FontPeer getFontPeer(string name, int style)
        {
            throw new NotImplementedException();
        }

        public override java.awt.peer.KeyboardFocusManagerPeer getKeyboardFocusManagerPeer()
        {
            return new NetKeyboardFocusManagerPeer();
        }

        public override java.awt.Dimension getScreenSize()
        {
            return new java.awt.Dimension(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }

        public override int getScreenResolution()
        {
            if (resolution == 0)
            {
                using Form form = new();
                using Graphics g = form.CreateGraphics();
                resolution = (int)Math.Round(g.DpiY);
            }
            return resolution;
        }

        public override java.awt.image.ColorModel getColorModel()
        {
            //we return the default ColorModel because this produce the fewest problems with convertions
            return java.awt.image.ColorModel.getRGBdefault();
        }

        public override void sync()
        {
        }

        public override java.awt.Image getImage(string filename)
        {
            try
            {
                filename = new java.io.File(filename).getPath(); //convert a Java file name to .NET filename (slahes, backslasches, etc)
                using FileStream stream = new(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return new java.awt.image.BufferedImage(new Bitmap(Image.FromStream(stream)));
            }
            catch (Exception)
            {
                return new NoImage(new sun.awt.image.FileImageSource(filename));
            }
        }

        public override java.awt.Image getImage(URL url)
        {
            // TODO extremely lame...
            MemoryStream mem = new();
            java.io.InputStream inS = url.openStream();
            int b;
            while ((b = inS.read()) >= 0)
            {
                mem.WriteByte((byte)b);
            }
            try
            {
                mem.Position = 0;
                return new java.awt.image.BufferedImage(new Bitmap(Image.FromStream(mem)));
            }
            catch
            {
                return new NoImage(new sun.awt.image.URLImageSource(url));
            }
        }

        public override java.awt.Image createImage(string filename)
        {
            return getImage(filename);
        }

        public override java.awt.Image createImage(URL url)
        {
            return getImage(url);
        }

        public override java.awt.Image createImage(byte[] imagedata, int imageoffset, int imagelength)
        {
            try
            {
                return new java.awt.image.BufferedImage(new Bitmap(new MemoryStream(imagedata, imageoffset, imagelength, false)));
            }
            catch (Exception)
            {
                return new NoImage(new sun.awt.image.ByteArrayImageSource(imagedata, imageoffset, imagelength));
            }
        }

        public override java.awt.PrintJob getPrintJob(java.awt.Frame frame, string jobtitle, Properties props)
        {
            throw new NotImplementedException();
        }

        public override void beep()
        {
            Console.Beep();
        }

        public override java.awt.datatransfer.Clipboard getSystemClipboard()
        {
            lock (this)
            {
                clipboard ??= new NetClipboard();
            }
            return clipboard;
        }

        public override java.awt.dnd.DragGestureRecognizer createDragGestureRecognizer(java.lang.Class abstractRecognizerClass, java.awt.dnd.DragSource ds, java.awt.Component c, int srcActions, java.awt.dnd.DragGestureListener dgl)
        {
            java.lang.Class clazz = typeof(java.awt.dnd.MouseDragGestureRecognizer);
            return abstractRecognizerClass == clazz
                 ? new NetMouseDragGestureRecognizer(ds, c, srcActions, dgl)
                 : null;
        }

        public override java.awt.dnd.peer.DragSourceContextPeer createDragSourceContextPeer(java.awt.dnd.DragGestureEvent dge)
        {
            return NetDragSourceContextPeer.createDragSourceContextPeer(dge);
        }

        public override Map mapInputMethodHighlight(java.awt.im.InputMethodHighlight highlight)
        {
            throw new NotImplementedException();
        }

#if false
		protected override java.awt.peer.LightweightPeer createComponent(java.awt.Component target)
		{
			if(target is java.awt.Container)
			{
				return new NetLightweightContainerPeer((java.awt.Container)target);
			}
			return new NetLightweightComponentPeer(target);
		}
#endif

        /*        public override java.awt.Font createFont(int format, java.io.InputStream stream)
                {
                    throw new NotImplementedException();
                }

                public override gnu.java.awt.peer.ClasspathFontPeer getClasspathFontPeer(string name, java.util.Map attrs)
                {
                    return new NetFontPeer(name, attrs);
                }

                public override java.awt.GraphicsEnvironment getLocalGraphicsEnvironment()
                {
                    return new NetGraphicsEnvironment();
                }

                public override RobotPeer createRobot(java.awt.GraphicsDevice screen)
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32Windows)
                    {
                        return new WindowsRobot(screen);
                    }
                    throw new java.awt.AWTException("Robot not supported for this OS");
                }

                public override gnu.java.awt.peer.EmbeddedWindowPeer createEmbeddedWindow(gnu.java.awt.EmbeddedWindow ew)
                {
                    throw new NotImplementedException();
                }
        */
        protected override java.awt.peer.DesktopPeer createDesktopPeer(java.awt.Desktop target)
        {
            return new NetDesktopPeer();
        }

        public override java.awt.Dimension getBestCursorSize(int preferredWidth, int preferredHeight)
        {
            // TODO
            return new java.awt.Dimension(preferredWidth, preferredHeight);
        }

        public override java.awt.Cursor createCustomCursor(java.awt.Image cursor, java.awt.Point hotSpot, string name)
        {
            return new NetCustomCursor(cursor, hotSpot, name);
        }

        private object getRegistry(string subKey, string valueName)
        {
            using Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subKey, false);
            return key?.GetValue(valueName);
        }

        protected override void initializeDesktopProperties()
        {
            //copied from WToolkit.java
            desktopProperties.put("DnD.Autoscroll.initialDelay", java.lang.Integer.valueOf(50));
            desktopProperties.put("DnD.Autoscroll.interval", java.lang.Integer.valueOf(50));

            try
            {
                if (isWin32())
                {
                    desktopProperties.put("Shell.shellFolderManager", "sun.awt.shell.Win32ShellFolderManager2");
                    object themeActive = getRegistry("Software\\Microsoft\\Windows\\CurrentVersion\\ThemeManager", "ThemeActive");
                    //                    string dllName = (string)getRegistry("Software\\Microsoft\\Windows\\CurrentVersion\\ThemeManager", "DllName");
                    //                    string sizeName = (string)getRegistry("Software\\Microsoft\\Windows\\CurrentVersion\\ThemeManager", "SizeName");
                    //                    string colorName = (string)getRegistry("Software\\Microsoft\\Windows\\CurrentVersion\\ThemeManager", "ColorName");
                    desktopProperties.put("win.xpstyle.themeActive", java.lang.Boolean.valueOf("1".Equals(themeActive)));
                    //                    desktopProperties.put("win.xpstyle.dllName", dllName);
                    //                    desktopProperties.put("win.xpstyle.sizeName", sizeName);
                    //                    desktopProperties.put("win.xpstyle.colorName", colorName);
                }
            }
            catch (java.lang.ClassNotFoundException)
            {
            }
        }

        protected override object lazilyLoadDesktopProperty(string name)
        {
            return name switch
            {
                "win.defaultGUI.font" => C2J.ConvertFont(Control.DefaultFont),
                "win.highContrast.on" => java.lang.Boolean.valueOf(SystemInformation.HighContrast),
                _ => null,
            };
        }

        protected override java.awt.peer.MouseInfoPeer getMouseInfoPeer()
        {
            return new NetMouseInfoPeer();
        }

        /*===============================
         * Implementations of interface IkvmToolkit
         */

        /// <summary>
        /// Get a helper class for implementing the print API
        /// </summary>
        /// <returns></returns>
        public sun.print.PrintPeer getPrintPeer()
        {
            return isWin32()
                 ? new Win32PrintPeer()
                 : new LinuxPrintPeer();
        }

        /// <summary>
        /// Create a outline from the given text and font parameter
        /// </summary>
        /// <param name="javaFont">the font</param>
        /// <param name="frc">font render context</param>
        /// <param name="text">the text</param>
        /// <param name="x">x - position</param>
        /// <param name="y">y - position</param>
        /// <returns></returns>
        public java.awt.Shape outline(java.awt.Font javaFont, java.awt.font.FontRenderContext frc, string text, float x, float y)
        {
            GraphicsPath path = new(FillMode.Winding);
            Font netFont = javaFont.getNetFont();
            FontFamily family = netFont.FontFamily;
            FontStyle style = netFont.Style;
            float factor = netFont.Size / family.GetEmHeight(style);
            float ascent = family.GetCellAscent(style) * factor;
            y -= ascent;

            StringFormat format = new(StringFormat.GenericTypographic)
            {
                FormatFlags = StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox,
                Trimming = StringTrimming.None
            };

            path.AddString(text, family, (int)style, netFont.Size, new PointF(x, y), format);
            return C2J.ConvertShape(path);
        }

        /*===============================
         * Implementations of interface SunToolkit
         */

        public override bool isModalExclusionTypeSupported(java.awt.Dialog.ModalExclusionType dmet)
        {
            return false;
        }

        public override bool isModalityTypeSupported(java.awt.Dialog.ModalityType type)
        {
            return type.ordinal() == java.awt.Dialog.ModalityType.MODELESS.ordinal() ||
                   type.ordinal() == java.awt.Dialog.ModalityType.APPLICATION_MODAL.ordinal();
        }

        public override java.awt.Window createInputMethodWindow(string __p1, sun.awt.im.InputContext __p2)
        {
            throw new NotImplementedException();
        }

        public override java.awt.peer.RobotPeer createRobot(java.awt.Robot r, java.awt.GraphicsDevice screen)
        {
            //if (isWin32())
            //{
            //    return new WindowsRobot(screen);
            //}
            throw new java.awt.AWTException("Robot not supported for this OS");
        }

        public override java.awt.peer.SystemTrayPeer createSystemTray(java.awt.SystemTray target)
        {
            NetSystemTrayPeer peer = new(target);
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.peer.TrayIconPeer createTrayIcon(java.awt.TrayIcon target)
        {
            NetTrayIconPeer peer = new(target);
            targetCreatedPeer(target, peer);
            return peer;
        }

        public override java.awt.im.spi.InputMethodDescriptor getInputMethodAdapterDescriptor()
        {
            return null;
        }

        protected override int getScreenHeight()
        {
            return Screen.PrimaryScreen.Bounds.Height;
        }

        protected override int getScreenWidth()
        {
            return Screen.PrimaryScreen.Bounds.Width;
        }

        public override java.awt.Insets getScreenInsets(java.awt.GraphicsConfiguration gc)
        {
            if (gc is NetGraphicsConfiguration ngc)
            {
                Rectangle rectWorkingArea = ngc.screen.WorkingArea;
                Rectangle rectBounds = ngc.screen.Bounds;
                return new java.awt.Insets(rectWorkingArea.Top - rectBounds.Top,
                                           rectWorkingArea.Left - rectBounds.Left,
                                           rectBounds.Bottom - rectWorkingArea.Bottom,
                                           rectBounds.Right - rectWorkingArea.Right);
            }
            else
            {
                return base.getScreenInsets(gc);
            }
        }

        public override void grab(java.awt.Window window)
        {
            NetWindowPeer peer = (NetWindowPeer)window.getPeer();
            peer?.Grab();
        }

        public override bool isDesktopSupported()
        {
            return true;
        }

        public override bool isTraySupported()
        {
            return true;
        }

        public override bool isFrameStateSupported(int state)
        {
            switch (state)
            {
                case java.awt.Frame.NORMAL:
                case java.awt.Frame.ICONIFIED:
                case java.awt.Frame.MAXIMIZED_BOTH:
                    return true;
                default:
                    return false;
            }
        }

        protected override bool syncNativeQueue(long l)
        {
            throw new NotImplementedException();
        }

        public override void ungrab(java.awt.Window window)
        {
            NetWindowPeer peer = (NetWindowPeer)window.getPeer();
            peer?.Ungrab(false);
        }

        internal new static object targetToPeer(object target)
        {
            return SunToolkit.targetToPeer(target);
        }

        internal new static void targetDisposedPeer(object target, object peer)
        {
            SunToolkit.targetDisposedPeer(target, peer);
        }

        internal static void BeginInvoke(MethodInvoker del)
        {
            if (WinFormsMessageLoop.InvokeRequired)
            {
                WinFormsMessageLoop.BeginInvoke(del);
            }
            else
            {
                del();
            }
        }

        internal static void BeginInvoke<T>(Action<T> del, T t)
        {
            if (WinFormsMessageLoop.InvokeRequired)
            {
                WinFormsMessageLoop.BeginInvoke(del, t);
            }
            else
            {
                del(t);
            }
        }
        internal static void Invoke<T>(Action<T> del, T t)
        {
            if (WinFormsMessageLoop.InvokeRequired)
            {
                WinFormsMessageLoop.Invoke(del, t);
            }
            else
            {
                del(t);
            }
        }

        internal static TResult Invoke<TResult>(Func<TResult> del)
        {
            return WinFormsMessageLoop.InvokeRequired ? (TResult)WinFormsMessageLoop.Invoke(del) : del();
        }

        internal static void Invoke(MethodInvoker del)
        {
            if (WinFormsMessageLoop.InvokeRequired)
            {
                WinFormsMessageLoop.Invoke(del);
            }
            else
            {
                del();
            }
        }

        public override bool areExtraMouseButtonsEnabled()
        {
            return true;
        }

        public override java.awt.peer.FramePeer createLightweightFrame(LightweightFrame lf)
        {
            throw new NotImplementedException();
        }

        public override sun.awt.datatransfer.DataTransferer getDataTransferer()
        {
            return NetDataTransferer.getInstanceImpl();
        }
    }

}
