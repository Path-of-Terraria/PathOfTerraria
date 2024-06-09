using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.ModPlayers;

namespace PathOfTerraria.Content.Passives;

internal class MartialMasteryPassive : Passive
{
	public MartialMasteryPassive()
	{
		Name = "Martial Mastery";
		Tooltip = "Increases your melee damage by 5% per level";
		MaxLevel = 5;
		TreePos = new Vector2(250, 300);
		Classes = [PlayerClass.Melee];
	}

	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Melee) += 0.05f * Level;
	}

	public override void Connect(List<Passive> all, Player player)
	{
		ClassModPlayer mp = player.GetModPlayer<ClassModPlayer>();
		if (mp.SelectedClass == PlayerClass.Melee)
		{
			Connect<CloseRangePassive>(all, player);
			Connect<BleedPassive>(all, player);
		}
	}
}

internal class MarksmanshipMasteryPassive : Passive
{
	public MarksmanshipMasteryPassive()
	{
		Name = "Marksmanship Mastery";
		Tooltip = "Increases your ranged damage by 5% per level";
		MaxLevel = 5;
		TreePos = new Vector2(350, 250);
		Classes = [PlayerClass.Ranged];
	}

	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Ranged) += 0.05f * Level;
	}

	public override void Connect(List<Passive> all, Player player)
	{
		ClassModPlayer mp = player.GetModPlayer<ClassModPlayer>();
		if (mp.SelectedClass == PlayerClass.Ranged)
		{
			Connect<LongRangePassive>(all, player);
			Connect<AmmoPassive>(all, player);			
		}
	}
}

internal class ArcaneMasteryPassive : Passive
{
	public ArcaneMasteryPassive()
	{
		Name = "Arcane Mastery";
		Tooltip = "Increases your magic damage by 5% per level";
		MaxLevel = 5;
		TreePos = new Vector2(450, 250);
		Classes = [PlayerClass.Magic];
	}

	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Magic) += 0.05f * Level;
	}

	public override void Connect(List<Passive> all, Player player)
	{
		ClassModPlayer mp = player.GetModPlayer<ClassModPlayer>();
		if (mp.SelectedClass == PlayerClass.Magic)
		{
			Connect<LongRangePassive>(all, player);
			Connect<ManaPassive>(all, player);
		}
	}
}

internal class SummoningMasteryPassive : Passive
{
	public SummoningMasteryPassive()
	{
		Name = "Summoning Mastery";
		Tooltip = "Increases your summon damage by 5% per level";
		MaxLevel = 5;
		TreePos = new Vector2(550, 300);
		Classes = [PlayerClass.Summoner];
	}

	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Summon) += 0.05f * Level;
	}

	public override void Connect(List<Passive> all, Player player)
	{
		ClassModPlayer mp = player.GetModPlayer<ClassModPlayer>();
		if (mp.SelectedClass == PlayerClass.Summoner)
		{ 
			Connect<MinionPassive>(all, player);
			Connect<SentryPassive>(all, player);
		}
	}
}