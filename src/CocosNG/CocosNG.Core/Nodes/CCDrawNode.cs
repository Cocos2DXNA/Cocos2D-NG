using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;
using CocosNG.Core.Primitives;

namespace CocosNG.Core.Nodes
{
    public class CCDrawNode : CCNode
    {
        private CCRawList<VertexPositionColor> m_pVertices;
        private CCBlendFunc m_sBlendFunc;
        private bool m_bDirty;

        public CCDrawNode()
        {
            Init();
        }

        public CCBlendFunc BlendFunc
        {
            get { return m_sBlendFunc; }
            set { m_sBlendFunc = value; }
        }

        public CCDrawNode WithBlendFunc(CCBlendFunc blendFunc)
        {
            BlendFunc = blendFunc;
            return this;
        }

        public override bool Init()
        {
            base.Init();

            m_sBlendFunc = CCBlendFunc.AlphaBlend;
            m_pVertices = new CCRawList<VertexPositionColor>(512);

            return true;
        }

        /** draw a dot at a position, with a given radius and color */

        public void DrawDot(CCPoint pos, float radius, CCColor4F color)
        {
            var cl = new Color(color.R, color.G, color.B, color.A);

            var a = new VertexPositionColor(new Vector3(pos.X - radius, pos.Y - radius, 0), cl); //{-1.0, -1.0}
            var b = new VertexPositionColor(new Vector3(pos.X - radius, pos.Y + radius, 0), cl); //{-1.0,  1.0}
            var c = new VertexPositionColor(new Vector3(pos.X + radius, pos.Y + radius, 0), cl); //{ 1.0,  1.0}
            var d = new VertexPositionColor(new Vector3(pos.X + radius, pos.Y - radius, 0), cl); //{ 1.0, -1.0}

            m_pVertices.Add(a);
            m_pVertices.Add(b);
            m_pVertices.Add(c);

            m_pVertices.Add(a);
            m_pVertices.Add(c);
            m_pVertices.Add(d);

            m_bDirty = true;
        }

        public CCDrawNode DrawDotAnd(CCPoint pos, float radius, CCColor4F color)
        {
            DrawDot(pos, radius, color);
            return this;
        }
        
        /// <summary>
        /// Creates 18 vertices that create a segment between the two points with the given radius of rounding
        /// on the segment end. The color is used to draw the segment.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <returns>The starting vertex index of the segment.</returns>
        public virtual int DrawSegment(CCPoint from, CCPoint to, float radius, CCColor4F color)
        {
            var cl = new Color(color.R, color.G, color.B, color.A);

            var a = from;
            var b = to;

            var n = CCPoint.Normalize(CCPoint.Perp(a - b));
            var t = CCPoint.Perp(n);

            var nw = n * radius;
            var tw = t * radius;
            var v0 = b - (nw + tw);
            var v1 = b + (nw - tw);
            var v2 = b - nw;
            var v3 = b + nw;
            var v4 = a - nw;
            var v5 = a + nw;
            var v6 = a - (nw - tw);
            var v7 = a + (nw + tw);

            int returnIndex = m_pVertices.Count;
            m_pVertices.Add(new VertexPositionColor(v0, cl)); //__t(v2fneg(v2fadd(n, t)))
            m_pVertices.Add(new VertexPositionColor(v1, cl)); //__t(v2fsub(n, t))
            m_pVertices.Add(new VertexPositionColor(v2, cl)); //__t(v2fneg(n))}

            m_pVertices.Add(new VertexPositionColor(v3, cl)); //__t(n)
            m_pVertices.Add(new VertexPositionColor(v1, cl)); //__t(v2fsub(n, t))
            m_pVertices.Add(new VertexPositionColor(v2, cl)); //__t(v2fneg(n))

            m_pVertices.Add(new VertexPositionColor(v3, cl)); //__t(n)
            m_pVertices.Add(new VertexPositionColor(v4, cl)); //__t(v2fneg(n))
            m_pVertices.Add(new VertexPositionColor(v2, cl)); //__t(v2fneg(n))

            m_pVertices.Add(new VertexPositionColor(v3, cl)); //__t(n)
            m_pVertices.Add(new VertexPositionColor(v4, cl)); //__t(v2fneg(n))
            m_pVertices.Add(new VertexPositionColor(v5, cl)); //__t(n)

            m_pVertices.Add(new VertexPositionColor(v6, cl)); //__t(v2fsub(t, n))
            m_pVertices.Add(new VertexPositionColor(v4, cl)); //__t(v2fneg(n))
            m_pVertices.Add(new VertexPositionColor(v5, cl)); //__t(n)

            m_pVertices.Add(new VertexPositionColor(v6, cl)); //__t(v2fsub(t, n))
            m_pVertices.Add(new VertexPositionColor(v7, cl)); //__t(v2fadd(n, t))
            m_pVertices.Add(new VertexPositionColor(v5, cl)); //__t(n)

            m_bDirty = true;
            return (returnIndex);
        }

        public CCDrawNode DrawSegmentAnd(CCPoint from, CCPoint to, float radius, CCColor4F color)
        {
            DrawSegment(from, to, radius, color);
            return this;
        }
    }
}
