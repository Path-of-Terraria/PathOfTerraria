using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Utilities.Terraria;

#pragma warning disable IDE2003 // Blank line required between block and subsequent statement

namespace PathOfTerraria.Common.AI;

/// <summary> Pushes entities out of NPCs, or vice-versa. </summary>
internal struct PushBehavior
{
	public ref struct Context(NPC npc)
	{
		public Vector2 Center = npc.Center;
		public ref Vector2 Velocity = ref npc.velocity;
		public int Direction = npc.direction;
		public (float Closest, float Farthest, float TargetSpeed, float Acceleration) Push = (6f, 32f, 2f, 0.5f);
		public int RequiredNpcType = -1;
		public int IgnoredNpcIndex = npc.whoAmI;
		public int IgnoredNpcType = -1;
		public bool ApplyOnlyOnGround = true;
		//public bool PushOthersRatherThanSelf = false;
	}

	public static void Update(in Context ctx)
	{
		//if (!ctx.PushOthersRatherThanSelf && ctx.ApplyOnlyOnGround && ctx.Velocity.Y != 0f) { return; }

		float minSqrDst = ctx.Push.Farthest * ctx.Push.Farthest;

		if (minSqrDst <= 0f) { return; }

		foreach (NPC other in Main.ActiveNPCs)
		{
			if (other.whoAmI == ctx.IgnoredNpcIndex || other.type == ctx.IgnoredNpcType) { continue; }
			if ((ctx.RequiredNpcType >= 0 && other.type != ctx.RequiredNpcType)) { continue; }
			//if (ctx.PushOthersRatherThanSelf && ctx.ApplyOnlyOnGround && other.velocity.Y != 0f) { continue; }

			Vector2 otherCenter = other.Center;
			Vector2 otherDiff = ctx.Center - otherCenter;

			float sqrDst = otherDiff.LengthSquared();
			if (sqrDst > minSqrDst) { continue; }
			float dst = MathF.Sqrt(sqrDst);

			float power = MathUtils.DistancePower(dst, ctx.Push.Closest, ctx.Push.Farthest);
			Vector2 direction = dst > 0f ? (otherDiff / dst) : (-Vector2.UnitX * ctx.Direction);

			ref Vector2 affectedVel = ref ctx.Velocity;

			//if (ctx.PushOthersRatherThanSelf)
			//{
			//	affectedVel = ref other.velocity;
			//	direction *= -1;
			//}

			affectedVel = MovementUtils.DirAccelQ(affectedVel, direction, ctx.Push.TargetSpeed, ctx.Push.Acceleration);
		}
	}
}
