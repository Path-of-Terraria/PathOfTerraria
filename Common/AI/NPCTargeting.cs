using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Core.Time;
using Terraria.DataStructures;

#pragma warning disable CA1822 // Mark members as static

namespace PathOfTerraria.Common.AI;

/// <summary> Manages target search and implements "delayed" self-relative target tracking, which allows NPCs to strike at "outdated" positions. </summary>
internal sealed class NPCTargeting : NPCComponent
{
	public struct Ctx(NPC npc)
	{
		public NPC NPC = npc;
		public bool ForceReset = false;
	}

	private uint lastTickCount;

	/// <summary>  Perceived target position vector, relative from the npc's own center. </summary>
	public Vector2 AimVector { get; set; }
	public Vector2 AimLag { get; set; } = default;

	public void ManualUpdate(in Ctx ctx)
	{
		NPC npc = ctx.NPC;

		if (!npc.HasValidTarget || ctx.ForceReset)
		{
			(int logicDir, int spriteDir) = (npc.direction, npc.spriteDirection);
			Terraria.Utilities.NPCUtils.TargetClosestCommon(npc, faceTarget: false);
			(npc.direction, npc.spriteDirection) = (logicDir, spriteDir);
		}
	}

	private bool UpdateTracking(NPC npc)
	{
		if (!Enabled) { return false; }

		uint tickCount = Main.GameUpdateCount;
		float deltaTime = (lastTickCount != 0 ? (tickCount - lastTickCount) : 1) * TimeSystem.LogicDeltaTime;

		NPCAimedTarget target = npc.GetTargetData(false);
		Vector2 relativeTargetCenter = !target.Invalid ? (target.Center - npc.Center) : default;

		AimVector = lastTickCount == 0 ? relativeTargetCenter : new Vector2
		(
			MathUtils.Damp(AimVector.X, relativeTargetCenter.X, AimLag.X, deltaTime),
			MathUtils.Damp(AimVector.Y, relativeTargetCenter.Y, AimLag.Y, deltaTime)
		);
		lastTickCount = tickCount;

		return true;
	}

	public Vector2 GetTargetCenter(NPC npc)
	{
		UpdateTracking(npc);

		Vector2 result = npc.Center + AimVector;
		return result;
	}
}
