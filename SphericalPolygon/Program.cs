using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SphericalPolygon
{
    class Program
    {
        static void Main(string[] args)
        {
            var polygon = SPolygonGenerator.GeneratePolygon(new Vector2[]
            {
                new Vector2(0.1f, 0.2f),
                new Vector2(-0.3f, 0.2f),
                new Vector2(-0.2f, -0.2f),
                new Vector2(-0.1f, 0.1f),
                new Vector2(-0.1f, -0.2f),
                new Vector2(0.1f, -0.2f),
            });
            var triangle = SPolygonGenerator.GenerateTriangle(
                new Vector2(-0.2f, 0),
                new Vector2(0.2f, -0.1f),
                new Vector2(0.2f, 0.1f)
            );
            var result = SPolygonCutter.Cut(polygon, triangle);
        }
    }
}
