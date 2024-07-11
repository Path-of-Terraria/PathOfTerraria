using PathOfTerraria.Content.Items.Pickups;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;

internal class OwlSummon : GrimoireSummon
{
	public override void SetDefaults()
	{
		Projectile.width = 20;
		Projectile.height = 20;
		Projectile.tileCollide = false;
		Projectile.friendly = true;
		Projectile.penetrate = -1;
	}

	public override void AI()
	{
		
	}

	public override int[] GetPartTypes()
	{
		return [ModContent.ItemType<OwlFeather>(), ModContent.ItemType<OwlFeather>(), ModContent.ItemType<BatWings>()];
	}
}
