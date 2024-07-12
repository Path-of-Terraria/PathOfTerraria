using ReLogic.Content;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Projectiles.Summoner;

internal abstract class GrimoireSummon : ModProjectile
{
	public override string Texture => $"PathOfTerraria/Assets/Projectiles/Summoner/GrimoireSummons/{GetType().Name}";

	public static Dictionary<int, Asset<Texture2D>> IconsById = [];

	protected Player Owner => Main.player[Projectile.owner];
	protected bool Channeling => Owner.channel;

	public sealed override void SetStaticDefaults()
	{
		if (!IconsById.ContainsKey(Type))
		{
			IconsById.Add(Type, ModContent.Request<Texture2D>(Texture + "_Icon"));
		}
	}

	/// <summary>
	/// Defines what types, in any order, the parts need to be on the sacrifice panel in order to unlock or use this summon.
	/// </summary>
	/// <returns>The types of the summon's parts. Max length of 5.</returns>
	public abstract Dictionary<int, int> GetRequiredParts();
}

public class GrimoireSummonLoader : ModSystem
{
	public Dictionary<int, Dictionary<int, int>> RequiredPartsByProjectileId = [];
	public HashSet<int> SummonIds = [];

	public override void PostSetupContent()
	{
		for (int i = 0; i < ProjectileLoader.ProjectileCount; ++i)
		{
			var proj = new Projectile();
			proj.SetDefaults(i);

			if (proj.ModProjectile is GrimoireSummon grim)
			{
				Dictionary<int, int> types = grim.GetRequiredParts();

				if (types.Count == 0)
				{
					throw new Exception("GrimoireSummon.GetPartTypes should not return an empty Dictionary!");
				}

				int total = 0;

				foreach (KeyValuePair<int, int> item in types)
				{
					total += item.Value;
				}

				if (total == 0)
				{
					throw new Exception("GrimoireSummon.GetPartTypes should not return a Dictionary with 0 required items!");
				}

				if (total > 5)
				{
					throw new Exception("GrimoireSummon.GetPartTypes should not return a Dictionary with more than 5 values, or over 5 required items!");
				}

				RequiredPartsByProjectileId.Add(i, types);
				SummonIds.Add(i);
			}
		}
	}
}