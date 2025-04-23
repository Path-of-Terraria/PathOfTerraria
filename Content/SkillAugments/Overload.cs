using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using Terraria.Localization;

namespace PathOfTerraria.Content.SkillAugments;

internal class Overload : SkillAugment
{
	public const float ManaMult = 1.25f;
	public const float DamageMult = 1.25f;

	public override string Tooltip
	{
		get
		{
			string tooltip = Language.GetTextValue("Mods.PathOfTerraria.SkillAugments." + Name + ".Tooltip").FormatWith((int)((ManaMult - 1) * 100), (int)((DamageMult - 1) * 100));
			return tooltip;
		}
	}

	public override void AugmentEffects(ref SkillBuff buff)
	{
		buff.ManaCost *= ManaMult;
		buff.Damage *= DamageMult;
	}
}