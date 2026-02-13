using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CocosNG.Core;
using CocosNG.Core.Primitives;

namespace CocosNG.Core.Input
{
    public class CCIMEKeyboardNotificationInfo
    {
        public CCRect begin;              // the soft keyboard rectangle when animatin begin
        public CCRect end;                // the soft keyboard rectangle when animatin end
        public float duration;           // the soft keyboard animation duration
    }
}
