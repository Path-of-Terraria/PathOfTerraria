using System.Linq;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.ModLoader.Core;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class AddSkillCommand : ModCommand
{
	public override string Command => "addskill";

	public override CommandType Type => CommandType.Chat;

	public override string Usage
		=> "[c/ff6a00:Usage: /addskill <skillName> <skillSlot> <duration> <timer> <maxCooldown> <coolDown> <manaCost> <weaponType> <level>]";

	public override string Description => "Replaces the skill at the given slot with the new skill.";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length < 2)
		{
			caller.Reply("Command expected 2 arguments (<string skillName> <int skillSlot>)", Color.Red);
			return;
		}

		Type[] asmTypes = AssemblyManager.GetLoadableTypes(Mod.Code);
		Type skillType =
			asmTypes.FirstOrDefault(x => typeof(Skill).IsAssignableFrom(x) && x.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));

		if (skillType == null)
		{
			caller.Reply($"Argument 0 (skillname) must correspond to a Skill name! (No Skill of name {args[0]} found.)", Color.Red);
			return;
		}

		if (!int.TryParse(args[1], out int skillSlot) || skillSlot < 0 || skillSlot > 2)
		{
			caller.Reply("Argument 1 (skillSlot) must be an int between 0 and 2, inclusive!", Color.Red);
			return;
		}

		var skill = Skill.GetAndPrepareSkill(skillType);

		if (args.Length == 2)
		{
			Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>().HotbarSkills[skillSlot] = skill;
			return;
		}

		bool durationValid = int.TryParse(args[2], out int duration);

		if (durationValid)
		{
			skill.Duration = duration;
		}

		if (SetSkillOrComplain(caller, args, skillSlot, skill, durationValid, 3, 2, "duration"))
		{
			return;
		}

		bool timerValid = int.TryParse(args[3], out int timer);

		if (timerValid)
		{
			skill.Timer = timer;
		}

		if (SetSkillOrComplain(caller, args, skillSlot, skill, timerValid, 4, 3, "timer"))
		{
			return;
		}

		bool maxCooldownValid = int.TryParse(args[4], out int maxCooldown);

		if (maxCooldownValid)
		{
			skill.MaxCooldown = maxCooldown;
		}

		if (SetSkillOrComplain(caller, args, skillSlot, skill, maxCooldownValid, 5, 4, "maxCooldown"))
		{
			return;
		}

		bool cooldownValid = int.TryParse(args[5], out int cooldown);

		if (cooldownValid)
		{
			skill.Cooldown = cooldown;
		}

		if (SetSkillOrComplain(caller, args, skillSlot, skill, cooldownValid, 6, 5, "cooldown"))
		{
			return;
		}

		bool manaCostValid = int.TryParse(args[6], out int manaCost);

		if (manaCostValid)
		{
			skill.ManaCost = manaCost;
		}

		if (SetSkillOrComplain(caller, args, skillSlot, skill, manaCostValid, 7, 6, "manaCost"))
		{
			return;
		}

		bool weaponTypeValid = Enum.TryParse(args[7], out ItemType weaponType);

		if (weaponTypeValid)
		{
			skill.WeaponType = weaponType;
		}

		if (SetSkillOrComplain(caller, args, skillSlot, skill, weaponTypeValid, 8, 7, "weaponType"))
		{
			return;
		}

		bool levelValid = int.TryParse(args[7], out int level);

		if (levelValid)
		{
			skill.Level = (byte)level;
		}
	}

	private static bool SetSkillOrComplain(
		CommandCaller caller,
		string[] args,
		int skillSlot,
		Skill skill,
		bool valid,
		int argCount,
		int slot,
		string name
	)
	{
		if (args.Length != argCount)
		{
			return false;
		}

		if (valid)
		{
			Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>().HotbarSkills[skillSlot] = skill;
			return true;
		}

		caller.Reply($"Argument {slot} ({name}) must be int!", Color.Red);
		return true;
	}
}
#endif