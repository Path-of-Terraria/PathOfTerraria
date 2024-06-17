using System.Collections.Generic;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.Affixes.GearTypes.WeaponAffixes;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class FireStarter : Sword
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Sword/FireStarter";
	public override float DropChance => 5f;
	public override int ItemLevel => 1;
	public override bool IsUnique => true;
	public override string Description => Language.GetTextValue("Mods.PathOfTerraria.Items.FireStarter.Description");
	public override string AltUseDescription => Language.GetTextValue("Mods.PathOfTerraria.Items.FireStarter.AltUseDescription");

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 4;
		Item.UseSound = SoundID.Item1;
		GearType = GearType.Sword;
	}
	
	public override string GenerateName()
	{
		return $"[c/FF0000:{Language.GetTextValue("Mods.PathOfTerraria.Items.FireStarter.DisplayName")}]";
	}
	
	public override List<GearAffix> GenerateAffixes()
	{
		var sharpAffix = (GearAffix)Affix.CreateAffix<PassiveAffixes.SharpGearAffix>();
		sharpAffix.MinValue = 1;
		sharpAffix.MaxValue = 4;
		
		var onFireAffix = (GearAffix)Affix.CreateAffix<ModifyHitAffixes.ChanceToApplyOnFireGearAffix>();
		onFireAffix.Value = 0.1f;
		return [sharpAffix, onFireAffix];
	}
	
	public override bool AltFunctionUse(Player player)
	{
		AltUseSystem modPlayer = player.GetModPlayer<AltUseSystem>();

		if (modPlayer.AltFunctionCooldown > 0)
		{
			return false;
		}
		
		modPlayer.AltFunctionCooldown = 600;
		modPlayer.AltFunctionActiveTimer = 180;
		return true;
	}
	
	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		AltUseSystem modPlayer = player.GetModPlayer<AltUseSystem>();
		
		if (modPlayer.AltFunctionActiveTimer > 0)
		{
			target.AddBuff(BuffID.OnFire, 180);
		}
		
		base.OnHitNPC(player, target, hit, damageDone);
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
		Vector2 velocity, int type, int damage, float knockback)
	{
		return false;
	}
	
	public override void PostUpdate()
	{
		base.PostUpdate();
		Item.SetDefaults(Item.type); // Update the item to ensure the texture is correct
	}
}