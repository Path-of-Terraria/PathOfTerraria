using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Core.Mechanics;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Content.Skills.Melee;

public class Berserk : Skill
{
	public override int MaxLevel => 3;

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = 2;// (60 - 5 * Level) * 60;
		Timer = 0;
		ManaCost = 10 + 5 * level;
		Duration = (15 + 5 * Level) * 60;
		WeaponType = Core.ItemType.Sword;
	}

	public override void UseSkill(Player player)
	{
		if (!CanUseSkill(player))
		{
			return;
		}

		player.statMana -= ManaCost;
		player.AddBuff(ModContent.BuffType<RageBuff>(), Duration);
		Timer = Cooldown;

		SoundEngine.PlaySound(SoundID.DD2_OgreAttack with { Volume = 0.5f, PitchRange = (0.6f, 0.8f)}, player.Center);
	}
}