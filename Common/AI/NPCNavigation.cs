//#define DEBUG_GIZMOS
//#define DEBUG_LOGGING

using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Core.Pathfinding;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using Wayfarer.API;
using Wayfarer.Data;
using Wayfarer.Edges;
using Wayfarer.Pathfinding;

#nullable enable
#pragma warning disable CA1069 // Enums values should not be duplicated

namespace PathOfTerraria.Common.AI;

/// <summary> Handles path searching and path following for NPCs. </summary>
internal sealed class NPCNavigation : NPCComponent
{
	[Flags]
	public enum BehaviorFlag : byte
	{
		Default = CheckGroundBeforeRepath,
		/// <summary> If set, repathing will only occur while the NPC is on the ground. </summary>
		CheckGroundBeforeRepath = 1 << 0,
	}

	[Flags]
	public enum StateFlag : byte
	{
		/// <summary> If set, a path result is currently pending. </summary>
		WaitingForPath = 1 << 0,
		/// <summary> If set, the last path search has failed. </summary>
		PathNotFound = 2 << 0,
	}

	public record struct Context
	{
		/// <summary> The NPC instance. </summary>
		public required NPC NPC;
		/// <summary> Where does this NPC aim to navigate. </summary>
		public required Vector2 TargetPosition;
	}

	public record struct Result
	{
		public Vector2 MovementVector;
		public Vector2 JumpVector;
		public bool FallThroughPlatforms;
		public bool HasPath;
		public bool GoalReached;
		public bool GoalJustReached;
	}

	private PathResult? path;
	private PathHandle? pathfinding;
	private uint lastPathRequestTick;
	private uint lastPathAdvanceTick;
	private Vector2 targetPosition;

	/// <summary> Possible jump range in tiles. </summary>
	public Vector2Int JumpRange { get; set; } = new(10, 10);
	/// <summary> Radius in tiles to compute paths in. </summary>
	public int SearchRadius { get; set; } = 100;
	/// <summary> Configuration flags. </summary>
	public BehaviorFlag BehaviorFlags { get; set; } = BehaviorFlag.Default;

	/// <summary> Path search state. </summary>
	public StateFlag StateFlags { get; private set; }
	/// <summary> A copy of the last result's <see cref="Result.FallThroughPlatforms"/>. </summary>
	public bool FallThroughPlatforms { get; private set; }

	public override void OnSpawn(NPC npc, IEntitySource source)
	{
		if (!Enabled) { return; }

		EnsureReady(npc);
	}

	public override void OnKill(NPC npc)
	{
		pathfinding?.Dispose();
		pathfinding = null;
	}

	private bool EnsureReady(NPC npc)
	{
		if (pathfinding != null)
		{
			return true;
		}

		int npcIndex = npc.whoAmI;

		float GetGravity()
		{
			return Main.npc[npcIndex] is { active: true } n ? n.gravity : 0f;
		}

		Rectangle hitbox = npc.Hitbox; //new Rectangle((int)npc.position.X - 8, (int)npc.position.Y - 32, npc.width + 16, npc.height + 32);
		var navMeshParameters = new NavMeshParameters(npc.Center.ToTileCoordinates(), 100, WayfarerPresets.DefaultIsTileValid);
		var navigatorParameters = new NavigatorParameters(hitbox, WayfarerPresets.DefaultJumpFunction, JumpRange, GetGravity, SelectDestination);

		Pathfinding.Attempt(navMeshParameters, navigatorParameters, out pathfinding);

		return pathfinding != null;
	}

	public void Process(out Result result, in Context ctx)
	{
		result = new();

		Navigation(in ctx, ref result);
		Movement(in ctx, ref result);

		FallThroughPlatforms = result.FallThroughPlatforms;
	}

