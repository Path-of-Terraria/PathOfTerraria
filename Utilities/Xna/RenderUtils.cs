using System;

namespace PathOfTerraria.Utilities.Xna;

internal static class RenderUtils
{
	public static short[] QuadTriangles { get; } = [0, 2, 3, 0, 1, 2];

    public static void DrawUserQuad<T>(GraphicsDevice gfx, T[] vertices, int numVertices) where T : struct, IVertexType
    {
    	gfx.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, numVertices, QuadTriangles, 0, QuadTriangles.Length / 3);
    }
}
