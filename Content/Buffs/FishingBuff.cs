using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;

namespace PathOfTerraria.Content.Buffs;

internal class FishingBuff : ModBuff
{
	public static void Apply(Player player)
	{
		player.AddBuff(ModContent.BuffType<FishingBuff>(), 10 * 60);
		List<int> stacks = player.GetModPlayer<FishingBuffPlayer>().Stacks;

		if (stacks.Count <= 4)
		{
			stacks.Add(10 * 60);
		}
		else
		{
			int min = stacks.Min();
			int index = stacks.IndexOf(min);
			stacks[index] = 10 * 60;
		}
	}

	public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
	{
		tip = Language.GetText("Mods.PathOfTerraria.Buffs.FishingBuff.Description").Format(Main.LocalPlayer.GetModPlayer<FishingBuffPlayer>().Stacks.Count * 10);
	}
}

internal class FishingBuffPlayer : ModPlayer
{ 
	internal readonly List<int> Stacks = new(5);

	public override void ResetEffects()
	{
		for (int i = 0; i < Stacks.Count; i++)
		{
			Stacks[i]--;
		}

		Stacks.RemoveAll(x => x <= 0);
		Player.fishingSkill += Stacks.Count * 10;
	}
}
