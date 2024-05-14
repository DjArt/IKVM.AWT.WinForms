/*
    Copyright (C) 2002, 2004, 2005, 2006, 2007 Jeroen Frijters
    Copyright (C) 2006 Active Endpoints, Inc.
    Copyright (C) 2006 - 2014 Volker Berlin (i-net software)
    Copyright (C) 2011 Karsten Heinrich (i-net software)
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

using java.awt.image;
using java.util;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace IKVM.AWT.WinForms
{
    internal abstract class NetGraphics : java.awt.Graphics2D//sun.java2d.SunGraphics2D
    {
        private java.awt.Color javaColor;
        private java.awt.Paint javaPaint;
        internal Color color;
        private Color bgcolor;
        private java.awt.Font font;
        private java.awt.Stroke stroke;
        private static java.awt.BasicStroke defaultStroke = new();
        private Font netfont;
        private int baseline;
        internal Brush brush;
        internal Pen pen;
        private CompositeHelper composite;
        private java.awt.Composite javaComposite = java.awt.AlphaComposite.SrcOver;
        private object textAntialiasHint;
        private object fractionalHint = java.awt.RenderingHints.VALUE_FRACTIONALMETRICS_DEFAULT;

        private static System.Collections.Generic.Dictionary<string, int> baselines = new();

        internal static readonly StringFormat FORMAT = new(StringFormat.GenericTypographic);
        static NetGraphics()
        {
            FORMAT.FormatFlags = StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox;
            FORMAT.Trimming = StringTrimming.None;
        }

        protected NetGraphics(Graphics g, object destination, java.awt.Font font, Color fgcolor, Color bgcolor) //: base( new sun.java2d.SurfaceData(destination) )
        {
            font ??= new java.awt.Font("Dialog", java.awt.Font.PLAIN, 12);
            this.font = font;
            netfont = font.getNetFont();
            color = fgcolor;
            this.bgcolor = bgcolor;
            composite = CompositeHelper.Create(javaComposite, g);
            init(g);
        }

        /// <summary>
        /// The current C# Graphics
        /// </summary>
        internal virtual Graphics g { get; set; }

        protected void init(Graphics graphics)
        {
            NetGraphicsState state = new();
            state.saveGraphics(this);
            g = graphics;
            state.restoreGraphics(this);
        }

        /// <summary>
        /// Get the size of the graphics. This is used as a hind for some hacks.
        /// </summary>
        /// <returns></returns>
        protected virtual SizeF GetSize()
        {
            return g.ClipBounds.Size;
        }

        public override void clearRect(int x, int y, int width, int height)
        {
            using Brush br = bgcolor != Color.Empty ? new SolidBrush(bgcolor) : brush;
            CompositingMode tempMode = g.CompositingMode;
            g.CompositingMode = CompositingMode.SourceCopy;
            g.FillRectangle(br, x, y, width, height);
            g.CompositingMode = tempMode;
        }

        public override void clipRect(int x, int y, int w, int h)
        {
            g.IntersectClip(new Rectangle(x, y, w, h));
        }

        public override void clip(java.awt.Shape shape)
        {
            if (shape == null)
            {
                // note that ComponentGraphics overrides clip() to throw a NullPointerException when shape is null
                g.ResetClip();
            }
            else
            {
                g.IntersectClip(new Region(J2C.ConvertShape(shape)));
            }
        }

        public override void dispose()
        {
            pen?.Dispose();
            brush?.Dispose();
            g.Dispose(); //for dispose we does not need to synchronize the buffer of a bitmap
        }

        public override void drawArc(int x, int y, int width, int height, int startAngle, int arcAngle)
        {
            g.DrawArc(pen, x, y, width, height, 360 - startAngle - arcAngle, arcAngle);
        }

        public override void drawBytes(byte[] data, int offset, int length, int x, int y)
        {
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = (char)data[offset + i];
            }
            drawChars(chars, 0, length, x, y);
        }

        public override void drawChars(char[] data, int offset, int length, int x, int y)
        {
            drawString(new string(data, offset, length), x, y);
        }

        public override bool drawImage(java.awt.Image img, int dx1, int dy1, int dx2, int dy2, int sx1, int sy1, int sx2, int sy2, java.awt.Color color, ImageObserver observer)
        {
            Image image = J2C.ConvertImage(img);
            if (image == null)
            {
                return false;
            }
            Rectangle destRect = new(dx1, dy1, dx2 - dx1, dy2 - dy1);
            using (Brush brush = new SolidBrush(composite.GetColor(color)))
            {
                g.FillRectangle(brush, destRect);
            }
            lock (image)
            {
                g.DrawImage(image, destRect, sx1, sy1, sx2 - sx1, sy2 - sy1, GraphicsUnit.Pixel, composite.GetImageAttributes());
            }
            return true;
        }

        public override bool drawImage(java.awt.Image img, int dx1, int dy1, int dx2, int dy2, int sx1, int sy1, int sx2, int sy2, ImageObserver observer)
        {
            Image image = J2C.ConvertImage(img);
            if (image == null)
            {
                return false;
            }
            Rectangle destRect = new(dx1, dy1, dx2 - dx1, dy2 - dy1);
            lock (image)
            {
                g.DrawImage(image, destRect, sx1, sy1, sx2 - sx1, sy2 - sy1, GraphicsUnit.Pixel, composite.GetImageAttributes());
            }
            return true;
        }

        public override bool drawImage(java.awt.Image img, int x, int y, int width, int height, java.awt.Color bgcolor, ImageObserver observer)
        {
            Image image = J2C.ConvertImage(img);
            if (image == null)
            {
                return false;
            }
            using (Brush brush = new SolidBrush(composite.GetColor(bgcolor)))
            {
                g.FillRectangle(brush, x, y, width, height);
            }
            lock (image)
            {
                g.DrawImage(image, new Rectangle(x, y, width, height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, composite.GetImageAttributes());
            }
            return true;
        }

        public override bool drawImage(java.awt.Image img, int x, int y, java.awt.Color bgcolor, ImageObserver observer)
        {
            return img switch
            {
                null => false,
                _ => drawImage(img, x, y, img.getWidth(observer), img.getHeight(observer), bgcolor, observer)
            };
        }

        public override bool drawImage(java.awt.Image img, int x, int y, int width, int height, ImageObserver observer)
        {
            Image image = J2C.ConvertImage(img);
            if (image == null)
            {
                return false;
            }
            lock (image)
            {
                g.DrawImage(image, new Rectangle(x, y, width, height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, composite.GetImageAttributes());
            }
            return true;
        }

        public override bool drawImage(java.awt.Image img, int x, int y, ImageObserver observer)
        {
            return img switch
            {
                null => false,
                _ => drawImage(img, x, y, img.getWidth(observer), img.getHeight(observer), observer)
            };
        }

        public override void drawLine(int x1, int y1, int x2, int y2)
        {
            // HACK DrawLine doesn't appear to draw the last pixel, so for single pixel lines, we have
            // a freaky workaround
            if (x1 == x2 && y1 == y2)
            {
                g.DrawLine(pen, x1, y1, x1 + 0.01f, y2 + 0.01f);
            }
            else
            {
                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        public override void drawOval(int x, int y, int w, int h)
        {
            g.DrawEllipse(pen, x, y, w, h);
        }

        public override void drawPolygon(java.awt.Polygon polygon)
        {
            drawPolygon(polygon.xpoints, polygon.ypoints, polygon.npoints);
        }

        public override void drawPolygon(int[] aX, int[] aY, int aLength)
        {
            Point[] points = new Point[aLength];
            for (int i = 0; i < aLength; i++)
            {
                points[i].X = aX[i];
                points[i].Y = aY[i];
            }
            g.DrawPolygon(pen, points);
        }

        /// <summary>
        /// Draw a sequence of connected lines
        /// </summary>
        /// <param name="aX">Array of x coordinates</param>
        /// <param name="aY">Array of y coordinates</param>
        /// <param name="aLength">Length of coordinate arrays</param>
        public override void drawPolyline(int[] aX, int[] aY, int aLength)
        {
            for (int i = 0; i < aLength - 1; i++)
            {
                Point point1 = new(aX[i], aY[i]);
                Point point2 = new(aX[i + 1], aY[i + 1]);
                g.DrawLine(pen, point1, point2);
            }
        }

        public override void drawRect(int x, int y, int width, int height)
        {
            g.DrawRectangle(pen, x, y, width, height);
        }

        /// <summary>
        /// Apparently there is no rounded rec function in .Net. Draw the
        /// rounded rectangle by using lines and arcs.
        /// </summary>
		public override void drawRoundRect(int x, int y, int w, int h, int arcWidth, int arcHeight)
        {
            using GraphicsPath gp = J2C.ConvertRoundRect(x, y, w, h, arcWidth, arcHeight);
            g.DrawPath(pen, gp);
        }

        public override void fill3DRect(int x, int y, int width, int height, bool raised)
        {
            java.awt.Paint p = getPaint();
            java.awt.Color c = getColor();
            java.awt.Color brighter = c.brighter();
            java.awt.Color darker = c.darker();

            if (!raised)
            {
                setColor(darker);
            }
            else if (p != c)
            {
                setColor(c);
            }
            fillRect(x + 1, y + 1, width - 2, height - 2);
            setColor(raised ? brighter : darker);
            fillRect(x, y, 1, height);
            fillRect(x + 1, y, width - 2, 1);
            setColor(raised ? darker : brighter);
            fillRect(x + 1, y + height - 1, width - 1, 1);
            fillRect(x + width - 1, y, 1, height - 1);
            setPaint(p);
        }

        public override void fillArc(int x, int y, int width, int height, int startAngle, int arcAngle)
        {
            g.FillPie(brush, x, y, width, height, 360 - startAngle - arcAngle, arcAngle);
        }

        public override void fillOval(int x, int y, int w, int h)
        {
            g.FillEllipse(brush, x, y, w, h);
        }

        public override void fillPolygon(java.awt.Polygon polygon)
        {
            fillPolygon(polygon.xpoints, polygon.ypoints, polygon.npoints);
        }

        public override void fillPolygon(int[] aX, int[] aY, int aLength)
        {
            Point[] points = new Point[aLength];
            for (int i = 0; i < aLength; i++)
            {
                points[i].X = aX[i];
                points[i].Y = aY[i];
            }
            g.FillPolygon(brush, points);
        }

        public override void fillRect(int x, int y, int width, int height)
        {
            g.FillRectangle(brush, x, y, width, height);
        }

        public override void fillRoundRect(int x, int y, int w, int h, int arcWidth, int arcHeight)
        {
            GraphicsPath gp = J2C.ConvertRoundRect(x, y, w, h, arcWidth, arcHeight);
            g.FillPath(brush, gp);
            gp.Dispose();
        }

        public override java.awt.Shape getClip()
        {
            return getClipBounds();
        }

        public override java.awt.Rectangle getClipBounds(java.awt.Rectangle r)
        {
            using Region clip = g.Clip;
            if (!clip.IsInfinite(g))
            {
                RectangleF rec = clip.GetBounds(g);
                r.x = (int)rec.X;
                r.y = (int)rec.Y;
                r.width = (int)rec.Width;
                r.height = (int)rec.Height;
            }
            return r;
        }

        public override java.awt.Rectangle getClipBounds()
        {
            using Region clip = g.Clip;
            if (clip.IsInfinite(g))
            {
                return null;
            }
            RectangleF rec = clip.GetBounds(g);
            return C2J.ConvertRectangle(rec);
        }

        [Obsolete]
        public override java.awt.Rectangle getClipRect()
        {
            return getClipBounds();
        }

        public override java.awt.Color getColor()
        {
            javaColor ??= composite.GetColor(color);
            return javaColor;
        }

        public override java.awt.Font getFont()
        {
            return font;
        }

        public override java.awt.FontMetrics getFontMetrics(java.awt.Font f)
        {
            return new NetFontMetrics(f);
        }

        public override java.awt.FontMetrics getFontMetrics()
        {
            return new NetFontMetrics(font);
        }

        public override void setClip(int x, int y, int width, int height)
        {
            g.Clip = new Region(new Rectangle(x, y, width, height));
        }

        public override void setClip(java.awt.Shape shape)
        {
            if (shape == null)
            {
                Region clip = g.Clip;
                clip.MakeInfinite();
                g.Clip = clip;
            }
            else
            {
                g.Clip = new Region(J2C.ConvertShape(shape));
            }
        }

        public override void setColor(java.awt.Color color)
        {
            if (color == null || color == javaPaint)
            {
                // Does not change the color, if it is null like in SunGraphics2D
                return;
            }
            javaPaint = javaColor = color;
            this.color = composite.GetColor(color);
            if (brush is SolidBrush)
            {
                ((SolidBrush)brush).Color = this.color;
            }
            else
            {
                brush.Dispose();
                brush = new SolidBrush(this.color);
            }
            pen.Color = this.color;
            pen.Brush = brush;
        }

        public override void setFont(java.awt.Font f)
        {
            if (f != null && f != font)
            {
                netfont = f.getNetFont();
                font = f;
                baseline = getBaseline(netfont, g.TextRenderingHint);
            }
        }

        public override void setPaintMode()
        {
            throw new NotImplementedException();
        }

        public override void setXORMode(java.awt.Color param)
        {
            if (param == null)
            {
                throw new java.lang.IllegalArgumentException("null XORColor");
            }
            throw new NotImplementedException();
        }

        public override void translate(int x, int y)
        {
            Matrix transform = g.Transform;
            transform.Translate(x, y);
            g.Transform = transform;
        }

        public override void draw(java.awt.Shape shape)
        {
            using GraphicsPath gp = J2C.ConvertShape(shape);
            g.DrawPath(pen, gp);
        }

        public override bool drawImage(java.awt.Image img, java.awt.geom.AffineTransform xform, ImageObserver observer)
        {
            if (img == null)
            {
                return true;
            }

            if (xform == null || xform.isIdentity())
            {
                return drawImage(img, 0, 0, null, observer);
            }

            NetGraphics clone = (NetGraphics)create();
            clone.transform(xform);
            bool rendered = clone.drawImage(img, 0, 0, null, observer);
            clone.dispose();
            return rendered;
        }

        public override void drawImage(BufferedImage image, BufferedImageOp op, int x, int y)
        {
            switch (op)
            {
                case null:
                    drawImage(image, x, y, null);
                    break;
                case AffineTransformOp:
                    Console.WriteLine(new System.Diagnostics.StackTrace());
                    throw new NotImplementedException();
                default:
                    drawImage(op.filter(image, null), x, y, null);
                    break;
            }
        }

        public override void drawRenderedImage(RenderedImage img, java.awt.geom.AffineTransform xform)
        {
            if (img == null)
            {
                return;
            }

            // BufferedImage case: use a simple drawImage call
            if (img is BufferedImage bufImg)
            {
                drawImage(bufImg, xform, null);
                return;
            }
            throw new NotImplementedException("drawRenderedImage not implemented for images which are not BufferedImages.");
        }

        public override void drawRenderableImage(java.awt.image.renderable.RenderableImage image, java.awt.geom.AffineTransform xform)
        {
            throw new NotImplementedException();
        }

        public override void drawString(string str, int x, int y)
        {
            drawString(str, x, y);
        }

        public override void drawString(string text, float x, float y)
        {
            if (text.Length == 0)
            {
                return;
            }
            CompositingMode origCM = g.CompositingMode;
            try
            {
                if (origCM != CompositingMode.SourceOver)
                {
                    // Java has a different behaviar for AlphaComposite and Text Antialiasing
                    g.CompositingMode = CompositingMode.SourceOver;
                }

                bool fractional = isFractionalMetrics();
                if (fractional || !sun.font.StandardGlyphVector.isSimpleString(font, text))
                {
                    g.DrawString(text, netfont, brush, x, y - baseline, FORMAT);
                }
                else
                {
                    // fixed metric for simple text, we position every character to simulate the Java behaviour
                    //java.awt.font.FontRenderContext frc = new(null, isAntiAlias(), fractional);
                    java.awt.FontMetrics metrics = getFontMetrics(font);
                    y -= baseline;
                    for (int i = 0; i < text.Length; i++)
                    {
                        g.DrawString(text.Substring(i, 1), netfont, brush, x, y, FORMAT);
                        x += metrics.charWidth(text[i]);
                    }
                }
            }
            finally
            {
                if (origCM != CompositingMode.SourceOver)
                {
                    g.CompositingMode = origCM;
                }
            }
        }

        public override void drawString(java.text.AttributedCharacterIterator iterator, int x, int y)
        {
            drawString(iterator, x, y);
        }

        public override void drawString(java.text.AttributedCharacterIterator iterator, float x, float y)
        {
            if (iterator == null)
            {
                throw new java.lang.NullPointerException("AttributedCharacterIterator is null");
            }
            if (iterator.getBeginIndex() == iterator.getEndIndex())
            {
                return; /* nothing to draw */
            }
            java.awt.font.TextLayout tl = new(iterator, getFontRenderContext());
            tl.draw(this, x, y);
        }

        public override void fill(java.awt.Shape shape)
        {
            g.FillPath(brush, J2C.ConvertShape(shape));
        }

        public override bool hit(java.awt.Rectangle rect, java.awt.Shape s, bool onStroke)
        {
            if (onStroke)
            {
                //TODO use stroke
                //s = stroke.createStrokedShape(s);
            }
            return s.intersects(rect);
        }

        public override java.awt.GraphicsConfiguration getDeviceConfiguration()
        {
            return new NetGraphicsConfiguration(Screen.PrimaryScreen);
        }

        public override void setComposite(java.awt.Composite comp)
        {
            if (javaComposite == comp)
            {
                return;
            }

            javaComposite = comp ?? throw new java.lang.IllegalArgumentException("null Composite");
            java.awt.Paint oldPaint = getPaint(); //getPaint() is never null
            composite = CompositeHelper.Create(comp, g);
            javaPaint = null;
            setPaint(oldPaint);
        }

        public override void setPaint(java.awt.Paint paint)
        {
            if (paint == null || javaPaint == paint)
            {
                return;
            }
            else if (paint is java.awt.Color _color)
            {
                setColor(_color);
                return;
            }

            javaPaint = paint;

            if (paint is java.awt.GradientPaint gp)
            {
                LinearGradientBrush linear;
                if (gp.isCyclic())
                {
                    linear = new LinearGradientBrush(
                        J2C.ConvertPoint(gp.getPoint1()),
                        J2C.ConvertPoint(gp.getPoint2()),
                        composite.GetColor(gp.getColor1()),
                        composite.GetColor(gp.getColor2()));
                }
                else
                {
                    //HACK because .NET does not support continue gradient like Java else Tile Gradient
                    //that we receize the rectangle very large (factor z) and set 4 color values
                    // a exact solution will calculate the size of the Graphics with the current transform
                    Color color1 = composite.GetColor(gp.getColor1());
                    Color color2 = composite.GetColor(gp.getColor2());
                    float x1 = (float)gp.getPoint1().getX();
                    float x2 = (float)gp.getPoint2().getX();
                    float y1 = (float)gp.getPoint1().getY();
                    float y2 = (float)gp.getPoint2().getY();
                    float diffX = x2 - x1;
                    float diffY = y2 - y1;
                    const float z = 60; //HACK zoom factor, with a larger factor .NET will make the gradient wider.
                    linear = new LinearGradientBrush(
                        new PointF(x1 - z * diffX, y1 - z * diffY),
                        new PointF(x2 + z * diffX, y2 + z * diffY),
                        color1,
                        color1);
                    ColorBlend colorBlend = new(4);
                    Color[] colors = colorBlend.Colors;
                    colors[0] = colors[1] = color1;
                    colors[2] = colors[3] = color2;
                    float[] positions = colorBlend.Positions;
                    positions[1] = z / (2 * z + 1);
                    positions[2] = (z + 1) / (2 * z + 1);
                    positions[3] = 1.0f;
                    linear.InterpolationColors = colorBlend;
                }
                linear.WrapMode = WrapMode.TileFlipXY;
                brush = linear;
                pen.Brush = brush;
                return;
            }
            else if (paint is java.awt.TexturePaint tp)
            {
                Bitmap txtr = J2C.ConvertImage(tp.getImage());
                java.awt.geom.Rectangle2D anchor = tp.getAnchorRect();
                TextureBrush txtBrush;
                brush = txtBrush = new TextureBrush(txtr, new Rectangle(0, 0, txtr.Width, txtr.Height), composite.GetImageAttributes());
                txtBrush.TranslateTransform((float)anchor.getX(), (float)anchor.getY());
                txtBrush.ScaleTransform((float)anchor.getWidth() / txtr.Width, (float)anchor.getHeight() / txtr.Height);
                txtBrush.WrapMode = WrapMode.Tile;
                pen.Brush = brush;
                return;
            }
            else if (paint is java.awt.LinearGradientPaint lgp)
            {
                PointF start = J2C.ConvertPoint(lgp.getStartPoint());
                PointF end = J2C.ConvertPoint(lgp.getEndPoint());

                java.awt.Color[] javaColors = lgp.getColors();
                ColorBlend colorBlend;
                Color[] colors;
                bool noCycle = lgp.getCycleMethod() == java.awt.MultipleGradientPaint.CycleMethod.NO_CYCLE;
                if (noCycle)
                {
                    //HACK because .NET does not support continue gradient like Java else Tile Gradient
                    //that we receize the rectangle very large (factor z) and set 2 additional color values
                    //an exact solution will calculate the size of the Graphics with the current transform
                    float diffX = end.X - start.X;
                    float diffY = end.Y - start.Y;
                    SizeF size = GetSize();
                    //HACK zoom factor, with a larger factor .NET will make the gradient wider.
                    float z = Math.Min(10, Math.Max(size.Width / diffX, size.Height / diffY));
                    start.X -= z * diffX;
                    start.Y -= z * diffY;
                    end.X += z * diffX;
                    end.Y += z * diffY;

                    colorBlend = new ColorBlend(javaColors.Length + 2);
                    colors = colorBlend.Colors;
                    float[] fractions = lgp.getFractions();
                    float[] positions = colorBlend.Positions;
                    for (int i = 0; i < javaColors.Length; i++)
                    {
                        colors[i + 1] = composite.GetColor(javaColors[i]);
                        positions[i + 1] = (z + fractions[i]) / (2 * z + 1);
                    }
                    colors[0] = colors[1];
                    colors[^1] = colors[^2];
                    positions[^1] = 1.0f;
                }
                else
                {
                    colorBlend = new ColorBlend(javaColors.Length);
                    colors = colorBlend.Colors;
                    colorBlend.Positions = lgp.getFractions();
                    for (int i = 0; i < javaColors.Length; i++)
                    {
                        colors[i] = composite.GetColor(javaColors[i]);
                    }
                }
                LinearGradientBrush linear = new(start, end, colors[0], colors[^1])
                {
                    InterpolationColors = colorBlend
                };
                switch (lgp.getCycleMethod().ordinal())
                {
                    case (int)java.awt.MultipleGradientPaint.CycleMethod.__Enum.NO_CYCLE:
                    case (int)java.awt.MultipleGradientPaint.CycleMethod.__Enum.REFLECT:
                        linear.WrapMode = WrapMode.TileFlipXY;
                        break;
                    case (int)java.awt.MultipleGradientPaint.CycleMethod.__Enum.REPEAT:
                        linear.WrapMode = WrapMode.Tile;
                        break;
                }
                brush = linear;
                pen.Brush = brush;
                return;
            }
            else if (paint is java.awt.RadialGradientPaint rgp)
            {
                GraphicsPath path = new();
                SizeF size = GetSize();

                PointF center = J2C.ConvertPoint(rgp.getCenterPoint());

                float radius = rgp.getRadius();
                int factor = (int)Math.Ceiling(Math.Max(size.Width, size.Height) / radius);

                float diameter = radius * factor;
                path.AddEllipse(center.X - diameter, center.Y - diameter, diameter * 2, diameter * 2);

                java.awt.Color[] javaColors = rgp.getColors();
                float[] fractions = rgp.getFractions();
                int length = javaColors.Length;
                ColorBlend colorBlend = new(length * factor);
                Color[] colors = colorBlend.Colors;
                float[] positions = colorBlend.Positions;

                for (int c = 0, j = length - 1; j >= 0;)
                {
                    positions[c] = (1 - fractions[j]) / factor;
                    colors[c++] = composite.GetColor(javaColors[j--]);
                }

                java.awt.MultipleGradientPaint.CycleMethod.__Enum cycle = (java.awt.MultipleGradientPaint.CycleMethod.__Enum)rgp.getCycleMethod().ordinal();
                for (int f = 1; f < factor; f++)
                {
                    int off = f * length;
                    for (int c = 0, j = length - 1; j >= 0; j--, c++)
                    {
                        switch (cycle)
                        {
                            case java.awt.MultipleGradientPaint.CycleMethod.__Enum.REFLECT:
                                if (f % 2 == 0)
                                {
                                    positions[off + c] = (f + 1 - fractions[j]) / factor;
                                    colors[off + c] = colors[c];
                                }
                                else
                                {
                                    positions[off + c] = (f + fractions[c]) / factor;
                                    colors[off + c] = colors[j];
                                }
                                break;
                            case java.awt.MultipleGradientPaint.CycleMethod.__Enum.NO_CYCLE:
                                positions[off + c] = (f + 1 - fractions[j]) / factor;
                                break;
                            default: //CycleMethod.REPEAT
                                positions[off + c] = (f + 1 - fractions[j]) / factor;
                                colors[off + c] = colors[c];
                                break;
                        }
                    }
                }
                if (cycle == java.awt.MultipleGradientPaint.CycleMethod.__Enum.NO_CYCLE && factor > 1)
                {
                    Array.Copy(colors, 0, colors, colors.Length - length, length);
                    Color color = colors[length - 1];
                    for (int i = colors.Length - length - 1; i >= 0; i--)
                    {
                        colors[i] = color;
                    }
                }

                PathGradientBrush pathBrush = new(path)
                {
                    CenterPoint = center,
                    InterpolationColors = colorBlend
                };

                brush = pathBrush;
                pen.Brush = brush;
                return;
            }
            else
            {
                //generic paint to brush conversion for custom paints
                //the tranform of the graphics should not change between the creation and it usage
                using Matrix transform = g.Transform;
                SizeF size = GetSize();
                int width = (int)size.Width;
                int height = (int)size.Height;
                java.awt.Rectangle bounds = new(0, 0, width, height);

                java.awt.PaintContext context = paint.createContext(ColorModel.getRGBdefault(), bounds, bounds, C2J.ConvertMatrix(transform), getRenderingHints());
                WritableRaster raster = (WritableRaster)context.getRaster(0, 0, width, height);
                BufferedImage txtrImage = new(context.getColorModel(), raster, true, null);
                Bitmap txtr = J2C.ConvertImage(txtrImage);

                TextureBrush txtBrush;
                brush = txtBrush = new TextureBrush(txtr, new Rectangle(0, 0, width, height), composite.GetImageAttributes());
                transform.Invert();
                txtBrush.Transform = transform;
                txtBrush.WrapMode = WrapMode.Tile;
                pen.Brush = brush;
                return;
            }
        }

        public override void setStroke(java.awt.Stroke stroke)
        {
            if (this.stroke != null && this.stroke.Equals(stroke))
            {
                return;
            }
            this.stroke = stroke;
            if (stroke is java.awt.BasicStroke s)
            {
                pen.Width = s.getLineWidth();

                SetLineJoin(s);
                SetLineDash(s);
            }
            else
            {
                Console.WriteLine("Unknown Stroke type: " + stroke.GetType().FullName);
            }
        }

        private void SetLineJoin(java.awt.BasicStroke s)
        {
            pen.MiterLimit = s.getMiterLimit();
            pen.LineJoin = J2C.ConvertLineJoin(s.getLineJoin());
        }

        private void SetLineDash(java.awt.BasicStroke s)
        {
            float[] dash = s.getDashArray();
            if (dash == null)
            {
                pen.DashStyle = DashStyle.Solid;
            }
            else
            {
                if (dash.Length % 2 == 1)
                {
                    int len = dash.Length;
                    Array.Resize(ref dash, len * 2);
                    Array.Copy(dash, 0, dash, len, len);
                }
                float lineWidth = s.getLineWidth();
                if (lineWidth > 1) // for values < 0 there is no correctur needed
                {
                    for (int i = 0; i < dash.Length; i++)
                    {
                        //dividing by line thickness because of the representation difference
                        dash[i] = dash[i] / lineWidth;
                    }
                }
                // To fix the problem where solid style in Java can be represented at { 1.0, 0.0 }.
                // In .NET, however, array can only have positive value
                if (dash.Length == 2 && dash[^1] == 0)
                {
                    Array.Resize(ref dash, 1);
                }

                float dashPhase = s.getDashPhase();
                // correct the dash cap
                switch (s.getEndCap())
                {
                    case java.awt.BasicStroke.CAP_BUTT:
                        pen.DashCap = DashCap.Flat;
                        break;
                    case java.awt.BasicStroke.CAP_ROUND:
                        pen.DashCap = DashCap.Round;
                        break;
                    case java.awt.BasicStroke.CAP_SQUARE:
                        pen.DashCap = DashCap.Flat;
                        // there is no equals DashCap in .NET, we need to emulate it
                        dashPhase += lineWidth / 2;
                        for (int i = 0; i < dash.Length; i++)
                        {
                            if (i % 2 == 0)
                            {
                                dash[i] += 1;
                            }
                            else
                            {
                                dash[i] = Math.Max(0.00001F, dash[i] - 1);
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("Unknown dash cap type:" + s.getEndCap());
                        break;
                }

                // calc the dash offset
                if (lineWidth > 0)
                {
                    //dividing by line thickness because of the representation difference
                    pen.DashOffset = dashPhase / lineWidth;
                }
                else
                {
                    // thickness == 0
                    if (dashPhase > 0)
                    {
                        pen.Width = lineWidth = 0.001F; // hack to prevent a division with 0
                        pen.DashOffset = dashPhase / lineWidth;
                    }
                    else
                    {
                        pen.DashOffset = 0;
                    }
                }

                // set the final dash pattern 
                pen.DashPattern = dash;
            }
        }

        public override void setRenderingHint(java.awt.RenderingHints.Key hintKey, object hintValue)
        {
            if (hintKey == java.awt.RenderingHints.KEY_ANTIALIASING)
            {
                if (hintValue == java.awt.RenderingHints.VALUE_ANTIALIAS_DEFAULT)
                {
                    g.SmoothingMode = SmoothingMode.Default;
                    g.PixelOffsetMode = PixelOffsetMode.Default;
                    return;
                }
                else if (hintValue == java.awt.RenderingHints.VALUE_ANTIALIAS_OFF)
                {
                    g.SmoothingMode = SmoothingMode.None;
                    g.PixelOffsetMode = PixelOffsetMode.Default;
                    return;
                }
                else if (hintValue == java.awt.RenderingHints.VALUE_ANTIALIAS_ON)
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    return;
                }
                return;
            }
            else if (hintKey == java.awt.RenderingHints.KEY_INTERPOLATION)
            {
                if (hintValue == java.awt.RenderingHints.VALUE_INTERPOLATION_BILINEAR)
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    return;
                }
                else if (hintValue == java.awt.RenderingHints.VALUE_INTERPOLATION_BICUBIC)
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    return;
                }
                else if (hintValue == java.awt.RenderingHints.VALUE_INTERPOLATION_NEAREST_NEIGHBOR)
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    return;
                }
                return;
            }
            else if (hintKey == java.awt.RenderingHints.KEY_TEXT_ANTIALIASING)
            {
                if (hintValue == java.awt.RenderingHints.VALUE_TEXT_ANTIALIAS_DEFAULT ||
                    hintValue == java.awt.RenderingHints.VALUE_TEXT_ANTIALIAS_OFF)
                {
                    setTextRenderingHint(TextRenderingHint.SingleBitPerPixelGridFit);
                    textAntialiasHint = hintValue;
                    return;
                }
                else if (hintValue == java.awt.RenderingHints.VALUE_TEXT_ANTIALIAS_ON)
                {
                    setTextRenderingHint(TextRenderingHint.AntiAlias);
                    textAntialiasHint = hintValue;
                    return;
                }
                return;
            }
            else if (hintKey == java.awt.RenderingHints.KEY_FRACTIONALMETRICS)
            {
                if (hintValue == java.awt.RenderingHints.VALUE_FRACTIONALMETRICS_DEFAULT ||
                    hintValue == java.awt.RenderingHints.VALUE_FRACTIONALMETRICS_OFF ||
                    hintValue == java.awt.RenderingHints.VALUE_FRACTIONALMETRICS_ON)
                {
                    fractionalHint = hintValue;
                }
                return;
            }

        }

        public override object getRenderingHint(java.awt.RenderingHints.Key hintKey)
        {
            return getRenderingHints().get(hintKey);
        }

        public override void setRenderingHints(Map hints)
        {
            addRenderingHints(hints);
            //TODO all not included values should reset to default, but was is default?
        }

        public override void addRenderingHints(Map hints)
        {
            Iterator iterator = hints.entrySet().iterator();
            while (iterator.hasNext())
            {
                Map.Entry entry = (Map.Entry)iterator.next();
                setRenderingHint((java.awt.RenderingHints.Key)entry.getKey(), entry.getValue());
            }
        }

        public override java.awt.RenderingHints getRenderingHints()
        {
            java.awt.RenderingHints hints = new(null);
            switch (g.SmoothingMode)
            {
                case SmoothingMode.Default:
                    hints.put(java.awt.RenderingHints.KEY_ANTIALIASING, java.awt.RenderingHints.VALUE_ANTIALIAS_DEFAULT);
                    break;
                case SmoothingMode.None:
                    hints.put(java.awt.RenderingHints.KEY_ANTIALIASING, java.awt.RenderingHints.VALUE_ANTIALIAS_OFF);
                    break;
                case SmoothingMode.AntiAlias:
                    hints.put(java.awt.RenderingHints.KEY_ANTIALIASING, java.awt.RenderingHints.VALUE_ANTIALIAS_ON);
                    break;
            }

            switch (g.InterpolationMode)
            {
                case InterpolationMode.Bilinear:
                case InterpolationMode.HighQualityBilinear:
                    hints.put(java.awt.RenderingHints.KEY_INTERPOLATION, java.awt.RenderingHints.VALUE_INTERPOLATION_BILINEAR);
                    break;
                case InterpolationMode.Bicubic:
                case InterpolationMode.HighQualityBicubic:
                    hints.put(java.awt.RenderingHints.KEY_INTERPOLATION, java.awt.RenderingHints.VALUE_INTERPOLATION_BICUBIC);
                    break;
                case InterpolationMode.NearestNeighbor:
                    hints.put(java.awt.RenderingHints.KEY_INTERPOLATION, java.awt.RenderingHints.VALUE_INTERPOLATION_NEAREST_NEIGHBOR);
                    break;
            }

            hints.put(java.awt.RenderingHints.KEY_TEXT_ANTIALIASING, textAntialiasHint);
            hints.put(java.awt.RenderingHints.KEY_FRACTIONALMETRICS, fractionalHint);
            return hints;
        }

        public override void translate(double x, double y)
        {
            Matrix transform = g.Transform;
            transform.Translate((float)x, (float)y);
            g.Transform = transform;
        }

        private static double RadiansToDegrees(double radians)
        {
            return radians * (180 / Math.PI);
        }

        public override void rotate(double theta)
        {
            Matrix transform = g.Transform;
            transform.Rotate((float)RadiansToDegrees(theta));
            g.Transform = transform;
        }

        public override void rotate(double theta, double x, double y)
        {
            Matrix transform = g.Transform;
            transform.Translate((float)x, (float)y);
            transform.Rotate((float)RadiansToDegrees(theta));
            transform.Translate(-(float)x, -(float)y);
            g.Transform = transform;
        }

        public override void scale(double scaleX, double scaleY)
        {
            using Matrix transform = g.Transform;
            transform.Scale((float)scaleX, (float)scaleY);
            g.Transform = transform;
        }

        public override void shear(double shearX, double shearY)
        {
            using Matrix transform = g.Transform;
            transform.Shear((float)shearX, (float)shearY);
            g.Transform = transform;
        }

        public override void transform(java.awt.geom.AffineTransform tx)
        {
            using Matrix transform = g.Transform,
                matrix = J2C.ConvertTransform(tx);
            transform.Multiply(matrix);
            g.Transform = transform;
        }

        public override void setTransform(java.awt.geom.AffineTransform tx)
        {
            g.Transform = J2C.ConvertTransform(tx);
        }

        public override java.awt.geom.AffineTransform getTransform()
        {
            using Matrix matrix = g.Transform;
            return C2J.ConvertMatrix(matrix);
        }

        public override java.awt.Paint getPaint()
        {
            javaPaint ??= composite.GetColor(color);
            return javaPaint;
        }

        public override java.awt.Composite getComposite()
        {
            return javaComposite;
        }

        public override void setBackground(java.awt.Color backcolor)
        {
            bgcolor = backcolor == null ? Color.Empty : Color.FromArgb(backcolor.getRGB());
        }

        public override java.awt.Color getBackground()
        {
            return bgcolor == Color.Empty ? null : new java.awt.Color(bgcolor.ToArgb(), true);
        }

        public override java.awt.Stroke getStroke()
        {
            return stroke switch
            {
                null => defaultStroke,
                _ => stroke
            };
        }

        internal void setTextRenderingHint(TextRenderingHint hint)
        {
            g.TextRenderingHint = hint;
            baseline = getBaseline(netfont, hint);
        }

        /// <summary>
        /// Caclulate the baseline from a font and a TextRenderingHint
        /// </summary>
        /// <param name="font">the font</param>
        /// <param name="hint">the used TextRenderingHint</param>
        /// <returns></returns>
        private static int getBaseline(Font font, TextRenderingHint hint)
        {
            lock (baselines)
            {
                string key = font.ToString() + hint.ToString();
                if (!baselines.TryGetValue(key, out int baseline))
                {
                    FontFamily family = font.FontFamily;
                    FontStyle style = font.Style;
                    float ascent = family.GetCellAscent(style);
                    float lineSpace = family.GetLineSpacing(style);

                    baseline = (int)Math.Round(font.GetHeight() * ascent / lineSpace);

                    // Until this point the calulation use only the Font. But with different TextRenderingHint there are smal differences.
                    // There is no API that calulate the offset from TextRenderingHint that we messure it.
                    const int w = 3;
                    const int h = 3;

                    using (Bitmap bitmap = new(w, h))
                    {
                        Graphics g = Graphics.FromImage(bitmap);
                        g.TextRenderingHint = hint;
                        g.FillRectangle(new SolidBrush(Color.White), 0, 0, w, h);
                        g.DrawString("A", font, new SolidBrush(Color.Black), 0, -baseline, FORMAT);
                        g.DrawString("X", font, new SolidBrush(Color.Black), 0, -baseline, FORMAT);
                        g.Dispose();


                        int y = 0;
                    LINE:
                        while (y < h)
                        {
                            for (int x = 0; x < w; x++)
                            {
                                Color color = bitmap.GetPixel(x, y);
                                if (color.GetBrightness() < 0.5)
                                {
                                    //there is a black pixel, we continue in the next line.
                                    baseline++;
                                    y++;
                                    goto LINE;
                                }
                            }
                            break; // there was a line without black pixel
                        }
                    }

                    baselines[key] = baseline;
                }
                return baseline;
            }
        }

        private bool isAntiAlias()
        {
            switch (g.TextRenderingHint)
            {
                case TextRenderingHint.AntiAlias:
                case TextRenderingHint.AntiAliasGridFit:
                case TextRenderingHint.ClearTypeGridFit:
                    return true;
                default:
                    return false;
            }
        }

        private bool isFractionalMetrics()
        {
            return fractionalHint == java.awt.RenderingHints.VALUE_FRACTIONALMETRICS_ON;
        }

        public override java.awt.font.FontRenderContext getFontRenderContext()
        {
            return new java.awt.font.FontRenderContext(getTransform(), isAntiAlias(), isFractionalMetrics());
        }

        public override void drawGlyphVector(java.awt.font.GlyphVector gv, float x, float y)
        {
            java.awt.font.FontRenderContext frc = gv.getFontRenderContext();
            Matrix currentMatrix = null;
            Font currentFont = netfont;
            TextRenderingHint currentHint = g.TextRenderingHint;
            int currentBaseline = baseline;
            try
            {
                java.awt.Font javaFont = gv.getFont();
                if (javaFont != null)
                {
                    netfont = javaFont.getNetFont();
                }
                TextRenderingHint hint;
                if (frc.isAntiAliased())
                {
                    hint = frc.usesFractionalMetrics() ? TextRenderingHint.AntiAlias : TextRenderingHint.AntiAliasGridFit;
                }
                else
                {
                    hint = frc.usesFractionalMetrics() ? TextRenderingHint.SingleBitPerPixel : TextRenderingHint.SingleBitPerPixelGridFit;
                }
                g.TextRenderingHint = hint;
                baseline = getBaseline(netfont, hint);
                if (!frc.getTransform().equals(getTransform()))
                {
                    // save the old context and use the transformation from the renderContext
                    currentMatrix = g.Transform;
                    g.Transform = J2C.ConvertTransform(frc.getTransform());
                }
                drawString(J2C.ConvertGlyphVector(gv), x, y);
            }
            finally
            {
                // Restore the old context if needed
                g.TextRenderingHint = currentHint;
                baseline = currentBaseline;
                netfont = currentFont;
                if (currentMatrix != null)
                {
                    g.Transform = currentMatrix;
                }
            }
        }
    }

}
