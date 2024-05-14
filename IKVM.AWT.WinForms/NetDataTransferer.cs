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
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using java.util;

namespace IKVM.AWT.WinForms
{
#if NETSTANDARD
	// .NET Core does not implement the menu functionality.

	class MenuItemCollection
	{
		internal void Add(MenuItem item) {}
		internal void RemoveAt(int i) {}
		internal MenuItem this[int i] { get { return null; } }
	}

	class Menu
	{
		internal void Show(Control control, Point pt) {}
		internal void Dispose() {}
		internal MenuItemCollection MenuItems { get { return null; } }
		internal object Tag { get { return null; } set {} }
	}

	class MainMenu : Menu {}

	class ContextMenu : Menu {}

	class MenuItem
	{
		internal MenuItem() {}
		internal MenuItem(string s) {}
		internal bool Checked { get { return false; } set {} }
		internal bool Enabled { get { return false; } set {} }
		internal string Text { get { return ""; } set {} }
		internal object Tag { get { return null; } set {} }
		internal event EventHandler Click;
		internal MenuItemCollection MenuItems { get { return null; } }
		internal void Dispose() {}
	}
#endif

    internal delegate TResult Func<TResult>();
    internal delegate void Action<T>(T t);

    public class NetDataTransferer : sun.awt.IkvmDataTransferer
    {
        class NetToolkitThreadBlockedHandler : sun.awt.datatransfer.ToolkitThreadBlockedHandler
        {
            private bool locked;
            private Thread owner;

            protected bool isOwned()
            {
                return locked && Thread.CurrentThread == owner;
            }

            public void enter()
            {
                if (!isOwned())
                {
                    throw new java.lang.IllegalMonitorStateException();
                }
                unlock();
                if (Application.MessageLoop)
                {
                    Application.DoEvents();
                }
                @lock();
            }

            public void exit()
            {
                if (!isOwned())
                {
                    throw new java.lang.IllegalMonitorStateException();
                }
            }

            public void @lock()
            {
                lock (this)
                {
                    if (locked && Thread.CurrentThread == owner)
                    {
                        throw new java.lang.IllegalMonitorStateException();
                    }
                    do
                    {
                        if (!locked)
                        {
                            locked = true;
                            owner = Thread.CurrentThread;
                        }
                        else
                        {
                            try
                            {
                                Monitor.Wait(this);
                            }
                            catch (ThreadInterruptedException)
                            {
                                // try again
                            }
                        }
                    } while (owner != Thread.CurrentThread);
                }
            }

            public void unlock()
            {
                lock (this)
                {
                    if (Thread.CurrentThread != owner)
                    {
                        throw new java.lang.IllegalMonitorStateException();
                    }
                    owner = null;
                    locked = false;
                    Monitor.Pulse(this);
                }
            }
        }


        private static readonly NetDataTransferer instance = new();
        private static readonly NetToolkitThreadBlockedHandler handler = new();

        public static NetDataTransferer getInstanceImpl()
        {
            return instance;
        }

        internal long[] getClipboardFormatCodes(string[] formats)
        {
            long[] longData = new long[formats.Length];
            for (int i = 0; i < formats.Length; i++)
            {
                DataFormats.Format dataFormat = DataFormats.GetFormat(formats[i]);
                longData[i] = dataFormat == null ? 0 : dataFormat.Id;
            }
            return longData;
        }

        internal string getNativeClipboardFormatName(long format)
        {
            DataFormats.Format dataFormat = DataFormats.GetFormat((int)format);
            return dataFormat switch
            {
                null => null,
                _ => dataFormat.Name
            };
        }

        internal Map translateFromClipboard(IDataObject data)
        {
            java.awt.datatransfer.FlavorTable defaultFlavorMap = (java.awt.datatransfer.FlavorTable)java.awt.datatransfer.SystemFlavorMap.getDefaultFlavorMap();
            Map/*<DataFlavor,object>*/ map = new HashMap();
            if (data == null)
            {
                return map;
            }
            string[] formats = data.GetFormats();
            if (formats != null && formats.Length > 0)
            {
                long[] longFormats = getClipboardFormatCodes(formats);
                Map /*<DataFlavor,long>*/ flavorMap = getFlavorsForFormats(longFormats, defaultFlavorMap);
                java.awt.datatransfer.DataFlavor[] flavors =
                    (java.awt.datatransfer.DataFlavor[])
                    flavorMap.keySet().toArray(new java.awt.datatransfer.DataFlavor[0]);
                for (int i = 0; i < flavors.Length; i++)
                {
                    java.awt.datatransfer.DataFlavor df = flavors[i];
                    long format = ((java.lang.Long)flavorMap.get(df)).longValue();
                    string stringFormat = getNativeClipboardFormatName(format);
                    if (stringFormat == null) continue; // clipboard format is not registered in Windows system
                    object formatData = data.GetData(stringFormat);
                    if (formatData == null) continue; // no data for that format
                    object translatedData = null;
                    if (df.isFlavorJavaFileListType())
                    {
                        // translate string[] into java.util.List<java.io.File>
                        string[] nativeFileList = (string[])formatData;
                        List fileList = new ArrayList(nativeFileList.Length);
                        for (int j = 0; j < nativeFileList.Length; j++)
                        {
                            java.io.File file = new(nativeFileList[i]);
                            fileList.add(file);
                        }
                        translatedData = fileList;
                    }
                    else if (java.awt.datatransfer.DataFlavor.imageFlavor.equals(df) && formatData is Bitmap)
                    {
                        // translate System.Drawing.Bitmap into java.awt.Image
                        translatedData = new java.awt.image.BufferedImage((Bitmap)formatData);
                    }
                    else if (formatData is string formatString)
                    {
                        if (df.isFlavorTextType())
                            translatedData = formatData;
                        else if (((java.lang.Class)typeof(java.io.Reader)).equals(df.getRepresentationClass()))
                            translatedData = new java.io.StringReader(formatString);
                        else if (((java.lang.Class)typeof(java.io.InputStream)).equals(df.getRepresentationClass()))
                            translatedData = new java.io.StringBufferInputStream(formatString);
                        else
                            throw new java.awt.datatransfer.UnsupportedFlavorException(df);
                    }
                    if (translatedData != null)
                        map.put(df, translatedData);
                }
            }
            return map;
        }

