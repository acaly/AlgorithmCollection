using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SphericalPolygon
{
    static class SPolygonCutter
    {
        private class CutInfo
        {
            public Vector3 PointF, PointL;
            public int IndexF, IndexL;
            public float TriangleF, TriangleL;

            public float TriangleLen
            {
                get
                {
                    var ret = TriangleF - TriangleL;
                    if (ret < 0) ret += 3;
                    return ret;
                }
            }
        }

        private struct EdgeInfo
        {
            public int IndexF, IndexL;
        }

        public static List<SPolygonRegion> Cut(SPolygonRegion region, STriangle t)
        {
            int startPoint = -1;
            for (int i = 0; i < region.Vertices.Count; ++i)
            {
                if (TestTriangle(ref t, region.Vertices[i]) < 0)
                {
                    startPoint = i + 1;
                    break;
                }
            }
            if (startPoint < 0)
            {
                //TODO check triangle is not inside the region (we don't support this)
                return new List<SPolygonRegion>();
            }

            int pointPos = -1;
            int pointCompatibleEdge = 0;
            float lastPointTE = -1;
            int edgeStartPos = 0;

            Vector3 cutStartPoint = new Vector3();
            int cutStartIndex = -1;
            float cutStartTE = -1;
            List<CutInfo> cutList = new List<CutInfo>();

            for (int i = 0; i < region.Vertices.Count; ++i)
            {
                int index = i + startPoint;
                if (index >= region.Vertices.Count) index -= region.Vertices.Count;
                int lastIndex = index - 1;
                if (lastIndex == -1) lastIndex = region.Vertices.Count - 1;

                int nextPointPos = TestTriangle(ref t, region.Vertices[index], out var ee, out var nte);

                if (pointPos == 0)
                {
                    //Currently on edge of the triangle
                    if (nextPointPos == 0)
                    {
                        if ((ee & pointCompatibleEdge) != 0)
                        {
                            //On same edge of the triangle. 
                            //Update compatible bits (when moved to another edge).
                            pointCompatibleEdge = ee;
                        }
                        else
                        {
                            //Jump to another edge.
                            //This is considered to be a inward edge.
                            //Note this rely on that the triangle is always smaller than semisphere.

                            if (edgeStartPos > 0)
                            {
                                //Both sides are inside triangle. Ignore this chain.
                                pointCompatibleEdge = ee;
                            }
                            else
                            {
                                //Start a cut. (Do nothing.)
                                edgeStartPos = 1;
                                pointPos = 0;
                                pointCompatibleEdge = ee;
                            }
                        }
                    }
                    else if (nextPointPos > 0)
                    {
                        if (edgeStartPos > 0)
                        {
                            //Ignore
                            pointPos = 1;
                        }
                        else
                        {
                            //Start a cut. (Do nothing.)
                            pointPos = 1;
                        }
                    }
                    else //nextPointPos < 0
                    {
                        if (edgeStartPos > 0)
                        {
                            //Finish a cut
                            var cutEndPoint = region.Vertices[lastIndex];
                            var cutEndTE = lastPointTE;
                            var cutEndIndex = lastIndex;
                            cutList.Add(new CutInfo
                            {
                                PointF = cutStartPoint,
                                IndexF = cutStartIndex,
                                PointL = cutEndPoint,
                                IndexL = cutEndIndex,
                                TriangleF = cutStartTE,
                                TriangleL = cutEndTE,
                            });
                            pointPos = -1;
                        }
                        else
                        {
                            //Ignore
                            pointPos = -1;
                        }
                    }
                }
                else if (pointPos < 0)
                {
                    if (nextPointPos < 0)
                    {
                        //Out-out. Possibly go in and out within one edge
                        var c = CheckSingleEdgeInOut(ref t, region.Vertices[lastIndex],
                            region.Vertices[index], index, lastIndex);
                        if (c != null)
                        {
                            cutList.Add(c);
                        }
                    }
                    else if (nextPointPos > 0)
                    {
                        //Start a cut.
                        cutStartPoint = IntersectTriangle(ref t, region.Vertices[index],
                            region.Vertices[lastIndex], out cutStartTE);
                        cutStartIndex = index;
                        pointPos = 1;
                    }
                    else
                    {
                        cutStartPoint = region.Vertices[index];
                        cutStartTE = nte;
                        cutStartIndex = index;
                        pointPos = 0;
                        pointCompatibleEdge = ee;
                        edgeStartPos = -1;
                    }
                }
                else //pointPos > 0
                {
                    if (nextPointPos > 0)
                    {
                        //Do nothing
                    }
                    else if (nextPointPos < 0)
                    {
                        var cutEndPoint = IntersectTriangle(ref t, region.Vertices[lastIndex],
                            region.Vertices[index], out var cutEndTE);
                        var cutEndIndex = lastIndex;
                        cutList.Add(new CutInfo
                        {
                            PointF = cutStartPoint,
                            IndexF = cutStartIndex,
                            PointL = cutEndPoint,
                            IndexL = cutEndIndex,
                            TriangleF = cutStartTE,
                            TriangleL = cutEndTE,
                        });
                        pointPos = -1;
                    }
                    else
                    {
                        //Don't calculate start point (delay)
                        pointPos = 0;
                        pointCompatibleEdge = ee;
                        edgeStartPos = 1;
                    }
                }
                lastPointTE = nte;
            }

            if (cutList.Count == 0)
            {
                return new List<SPolygonRegion> { region };
            }
            else
            {
                List<EdgeInfo> edges = new List<EdgeInfo>();
                edges.Add(new EdgeInfo
                {
                    IndexF = cutList[cutList.Count - 1].IndexL,
                    IndexL = cutList[0].IndexF,
                });
                if (cutList.Count > 1)
                {
                    for (int i = 1; i < cutList.Count; ++i)
                    {
                        edges.Add(new EdgeInfo
                        {
                            IndexF = cutList[i - 1].IndexL,
                            IndexL = cutList[i].IndexF,
                        });
                    }
                    cutList.Sort((a, b) => Math.Sign(a.TriangleLen - b.TriangleLen));
                    var cutCount = cutList.Count;
                    while (cutList[cutCount - 2].TriangleLen + cutList[cutCount - 1].TriangleLen > 3)
                    {
                        //exchange L
                        var point = cutList[cutCount - 1].PointL;
                        cutList[cutCount - 1].PointL = cutList[cutCount - 2].PointL;
                        cutList[cutCount - 2].PointL = point;

                        var index = cutList[cutCount - 1].IndexL;
                        cutList[cutCount - 1].IndexL = cutList[cutCount - 2].IndexL;
                        cutList[cutCount - 2].IndexL = index;

                        var triangle = cutList[cutCount - 1].TriangleL;
                        cutList[cutCount - 1].TriangleL = cutList[cutCount - 2].TriangleL;
                        cutList[cutCount - 2].TriangleL = triangle;

                        cutList.Sort((a, b) => Math.Sign(a.TriangleLen - b.TriangleLen));
                    }
                }

                var result = new List<SPolygonRegion>();
                while (edges.Count > 0)
                {
                    var newRegion = new SPolygonRegion
                    {
                        RefPoint = region.RefPoint,
                        Vertices = new List<Vector3>(),
                    };
                    var edgeStart = edges[edges.Count - 1].IndexF;
                    var closeIndex = edges[edges.Count - 1].IndexF;
                    while (true)
                    {
                        var edgeIndex = edges.FindIndex(ee => ee.IndexF == edgeStart);
                        if (edgeIndex == -1) throw new Exception("Internal exception");
                        var e = edges[edgeIndex];
                        edges.RemoveAt(edgeIndex);
                        //TODO optimize?
                        int j = (e.IndexF + 1) % region.Vertices.Count;
                        do
                        {
                            newRegion.Vertices.Add(region.Vertices[j]);
                            j = (j + 1) % region.Vertices.Count;
                        } while (j != (e.IndexL + region.Vertices.Count) % region.Vertices.Count);

                        var cutIndex = cutList.FindIndex(cc => cc.IndexF == e.IndexL);
                        if (cutIndex == -1) throw new Exception("Internal exception");
                        var cut = cutList[cutIndex];
                        newRegion.Vertices.Add(cut.PointF);
                        FillTrianglePoints(ref t, cut.TriangleL, cut.TriangleF, newRegion.Vertices);
                        newRegion.Vertices.Add(cut.PointL);
                        edgeStart = cut.IndexL;
                        if (edgeStart == closeIndex) break;
                    }
                    result.Add(newRegion);
                }
                return result;
            }
        }

        private static bool IsZero(this float val)
        {
            return val > -1E-7 && val < 1E-7;
        }

        private static bool IsNegative(this float val)
        {
            return val <= -1E-7;
        }

        private static int TestTriangle(ref STriangle t, Vector3 p)
        {
            var m0 = Vector3.Dot(t.PoleA, p);
            var m1 = Vector3.Dot(t.PoleB, p);
            var m2 = Vector3.Dot(t.PoleC, p);
            if (m0.IsNegative() || m1.IsNegative() || m2.IsNegative()) return -1;
            if (m0.IsZero() || m1.IsZero() || m2.IsZero()) return 0;
            return 1;
        }

        private static int TestTriangle(ref STriangle t, Vector3 p, out int edge, out float triangleEdgePos)
        {
            edge = 0;
            triangleEdgePos = 0;
            var m0 = Vector3.Dot(t.PoleA, p);
            var m1 = Vector3.Dot(t.PoleB, p);
            var m2 = Vector3.Dot(t.PoleC, p);
            if (m0.IsNegative() || m1.IsNegative() || m2.IsNegative()) return -1;
            if (m0.IsZero() || m1.IsZero() || m2.IsZero())
            {
                if (m0.IsZero()) edge += 1;
                if (m1.IsZero()) edge += 2;
                if (m2.IsZero()) edge += 4;
                switch (edge)
                {
                    case 5: triangleEdgePos = 0; break;
                    case 3: triangleEdgePos = 1; break;
                    case 6: triangleEdgePos = 2; break;
                    case 1: triangleEdgePos = SLerpT(t.PointA, t.PointB, p); break;
                    case 2: triangleEdgePos = SLerpT(t.PointB, t.PointC, p); break;
                    case 4: triangleEdgePos = SLerpT(t.PointC, t.PointA, p); break;
                    default: throw new Exception("Internal exception"); //should never get here
                }
                return 0;
            }
            return 1;
        }

        private static Vector3 IntersectTriangle(ref STriangle t, Vector3 pIn, Vector3 pOut, out float triangleEdgePos)
        {
            var linePole = Vector3.Normalize(Vector3.Cross(pOut, pIn)); //TODO no need to normalize?
            var xa = Vector3.Cross(t.PoleA, linePole);
            if (!Vector3.Dot(xa, t.PoleB).IsNegative() && !Vector3.Dot(xa, t.PoleC).IsNegative())
            {
                xa = Vector3.Normalize(xa);
                triangleEdgePos = SLerpT(t.PointB, t.PointC, xa) + 1;
                return xa;
            }
            var xb = Vector3.Cross(t.PoleB, linePole);
            if (!Vector3.Dot(xb, t.PoleC).IsNegative() && !Vector3.Dot(xb, t.PoleA).IsNegative())
            {
                xb = Vector3.Normalize(xb);
                triangleEdgePos = SLerpT(t.PointC, t.PointA, xb) + 2;
                return xb;
            }
            var xc = Vector3.Cross(t.PoleC, linePole);
            if (!Vector3.Dot(xa, t.PoleA).IsNegative() && !Vector3.Dot(xa, t.PoleB).IsNegative())
            {
                xc = Vector3.Normalize(xc);
                triangleEdgePos = SLerpT(t.PointA, t.PointB, xc) + 0;
                return xc;
            }
            throw new Exception("Internal exception"); //should never get here
        }

        private static float SLerpT(Vector3 v1, Vector3 v2, Vector3 vt)
        {
            var ret = (float)(Math.Acos(Vector3.Dot(v1, vt)) / Math.Acos(Vector3.Dot(v1, v2)));
            if (Vector3.Dot(vt - v1, v2 - v1) < 0) ret = -ret;
            return ret;
        }

        private static Vector3 SLerp(Vector3 v1, Vector3 v2, float t)
        {
            double th = Math.Acos(Vector3.Dot(v1, v2));
            var sin = Math.Sin(th);
            var k1 = Math.Sin((1 - t) * th) / sin;
            var k2 = Math.Sin(t * th) / sin;
            return v1 * (float)k1 + v2 * (float)k2;
        }

        private static void FillTrianglePoints(ref STriangle t, float f, float l, List<Vector3> targetList)
        {
            if (l < f) l += 3;
            var i = (int)Math.Floor(f) + 1;
            while (l > i)
            {
                AddTrianglePoint(ref t, i, targetList);
                i += 1;
            }
        }

        private static void AddTrianglePoint(ref STriangle t, int index, List<Vector3> targetList)
        {
            index = index % 3;
            Vector3 p;
            switch (index)
            {
                case 0:
                    p = t.PointA;
                    break;
                case 1:
                    p = t.PointB;
                    break;
                case 2:
                    p = t.PointC;
                    break;
                default:
                    throw new Exception("Internal exception");
            }
            targetList.Add(p);
        }

        private static CutInfo CheckSingleEdgeInOut(ref STriangle t, Vector3 p1, Vector3 p2, int index, int lastIndex)
        {
            //Check how many triangle points are on p1-p2
            var pole = Vector3.Normalize(Vector3.Cross(p1, p2)); //TODO no need to normalize?
            var x1 = IsOnSegment(t.PointA, p1, p2, pole) ? 1 : 0;
            var x2 = IsOnSegment(t.PointB, p1, p2, pole) ? 1 : 0;
            var x3 = IsOnSegment(t.PointC, p1, p2, pole) ? 1 : 0;
            if (x1 + x2 + x3 == 2) return null;
            CutInfo ret = null;
            if (x1 + x2 + x3 == 1)
            {
                if (x1 == 1)
                {
                    var x = IntersectEdge(t.PoleA, t.PoleB, t.PoleC, p1, p2, pole);
                    if (!x.HasValue) return null;
                    ret = new CutInfo
                    {
                        PointF = t.PointA,
                        TriangleF = 0,
                        PointL = x.Value,
                        TriangleL = SLerpT(t.PointB, t.PointC, x.Value),
                    };
                }
                else if (x2 == 1)
                {
                    var x = IntersectEdge(t.PoleB, t.PoleC, t.PoleA, p1, p2, pole);
                    if (!x.HasValue) return null;
                    ret = new CutInfo
                    {
                        PointF = t.PointB,
                        TriangleF = 1,
                        PointL = x.Value,
                        TriangleL = SLerpT(t.PointC, t.PointA, x.Value),
                    };
                }
                else if (x2 == 2)
                {
                    var x = IntersectEdge(t.PoleC, t.PoleA, t.PoleB, p1, p2, pole);
                    if (!x.HasValue) return null;
                    ret = new CutInfo
                    {
                        PointF = t.PointC,
                        TriangleF = 2,
                        PointL = x.Value,
                        TriangleL = SLerpT(t.PointA, t.PointB, x.Value),
                    };
                }
            }
            else
            {
                var xm1 = IntersectEdge(t.PoleA, t.PoleB, t.PoleC, p1, p2, pole);
                var xm2 = IntersectEdge(t.PoleB, t.PoleC, t.PoleA, p1, p2, pole);
                var xm3 = IntersectEdge(t.PoleC, t.PoleA, t.PoleB, p1, p2, pole);
                if (!xm1.HasValue && !xm2.HasValue && !xm3.HasValue) return null;
                if (!xm1.HasValue)
                {
                    ret = new CutInfo
                    {
                        PointF = xm2.Value,
                        TriangleF = SLerpT(t.PointC, t.PointA, xm2.Value) + 2,
                        PointL = xm3.Value,
                        TriangleL = SLerpT(t.PointA, t.PointB, xm3.Value) + 0,
                    };
                }
                else if (!xm2.HasValue)
                {
                    ret = new CutInfo
                    {
                        PointF = xm1.Value,
                        TriangleF = SLerpT(t.PointB, t.PointC, xm1.Value) + 1,
                        PointL = xm3.Value,
                        TriangleL = SLerpT(t.PointA, t.PointB, xm3.Value) + 0,
                    };
                }
                else if (!xm3.HasValue)
                {
                    ret = new CutInfo
                    {
                        PointF = xm2.Value,
                        TriangleF = SLerpT(t.PointC, t.PointA, xm2.Value) + 2,
                        PointL = xm1.Value,
                        TriangleL = SLerpT(t.PointB, t.PointC, xm1.Value) + 1,
                    };
                }
                else
                {
                    throw new Exception("Internal exception");
                }
            }
            var tf = SLerpT(p1, p2, ret.PointF);
            var tl = SLerpT(p1, p2, ret.PointL);
            if (tf > tl)
            {
                var point = ret.PointF;
                ret.PointF = ret.PointL;
                ret.PointL = point;
                var triangle = ret.TriangleF;
                ret.TriangleF = ret.TriangleL;
                ret.TriangleL = triangle;
            }
            ret.IndexF = index;
            ret.IndexL = lastIndex;
            return ret;
        }

        private static bool IsOnSegment(Vector3 p, Vector3 p1, Vector3 p2, Vector3 pole)
        {
            if (!Vector3.Dot(p, pole).IsZero())
            {
                return false;
            }
            var t = SLerpT(p1, p2, p);
            return t >= 0 && t <= 1; //Should not be 0 or 1 (otherwise should not get here)
        }

        //TODO merge p1, p2, pole (ref of struct)
        private static Vector3? IntersectEdge(Vector3 trpole, Vector3 trpole1, Vector3 trpole2, Vector3 p1, Vector3 p2, Vector3 pole)
        {
            var x = Vector3.Cross(trpole, pole);
            if (Vector3.Dot(x, trpole1).IsNegative() || Vector3.Dot(x, trpole2).IsNegative())
            {
                if (Vector3.Dot(x, trpole1).IsNegative() && Vector3.Dot(x, trpole2).IsNegative())
                {
                    x = -x;
                }
                else
                {
                    return null;
                }
            }
            x = Vector3.Normalize(x);
            var t = SLerpT(p1, p2, x);
            if (t >= 0 && t <= 1)
            {
                return x;
            }
            return null;
        }
    }
}
