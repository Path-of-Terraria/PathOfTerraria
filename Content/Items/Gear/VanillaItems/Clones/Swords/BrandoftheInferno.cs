using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Systems.VanillaModifications;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class BrandoftheInferno : VanillaClone
{
	protected override short VanillaItemId => ItemID.DD2SquireDemonSword;

	public override void SetStaticDefaults()
	{
		AddValidShieldParryItems.AddParryItem(Type);
	}

	public override void Defaults()
	{
		ItemType = ItemType.Melee;
		base.Defaults();
	}

	public override bool AltFunctionUse(Player player)
	{
		return false;
	}

	public override void MeleeEffects(Player player, Rectangle hitbox)
	{
		Rectangle rect = SwordCommon.GetItemRectangle(player, Item);

		var dust = Dust.NewDustDirect(rect.TopLeft(), rect.Width, rect.Height, DustID.Torch, player.velocity.X * 0.2f + player.direction * 3, player.velocity.Y * 0.2f, 
			100, Color.Transparent, 0.7f);
		dust.noGravity = true;
		dust.velocity *= 2f;
		dust.fadeIn = 0.9f;
	}

	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (Main.rand.NextBool(4))
		{
			target.AddBuff(BuffID.OnFire3, 300);
		}
	}

	public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
	{
		if (Main.rand.NextBool(4))
		{
			target.AddBuff(BuffID.OnFire3, 300);
		}
	}
}