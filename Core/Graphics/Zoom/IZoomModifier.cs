using Terraria.Graphics;

namespace PathOfTerraria.Core.Graphics.Zoom;

public interface IZoomModifier
{
	/// <summary>
	///     Gets the unique identifier of the modifier.
	/// </summary>
	string Identifier { get; }
    
	/// <summary>
	///     Gets whether the modifier has finished.
	/// </summary>
	bool Finished { get; }

	void Update(ref SpriteViewMatrix matrix);
}