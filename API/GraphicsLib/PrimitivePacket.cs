using System.Collections.Generic;

namespace PathOfTerraria.API.GraphicsLib;

public class PrimitivePacket : IDisposable{
	public PrimitiveType type;

	internal List<VertexPositionColor> draws;

	/// <summary>
	/// Creates a new <seealso cref="PrimitivePacket"/> that <seealso cref="PrimitiveDrawing"/> will process
	/// </summary>
	/// <param name="type">The type of primitives this packet can cache</param>
	/// <param name="drawDepth">The desired draw depth for all primitives submitted to this packet</param>
	public PrimitivePacket(PrimitiveType type){
		this.type = type;

		draws = new List<VertexPositionColor>();
	}

	/// <summary>
	/// Adds a new primitive to the draw list
	/// </summary>
	/// <param name="additions">The sequence of <seealso cref="VertexPositionColor"/> data used for drawing a primitive</param>
	public void AddDraw(params VertexPositionColor[] additions){
		void CheckError(int expected){
			if(additions.Length != expected)
			{
				throw new ArgumentException($"Primitive drawing package ({type}) received an invalid amount of draw data to cache. Expected: {expected}, Received: {additions.Length}");
			}
		}

		switch(type){
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
				CheckError(draws.Count == 0 ? 2 : 1);
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
				CheckError(draws.Count == 0 ? 3 : 2);
				break;
		}

		for(int p = 0; p < additions.Length; p++){
			var data = additions[p];
			data.Position.Z = 0;

			draws.Add(additions[p]);
		}
	}

	public int GetPrimitivesCount(){
		switch(type){
			case PrimitiveType.LineList:
				return draws.Count / 2;
			case PrimitiveType.LineStrip:
				return draws.Count - 1;
			case PrimitiveType.TriangleList:
				return draws.Count / 3;
			case PrimitiveType.TriangleStrip:
				return draws.Count - 2;
			default:
				return 0;
		}
	}

	private bool disposed;

	~PrimitivePacket() => Dispose(false);

	public void Dispose(){
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing){
		if(!disposed){
			disposed = true;

			if(disposing){
				draws.Clear();
				draws = null;
			}
		}
	}
}