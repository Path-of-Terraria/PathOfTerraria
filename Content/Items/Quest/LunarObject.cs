using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Quest;

internal class LunarObject : ModItem
{
	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 1;

		ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 5));
	}

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(24, 18);
		Item.rare = ItemRarityID.Quest;
	}

	public override void Update(ref float gravity, ref float maxFallSpeed)
	{
		if (Collision.SolidCollision(Item.BottomLeft, Item.width, 6, true) && Item.velocity.Y > 0.3f)
		{
			Item.active = false;

			SoundEngine.PlaySound(SoundID.Shatter, Item.Center);
			Projectile.NewProjectile(Item.GetSource_Death(), Item.Center, Vector2.Zero, ModContent.ProjectileType<EoCRitualProj>(), 0, 0, Main.myPlayer);

			if (Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 2; ++i)
				{
					Gore.NewGore(Item.GetSource_Death(), Item.position, Item.velocity, ModContent.Find<ModGore>($"{PoTMod.ModName}/LunarConcoction_" + i).Type);
				}

				for (int i = 0; i < 60; ++i)
				{
					Vector2 vel = new Vector2(0, -Main.rand.NextFloat(9, 12f)).RotatedByRandom(MathHelper.Pi / 3);
					Dust.NewDustPerfect(Item.Center, DustID.YellowStarDust, vel, Scale: Main.rand.NextFloat(0.8f, 1.2f));
				}
			}
		}
	}
}
