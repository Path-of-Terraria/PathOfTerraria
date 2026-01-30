namespace PathOfTerraria.Content.Buffs;

internal class MosquitoTrappedDebuff : ModBuff
{
	public class MosquitoTrappedPlayer : ModPlayer
	{
		public override void UpdateBadLifeRegen()
		{
			if (Player.HasBuff<MosquitoTrappedDebuff>())
			{
				Player.lifeRegen = Math.Min(0, Player.lifeRegen);
				Player.lifeRegen -= 30;
			}
		}
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
		Main.buffNoTimeDisplay[Type] = true;
	}
}
