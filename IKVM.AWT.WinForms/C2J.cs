/*
  Copyright (C) 2002, 2004, 2005, 2006, 2007 Jeroen Frijters
  Copyright (C) 2006 Active Endpoints, Inc.
  Copyright (C) 2006 - 2010 Volker Berlin (i-net software)
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
using System.Drawing.Drawing2D;

namespace IKVM.AWT.WinForms
{
    /// <summary>
    /// This class has some static convertion function from C# to Java objects
    /// </summary>
    class C2J
    {
        internal static java.awt.geom.AffineTransform ConvertMatrix(Matrix matrix)
        {
            float[] elements = matrix.Elements;
            return new java.awt.geom.AffineTransform(elements);
        }

        internal static java.awt.Rectangle ConvertRectangle(RectangleF rec)
        {
            return new java.awt.Rectangle((int)rec.X, (int)rec.Y, (int)rec.Width, (int)rec.Height);
        }

        internal static java.awt.Color ConvertColor(Color color)
        {
            return color == Color.Empty ? null : new java.awt.Color(color.ToArgb(), true);
        }

        internal static java.awt.Font ConvertFont(Font font)
        {
            float size = font.Size;
            if (font.Unit != GraphicsUnit.Pixel)
            {
                size = font.SizeInPoints * java.awt.Toolkit.getDefaultToolkit().getScreenResolution() / 72;
            }
            java.awt.Font jFont = new(font.Name, (int)font.Style, (int)size);
            if (jFont.getSize2D() != size)
            {
                jFont = jFont.deriveFont(size);
            }
            //TODO performance we should set the .NET Font, we can do it with an aditional constructor.
            return jFont;
        }

        internal static java.awt.Shape ConvertShape(GraphicsPath path)
        {
            java.awt.geom.GeneralPath shape = new();
            shape.setWindingRule((int)path.FillMode);

            int pointCount = path.PointCount;
            if (pointCount > 0)
            {
                // get these here because a lot of processing goes on to generate these arrays
                PointF[] points = path.PathPoints;
                byte[] types = path.PathTypes;

                for (int i = 0; i < pointCount; i++)
                {
                    byte pathType = types[i];
                    int type = pathType & 0x07;
                    PointF point = points[i];
                    switch (type)
                    {
                        case 0:
                            // Indicates that the point is the start of a figure. 
                            shape.moveTo(point.X, point.Y);
                            break;
                        case 1:
                            // Indicates that the point is one of the two endpoints of a line. 
                            shape.lineTo(point.X, point.Y);
                            break;
                        case 3:
                            // Indicates that the point is an endpoint or control point of a cubic B?zier spline. 
                            PointF point2 = points[++i];
                            PointF point3 = points[++i];
                            shape.curveTo(point.X, point.Y, point2.X, point2.Y, point3.X, point3.Y);
                            pathType = types[i];
                            break;
                        default:
                            Console.WriteLine("Unknown GraphicsPath type: " + type);
                            break;
                    }
                    if ((pathType & 0x80) > 0)
                    {
                        // Specifies that the point is the last point in a closed subpath (figure).
                        shape.closePath();
                    }
                }
            }

            return shape;
        }
    }
}
