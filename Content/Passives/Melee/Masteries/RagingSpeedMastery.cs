using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Utilities;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

internal class RagingSpeedMastery : Passive
{
	internal class RagingSpeedPlayer : ModPlayer
	{
		public struct DelayedHit(int damage, int npcWho)
		{
			public readonly int Damage = damage;
			public readonly int NpcWho = npcWho;

			public int Timer = 60;
		}

		private static bool DelayedHitRunning = false;

		public List<DelayedHit> DelayedHits = [];

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<RagingSpeedMastery>(out float value) && Player.TryGetModPlayer(out RagePlayer rage) 
				&& rage.Rage == rage.MaxRage.Value && Main.rand.NextFloat() < Chance && !DelayedHitRunning)
			{
				DelayedHits.Add(new DelayedHit(damageDone, target.whoAmI));
			}
		}

		public override void PostUpdateEquips()
		{
			foreach (ref DelayedHit hit in CollectionsMarshal.AsSpan(DelayedHits))
			{
				hit.Timer--;

				if (hit.Timer <= 0)
				{
					var hitInfo = new NPC.HitInfo()
					{
						Damage = hit.Damage,
					};

					using var _ = ValueOverride.Create(ref DelayedHitRunning, true);

					Main.npc[hit.NpcWho].StrikeNPC(hitInfo);
					PlayerLoader.OnHitNPC(Player, Main.npc[hit.NpcWho], in hitInfo, hit.Damage);
				}
			}

			DelayedHits.RemoveAll(x => x.Timer <= 0);
		}
	}

	const float Chance = 0.25f;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, (Chance * 100).ToString("00"));

	public override void BuffPlayer(Player player)
	{
		player.GetAttackSpeed(DamageClass.Generic) += player.GetModPlayer<RagePlayer>().Rage * Value * 0.01f;
	}
}
