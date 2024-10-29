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
		if (Collision.SolidCollision(Item.BottomLeft, Item.width, 6, true) && Item.velocity.Y > 0)
		{
			Item.active = false;

			SoundEngine.PlaySound(SoundID.Shatter, Item.Center);
			Projectile.NewProjectile(Item.GetSource_Death(), Item.Center, Vector2.Zero, ModContent.ProjectileType<EoCRitualProj>(), 0, 0, Main.myPlayer);
		}
	}
}
