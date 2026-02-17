using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

internal class ToxicSmogDebuff : ModBuff
{
	internal class ToxicSmogPlayer : ModPlayer 
	{
		private int _timeToxic = 0;

		public override void ResetEffects()
		{
			if (!Player.HasBuff<ToxicSmogDebuff>())
			{
				_timeToxic = 0;
			}
			else
			{
				_timeToxic++;
			}
		}

		public override void UpdateBadLifeRegen()
		{
			if (Player.HasBuff<ToxicSmogDebuff>())
			{
				Player.lifeRegen = Math.Min(0, Player.lifeRegen);
				Player.lifeRegen -= _timeToxic / 8;
			}
		}
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;

		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}

	public override void Update(Player player, ref int buffIndex)
	{
		base.Update(player, ref buffIndex);
	}
}
