using PathOfTerraria.Common.Events;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Buffs;

class BattleaxeBuff() : SmartBuff(false)
{
	public override string Texture => $"{nameof(PathOfTerraria)}/Assets/Buffs/Base";

	public override void Load()
	{
		PathOfTerrariaNpcEvents.ModifyHitPlayerEvent += TakeMoreDamage;
	}

	private void TakeMoreDamage(NPC npc, Player target, ref Player.HurtModifiers modifiers)
	{
		modifiers.FinalDamage += 3;
		modifiers.Knockback += 2;
	}

	public override void Update(Player player, ref int buffIndex)
	{ 
		player.GetDamage(DamageClass.Generic) += 1.5f; //150% more damage
		Lighting.AddLight(player.Center, 1f, 0f, 0f);
	}
}

public class BattleaxeBuffModPlayer : ModPlayer
{
	private bool _isRedTintActive;
	
	public override void PreUpdate()
	{
		// Apply red tint by changing the player color
		Main.LocalPlayer.GetModPlayer<BattleaxeBuffModPlayer>()._isRedTintActive = Main.LocalPlayer.HasBuff(ModContent.BuffType<BattleaxeBuff>());
	}

	public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
	{
		if (_isRedTintActive)
		{
			drawInfo.colorArmorBody = Color.Red;
			drawInfo.colorArmorLegs = Color.Red;
			drawInfo.colorArmorHead = Color.Red;
			drawInfo.colorHair = Color.Red;
			drawInfo.colorEyeWhites = Color.Red;
			drawInfo.colorEyes = Color.Red;
			drawInfo.colorHead = Color.Red;
			drawInfo.colorBodySkin = Color.Red;
			drawInfo.colorLegs = Color.Red;
		}
	}
}
