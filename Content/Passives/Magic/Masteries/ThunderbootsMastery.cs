using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Skills.Magic;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class ThunderbootsMastery : Passive
{
	private static int lastJump = -1;

	public override void OnLoad()
	{
		IL_Player.JumpMovement += AddOnJumpHook;
	}

	private void AddOnJumpHook(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(x => x.MatchLdsfld<Player>(nameof(Player.jumpHeight)), x => x.MatchStfld<Player>(nameof(Player.jump))))
		{
			return;
		}

		// `this` is always arg[0]...don't think I need to obtain safely?
		c.Emit(OpCodes.Ldarg_0);
		c.EmitDelegate((Player player) =>
		{
			lastJump = player.jump;
		});

		c.Index++;

		c.Emit(OpCodes.Ldarg_0);
		c.EmitDelegate(OnJump);
	}

	private static void OnJump(Player player)
	{
		if (lastJump != player.jump || Main.myPlayer != player.whoAmI || !player.GetModPlayer<PassiveTreePlayer>().HasNode<ThunderbootsMastery>())
		{
			return;
		}

		int projType = ModContent.ProjectileType<Nova.NovaProjectile>();
		int proj = Projectile.NewProjectile(player.GetSource_Misc("Jump"), player.Center, Vector2.Zero, projType, 10, 2, player.whoAmI, (int)Nova.NovaType.Lightning);
		Projectile projectile = Main.projectile[proj];
		projectile.timeLeft = 7;
		projectile.netUpdate = true;
		(projectile.ModProjectile as Nova.NovaProjectile).FullShock = true;
		ElementalContainer container = projectile.GetGlobalProjectile<ElementalProjectile>().Container;
		container[ElementType.Lightning].DamageModifier.AddModifiers(0, 1);
	}

	public override void BuffPlayer(Player player)
	{
		if (player.statMana <= player.statManaMax2 * 0.5f)
		{
			player.GetDamage(DamageClass.Generic) += 0.12f;
		}
	}
}
