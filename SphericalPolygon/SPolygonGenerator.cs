using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SphericalPolygon
{
    static class SPolygonGenerator
    {
        private const float ScaleXY = 0.5f;

        public static SPolygonRegion GeneratePolygon(params Vector2[] points)
        {
            var ret = new SPolygonRegion();
            ret.Segments = points.Select(Transform).ToList();
            ret.RefPoint = new Vector3(0, 0, -1);
            return ret;
        }

        private static Vector3 Transform(Vector2 p)
        {
            if (p.X * p.X > 1 || p.Y * p.Y > 1) throw new Exception("Invalid point");
            var x = p.X * ScaleXY;
            var y = p.Y * ScaleXY;
            var z = (float)Math.Sqrt(1 - x * x - y * y);
            return new Vector3(x, y, z);
        }

        public static STriangle GenerateTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            var pp1 = Transform(p1);
            var pp2 = Transform(p2);
            var pp3 = Transform(p3);
            if (Vector3.Dot(Vector3.Cross(pp1, pp2), pp3) < 0)
            {
                var ppx = pp2;
                pp2 = pp3;
                pp3 = ppx;
            }
            return new STriangle
            {
                PointA = pp1,
                PointB = pp2,
                PointC = pp3,
                PoleA = Vector3.Normalize(Vector3.Cross(pp2, pp3)),
                PoleB = Vector3.Normalize(Vector3.Cross(pp3, pp1)),
                PoleC = Vector3.Normalize(Vector3.Cross(pp1, pp2)),
            };
        }
    }
}
