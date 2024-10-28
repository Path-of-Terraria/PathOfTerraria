using System.Collections.Generic;
using System.Linq;

namespace PathOfTerraria.Common.Systems;

/// <summary>
/// Helper class for building an item's tooltips easily.
/// </summary>
internal static class ItemTooltipBuilder
{
	/// <summary>
	/// Builds and returns an item's tooltips.
	/// </summary>
	/// <param name="item">Item to use.</param>
	/// <param name="player">Player who's looking at the tooltips.</param>
	/// <returns>The lines for each tooltip.</returns>
	public static List<DrawableTooltipLine> BuildTooltips(Item item, Player player)
	{
		int yoyoLogo = -1;
		int researchLine = -1;
		float knockBack = item.knockBack;
		float knockbackMultiplier = 1f;

		if (item.CountsAsClass(DamageClass.Melee) && player.kbGlove)
		{
			knockbackMultiplier += 1f;
		}

		if (player.kbBuff)
		{
			knockbackMultiplier += 0.5f;
		}

		if (knockbackMultiplier != 1f)
		{
			item.knockBack *= knockbackMultiplier;
		}

		if (item.CountsAsClass(DamageClass.Ranged) && player.shroomiteStealth)
		{
			item.knockBack *= 1f + (1f - player.stealth) * 0.5f;
		}

		int maxLines = 30;
		int lineCount = 1;
		string[] text = new string[maxLines];
		bool[] mod = new bool[maxLines];
		bool[] badMod = new bool[maxLines];

		for (int j = 0; j < maxLines; j++)
		{
			mod[j] = false;
			badMod[j] = false;
		}

		string[] names = new string[maxLines];
		Main.MouseText_DrawItemTooltip_GetLinesInfo(item, ref yoyoLogo, ref researchLine, knockBack, ref lineCount, text, mod, badMod, names, out int prefixlineIndex);
		List<TooltipLine> lines = ItemLoader.ModifyTooltips(item, ref lineCount, names, ref text, ref mod, ref badMod, ref yoyoLogo, out _, prefixlineIndex);
		return lines.Select((TooltipLine x, int i) => new DrawableTooltipLine(x, i, 0, 0, Color.White)).ToList();
	}
}
