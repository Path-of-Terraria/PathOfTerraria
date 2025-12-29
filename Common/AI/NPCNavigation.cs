//#define DEBUG_GIZMOS
//#define DEBUG_LOGGING

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Core.Pathfinding;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.IO;
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
		/// <summary> If set, a path value currently exists. </summary>
		HasPath = 1 << 0,
		/// <summary> If set, a path result is currently pending. </summary>
		WaitingForPath = 1 << 1,
		/// <summary> If set, the last path search has failed. Differs from not having a path. </summary>
		PathNotFound = 1 << 2,
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
		public bool HadPath;
		public bool GoalReached;
		public bool GoalJustReached;
	}

	private static bool CanPathfind => Main.netMode != NetmodeID.MultiplayerClient;

	private PathResult? path;
	private PathHandle? pathfinding;
	private uint lastPathRequestTick;
	private uint lastPathAdvanceTick;
	private Vector2 targetPosition;
	private Vector2 lastPathSourcePos;
	private Vector2 lastPathTargetPos;

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

	/// <summary> Serializes navigation information. </summary>
	public void SendPath(NPC npc, BinaryWriter writer)
	{
		_ = npc;

		// There must not be any desync.
		Debug.Assert((path != null) == StateFlags.HasFlag(StateFlag.HasPath));

		writer.Write((byte)StateFlags);
		if (StateFlags.HasFlag(StateFlag.HasPath))
		{
			WayfarerAPI.WriteResultTo(path, writer);
			writer.Write7BitEncodedInt(path!.Index);
		}
	}
	/// <summary> Deserializes navigation information. </summary>
	public void ReceivePath(NPC npc, BinaryReader reader)
	{
		_ = npc;

		StateFlags = (StateFlag)reader.ReadByte();

		if (StateFlags.HasFlag(StateFlag.HasPath))
		{
			path = WayfarerAPI.ReadResultFrom(reader);

			for (int idx = reader.Read7BitEncodedInt(); path.Index < idx;)
			{
				path.Advance(out _);
			}
		}
		else
		{
			path = null;
		}
	}

	private bool EnsureReady(NPC npc)
	{
		if (pathfinding != null)
		{
			return true;
		}

		if (!CanPathfind)
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

		const uint GlobalRepathingDelay = 15;
		const uint InEdgeRepathingDelay = 30;
		const float MinTargetMove = 32f;
		const float MinSelfMove = 64f;

		uint tick = Main.GameUpdateCount;

		if (lastPathRequestTick != 0
		&& ((tick - lastPathRequestTick) < GlobalRepathingDelay || (tick - lastPathAdvanceTick) < InEdgeRepathingDelay))
		{
			return;
		}

		// Halt if no one has moved.
		if (Vector2.DistanceSquared(npc.Center, lastPathSourcePos) <= (MinSelfMove * MinSelfMove)
		&& Vector2.DistanceSquared(ctx.TargetPosition, lastPathTargetPos) <= (MinTargetMove * MinTargetMove))
		{
			return;
		}

		if (BehaviorFlags.HasFlag(BehaviorFlag.CheckGroundBeforeRepath) && npc.velocity.Y != 0f)
		{
			return;
		}

		if (!EnsureReady(npc))
		{
			return;
		}

		if (!CanPathfind)
		{
			StateFlags |= StateFlag.WaitingForPath;
			return;
		}

		targetPosition = ctx.TargetPosition;

		var bottomL = (Point)(npc.BottomLeft + new Vector2(-16f, -16f)).ToTileCoordinates();
		var bottomR = (Point)(npc.BottomRight + new Vector2(+16f, -16f)).ToTileCoordinates();
		var footingTiles = new Point[bottomR.X - bottomL.X + 1];

		for (int i = 0, x = bottomL.X; x <= bottomR.X; x++, i++)
		{
			for (int y = bottomL.Y, yEnd = Math.Min(Main.maxTilesY, bottomL.Y + 4); y < yEnd; y++)
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
		lastPathSourcePos = npc.Center;
		lastPathTargetPos = targetPosition;

		WayfarerAPI.RecalculateNavMesh(pathfinding!.WfHandle, npc.Center.ToTileCoordinates());
		WayfarerAPI.RecalculatePath(pathfinding!.WfHandle, footingTiles, r => OnPathFound(npc, r));
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

		//if (!BehaviorFlags.HasFlag(BehaviorFlag.CheckGroundBeforeRepath) || npc.velocity.Y == 0f)
		{
			path = result;
		}

		StateFlags = (path != null) ? (StateFlags | StateFlag.HasPath) : (StateFlags & ~StateFlag.HasPath);
		StateFlags &= ~StateFlag.WaitingForPath;
		npc.netUpdate = true;
	}

	private void Movement(in Context ctx, ref Result result)
	{
		NPC npc = ctx.NPC;
		uint tick = Main.GameUpdateCount;
		(float Src, float Dst)? horizontalMovement = null;

		// DirMul is a Left/Up/Right/Down directional vector difference multiplier.
		// It is used to make axes matter less in distance checks.
		// It is also used to make overshoots in specific directions count as the target being reached.
		bool CheckDestination(ref Result result, Vector2 src, Vector2 dst, Vector4 dirMul)
		{
			float closeEnoughDistance = npc.width * 0.25f;
			float closeEnoughSqrDistance = closeEnoughDistance * closeEnoughDistance;
			Vector2 dstDiff = dst - src;
			Vector2 diffMul = new(dstDiff.X >= 0f ? dirMul.Z : dirMul.X, dstDiff.Y >= 0f ? dirMul.W : dirMul.Y);
			float sqrDist = (dstDiff * diffMul).LengthSquared();
			bool hasAdvanced = false;

			if (sqrDist <= closeEnoughSqrDistance && !path!.IsAlreadyAtGoal)
			{
				path.Advance(out bool atGoal);
				result.GoalJustReached = atGoal;
				lastPathAdvanceTick = tick;
				hasAdvanced = true;
			}

			result.GoalReached = result.GoalJustReached || path!.IsAlreadyAtGoal;
			return hasAdvanced;
		}

		while (true)
		{
			if (path is not { HasPath: true, Current: PathEdge edge })
			{
				break;
			}

			result.HadPath = true;

			int signSrcToDst = edge.To.X >= edge.From.X ? 1 : -1;
			(Vector2 Src, Vector2 Dst, Vector4 DirMul) destination;

			// Setup destination points.
			if (edge.Is<Walk>())
			{
				destination = (npc.Bottom, edge.To.ToWorldCoordinates(8, 0), new(1.0f, 0.25f, 1.0f, 0.25f));
			}
			else if (edge.Is<Fall>())
			{
				destination = ((signSrcToDst > 0 ? npc.BottomLeft : npc.BottomRight), edge.To.ToWorldCoordinates((signSrcToDst > 0 ? 16 : 0), 0), new(0.0f, 1.0f, 0.0f, 1.0f));
				//destination = ((signSrcToDst > 0 ? npc.BottomLeft : npc.BottomRight), edge.To.ToWorldCoordinates(8, 0), new(0.0f, 1.0f, 0.0f, 1.0f));
			}
			else if (edge.Is<Jump>())
			{
				destination = ((signSrcToDst > 0 ? npc.BottomRight : npc.BottomLeft), edge.To.ToWorldCoordinates(8, 0), new(1.0f, 1.0f, 1.0f, 1.0f));
			}
			else
			{
				break;
			}

			// If the next edge is continued movement into the same direction,
			// then prevent backtracking from occurring during overshooting.
			if (path.Next is { } next)
			{
				int nextSrcToDst = next.To.X >= next.From.X ? 1 : -1;
				int destinationSign = destination.Dst.X >= destination.Src.X ? 1 : -1;

				if (signSrcToDst == nextSrcToDst && destinationSign != signSrcToDst)
				{
					destination.DirMul *= (Vector4)(signSrcToDst == 1 ? new(0f, 1f, 1f, 1f) : new(1f, 1f, 0f, 1f));
				}
			}

			// Advance if goal is reached.
			if (CheckDestination(ref result, destination.Src, destination.Dst, destination.DirMul))
			{
				// Netsync this!
				npc.netUpdate = true;

				// Look at the next edge, do not apply any movement for the one we advanced from.
				continue;
			}

#if DEBUG && DEBUG_LOGGING
			Main.NewText($"edge type: {edge.EdgeType switch { 0 => "walk", 1 => "fall", 2 => "jump", _ => "idk" }}\nvelY: {npc.velocity.Y:0.00}");
#endif

			NPCID.Sets.TrailingMode[npc.type] = 1;

			// Apply behavior.
			if (edge.Is<Walk>())
			{
				horizontalMovement = (destination.Src.X, destination.Dst.X);
			}
			else if (edge.Is<Fall>())
			{
				// Move horizontally until we fall off the initial elevation.
				if ((npc.Bottom.Y) <= destination.Src.Y)
				{
					horizontalMovement = (destination.Src.X, signSrcToDst > 0 ? float.PositiveInfinity : float.NegativeInfinity);
				}
				else
				{
					horizontalMovement = (destination.Src.X, destination.Dst.X);
				}

				result.FallThroughPlatforms = true;
			}
			else if (edge.Is<Jump>())
			{
				if (npc.velocity.Y == 0f || npc.oldVelocity.Y == 0f)
				{
					Vector2 src = (signSrcToDst > 0 ? npc.BottomLeft : npc.BottomRight);
					Vector2 dst = edge.From.ToWorldCoordinates(8, 0); //(signSrcToDst > 0 ? 1 : 15)

					bool atJumpPoint = MathF.Abs(dst.X - src.X) <= 3f;
					bool mayBeStuck = npc.velocity == default && npc.collideX && npc.collideY && npc.oldPos.All(p => p == npc.position);

#if DEBUG_LOGGING
					if (mayBeStuck && !atJumpPoint)
					{
						Main.NewText("Stuck! Jumping!", Color.Aqua);
					}
#endif

					if (atJumpPoint || mayBeStuck)
					{
						npc.direction = signSrcToDst;

						Vector2 edgeJump = WayfarerPresets.DefaultJumpFunction(edge.From.ToWorldCoordinates(), edge.To.ToWorldCoordinates(), () => npc.gravity);
						//Vector2 bodyJump = WayfarerPresets.DefaultJumpFunction(src, edge.To.ToWorldCoordinates(), () => npc.gravity);
						//result.JumpVector = new Vector2(
						//	MathUtils.MaxAbs(edgeJump.X, bodyJump.X),
						//	Math.Min(edgeJump.Y, bodyJump.Y)
						//);
						result.JumpVector = edgeJump;
					}
					else
					{
						horizontalMovement = (src.X, dst.X);
					}
				}
				
				if (npc.velocity.Y != 0f)
				{
					// Force a bit of horizontal velocity to climb ledges.
					const float XSpeed = 0.2f;
					npc.velocity.X = signSrcToDst > 0 ? Math.Max(+XSpeed, npc.velocity.X) : Math.Min(-XSpeed, npc.velocity.X);
				}
			}

			break;
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
