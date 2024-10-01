namespace PathOfTerraria.Common.Systems.DisableBuilding;

/// <summary>
/// Adds a hook for if a tile can be cut. 
/// <see cref="Main.tileCut"/> should be set to true for the given tile type, and specifics defined in the hook.
/// </summary>
internal interface ICanCutTile
{
	/// <summary>
	/// Whether the tile can be cut or not.
	/// </summary>
	/// <param name="i">X position of the tile.</param>
	/// <param name="j">Y position of the tile.</param>
	/// <returns>Whether the tile can be cut or not.</returns>
	public bool CanCut(int i, int j)
	{
		return true;
	}
}
