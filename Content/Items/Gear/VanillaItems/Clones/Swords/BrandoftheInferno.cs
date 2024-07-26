using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class BrandoftheInferno : VanillaClone
{
	protected override short VanillaItemId => ItemID.DD2SquireDemonSword;
	public override void Load()
	{
		IL_Player.ItemCheck_ManageRightClickFeatures_ShieldRaise += AddModdedShieldRaiseItem;
	}

	private void AddModdedShieldRaiseItem(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(x => x.MatchLdfld<Player>(nameof(Player.shield_parry_cooldown))))
		{
			return;
		}

		c.Emit(OpCodes.Ldarg_S, (byte)0);
		c.Emit(OpCodes.Ldarg_S, (byte)1);
		c.Emit(OpCodes.Ldloc_S, (byte)0);
		c.Emit(OpCodes.Ldloca_S, (byte)1);
		c.EmitDelegate(ModifyCanParry);
	}
	
	public static void ModifyCanParry(Player self, bool theGeneralCheck, bool mouseRight, ref bool shouldGuard)
	{
		int id = ModContent.ItemType<BrandoftheInferno>();

		if (theGeneralCheck && self.HeldItem.type == id && self.hasRaisableShield && !self.mount.Active && (self.itemAnimation == 0 || mouseRight))
		{
			shouldGuard = true;
		}
	}

	public override void SetDefaults()
	{
		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Core.ItemType.Melee;
	}

	public override bool AltFunctionUse(Player player)
	{
		return false;
	}

	public override void MeleeEffects(Player player, Rectangle hitbox)
	{
		Rectangle rect = SwordCommon.GetItemRectangle(player, Item);

		var dust = Dust.NewDustDirect(rect.TopLeft(), rect.Width, rect.Height, DustID.Torch, player.velocity.X * 0.2f + player.direction * 3, player.velocity.Y * 0.2f, 
			100, Color.Transparent, 0.7f);
		dust.noGravity = true;
		dust.velocity *= 2f;
		dust.fadeIn = 0.9f;
	}

	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (Main.rand.NextBool(4))
		{
			target.AddBuff(BuffID.OnFire3, 300);
		}
	}

	public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
	{
		if (Main.rand.NextBool(4))
		{
			target.AddBuff(BuffID.OnFire3, 300);
		}
	}
}