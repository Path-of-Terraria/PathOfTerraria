using ReLogic.Content;

namespace PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;

internal abstract class GrimoireSummon : ModProjectile
{
	public override string Texture => $"PathOfTerraria/Assets/Projectiles/Summoner/GrimoireSummons/{GetType().Name}";

	public Asset<Texture2D> Icon = null;

	public sealed override void SetStaticDefaults()
	{
		Icon = ModContent.Request<Texture2D>(Texture + "_Icon");
	}

	/// <summary>
	/// Defines what types, in any order, the parts need to be on the sacrifice panel in order to unlock or use this summon.
	/// </summary>
	/// <returns>The types of the summon's parts. Max length of 5.</returns>
	public abstract int[] GetPartTypes();
}
