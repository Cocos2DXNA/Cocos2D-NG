using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CocosNG.Core;

namespace CocosNG.Core.Gestures
{
    public interface IGestureRecognizer
    {
        /// <summary>
        /// The node this recognizer is attached to.
        /// </summary>
        CCNode Target { get; }

        /// <summary>
        /// Attach this recognizer to a node and subscribe to its touch events.
        /// </summary>
        void Attach(CCNode node);

        /// <summary>
        /// Detach this recognizer from its node and clean up subscriptions.
        /// </summary>
        void Detach();
    }
}
