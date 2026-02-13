using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;

namespace CocosNG.Core
{
    public class CCGraphicsResource : IDisposable
    {
        private static CCRawList<WeakReference> _createdResources = new CCRawList<WeakReference>();

        private bool _isDisposed;

        private WeakReference _wr;

        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public CCGraphicsResource()
        {
            _wr = new WeakReference(this);

            lock (_createdResources)
            {
                _createdResources.Add(_wr);
            }
        }

        ~CCGraphicsResource()
        {
            if (!IsDisposed)
            {
                Dispose();
            }

            lock (_createdResources)
            {
                _createdResources.Remove(_wr);
            }
        }

        public virtual void Dispose()
        {
            _isDisposed = true;
        }

        public virtual void Reinit()
        {
        }

        internal static void ReinitAllResources()
        {
            lock (_createdResources)
            {
                var resources = _createdResources.Elements;
                for (int i = 0, count = _createdResources.Count; i < count; i++)
                {
                    if (resources[i].IsAlive)
                    {
                        ((CCGraphicsResource) resources[i].Target).Reinit();
                    }
                }
            }
        }

        internal static void DisposeAllResources()
        {
            lock (_createdResources)
            {
                var resources = _createdResources.Elements;
                for (int i = 0, count = _createdResources.Count; i < count; i++)
                {
                    if (resources[i].IsAlive)
                    {
                        ((CCGraphicsResource) resources[i].Target).Dispose();
                    }
                }
            }
        }
    }
}
