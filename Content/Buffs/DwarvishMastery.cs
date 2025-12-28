using PathOfTerraria.Content.Passives.Utility.Masteries;

namespace PathOfTerraria.Content.Buffs;

internal class DwarvishBuff : ModBuff
{
	public override void Update(Player player, ref int buffIndex)
	{
		player.pickSpeed *= RockAndStoneMastery.Speed;
	}
}
