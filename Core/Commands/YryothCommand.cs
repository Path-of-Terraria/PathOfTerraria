#if DEBUG
using PathOfTerraria.Content.Conflux;

#nullable enable

namespace PathOfTerraria.Core.Commands;

public sealed class YryothCommand : ModCommand
{
	public override string Command => "yryoth";
	public override CommandType Type => CommandType.World;
	public override string Usage => "[c/ff6a00:Usage: /yryoth <spawn|first|second|death>]";
	public override string Description => "Spawns Yryoth or jumps the active fight to a test phase";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		string action = args.Length > 0 ? args[0].ToLowerInvariant() : "spawn";
		GlacialBoss? boss = null;
		foreach (NPC activeNpc in Main.ActiveNPCs)
		{
			if (activeNpc.ModNPC is GlacialBoss foundBoss)
			{
				boss = foundBoss;
				break;
			}
		}

		if (action == "spawn")
		{
			if (boss is not null)
			{
				caller.Reply("Yryoth is already active.", Color.Orange);
				return;
			}

			NPC npc = NPC.NewNPCDirect(caller.Player.GetSource_Misc("YryothDebug"),
				(int)caller.Player.Center.X, (int)caller.Player.Center.Y - 400, ModContent.NPCType<GlacialBoss>());
			npc.netUpdate = true;
			caller.Reply("Spawned Yryoth.", Color.Cyan);
			return;
		}

		if (boss is null)
		{
			caller.Reply("No active Yryoth was found.", Color.OrangeRed);
			return;
		}

		switch (action)
		{
			case "first":
				boss.DebugSetPhase(GlacialBoss.PhaseType.First);
				break;
			case "second":
				boss.DebugSetPhase(GlacialBoss.PhaseType.Second);
				break;
			case "death":
				boss.DebugBeginDeath();
				break;
			default:
				caller.Reply(Usage, Color.OrangeRed);
				return;
		}

		caller.Reply($"Yryoth debug action '{action}' applied.", Color.Cyan);
	}
}
#endif
