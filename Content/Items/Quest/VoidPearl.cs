using PathOfTerraria.Content.Projectiles.Utility;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Utilities;

namespace PathOfTerraria.Content.Items.Quest;

internal class VoidPearl : ModItem
{
	private readonly static WeightedRandom<int> DustIDs = new(new Tuple<int, double>(DustID.Ash, 0.5), new Tuple<int, double>(DustID.Torch, 1));

	public readonly record struct Particle(Dust Dust, bool Overlayer);

	private readonly List<Particle> localDusts = [];

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(14, 14);
		Item.rare = ItemRarityID.Quest;
		Item.useTime = Item.useAnimation = 60;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.noUseGraphic = true;
		Item.shoot = ModContent.ProjectileType<VoidPearlThrown>();
		Item.shootSpeed = 16;
		Item.consumable = true;
		Item.maxStack = 1;

		for (int i = 0; i < 25; ++i)
		{
			localDusts.Add(new Particle(GenerateDust(), Main.rand.NextBool()));
		}
	}

	public override bool CanUseItem(Player player)
	{
		return SubworldSystem.Current is null && NoPortalProjectile();
	}

	private static bool NoPortalProjectile()
	{
		foreach (Projectile proj in Main.ActiveProjectiles)
		{
			if (proj.type == ModContent.ProjectileType<WoFPortal>())
			{
				return false;
			}
		}

		return true;
	}

	public override void Update(ref float gravity, ref float maxFallSpeed)
	{
		gravity = 0;

		if (Item.wet || Item.honeyWet || Item.lavaWet)
		{
			Item.velocity.Y = MathHelper.Max(Item.velocity.Y - 0.1f, -2);
		}

		Item.velocity *= 0.98f;
	}

	public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		DrawParticleList(true, true, position);
	}

	public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		DrawParticleList(true, false, Item.Center);
	}

	public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		DrawParticleList(false, true, position);
		return true;
	}

	public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
	{
		DrawParticleList(false, false, Item.Center);
		return true;
	}

	private void DrawParticleList(bool overlayer, bool inventory, Vector2 position)
	{
		foreach (Particle item in localDusts)
		{
			if (item.Overlayer == overlayer)
			{
				DrawParticle(item, inventory, position);
			}
		}
	}

	internal static void DrawParticle(Particle item, bool inInventory, Vector2 position)
	{
		Dust dust = item.Dust;
		Color col = Color.White;
		Vector2 pos = dust.position + position;

		if (!inInventory)
		{
			pos -= Main.screenPosition;
			col = Lighting.GetColor((dust.position + position).ToTileCoordinates());
		}

		Main.spriteBatch.Draw(TextureAssets.Dust.Value, pos, dust.frame, col, dust.rotation, dust.frame.Size() / 2f, dust.scale, SpriteEffects.None, 0);

		dust.position += dust.velocity;
		dust.velocity += dust.position.DirectionTo(Vector2.Zero).RotatedByRandom(0.4f) * 0.6f;

		if (dust.velocity.LengthSquared() > 4 * 4)
		{
			dust.velocity = Vector2.Normalize(dust.velocity) * 4;
		}
	}

	internal static Dust GenerateDust()
	{
		var dust = new Dust
		{
			type = DustIDs,
			scale = Main.rand.NextFloat(0.8f, 1.1f),
			shader = null,
			noLightEmittence = false,
			velocity = new Vector2(3, 0).RotatedByRandom(MathHelper.Pi),
			frame = new Rectangle(0, 0, 8, 8)
		};

		dust.frame.X = 10 * dust.type;
		dust.frame.Y = 10 * Main.rand.Next(3);

		int checkType = dust.type;

		while (checkType >= 100)
		{
			checkType -= 100;
			dust.frame.X -= 1000;
			dust.frame.Y += 30;
		}

		return dust;
	}
}
