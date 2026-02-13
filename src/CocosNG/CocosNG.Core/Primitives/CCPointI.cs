using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CocosNG.Core;

namespace CocosNG.Core.Primitives
{
    public struct CCPointI
    {
        public int X;
        public int Y;

        public CCPointI(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int Distance(ref CCPointI p)
        {
            var hside = X - p.X;
            var vside = Y - p.Y;

            return (int)Math.Sqrt(hside * hside + vside * vside);
        }

        public int DistanceSQ(ref CCPointI p)
        {
            var hside = X - p.X;
            var vside = Y - p.Y;

            return hside * hside + vside * vside;
        }

        public bool Equals(ref CCPointI p)
        {
            return X == p.X && Y == p.Y;
        }

        public static implicit operator CCPoint(CCPointI p)
        {
            return new CCPoint(p.X, p.Y);
        }

        #region Operator Overloads

        public static bool operator ==(CCPointI p1, CCPointI p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        public static bool operator !=(CCPointI p1, CCPointI p2)
        {
            return p1.X != p2.X || p1.Y != p2.Y;
        }

        public static CCPointI operator -(CCPointI p1, CCPointI p2)
        {
            CCPointI pt;
            pt.X = p1.X - p2.X;
            pt.Y = p1.Y - p2.Y;
            return pt;
        }

        public static CCPointI operator -(CCPointI p1)
        {
            CCPointI pt;
            pt.X = -p1.X;
            pt.Y = -p1.Y;
            return pt;
        }

        public static CCPointI operator +(CCPointI p1, CCPointI p2)
        {
            CCPointI pt;
            pt.X = p1.X + p2.X;
            pt.Y = p1.Y + p2.Y;
            return pt;
        }

        public static CCPointI operator +(CCPointI p1)
        {
            CCPointI pt;
            pt.X = +p1.X;
            pt.Y = +p1.Y;
            return pt;
        }

        #endregion
    }
}
