using System.Collections.Generic;
using Terraria.ID;
using PathOfTerraria.Content.Items.Quest;
using Terraria.DataStructures;

using Particle = PathOfTerraria.Content.Items.Quest.VoidPearl.Particle;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class VoidPearlThrown : ModProjectile
{
	public override string Texture => "PathOfTerraria/Assets/Items/Quest/VoidPearl";

	private readonly List<Particle> localDusts = [];

	public override void SetDefaults()
	{
		Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
		Projectile.friendly = false;
		Projectile.width = Projectile.height = 14;

		for (int i = 0; i < 25; ++i)
		{
			localDusts.Add(new Particle(VoidPearl.GenerateDust(), Main.rand.NextBool()));
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		foreach (Particle item in localDusts)
		{
			if (!item.Overlayer)
			{
				VoidPearl.DrawParticle(item, false, Projectile.Center);
			}
		}

		return true;
	}

	public override void PostDraw(Color lightColor)
	{
		foreach (Particle item in localDusts)
		{
			if (item.Overlayer)
			{
				VoidPearl.DrawParticle(item, false, Projectile.Center);
			}
		}
	}

	public override void OnKill(int timeLeft)
	{
		if (AnyAshNearby())
		{
			IEntitySource src = Projectile.GetSource_Death();
			int type = ModContent.ProjectileType<WoFRitualProj>();
			Projectile.NewProjectile(src, Projectile.Center, new Vector2(0, 0), type, 0, 0, Main.myPlayer);

			foreach (Particle particle in localDusts)
			{
				Dust dust = particle.Dust;
				Dust.NewDustPerfect(Projectile.Center + dust.position, dust.type, dust.velocity);
			}
		}
		else
		{
			int item = Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.Hitbox, ModContent.ItemType<VoidPearl>());
		}
	}

	private bool AnyAshNearby()
	{
		Point16 pos = Projectile.Center.ToTileCoordinates16();

		if (!WorldGen.InWorld(pos.X, pos.Y, 10))
		{
			return false;
		}

		for (int i = pos.X - 5; i < pos.X + 5; i++)
		{
			for (int j = pos.Y - 5; j < pos.Y + 5; j++)
			{
				Tile tile = Main.tile[i, j];

				if (tile.HasTile && tile.TileType == TileID.Ash)
				{
					return true;
				}
			}
		}

		return false;
	}
}