	private void Navigation(in Context ctx, ref Result result)
	{
		NPC npc = ctx.NPC;

		const uint GlobalRepathingDelay = 30;
		const uint InEdgeRepathingDelay = 30;

		uint tick = Main.GameUpdateCount;

		if (path != null
		&& lastPathRequestTick != 0
		&& ((tick - lastPathRequestTick) < GlobalRepathingDelay || (tick - lastPathAdvanceTick) < InEdgeRepathingDelay))
		{
			return;
		}

		if (BehaviorFlags.HasFlag(BehaviorFlag.CheckGroundBeforeRepath) && npc.velocity.Y != 0f)
		{
			return;
		}

		if (!EnsureReady(npc) || pathfinding == null)
		{
			return;
		}

		targetPosition = ctx.TargetPosition;

		var bottomL = (Point)(npc.BottomLeft + new Vector2(-16f, -16f)).ToTileCoordinates();
		var bottomR = (Point)(npc.BottomRight + new Vector2(+16f, -16f)).ToTileCoordinates();
		var footingTiles = new Point[bottomR.X - bottomL.X + 1];

		for (int i = 0, x = bottomL.X; x <= bottomR.X; x++, i++)
		{
			for (int y = bottomL.Y, yEnd = Math.Min(Main.maxTilesY, bottomL.Y + 3); y < yEnd; y++)
			{
				Tile tile = Main.tile[x, y];
				bool isValid = tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]);

#if DEBUG && DEBUG_GIZMOS
				int xCopy = x;
				int yCopy = y;
				Core.Debugging.DebugUtils.DrawInWorld(sb =>
				{
					sb.DrawString(FontAssets.MouseText.Value, isValid ? "x" : "o", new Vector2(xCopy * 16, yCopy * 16) - Main.screenPosition, Color.White);
				});
#endif

				if (isValid)
				{
					footingTiles[i] = new Point(x, y);
					break;
				}
			}
		}

		StateFlags |= StateFlag.WaitingForPath;
		lastPathRequestTick = tick;

		WayfarerAPI.RecalculateNavMesh(pathfinding.WfHandle, npc.Center.ToTileCoordinates());
		WayfarerAPI.RecalculatePath(pathfinding.WfHandle, footingTiles, r => OnPathFound(npc, r));
	}

	private Point SelectDestination(IReadOnlySet<Point> points)
	{
		(Point Point, float SqrDst) closest = (points.First(), float.PositiveInfinity);

		foreach (Point point in points)
		{
			if (point.ToWorldCoordinates().DistanceSQ(targetPosition) is { } sqrDst && sqrDst < closest.SqrDst)
			{
				closest = (point, sqrDst);
			}
		}

		return closest.Point;
	}

	private void OnPathFound(NPC npc, PathResult result)
	{
		if (result == null)
		{
			StateFlags |= StateFlag.PathNotFound;
		}
		else
		{
			StateFlags &= ~StateFlag.PathNotFound;
		}

		if (!BehaviorFlags.HasFlag(BehaviorFlag.CheckGroundBeforeRepath) || npc.velocity.Y == 0f)
		{
			path = result;
		}

		StateFlags &= ~StateFlag.WaitingForPath;
	}

	private void Movement(in Context ctx, ref Result result)
	{
		NPC npc = ctx.NPC;
		uint tick = Main.GameUpdateCount;

		if (path is not { HasPath: true })
		{
			return;
		}

		result.HasPath = true;

		PathEdge edge = path.Current;
		Vector2 srcCenter = edge.From.ToWorldCoordinates();
		Vector2 dstCenter = edge.To.ToWorldCoordinates();
		int signSrcToDst = dstCenter.X >= srcCenter.X ? 1 : -1;
		(float Src, float Dst)? horizontalMovement = null;
		(Vector2 Src, Vector2 Dst, Vector2 Axes) destination;

#if DEBUG && DEBUG_LOGGING
		Main.NewText($"edge type: {edge.EdgeType switch { 0 => "walk", 1 => "fall", 2 => "jump", _ => "idk" }}");
#endif

		if (edge.Is<Walk>())
		{
			destination = (npc.Bottom, edge.To.ToWorldCoordinates(8, 0), new(1.0f, 0.0f));
			horizontalMovement = (destination.Src.X, destination.Dst.X);
		}
		else if (edge.Is<Fall>())
		{
			destination = ((signSrcToDst > 0 ? npc.BottomLeft : npc.BottomRight), edge.To.ToWorldCoordinates((signSrcToDst > 0 ? 16 : 0), 0), new(0.0f, 1.0f));
			horizontalMovement = (destination.Src.X, destination.Dst.X);
			result.FallThroughPlatforms = true;
		}
		else if (edge.Is<Jump>())
		{
			destination = ((signSrcToDst > 0 ? npc.BottomRight : npc.BottomLeft), edge.To.ToWorldCoordinates(8, 0), new(1.0f, 1.0f));

			if (npc.velocity.Y == 0f)
			{
				float srcX = (signSrcToDst > 0 ? npc.BottomRight : npc.BottomLeft).X;
				float dstX = edge.From.ToWorldCoordinates(8, 0).X;

				if (MathF.Abs(dstX - srcX) <= 5f)
				{
					npc.direction = signSrcToDst;

					// Pick the strongest of the two computed forces. Weird, yes.
					Vector2 jumpA = WayfarerPresets.DefaultJumpFunction(npc.Bottom, edge.To.ToWorldCoordinates(), () => npc.gravity);
					Vector2 jumpB = WayfarerPresets.DefaultJumpFunction(edge.From.ToWorldCoordinates(), edge.To.ToWorldCoordinates(), () => npc.gravity);
					result.JumpVector = jumpB;
					//result.JumpVector = new Vector2(MathUtils.MaxAbs(jumpA.X, jumpB.X), MathUtils.MaxAbs(jumpA.Y, jumpB.Y));
				}
				else
				{
					horizontalMovement = (srcX, dstX);
				}
			}
			else
			{
				// Force a bit of horizontal velocity to climb ledges.
				const float XSpeed = 0.1f;
				npc.velocity.X = signSrcToDst > 0 ? Math.Max(+XSpeed, npc.velocity.X) : Math.Min(-XSpeed, npc.velocity.X);
			}
		}
		else
		{
			return;
		}

		// Horizontal movement.
		if (horizontalMovement is { } move)
		{
			int dir = move.Dst >= move.Src ? 1 : -1;
			result.MovementVector.X = dir;

#if DEBUG_GIZMOS
			Core.Debugging.DebugUtils.DrawInWorld(sb =>
			{
				sb.DrawString(FontAssets.MouseText.Value, "|", new Vector2(move.Src, npc.Bottom.Y - 6) - Main.screenPosition, Color.Lerp(Color.White, Color.Red, 0.75f));
				sb.DrawString(FontAssets.MouseText.Value, "|", new Vector2(move.Dst, npc.Bottom.Y + 6) - Main.screenPosition, Color.Lerp(Color.White, Color.GreenYellow, 0.75f));
			});
#endif
		}

		// Advance if goal is reached.
		float closeEnoughDistance = npc.width * 0.66f;
		float closeEnoughSqrDistance = closeEnoughDistance * closeEnoughDistance;

		float sqrDist = ((destination.Dst - destination.Src) * destination.Axes).LengthSquared();
		if (sqrDist <= closeEnoughSqrDistance && !path.IsAlreadyAtGoal)
		{
			path.Advance(out bool atGoal);
			result.GoalJustReached = atGoal;
			lastPathAdvanceTick = tick;
		}

		result.GoalReached = result.GoalJustReached || path.IsAlreadyAtGoal;
	}

	public override void PostDraw(NPC npc, SpriteBatch sb, Vector2 screenPos, Color color)
	{
#if DEBUG && DEBUG_GIZMOS
		if (path is { HasPath: true })
		{
			WayfarerAPI.DebugRenderPath(pathfinding!.WfHandle, sb, path);
		}
#endif
	}
}
