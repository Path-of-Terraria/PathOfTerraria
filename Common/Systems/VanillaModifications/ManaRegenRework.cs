using Microsoft.Xna.Framework;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.VanillaModifications;

internal sealed class ManaRegenRework : ModSystem
{
	private const float BaseManaRegenMultiplier = 0.1f;
	private const int MinimumBaseManaRegen = 1;
	private const int TicksPerSecond = 60;
	private const int VanillaManaRegenCountPerMana = 120;
	private const bool PreventManaSickness = true;
	private const bool PersistManaRegenWhileCasting = true;
	private const bool NormalizeStandingStillManaRegen = true;

	internal sealed class ManaRegenPlayer : ModPlayer
	{
		public StatModifier ManaRegen = StatModifier.Default;
		public int LastManaRegen { get; internal set; }

		public override void ResetEffects()
		{
			ManaRegen = StatModifier.Default;
		}
	}

	internal static int ManaPerSecondToManaRegen(float manaPerSecond)
	{
		return (int)(manaPerSecond * VanillaManaRegenCountPerMana / TicksPerSecond);
	}

	public override void Load()
	{
		On_Player.AddBuff += AddBuff;
		On_Player.UpdateManaRegen += UpdateManaRegen;
	}

	private static void AddBuff(On_Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack)
	{
		if (PreventManaSickness && type == BuffID.ManaSickness)
		{
			return;
		}

		orig(self, type, timeToAdd, quiet, foodHack);
	}

	private static void UpdateManaRegen(On_Player.orig_UpdateManaRegen orig, Player self)
	{
		int manaRegenBonus = self.manaRegenBonus;
		int statMana = self.statMana;
		int manaRegenCount = self.manaRegenCount;
		bool controlLeft = self.controlLeft;
		bool controlRight = self.controlRight;
		bool controlJump = self.controlJump;
		bool controlDown = self.controlDown;
		Vector2 velocity = self.velocity;

		if (PersistManaRegenWhileCasting)
		{
			self.manaRegenDelay = 0f;
		}

		self.manaRegenBonus -= GetBaseManaRegenPenalty(self);

		if (NormalizeStandingStillManaRegen)
		{
			ApplyMovingManaRegenState(self);
		}

		try
		{
			orig(self);
			ApplyManaRegenModifier(self, statMana, manaRegenCount);
		}
		finally
		{
			self.manaRegenBonus = manaRegenBonus;
			self.controlLeft = controlLeft;
			self.controlRight = controlRight;
			self.controlJump = controlJump;
			self.controlDown = controlDown;
			self.velocity = velocity;

			if (PersistManaRegenWhileCasting)
			{
				self.manaRegenDelay = 0f;
			}
		}
	}

	private static int GetBaseManaRegenPenalty(Player player)
	{
		int baseManaRegen = player.statManaMax2 / 7 + 1;
		int reducedBaseManaRegen = Math.Max(MinimumBaseManaRegen, (int)(baseManaRegen * BaseManaRegenMultiplier));
		return baseManaRegen - reducedBaseManaRegen;
	}

	private static void ApplyManaRegenModifier(Player player, int statMana, int manaRegenCount)
	{
		ManaRegenPlayer regenPlayer = player.GetModPlayer<ManaRegenPlayer>();
		int modifiedManaRegen = Math.Max(0, (int)regenPlayer.ManaRegen.ApplyTo(player.manaRegen));

		if (modifiedManaRegen == player.manaRegen)
		{
			regenPlayer.LastManaRegen = player.manaRegen;
			return;
		}

		int vanillaManaRestored = CountManaRestored(statMana, manaRegenCount, player.manaRegen, player.statManaMax2);
		player.statMana = Math.Clamp(player.statMana - vanillaManaRestored, 0, player.statManaMax2);
		player.manaRegen = modifiedManaRegen;
		player.manaRegenCount = manaRegenCount + modifiedManaRegen;

		while (player.manaRegenCount >= VanillaManaRegenCountPerMana && player.statMana < player.statManaMax2)
		{
			player.manaRegenCount -= VanillaManaRegenCountPerMana;
			player.statMana++;
		}

		regenPlayer.LastManaRegen = player.manaRegen;
	}

	private static int CountManaRestored(int statMana, int manaRegenCount, int manaRegen, int statManaMax)
	{
		int restored = 0;
		int simulatedManaRegenCount = manaRegenCount + manaRegen;

		while (simulatedManaRegenCount >= VanillaManaRegenCountPerMana && statMana + restored < statManaMax)
		{
			simulatedManaRegenCount -= VanillaManaRegenCountPerMana;
			restored++;
		}

		return restored;
	}

	private static void ApplyMovingManaRegenState(Player player)
	{
		player.controlLeft = true;

		if (player.velocity == Vector2.Zero)
		{
			player.velocity = Vector2.UnitX;
		}
	}
}
