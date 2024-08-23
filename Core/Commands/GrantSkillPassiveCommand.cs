using System.Linq;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.ModLoader.Core;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class GrnatSkillPassiveCommand : ModCommand
{
	public override string Command => "grantskillpassive";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /grantskillpassive <skillName>]";

	public override string Description => "Grants a skill passive points to a player";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length != 1)
		{
			caller.Reply("Command expected 1 arguments (<string skillName>)", Color.Red);
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

		var skill = Skill.GetAndPrepareSkill(skillType);
		if (!caller.Player.TryGetModPlayer(out SkillPassivePlayer skillPassivePlayer))
		{
			return;
		}
		
		skillPassivePlayer.AwardPassivePoint(skill);
		caller.Reply($"Granted skill point for {skill.Name}");
	}
}
#endif