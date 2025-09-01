namespace PathOfTerraria.Common.Systems.SurfaceBGModifications;

internal interface IBackgroundModifier
{
	/// <summary>
	/// Modifies the Y position of the background.
	/// </summary>
	/// <param name="isFar">Whether this is modifying the far or middle backgrounds.</param>
	public void ModifyPosition(bool isFar, ref int y);
}
