using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class SacrificialSummonsMastery : Passive
{
	internal class SacrificialSummonsPlayer : ModPlayer
	{
		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
		{
			if (AnyoneNearbyOwnsMastery(Player.Center, out float value) && Player.slotsMinions >= value)
			{
				foreach (Projectile proj in Main.ActiveProjectiles)
				{
					if (proj.owner == Player.whoAmI && proj.minion && proj.minionSlots > 0)
					{
						value -= proj.minionSlots;
						proj.active = false;
						proj.netUpdate = true;

						if (!Main.dedServ)
						{
							SoundEngine.PlaySound(SoundID.NPCHit54, proj.Center);
							Projectile.NewProjectile(proj.GetSource_Death(), proj.Center, new Vector2(0, -6f), ModContent.ProjectileType<ReviveEffect>(), 0, 0, proj.owner);
						}
					}

					if (value <= 0)
					{
						break;
					}
				}

				Player.statLife = 100;
				Player.SetImmuneTimeForAllTypes(120);
				return false;
			}

			return true;
		}

		private static bool AnyoneNearbyOwnsMastery(Vector2 center, out float value)
		{
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.DistanceSQ(center) < PoTMod.NearbyDistanceSq && !player.dead && !player.HasBuff<SacrificeReviveCooldown>()
					&& player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<SacrificialSummonsMastery>(out value))
				{
					player.AddBuff(ModContent.BuffType<SacrificeReviveCooldown>(), 5 * 60);
					return true;
				}
			}

			value = 0;
			return false;
		}
	}
}