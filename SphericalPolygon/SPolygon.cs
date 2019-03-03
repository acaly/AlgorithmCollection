using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SphericalPolygon
{
    struct SPolygonRegion
    {
        public List<Vector3> Vertices;
        public Vector3 RefPoint;
    }

    struct STriangle
    {
        public Vector3 PoleA, PoleB, PoleC;
        public Vector3 PointA, PointB, PointC;
    }
}
