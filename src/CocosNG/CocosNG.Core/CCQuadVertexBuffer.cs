using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;

namespace CocosNG.Core
{
    public class CCQuadVertexBuffer : CCVertexBuffer<CCV3F_C4B_T2F_Quad>
    {
        public CCQuadVertexBuffer(int vertexCount, BufferUsage usage) : base(vertexCount, usage)
        {
        }

        public void UpdateBuffer(CCRawList<CCV3F_C4B_T2F_Quad> data, int startIndex, int elementCount)
        {
            //TODO: 
            var tmp = _data;
            _data = data;
            
            UpdateBuffer(startIndex, elementCount);

            _data = tmp;
        }

        public override void UpdateBuffer(int startIndex, int elementCount)
        {
            if (elementCount == 0)
            {
                return;
            }

            var quads = _data.Elements;

            var tmp = CCDrawManager._tmpVertices;

            while (tmp.Capacity < elementCount)
            {
                tmp.Capacity = tmp.Capacity * 2;
            }
            tmp.Count = elementCount * 4;

            var vertices = tmp.Elements;

            int i4 = 0;
            for (int i = startIndex; i < startIndex + elementCount; i++)
            {
                vertices[i4 + 0] = quads[i].TopLeft;
                vertices[i4 + 1] = quads[i].BottomLeft;
                vertices[i4 + 2] = quads[i].TopRight;
                vertices[i4 + 3] = quads[i].BottomRight;

                i4 += 4;
            }

            _vertexBuffer.SetData(vertices, startIndex * 4, elementCount * 4);
        }

        public override void Reinit()
        {
            if (_vertexBuffer != null && !_vertexBuffer.IsDisposed)
            {
                _vertexBuffer.Dispose();
            }
            _vertexBuffer = new VertexBuffer(CCDrawManager.GraphicsDevice, typeof(CCV3F_C4B_T2F), _data.Capacity * 4, _usage);

            UpdateBuffer();
        }

        public override void Dispose()
        {
            base.Dispose();
            
            if (_vertexBuffer != null && !_vertexBuffer.IsDisposed)
            {
                _vertexBuffer.Dispose();
            }
            
            _vertexBuffer = null;
        }
    }
}
