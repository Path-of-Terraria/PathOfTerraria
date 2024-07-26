using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class PsychoKnife : VanillaClone
{
	protected override short VanillaItemId => ItemID.PsychoKnife;

	public override void Load()
	{
		IL_Player.Update += FixStealthBeingResetAnnoyingly;
	}

	private void FixStealthBeingResetAnnoyingly(ILContext il)
	{
		ILCursor c = new(il);

		while (c.TryGotoNext(x => x.MatchStfld<Player>(nameof(Player.stealth))))
		{ }

		c.Emit(OpCodes.Pop);
		c.Emit(OpCodes.Ldarg_0);
		c.EmitDelegate(ResetStealthIfNeeded);
	}
	
	public static float ResetStealthIfNeeded(Player player)
	{
		if (player.HeldItem.type == ModContent.ItemType<PsychoKnife>())
		{
			return player.stealth;
		}

		return 1f;
	}

	public override void SetDefaults()
	{
		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Core.ItemType.Melee;
	}

	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		player.stealth = 1;

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			NetMessage.SendData(MessageID.PlayerStealth, -1, -1, null, player.whoAmI);
		}
	}

	public class PsychoKnifePlayer : ModPlayer
	{
		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
		{
			if (Player.HeldItem.type != ModContent.ItemType<PsychoKnife>())
			{
				return;
			}

			r *= Player.stealth;
			g *= Player.stealth;
			b *= Player.stealth;
			a *= Player.stealth;
			fullBright = false;
		}

		public override void PostUpdateEquips()
		{
			if (Player.HeldItem.type != ModContent.ItemType<PsychoKnife>())
			{
				return;
			}

			if (Player.itemAnimation > 0)
			{
				Player.stealthTimer = 15;

				if (Player.stealth > 0f)
				{
					Player.stealth += 0.1f;
				}
			}
			else if (Player.velocity.X > -0.1 && Player.velocity.X < 0.1 && Player.velocity.Y > -0.1 && Player.velocity.Y < 0.1 && !Player.mount.Active)
			{
				if (Player.stealthTimer == 0 && Player.stealth > 0f)
				{
					Player.stealth -= 0.02f;
					if (Player.stealth <= 0.0)
					{
						Player.stealth = 0f;

						if (Main.netMode == NetmodeID.MultiplayerClient)
						{
							NetMessage.SendData(MessageID.PlayerStealth, -1, -1, null, Player.whoAmI);
						}
					}
				}
			}
			else
			{
				if (Player.stealth > 0f)
				{
					Player.stealth += 0.1f;
				}

				if (Player.mount.Active)
				{
					Player.stealth = 1f;
				}
			}

			if (Player.stealth >= 1f)
			{
				Player.stealth = 1f;
			}

			Player.GetDamage(DamageClass.Melee) += (1f - Player.stealth) * 3f;
			Player.GetCritChance(DamageClass.Melee) += (int)((1f - Player.stealth) * 30f);
			Player.GetKnockback(DamageClass.Melee) *= 1f + (1f - Player.stealth);
			Player.aggro -= (int)((1f - Player.stealth) * 750f);

			if (Player.stealthTimer > 0)
			{
				Player.stealthTimer--;
			}
		}

		public override void PostUpdateRunSpeeds()
		{
			float slowDown = Player.maxRunSpeed / 2f * (1f - Player.stealth);
			Player.maxRunSpeed -= slowDown;
			Player.accRunSpeed = Player.maxRunSpeed;
		}
	}
}