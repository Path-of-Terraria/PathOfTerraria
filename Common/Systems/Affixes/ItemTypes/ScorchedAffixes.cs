using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

/// <summary> The player permanently has reduced flames resistance. </summary>
internal class PermanentlyScorchedAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.AddBuff(ModContent.BuffType<ScorchingFlamesDebuff>(), 2);
	}
}

/// <summary> The player's victims spread scorched and ignite stacks to nearby enemies. </summary>
internal class SpreadIgniteScorchOnKillAffix : ItemAffix
{
	internal sealed class SpreadIgniteScorchOnKillNPC : GlobalNPC
	{
		public uint TicksLeft = 0;
		
		public override bool InstancePerEntity => true;

		// OnKill is not used due to the discrepancy of it being called world-side, while OnHit is called hitter-side.
		public override void HitEffect(NPC source, NPC.HitInfo hit)
		{
			if (TicksLeft <= 0) { return; }
			if (source.life > 0) { return; }
			
			int igniteType = ModContent.BuffType<IgnitedDebuff>();
			int scorchedType = ModContent.BuffType<ScorchingFlamesDebuff>();
			int igniteIndex = source.FindBuffIndex(igniteType);
			int scorchedIndex = source.FindBuffIndex(scorchedType);

			if (igniteIndex < 0 && scorchedIndex < 0) { return; }

			Vector2 sourceCenter = source.Center;
			Rectangle sourceRect = source.getRect();

			if (!Main.dedServ)
			{
				SoundStyle soundStyle = SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.8f, PitchVariance = 0.1f };
				SoundEngine.PlaySound(soundStyle, sourceCenter);

				//TODO: Update visuals?

				if (igniteIndex >= 0)
				{
					for (int i = 0; i < 10; i++)
					{
						Vector2 dustPos = Main.rand.NextVector2FromRectangle(source.getRect());
						Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f) - Vector2.UnitY;
						Dust.NewDustPerfect(dustPos, DustID.FlameBurst, dustVel, Alpha: 250);
					}
				}
				if (scorchedIndex >= 0)
				{
					for (int i = 0; i < 30; i++)
					{
						Vector2 dustPos = Main.rand.NextVector2FromRectangle(source.getRect());
						Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f) - Vector2.UnitY;
						Dust.NewDustPerfect(dustPos, DustID.RedMoss, dustVel);
					}
				}
			}

			foreach (NPC target in Main.ActiveNPCs)
			{
				if (target.friendly || target.dontTakeDamage) { continue; }
				if (target.buffImmune[scorchedType]) { continue; }

				Vector2 targetPoint = target.getRect().ClosestPointInRect(sourceCenter);
				Vector2 sourcePoint = sourceRect.ClosestPointInRect(target.Center);
				if (!targetPoint.WithinRange(sourcePoint, PoTMod.NearbyDistance)) { continue; }

				if (scorchedIndex >= 0) { target.AddBuff(scorchedType, source.buffTime[scorchedIndex]); }
				if (igniteIndex >= 0) { target.AddBuff(igniteType, source.buffTime[igniteIndex]); }
			}
		}
	}
	internal sealed class SpreadIgniteScorchOnKillPlayer : ModPlayer
	{
		public bool Active;

		public override void ResetEffects()
		{
			Active = false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!Active) { return; }
			if (!target.TryGetGlobalNPC(out SpreadIgniteScorchOnKillNPC npcGlobal)) { return; }

			// Give it a second to die in case some enemies have delayed deaths.
			// Overly delayed might not function?
			npcGlobal.TicksLeft = Math.Max(npcGlobal.TicksLeft, 60);
		}
	}
	
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<SpreadIgniteScorchOnKillPlayer>().Active = true;
	}
}
