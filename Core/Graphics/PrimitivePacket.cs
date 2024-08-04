using System.Collections.Generic;

namespace PathOfTerraria.Core.Graphics;

public class PrimitivePacket : IDisposable{
	public readonly PrimitiveType Type;

	internal List<VertexPositionColor> Draws;

	/// <summary>
	/// Creates a new <seealso cref="PrimitivePacket"/> that <seealso cref="PrimitiveDrawing"/> will process
	/// </summary>
	/// <param name="type">The type of primitives this packet can cache</param>
	public PrimitivePacket(PrimitiveType type){
		Type = type;

		Draws = [];
	}

	/// <summary>
	/// Adds a new primitive to the draw list
	/// </summary>
	/// <param name="additions">The sequence of <seealso cref="VertexPositionColor"/> data used for drawing a primitive</param>
	public void AddDraw(params VertexPositionColor[] additions){
		void CheckError(int expected){
			if(additions.Length != expected)
			{
				throw new ArgumentException($"Primitive drawing package ({Type}) received an invalid amount of draw data to cache. Expected: {expected}, Received: {additions.Length}");
			}
		}

		switch(Type){
			case PrimitiveType.LineList:
				//LineList expects there to be two entries per line: the start and end points
				/*
				 *     1--------------2
				 *
				 *
				 *                         3----------------4
				 */
				CheckError(2);
				break;
			case PrimitiveType.LineStrip:
				//LineStrip expects there to be 2 entries for the initial line then 1 entry for each successive line
				//  Each line is connected based on its order in the list
				/*
				 *     1--------------2
				 *                     \
				 *                      \
				 *                       \
				 *                        \
				 *                         3----------------4
				 */
				CheckError(Draws.Count == 0 ? 2 : 1);
				break;
			case PrimitiveType.TriangleList:
				//TriangleList expects there to be 3 entries per triangle: the three corners of the triangle drawn
				/*
				 *     1-----------2
				 *      \         /
				 *       \       /   4
				 *        \     /   / \
				 *         \   /   /   \
				 *          \ /   /     \
				 *           3   /       \
				 *              /         \
				 *             5-----------6
				 */
				CheckError(3);
				break;
			case PrimitiveType.TriangleStrip:
				//TriangleStrip expects there to be 3 entries for the initial triangle, then 2 entries for each successive triangle
				//  Each triangle is connected based on its order in the list
				/*
				 *     1-----------3-------------4
				 *      \         / \           /
				 *       \       /   \         /
				 *        \     /     \       /
				 *         \   /       \     /
				 *          \ /         \   /
				 *           2           \ /
				 *                        5
				 */
				CheckError(Draws.Count == 0 ? 3 : 2);
				break;
		}

		foreach (VertexPositionColor t in additions)
		{
			Draws.Add(t);
		}
	}

	public int GetPrimitivesCount()
	{
		return Type switch
		{
			PrimitiveType.LineList => Draws.Count / 2,
			PrimitiveType.LineStrip => Draws.Count - 1,
			PrimitiveType.TriangleList => Draws.Count / 3,
			PrimitiveType.TriangleStrip => Draws.Count - 2,
			_ => 0
		};
	}

	private bool _disposed;

	~PrimitivePacket()
	{
		Dispose(false);
	}

	public void Dispose(){
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing){
		if(!_disposed){
			_disposed = true;

			if(disposing){
				Draws.Clear();
				Draws = null;
			}
		}
	}
}