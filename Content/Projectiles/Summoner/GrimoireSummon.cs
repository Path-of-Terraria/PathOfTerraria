using PathOfTerraria.Content.Items.Gear.Weapons.Grimoire;
using PathOfTerraria.Content.Items.Pickups;
using PathOfTerraria.Core.Items;
using ReLogic.Content;
using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Content.Projectiles.Summoner;

/// <summary>
/// Defines a Grimoire Summon and its functionality.<br/><br/>
/// By default, this class uses <see cref="Projectile.ai"/>[0], alongside <see cref="Projectile.localAI"/>[0], for if the proj is despawning and for animation respectively.
/// </summary>
internal abstract class GrimoireSummon : ModProjectile
{
	public static Dictionary<int, Asset<Texture2D>> IconsById = [];

	public abstract int BaseDamage { get; }

	/// <summary>
	/// Whether the current projectile is despawning.
	/// Uses <see cref="Projectile.ai"/>[0].
	/// </summary>
	protected bool Despawning
	{
		get => Projectile.ai[0] == 1;
		set => Projectile.ai[0] = value ? 1 : 0;
	}

	protected ref float AnimationTimer => ref Projectile.localAI[0];

	protected Player Owner => Main.player[Projectile.owner];
	protected bool Channeling => Owner.channel;
	protected List<Affix> PartAffixes = [];

	public sealed override void SetStaticDefaults()
	{
		if (!IconsById.ContainsKey(Type))
		{
			IconsById.Add(Type, ModContent.Request<Texture2D>(Texture + "_Icon"));
		}

		StaticDefaults();
	}

	public virtual void StaticDefaults() { }

	public override bool? CanCutTiles()
	{
		return false;
	}

	public sealed override bool PreAI()
	{
		if (Owner.GetModPlayer<GrimoireSummonPlayer>().CurrentSummonId != Type || Owner.HeldItem.type != ModContent.ItemType<GrimoireItem>())
		{
			Projectile.Kill();
			return false;
		}

		if (!Channeling)
		{
			Despawning = true;
		}

		Owner.SetDummyItemTime(2);
		AnimateSelf();

		var modifier = new Common.Systems.EntityModifier();

		foreach (Item part in Owner.GetModPlayer<GrimoireSummonPlayer>().StoredParts)
		{
			if (part.ModItem is null)
			{
				continue;
			}	

			var pickup = part.ModItem as GrimoirePickup;
			PoTItemHelper.ApplyAffixes(pickup.Item, modifier, Owner);
		}

		modifier.ApplyTo(Projectile);

		if (Despawning)
		{
			DespawnBehaviour();
			return false;
		}

		if (Main.myPlayer == Projectile.owner && Main.mouseRight && Main.mouseRightRelease)
		{
			AltEffect();
		}

		return true;
	}

	/// <summary>
	/// Animates the projectile. This is done to make sure the projectile is animated even when <see cref="Despawning"/> is true.
	/// </summary>
	protected abstract void AnimateSelf();

	/// <summary>
	/// Called when the projectile should be despawning.
	/// Default behaviour fades away and slows down the summon until it is fully invisible.
	/// </summary>
	protected virtual void DespawnBehaviour()
	{
		Projectile.Opacity *= 0.85f;
		Projectile.velocity *= 0.98f;

		if (Projectile.Opacity < 0.05f)
		{
			Projectile.Kill();
		}
	}

	/// <summary>
	/// Called when using the weapon and using right click.
	/// </summary>
	protected abstract void AltEffect();

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