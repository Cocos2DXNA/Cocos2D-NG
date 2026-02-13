using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;

namespace CocosNG.Core
{
    public class CCIndexBuffer<T> : CCGraphicsResource where T : struct
    {
        private IndexBuffer _indexBuffer;
        private BufferUsage _usage;
        private CCRawList<T> _data;

        internal IndexBuffer IndexBuffer
        {
            get { return _indexBuffer; }
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

        public CCIndexBuffer(int indexCount, BufferUsage usage)
        {
            _data = new CCRawList<T>(indexCount);
            _usage = usage;
            Reinit();
        }

        public override void Reinit()
        {
            if (_indexBuffer != null && !_indexBuffer.IsDisposed)
            {
                _indexBuffer.Dispose();
            }

            _indexBuffer = new IndexBuffer(CCDrawManager.GraphicsDevice, typeof(T), _data.Capacity, _usage);

            UpdateBuffer();
        }

        public void UpdateBuffer()
        {
            UpdateBuffer(0, _data.Count);
        }
#if PSM
		private ushort[] _PSMTmpBuffer;
#endif
		
        public void UpdateBuffer(int startIndex, int elementCount)
        {
            if (elementCount > 0)
            {
#if PSM
				// HACK! PSM vertexbuffer only allows for ushort so we have to convert to ushort here.
				if(_PSMTmpBuffer == null || _PSMTmpBuffer.Length < elementCount) {
					_PSMTmpBuffer = new ushort[elementCount];
				}
				for(int i=0; i < elementCount; i++) {
					_PSMTmpBuffer[i] = (ushort)Convert.ChangeType(_data.Elements[startIndex+i], TypeCode.UInt16);
				}
                _indexBuffer.SetData(_PSMTmpBuffer, 0, elementCount);
#else
                _indexBuffer.SetData(_data.Elements, startIndex, elementCount);
#endif
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_indexBuffer != null && !_indexBuffer.IsDisposed)
            {
                _indexBuffer.Dispose();
            }

            _indexBuffer = null;
        }
    }
}
