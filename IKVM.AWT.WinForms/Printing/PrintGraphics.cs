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

using System;
using System.Drawing;

using java.awt.image;

namespace IKVM.AWT.WinForms.Printing
{
    internal class PrintGraphics : NetGraphics
    {
        private NetGraphicsState myState;
        private PrintGraphicsContext baseContext;
        private bool disposed = false;
        private bool isBase = true;

        internal PrintGraphics(Graphics g)
            : base(g, null, null, Color.White, Color.Black)
        {
            baseContext = new PrintGraphicsContext
            {
                Current = this
            };
        }

        public override java.awt.Graphics create()
        {
            checkState();
            myState = new NetGraphicsState();
            myState.saveGraphics(this);
            PrintGraphics newGraphics = (PrintGraphics)MemberwiseClone();
            newGraphics.myState = null;
            newGraphics.isBase = false;
            newGraphics.baseContext = baseContext;
            baseContext.Current = newGraphics; // since it is very likely that the next op will be on that graphics
            // this is similar to init
            myState.restoreGraphics(newGraphics);
            return newGraphics;
        }

        /// <summary>
        /// Checks whether the properties of this instance are set to the bse Graphics. If not, the context
        /// of the currently PrintGraphics is saved and the context if this instance is restored.
        /// </summary>
        private void checkState()
        {
            // this is required to simulate Graphics.create(), which is not possible in .NET
            // we simply call Save on create() an restore this state, if any method is called
            // on the current graphics. This will work for almost any use case of create()
            if (baseContext != null && baseContext.Current != this)
            {
                if (!baseContext.Current.disposed)
                {
                    if (baseContext.Current.myState == null)
                    {
                        baseContext.Current.myState = new NetGraphicsState(baseContext.Current);
                    }
                    else
                    {
                        baseContext.Current.myState.saveGraphics(baseContext.Current);
                    }
                }
                baseContext.Current = this;
                myState?.restoreGraphics(this);
            }
        }

        public override void copyArea(int x, int y, int width, int height, int dx, int dy)
        {
            throw new NotImplementedException();
        }

        public override void clearRect(int x, int y, int width, int height)
        {
            checkState();
            base.clearRect(x, y, width, height);
        }

        public override void clipRect(int x, int y, int w, int h)
        {
            checkState();
            base.clipRect(x, y, w, h);
        }

        public override void clip(java.awt.Shape shape)
        {
            checkState();
            base.clip(shape);
        }

        public override void dispose()
        {
            myState = null;
            pen?.Dispose();
            brush?.Dispose();
            disposed = true;
            if (!isBase)
            {
                // only dispose the underlying Graphics if this is the base PrintGraphics!
                return;
            }
            base.dispose();
        }

        public override void drawArc(int x, int y, int width, int height, int startAngle, int arcAngle)
        {
            checkState();
            base.drawArc(x, y, width, height, startAngle, arcAngle);
        }

        public override void drawBytes(byte[] data, int offset, int length, int x, int y)
        {
            checkState();
            base.drawBytes(data, offset, length, x, y);
        }

        public override void drawChars(char[] data, int offset, int length, int x, int y)
        {
            checkState();
            base.drawChars(data, offset, length, x, y);
        }

        public override bool drawImage(java.awt.Image img, int dx1, int dy1, int dx2, int dy2, int sx1, int sy1, int sx2, int sy2, java.awt.Color color, ImageObserver observer)
        {
            checkState();
            return base.drawImage(img, dx1, dy1, dx2, dy2, sx1, sy1, sx2, sy2, color, observer);
        }

        public override bool drawImage(java.awt.Image img, int dx1, int dy1, int dx2, int dy2, int sx1, int sy1, int sx2, int sy2, ImageObserver observer)
        {
            checkState();
            return base.drawImage(img, dx1, dy1, dx2, dy2, sx1, sy1, sx2, sy2, observer);
        }

        public override bool drawImage(java.awt.Image img, int x, int y, int width, int height, java.awt.Color bgcolor, ImageObserver observer)
        {
            checkState();
            return base.drawImage(img, x, y, width, height, bgcolor, observer);
        }

        public override bool drawImage(java.awt.Image img, int x, int y, java.awt.Color bgcolor, ImageObserver observer)
        {
            checkState();
            return base.drawImage(img, x, y, bgcolor, observer);
        }

