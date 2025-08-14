﻿using PathOfTerraria.Common.Systems.Networking.Handlers;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems;

// ReSharper disable once ClassNeverInstantiated.Global
internal class PotionPlayer : ModPlayer
{
	public int HealingLeft = 3;
	public int MaxHealing = 3;
	public int HealPower = 30;
	public int HealDelay = 300;

	public int ManaLeft = 3;
	public int MaxMana = 3;
	public int ManaPower = 50;
	public int ManaDelay = 300;

	public override void Load()
	{
		On_Player.QuickHeal += QuickHeal;
		On_Player.QuickMana += QuickMana;
	}

	private static void QuickHeal(On_Player.orig_QuickHeal orig, Player self)
	{
		PotionPlayer mp = self.GetModPlayer<PotionPlayer>();

		if (mp.HealingLeft <= 0 || self.HasBuff(BuffID.PotionSickness))
		{
			return;
		}

		UseHealingPotion(self);
	}

	internal static void UseHealingPotion(Player self, bool hasSync = false)
	{
		PotionPlayer mp = self.GetModPlayer<PotionPlayer>();

		self.Heal(mp.HealPower);
		self.AddBuff(BuffID.PotionSickness, mp.HealDelay);
		mp.HealingLeft--;

		SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/PickupPotion"));
		SoundEngine.PlaySound(SoundID.Item3);

		if (Main.netMode != NetmodeID.SinglePlayer && hasSync)
		{
			ModContent.GetInstance<HotbarPotionHandler>().Send((byte)self.whoAmI, true, (byte)mp.HealingLeft, false);
		}
	}

	private static void QuickMana(On_Player.orig_QuickMana orig, Player self)
	{
		PotionPlayer mp = self.GetModPlayer<PotionPlayer>();

		if (mp.ManaLeft <= 0 || self.HasBuff(BuffID.ManaSickness))
		{
			return;
		}

		UseManaPotion(self);
	}

	internal static void UseManaPotion(Player self, bool hasSync = false)
	{
		PotionPlayer mp = self.GetModPlayer<PotionPlayer>();

		self.ManaEffect(mp.ManaPower);
		self.statMana += mp.ManaPower;
		self.AddBuff(BuffID.ManaSickness, mp.ManaDelay);
		mp.ManaLeft--;

		SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/PickupPotion"));
		SoundEngine.PlaySound(SoundID.Item3);

		if (Main.netMode != NetmodeID.SinglePlayer && hasSync)
		{
			ModContent.GetInstance<HotbarPotionHandler>().Send((byte)self.whoAmI, false, (byte)mp.ManaLeft, false);
		}
	}

	public override void ResetEffects()
	{
		if (HealingLeft > MaxHealing)
		{
			HealingLeft = MaxHealing;
		}

		if (ManaLeft > MaxMana)
		{
			ManaLeft = MaxMana;
		}

		MaxHealing = 3;
		HealPower = 30;
		HealDelay = 300;

		MaxMana = 3;
		ManaPower = 50;
		ManaDelay = 300;
	}
}

internal class PotionGlobal : GlobalItem
{
	public override bool AppliesToEntity(Item entity, bool lateInstantiation)
	{
		return entity.healLife > 0 || entity.healMana > 0;
	}

	public override bool? UseItem(Item item, Player player)
	{
		return null;
	}
}