        internal IDataObject getDataObject(java.awt.datatransfer.Transferable transferable, java.awt.datatransfer.FlavorTable flavorMap)
        {
            DataObject obj = new();
            SortedMap/*<java.lang.Long,java.awt.datatransfer.DataFlavor>*/ formatMap = getFormatsForTransferable(transferable, flavorMap);
            for (Iterator iterator = formatMap.entrySet().iterator(); iterator.hasNext();)
            {
                Map.Entry entry = (Map.Entry)iterator.next();
                java.lang.Long lFormat = (java.lang.Long)entry.getKey();
                long format = lFormat == null ? -1 : lFormat.longValue();
                java.awt.datatransfer.DataFlavor flavor = (java.awt.datatransfer.DataFlavor)entry.getValue();
                object contents = transferable.getTransferData(flavor);
                if (contents == null) continue;
                try
                {
                    if (java.awt.datatransfer.DataFlavor.javaFileListFlavor.equals(flavor))
                    {
                        List list = (List)contents;
                        System.Collections.Specialized.StringCollection files = [];
                        for (int i = 0; i < list.size(); i++)
                        {
                            files.Add(((java.io.File)list.get(i)).getAbsolutePath());
                        }
                        obj.SetFileDropList(files);
                    }
                    else if (flavor.isFlavorTextType())
                    {
                        if (contents is string)
                        {
                            obj.SetText((string)transferable.getTransferData(flavor));
                        }
                        else
                        {
                            try
                            {
                                java.io.Reader reader = flavor.getReaderForText(transferable);
                                java.io.StringWriter writer = new();
                                char[] buffer = new char[1024];
                                int n;
                                while ((n = reader.read(buffer)) != -1)
                                {
                                    writer.write(buffer, 0, n);
                                }
                                obj.SetText(writer.toString());
                            }
                            catch
                            {
                            }
                        }
                    }
                    else if (java.awt.datatransfer.DataFlavor.imageFlavor.equals(flavor))
                    {
                        if (contents is java.awt.Image image)
                        {
                            Image netImage = J2C.ConvertImage(image);
                            if (netImage != null)
                            {
                                obj.SetImage(netImage);
                            }
                        }
                    }
                    else if (flavor.isRepresentationClassCharBuffer())
                    {
                        if (!(isFlavorCharsetTextType(flavor) && isTextFormat(format)))
                        {
                            throw new IOException("cannot transfer non-text data as CharBuffer");
                        }
                        java.nio.CharBuffer buffer = (java.nio.CharBuffer)contents;
                        int size = buffer.remaining();
                        char[] chars = new char[size];
                        buffer.get(chars, 0, size);
                        obj.SetText(new string(chars));
                    }
                    else
                    {
                        // don't know what to do with it...
                        obj.SetData(transferable.getTransferData(flavor));
                    }
                }
                catch (java.io.IOException e)
                {
                    if (!(flavor.isMimeTypeEqual(java.awt.datatransfer.DataFlavor.javaJVMLocalObjectMimeType) &&
                          e is java.io.NotSerializableException))
                    {
                        e.printStackTrace();
                    }
                }
            }
            return obj;
        }

        protected override string getClipboardFormatName(long format)
        {
            return getNativeClipboardFormatName(format);
        }

        protected override byte[] imageToStandardBytes(java.awt.Image image, string mimeType)
        {
            if (image is NoImage) return null;
            Image netImage = J2C.ConvertImage(image);
            ImageFormat format;
            switch (mimeType)
            {
                case "image/jpg":
                case "image/jpeg":
                    format = ImageFormat.Jpeg;
                    break;
                case "image/png":
                    format = ImageFormat.Png;
                    break;
                case "image/gif":
                    format = ImageFormat.Gif;
                    break;
                case "image/x-win-metafile":
                case "image/x-wmf":
                case "image/wmf":
                    format = ImageFormat.Wmf;
                    break;
                default:
                    return null;
            }
            using MemoryStream stream = new();
            netImage.Save(stream, format);
            return stream.GetBuffer();
        }

        public override sun.awt.datatransfer.ToolkitThreadBlockedHandler getToolkitThreadBlockedHandler()
        {
            return handler;
        }

        protected override java.io.ByteArrayOutputStream convertFileListToBytes(ArrayList fileList)
        {
            throw new ikvm.@internal.NotYetImplementedError();
        }

        protected override java.awt.Image platformImageBytesToImage(byte[] barr, long l)
        {
            throw new NotImplementedException();
        }
    }

}
