using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.ModPlayers;

namespace PathOfTerraria.Content.Passives;

internal class MartialMasteryPassive : Passive
{
	public override string Name => "Martial Mastery";
	public override string Tooltip => "Increases your melee damage by 5% per level";

	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Melee) += 0.05f * Level;
	}
}

internal class MarksmanshipMasteryPassive : Passive
{
	public override string Name => "Marksmanship Mastery";
	public override string Tooltip => "Increases your ranged damage by 5% per level";

	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Ranged) += 0.05f * Level;
	}
}

internal class ArcaneMasteryPassive : Passive
{
	public override string Name => "Arcane Mastery";
	public override string Tooltip => "Increases your magic damage by 5% per level";

	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Magic) += 0.05f * Level;
	}
}

internal class SummoningMasteryPassive : Passive
{
	public override string Name => "Summoning Mastery";
	public override string Tooltip => "Increases your summon damage by 5% per level";


	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Summon) += 0.05f * Level;
	}
}