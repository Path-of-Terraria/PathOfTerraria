using PathOfTerraria.Common.Systems.ModPlayers.LivesSystem;
using PathOfTerraria.Content.Swamp.NPCs.SwampBoss;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

internal class ToxicSmogDebuff : ModBuff
{
	internal class ToxicSmogPlayer : ModPlayer, IPreDomainRespawnPlayer
	{
		internal int TimeToxic = 0;

		public override void ResetEffects()
		{
			if (!Player.HasBuff<ToxicSmogDebuff>() || Player.HasBuff<ToxicSmogImmunityBuff>())
			{
				TimeToxic = 0;
			}
			else
			{
				TimeToxic++;
			}
		}

		public override void UpdateBadLifeRegen()
		{
			if (Player.HasBuff<ToxicSmogDebuff>())
			{
				Player.lifeRegen = Math.Min(0, Player.lifeRegen);
				Player.lifeRegen -= TimeToxic / 6;
			}
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			TimeToxic = 0;
		}

		public void OnDomainRespawn()
		{
			if (PoisonShaderFunctionality.Intensity > 0.1f)
			{
				Player.AddBuff(ModContent.BuffType<ToxicSmogImmunityBuff>(), 10 * 60);
			}
		}
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;

		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
	{
		drawParams.DrawColor = Color.Lerp(drawParams.DrawColor, Color.Red, Math.Min(Main.LocalPlayer.GetModPlayer<ToxicSmogPlayer>().TimeToxic / 1200f, 1));
		return true;
	}
}