        public override bool drawImage(java.awt.Image img, int x, int y, int width, int height, ImageObserver observer)
        {
            checkState();
            return base.drawImage(img, x, y, width, height, observer);
        }

        public override bool drawImage(java.awt.Image img, int x, int y, ImageObserver observer)
        {
            checkState();
            return base.drawImage(img, x, y, observer);
        }

        public override void drawLine(int x1, int y1, int x2, int y2)
        {
            checkState();
            base.drawLine(x1, y1, x2, y2);
        }

        public override void drawOval(int x, int y, int w, int h)
        {
            checkState();
            base.drawOval(x, y, w, h);
        }

        public override void drawPolygon(java.awt.Polygon polygon)
        {
            checkState();
            base.drawPolygon(polygon);
        }

        public override void drawPolygon(int[] aX, int[] aY, int aLength)
        {
            checkState();
            base.drawPolygon(aX, aY, aLength);
        }

        public override void drawPolyline(int[] aX, int[] aY, int aLength)
        {
            checkState();
            base.drawPolyline(aX, aY, aLength);
        }

        public override void drawRect(int x, int y, int width, int height)
        {
            checkState();
            base.drawRect(x, y, width, height);
        }

        public override void drawRoundRect(int x, int y, int w, int h, int arcWidth, int arcHeight)
        {
            checkState();
            base.drawRoundRect(x, y, w, h, arcWidth, arcHeight);
        }

        public override void fill3DRect(int x, int y, int width, int height, bool raised)
        {
            checkState();
            base.fill3DRect(x, y, width, height, raised);
        }

        public override void fillArc(int x, int y, int width, int height, int startAngle, int arcAngle)
        {
            checkState();
            base.fillArc(x, y, width, height, startAngle, arcAngle);
        }

        public override void fillOval(int x, int y, int w, int h)
        {
            checkState();
            base.fillOval(x, y, w, h);
        }

        public override void fillPolygon(java.awt.Polygon polygon)
        {
            checkState();
            base.fillPolygon(polygon);
        }

        public override void fillPolygon(int[] aX, int[] aY, int aLength)
        {
            checkState();
            base.fillPolygon(aX, aY, aLength);
        }

        public override void fillRect(int x, int y, int width, int height)
        {
            checkState();
            base.fillRect(x, y, width, height);
        }

        public override void fillRoundRect(int x, int y, int w, int h, int arcWidth, int arcHeight)
        {
            checkState();
            base.fillRoundRect(x, y, w, h, arcWidth, arcHeight);
        }

        public override java.awt.Shape getClip()
        {
            checkState();
            return base.getClip();
        }

        public override java.awt.Rectangle getClipBounds(java.awt.Rectangle r)
        {
            checkState();
            return base.getClipBounds(r);
        }

        public override java.awt.Rectangle getClipBounds()
        {
            checkState();
            return base.getClipBounds();
        }

        [Obsolete]
        public override java.awt.Rectangle getClipRect()
        {
            checkState();
            return base.getClipRect();
        }

        public override java.awt.Color getColor()
        {
            checkState();
            return base.getColor();
        }

        public override java.awt.Font getFont()
        {
            checkState();
            return base.getFont();
        }

        public override java.awt.FontMetrics getFontMetrics(java.awt.Font f)
        {
            checkState();
            return base.getFontMetrics(f);
        }

        public override java.awt.FontMetrics getFontMetrics()
        {
            checkState();
            return base.getFontMetrics();
        }

        public override void setClip(int x, int y, int width, int height)
        {
            checkState();
            base.setClip(x, y, width, height);
        }

        public override void setClip(java.awt.Shape shape)
        {
            checkState();
            base.setClip(shape);
        }

        public override void setColor(java.awt.Color color)
        {
            checkState();
            base.setColor(color);
        }

        public override void setFont(java.awt.Font f)
        {
            checkState();
            base.setFont(f);
        }

        public override void setPaintMode()
        {
            checkState();
            base.setPaintMode();
        }

        public override void setXORMode(java.awt.Color param)
        {
            checkState();
            base.setXORMode(param);
        }

        public override void translate(int x, int y)
        {
            checkState();
            base.translate(x, y);
        }

        public override void draw(java.awt.Shape shape)
        {
            checkState();
            base.draw(shape);
        }

        public override bool drawImage(java.awt.Image img, java.awt.geom.AffineTransform xform, ImageObserver observer)
        {
            checkState();
            return base.drawImage(img, xform, observer);
        }

