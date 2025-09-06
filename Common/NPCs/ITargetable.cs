using MonoMod.Cil;

namespace PathOfTerraria.Common.NPCs;

public interface ITargetable
{
	/// <summary> Checks whether this is a valid target for <paramref name="npc"/> and outputs basic targeting info. </summary>
	public bool CanBeTargetedBy(NPC npc, out TargetLoader.FocusPoint focus);
}

public sealed class TargetLoader : ILoadable
{
	public readonly record struct FocusPoint(Rectangle Hitbox, int Priority);

	public void Load(Mod mod)
	{
		//Emit a delegate right after target logic and before final checks
		IL_NPC.SetTargetTrackingValues += static (il) =>
		{
			ILCursor c = new(il);
			if (!c.TryGotoNext(MoveType.Before, x => x.MatchLdfld<NPC>("confused")))
			{
				PoTMod.Instance.Logger.Error("IL edit TargetSystem.SetTargetTrackingValues has failed.");
				return;
			}

			c.EmitLdarg0(); //emit NPC
			c.EmitDelegate(EnhanceTracking);
		};
	}

	public void Unload() { }

	private static void EnhanceTracking(NPC npc)
	{
		if (TryFindPriorityPoint(npc, out FocusPoint pt))
		{
			//Only target this point if 'npc' has no player target or the player's aggro value is lower than that of 'pt'
			if (!npc.HasPlayerTarget || Main.player[npc.target].aggro < pt.Priority)
			{
				npc.targetRect = pt.Hitbox;
				npc.direction = (npc.Center.X > npc.targetRect.Center.X) ? -1 : 1;
				npc.directionY = (npc.Center.Y > npc.targetRect.Center.Y) ? -1 : 1;
			}
		}
	}

	/// <summary> Attempts to find the highest priority item of <see cref="Registered"/>. </summary>
	private static bool TryFindPriorityPoint(NPC npc, out FocusPoint point)
	{
		point = new(Rectangle.Empty, int.MinValue);

		foreach (NPC n in Main.ActiveNPCs) //Check against ITargetable NPCs
		{
			if (n.ModNPC is ITargetable item && item.CanBeTargetedBy(npc, out FocusPoint pt) && pt.Priority > point.Priority)
			{
				point = pt;
			}
		}

		/*foreach (Projectile p in Main.ActiveProjectiles) //Check against ITargetable projectiles //Not necessary yet
		{
			if (p.ModProjectile is ITargetable item && item.CanBeTargetedBy(npc, out FocusPoint pt) && pt.Priority > point.Priority)
			{
				point = pt;
			}
		}*/

		return point.Priority > int.MinValue;
	}
}