namespace PathOfTerraria.Common.Systems.SunModifications;

internal class SunEditInstance : ModType
{
	protected override void Register()
	{
		ModTypeLookup<SunEditInstance>.Register(this);
	}

	public virtual void PreModifySunDrawing(ref Texture2D texture, ref Vector2 position, ref Color color)
	{
	}
}