        public override void drawImage(BufferedImage image, BufferedImageOp op, int x, int y)
        {
            checkState();
            base.drawImage(image, op, x, y);
        }

        public override void drawRenderedImage(RenderedImage img, java.awt.geom.AffineTransform xform)
        {
            checkState();
            base.drawRenderedImage(img, xform);
        }

        public override void drawRenderableImage(java.awt.image.renderable.RenderableImage image, java.awt.geom.AffineTransform xform)
        {
            checkState();
            base.drawRenderableImage(image, xform);
        }

        public override void drawString(string str, int x, int y)
        {
            checkState();
            base.drawString(str, x, y);
        }

        public override void drawString(string text, float x, float y)
        {
            checkState();
            base.drawString(text, x, y);
        }

        public override void drawString(java.text.AttributedCharacterIterator iterator, int x, int y)
        {
            checkState();
            base.drawString(iterator, x, y);
        }

        public override void drawString(java.text.AttributedCharacterIterator iterator, float x, float y)
        {
            checkState();
            base.drawString(iterator, x, y);
        }

        public override void fill(java.awt.Shape shape)
        {
            checkState();
            base.fill(shape);
        }

        public override bool hit(java.awt.Rectangle rect, java.awt.Shape s, bool onStroke)
        {
            checkState();
            return base.hit(rect, s, onStroke);
        }

        public override java.awt.GraphicsConfiguration getDeviceConfiguration()
        {
            // no check here, since invariant
            return base.getDeviceConfiguration();
        }

        public override void setComposite(java.awt.Composite comp)
        {
            checkState();
            base.setComposite(comp);
        }

        public override void setPaint(java.awt.Paint paint)
        {
            checkState();
            base.setPaint(paint);
        }

        public override void setStroke(java.awt.Stroke stroke)
        {
            checkState();
            base.setStroke(stroke);
        }

        public override void setRenderingHint(java.awt.RenderingHints.Key hintKey, object hintValue)
        {
            checkState();
            base.setRenderingHint(hintKey, hintValue);
        }

        public override object getRenderingHint(java.awt.RenderingHints.Key hintKey)
        {
            checkState();
            return base.getRenderingHint(hintKey);
        }

        public override void setRenderingHints(java.util.Map hints)
        {
            checkState();
            base.setRenderingHints(hints);
        }

        public override void addRenderingHints(java.util.Map hints)
        {
            checkState();
            base.addRenderingHints(hints);
        }

        public override java.awt.RenderingHints getRenderingHints()
        {
            checkState();
            return base.getRenderingHints();
        }

        public override void translate(double x, double y)
        {
            checkState();
            base.translate(x, y);
        }

        public override void rotate(double theta)
        {
            checkState();
            base.rotate(theta);
        }

        public override void rotate(double theta, double x, double y)
        {
            checkState();
            base.rotate(theta, x, y);
        }

        public override void scale(double scaleX, double scaleY)
        {
            checkState();
            base.scale(scaleX, scaleY);
        }

        public override void shear(double shearX, double shearY)
        {
            checkState();
            base.shear(shearX, shearY);
        }

        public override void transform(java.awt.geom.AffineTransform tx)
        {
            checkState();
            base.transform(tx);
        }

        public override void setTransform(java.awt.geom.AffineTransform tx)
        {
            checkState();
            base.setTransform(tx);
        }

        public override java.awt.geom.AffineTransform getTransform()
        {
            checkState();
            return base.getTransform();
        }

        public override java.awt.Paint getPaint()
        {
            checkState();
            return base.getPaint();
        }

        public override java.awt.Composite getComposite()
        {
            checkState();
            return base.getComposite();
        }

        public override void setBackground(java.awt.Color color)
        {
            checkState();
            base.setBackground(color);
        }

        public override java.awt.Color getBackground()
        {
            checkState();
            return base.getBackground();
        }

        public override java.awt.Stroke getStroke()
        {
            checkState();
            return base.getStroke();
        }

        public override java.awt.font.FontRenderContext getFontRenderContext()
        {
            checkState();
            return base.getFontRenderContext();
        }

        public override void drawGlyphVector(java.awt.font.GlyphVector gv, float x, float y)
        {
            checkState();
            base.drawGlyphVector(gv, x, y);
        }
    }

}
