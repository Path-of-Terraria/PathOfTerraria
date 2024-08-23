using System.Linq;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.ModLoader.Core;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class AddSkillBasicCommand : ModCommand
{
	public override string Command => "addskill_b";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /addskill_b <skillName> <skillSlot> <level>]";

	public override string Description => "Replaces the skill at the given slot with the new skill (short form).";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length != 3)
		{
			caller.Reply("Command expected 3 arguments (<string skillName> <int skillSlot>, <integer level>)", Color.Red);
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
		bool levelValid = int.TryParse(args[2], out int level);

		if (!caller.Player.TryGetModPlayer(out SkillCombatPlayer skillPlayer))
		{
			return;
		}
		
		skillPlayer.Points++;

		if (levelValid)
		{
			skill.LevelTo((byte)Math.Clamp(level, 0, skill.MaxLevel));
			Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>().HotbarSkills[skillSlot] = skill;
		}
		else
		{
			caller.Reply("Argument 2 (level) must be an int!", Color.Red);
		}
	}
}
#endif