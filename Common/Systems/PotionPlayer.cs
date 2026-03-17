using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Content.Passives.Summon.Masteries;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader.Core;

namespace PathOfTerraria.Common.Systems;

// ReSharper disable once ClassNeverInstantiated.Global
internal class PotionPlayer : ModPlayer
{
	/// <summary>
	/// Allows ModPlayers to run code when a hotbar potion is used.
	/// </summary>
	public interface IOnCustomPotionPlayer
	{
		private static readonly HookList<ModPlayer> Hook = PlayerLoader.AddModHook(HookList<ModPlayer>.Create(i => ((IOnCustomPotionPlayer)i).OnCustomPotion));

		public void OnCustomPotion(bool healing, int amount);

		public static void Invoke(Player player, bool healing, int amount)
		{
			foreach (IOnCustomPotionPlayer g in Hook.Enumerate(player.ModPlayers))
			{
				g.OnCustomPotion(healing, amount);
			}
		}
	}

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

		// Default checks, constant
		if (mp.HealingLeft <= 0 || self.HasBuff(BuffID.PotionSickness))
		{
			return;
		}

		// Disable overheal unless using the Overheal Pulse mastery
		if (self.statLife >= self.statLifeMax2 && !self.GetModPlayer<PassiveTreePlayer>().HasNode<OverhealMastery>())
		{ 
			return;
		}

		UseHealingPotion(self);
	}

	internal static void UseHealingPotion(Player self, bool hasSync = false)
	{
		PotionPlayer mp = self.GetModPlayer<PotionPlayer>();

		// Changed from flat healing to percentage-based (Using statLifeMax2 is MaxLife after adjusted buffs/equipment)
		int healAmount = (int)(self.statLifeMax2 * (mp.HealPower / 100f));
		self.Heal(healAmount);

		IOnCustomPotionPlayer.Invoke(self, true, healAmount);

		self.AddBuff(BuffID.PotionSickness, mp.HealDelay);
		mp.HealingLeft--;

		SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/PickupPotion"));
		SoundEngine.PlaySound(SoundID.Item3);

		if (Main.netMode != NetmodeID.SinglePlayer && hasSync)
		{
			HotbarPotionHandler.Send(true, (byte)mp.HealingLeft);
		}
	}

	private static void QuickMana(On_Player.orig_QuickMana orig, Player self)
	{
		PotionPlayer mp = self.GetModPlayer<PotionPlayer>();

		if (mp.ManaLeft <= 0 || self.HasBuff(BuffID.ManaSickness) || self.statMana >= self.statManaMax2)
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

		IOnCustomPotionPlayer.Invoke(self, true, mp.ManaPower);

		self.AddBuff(BuffID.ManaSickness, mp.ManaDelay);
		mp.ManaLeft--;

		SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/PickupPotion"));
		SoundEngine.PlaySound(SoundID.Item3);

		if (Main.netMode != NetmodeID.SinglePlayer && hasSync)
		{
			HotbarPotionHandler.Send(false, (byte)mp.ManaLeft);
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
		//This is now 30%
		HealPower = 30;
		HealDelay = 300;

		MaxMana = 3;
		ManaPower = 50;
		ManaDelay = 300;
	}
}