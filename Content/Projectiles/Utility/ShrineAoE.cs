using PathOfTerraria.Common.Subworlds;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Utilities;

namespace PathOfTerraria.Content.Projectiles.Utility;

public abstract class ShrineAoE : ModProjectile
{
	public static readonly Dictionary<int, Asset<Texture2D>> IconsByType = [];

	public override string Texture => $"{PoTMod.ModName}/Assets/Projectiles/Utility/ShrineAoE";

	public abstract Color Tint { get; }
	public abstract int BuffId { get; }
	public virtual string Icon => (GetType().Namespace + "." + Name).Replace('.', '/').Replace("AoE", "Icon");

	private ref float ShrineType => ref Projectile.ai[0];

	private bool HasInitialized
	{
		get => Projectile.ai[1] == 1;
		set => Projectile.ai[1] = value ? 1 : 0;
	}

	public override void SetStaticDefaults()
	{
		IconsByType.Add(Type, ModContent.Request<Texture2D>(Icon));
	}

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 3;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(2030, 2030);
		Projectile.netImportant = true;
		Projectile.penetrate = -1;
		Projectile.Opacity = 0.06f;
	}

	public override void AI()
	{
		Projectile.timeLeft = 3;

		if (!HasInitialized && AnyNearbyPlayer())
		{
			HasInitialized = true;
			WeightedRandom<int> random = ShrineMobPools.GetPool((int)ShrineType);

			for (int i = 0; i < 8; ++i)
			{
				NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)Projectile.Center.X, (int)Projectile.Center.Y, random);
			}
		}

		int halfWidthSquared = (int)MathF.Pow(Projectile.width / 2f, 2);

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.DistanceSQ(Projectile.Center) < halfWidthSquared)
			{
				npc.AddBuff(BuffId, 2);
			}
		}
	}

	private bool AnyNearbyPlayer()
	{
		foreach (Player player in Main.ActivePlayers)
		{
			if (player.DistanceSQ(Projectile.Center) < Projectile.width * Projectile.width * 1.05f)
			{
				return true;
			}
		}

		return false;
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Tint * Projectile.Opacity;
	}

	public override void PostDraw(Color lightColor)
	{
		Texture2D tex = IconsByType[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition - new Vector2(0, 45 + MathF.Sin((float)Main.timeForVisualEffects * 0.035f) * 10);
		Main.spriteBatch.Draw(tex, position, null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
	}
}
