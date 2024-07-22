using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class PsychoKnife : VanillaClone
{
	protected override short VanillaItemId => ItemID.PsychoKnife;

	public override void Defaults()
	{
		ItemType = Core.ItemType.Melee;
		base.Defaults();
	}

	public override void HoldItem(Player player)
	{
		if (player.itemAnimation > 0)
		{
			player.stealthTimer = 15;

			if (player.stealth > 0f)
			{
				player.stealth += 0.1f;
			}
		}
		else if (player.velocity.Length() < 0.141421363f && !player.mount.Active)
		{
			if (player.stealthTimer == 0 && player.stealth > 0f)
			{
				player.stealth -= 0.02f;
				if (player.stealth <= 0)
				{
					player.stealth = 0f;

					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						NetMessage.SendData(MessageID.PlayerStealth, -1, -1, null, player.whoAmI);
					}
				}
			}
		}
		else
		{
			if (player.stealth > 0f)
			{
				player.stealth += 0.1f;
			}

			if (player.mount.Active)
			{
				player.stealth = 1f;
			}
		}

		if (player.stealth != 1f)
		{
			player.stealth = 0;
		}

		player.GetDamage(DamageClass.Melee) += (1f - player.stealth) * 3f;
		player.GetCritChance(DamageClass.Melee) += (int)((1f - player.stealth) * 30f);
		player.GetKnockback(DamageClass.Melee) *= 1f + (1f - player.stealth);
		player.aggro -= (int)((1f - player.stealth) * 750f);

		if (player.stealthTimer > 0)
		{
			player.stealthTimer--;
		}
	}
}