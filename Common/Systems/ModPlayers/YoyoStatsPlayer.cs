using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ModPlayers;

public sealed class YoyoStatsPlayer : ModPlayer
{
	public StatModifier YoyoRange = StatModifier.Default;
	public StatModifier YoyoSpeed = StatModifier.Default;
	public StatModifier YoyoLifeTime = StatModifier.Default;

	public override void ResetEffects()
	{
		YoyoRange = StatModifier.Default;
		YoyoSpeed = StatModifier.Default;
		YoyoLifeTime = StatModifier.Default;
	}

	public override void Load()
	{
		IL_Projectile.AI_099_2 += InjectYoyoStats;
	}

	private static void InjectYoyoStats(ILContext il)
	{
		ILCursor c = new(il);

		c.GotoNext(
			MoveType.After,
			x => x.MatchLdsfld(typeof(ProjectileID.Sets), nameof(ProjectileID.Sets.YoyosLifeTimeMultiplier))
		);

		c.Index += 4;

		c.EmitLdarg0(); //Projectile
		c.Emit(OpCodes.Ldloca_S, (byte)8); //LifeTime

		c.EmitDelegate(ModifyLifeTime);

		c.GotoNext(
			MoveType.After,
			x => x.MatchLdsfld(typeof(ProjectileID.Sets), nameof(ProjectileID.Sets.YoyosTopSpeed))
		);

		c.Index += 4;

		c.EmitLdarg0(); //Projectile
		c.Emit(OpCodes.Ldloca_S, (byte)5); //Range
		c.Emit(OpCodes.Ldloca_S, (byte)3); //Speed

		c.EmitDelegate(ModifyStats);

		MonoModHooks.DumpIL(PoTMod.Instance, il);
	}

	private static void ModifyLifeTime(Projectile projectile, ref float lifeTime)
	{
		Player owner = Main.player[projectile.owner];
		if (owner.TryGetModPlayer(out YoyoStatsPlayer modPlayer))
		{
			lifeTime = modPlayer.YoyoLifeTime.ApplyTo(lifeTime);
		}
	}

	private static void ModifyStats(Projectile projectile, ref float range, ref float speed)
	{
		Player owner = Main.player[projectile.owner];
		if (owner.TryGetModPlayer(out YoyoStatsPlayer modPlayer))
		{
			range = modPlayer.YoyoRange.ApplyTo(range);
			speed = modPlayer.YoyoSpeed.ApplyTo(speed);
		}
	}
}