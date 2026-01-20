using PathOfTerraria.Common.World.Generation;

namespace PathOfTerraria.Common.Systems;

internal class StaticNoise : ILoadable
{
	public static FastNoiseLite Perlin { get; private set; }

	public void Load(Mod mod)
	{
		Perlin = new FastNoiseLite(1500);
	}

	public void Unload()
	{
	}
}
