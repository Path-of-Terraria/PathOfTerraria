using Terraria.DataStructures;

namespace PathOfTerraria.Content.Buffs;

public sealed class BattleAxeBuff : ModBuff
{
	private sealed class BattleAxeGlobalNPCImpl : GlobalNPC
	{
		public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
		{
			base.ModifyHitPlayer(npc, target, ref modifiers);

			if (!target.HasBuff<BattleAxeBuff>())
			{
				return;
			}

			modifiers.FinalDamage += 3;
			modifiers.Knockback += 2;
		}
	}

	private sealed class BattleAxeModPlayerImpl : ModPlayer
	{
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			base.ModifyDrawInfo(ref drawInfo);

			if (!Player.HasBuff<BattleAxeBuff>())
			{
				return;
			}

			drawInfo.colorArmorBody = _playerDrawColor;
			drawInfo.colorArmorLegs = _playerDrawColor;
			drawInfo.colorArmorHead = _playerDrawColor;
			drawInfo.colorHair = _playerDrawColor;
			drawInfo.colorEyeWhites = _playerDrawColor;
			drawInfo.colorEyes = _playerDrawColor;
			drawInfo.colorHead = _playerDrawColor;
			drawInfo.colorBodySkin = _playerDrawColor;
			drawInfo.colorLegs = _playerDrawColor;
		}
	}

	private static readonly Color _playerDrawColor = Color.Red;

	public override string Texture => $"{PoTMod.ModName}/Assets/Buffs/Base";

	public override void Update(Player player, ref int buffIndex)
	{
		player.GetDamage(DamageClass.Generic) += 1.5f;

		Lighting.AddLight(player.Center, 1f, 0f, 0f);
	}
}
