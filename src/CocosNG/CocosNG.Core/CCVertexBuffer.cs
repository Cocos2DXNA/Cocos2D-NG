using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;

namespace CocosNG.Core
{
    public class CCVertexBuffer<T> : CCGraphicsResource where T : struct, IVertexType
    {
        protected VertexBuffer _vertexBuffer;
        protected BufferUsage _usage;
        protected CCRawList<T> _data;

        internal VertexBuffer VertexBuffer
        {
            get { return _vertexBuffer; }
        }

        public CCRawList<T> Data
        {
            get { return _data; }
        }

        public int Count
        {
            get { return _data.Count; }
            set
            {
                Debug.Assert(value <= _data.Capacity);
                _data.count = value;
            }
        }

        public int Capacity
        {
            get { return _data.Capacity; }
            set
            {
                if (_data.Capacity != value)
                {
                    _data.Capacity = value;
                    Reinit();
                }
            }
        }

        public CCVertexBuffer(int vertexCount, BufferUsage usage)
        {
            _data = new CCRawList<T>(vertexCount);
            _usage = usage;
            Reinit();
        }
        
        public void UpdateBuffer()
        {
            UpdateBuffer(0, _data.Count);
        }

        public virtual void UpdateBuffer(int startIndex, int elementCount)
        {
            if (elementCount > 0)
            {
                _vertexBuffer.SetData(_data.Elements, startIndex, elementCount);
            }
        }

        public override void Reinit()
        {
            if (_vertexBuffer != null && !_vertexBuffer.IsDisposed)
            {
                _vertexBuffer.Dispose();
            }
            _vertexBuffer = new VertexBuffer(CCDrawManager.GraphicsDevice, typeof(T), _data.Capacity, _usage);
        }
    }
}
