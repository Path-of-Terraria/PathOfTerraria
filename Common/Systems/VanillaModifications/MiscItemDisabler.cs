using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.Utilities;
using SubworldLibrary;
using System.Collections;
using System.Collections.Generic;
using Terraria.Chat;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.VanillaModifications;

internal class MiscItemDisabler : ModSystem
{
	public readonly static HashSet<int> DisabledItems = [ItemID.HealingPotion, ItemID.GreaterHealingPotion, ItemID.LesserHealingPotion, ItemID.SuperHealingPotion,
		ItemID.ManaPotion, ItemID.GreaterManaPotion, ItemID.LesserManaPotion, ItemID.SuperManaPotion, ItemID.ManaFlower];

	private static bool DroppingPotions = false;

	public override void Load()
	{
		On_NPC.DoDeathEvents_DropBossPotionsAndHearts += AddFlag;
		On_Item.NewItem_Inner += StopPotionDrops;
	}

	private int StopPotionDrops(On_Item.orig_NewItem_Inner orig, Terraria.DataStructures.IEntitySource source, int X, int Y, int Width, int Height, Item itemToClone, 
		int Type, int Stack, bool noBroadcast, int pfix, bool noGrabDelay, bool reverseLookup)
	{
		if (DroppingPotions && DisabledItems.Contains(Type))
		{
			return Main.maxItems - 1;
		}

		return orig(source, X, Y, Width, Height, itemToClone, Type, Stack, noBroadcast, pfix, noGrabDelay, reverseLookup);
	}

	private void AddFlag(On_NPC.orig_DoDeathEvents_DropBossPotionsAndHearts orig, NPC self, ref string typeName)
	{
		DroppingPotions = true;
		orig(self, ref typeName);
		DroppingPotions = false;
	}

	public override void PostAddRecipes()
	{
		foreach (Recipe recipe in Main.recipe)
		{
			// Disable all relevant vanilla boss spawners
			if (DisabledItems.Contains(recipe.createItem.type))
			{
				recipe.DisableRecipe();
			}
		}
	}

	public override void PostWorldGen()
	{
		foreach (Chest chest in Main.chest)
		{
			if (chest is null)
			{
				continue;
			}

			foreach (Item item in chest.item)
			{
				if (DisabledItems.Contains(item.type) && !item.IsAir)
				{
					item.SetDefaults(ItemID.GoldCoin);
					item.stack = WorldGen.genRand.Next(1, 4);
				}
			}
		}
	}

	internal class MiscLootDisabler : GlobalNPC
	{
		public override void Load()
		{
			On_NPC.NPCLoot_DropItems += NonsenseFixForSkelePrimeBag;
		}

		private void NonsenseFixForSkelePrimeBag(On_NPC.orig_NPCLoot_DropItems orig, NPC self, Player closestPlayer)
		{
			// I don't know why this fix works, what causes it, or why it's - to my knowledge, only Skeletron Prime.
			// If you run orig for Skeletron Prime, the boss bag WILL NOT DROP unless the boss is somehow killed more than once in the domain,
			// or if it's beaten in the overworld. Neither should be possible in the mod. This if just contains vanilla's NPCLoot_DropItems code copy pasted as is.

			// For context, down the line from ItemDropSolver.TryDropping, in ItemDropSolver.ResolveRule, this is thrown right before the item can drop:
			// Internal error in the C# compiler
			//		Compiler Exception Type: System.Collections.Generic.KeyNotFoundException
			//		Compiler Exception Message: The given key was not present in the dictionary.
			//		Compiler Exception Stack Trace: mscorlib.dll!System.ThrowHelper.ThrowKeyNotFoundException() + 0x5
			// mscorlib.dll!System.Collections.Generic.Dictionary`2.get_Item(TKey) + 0x1E
			// Why? I don't know. Why only in the domain? I don't know.
			// But this works. - GabeHasWon

			if (self.type == NPCID.SkeletronPrime)
			{
				DropAttemptInfo dropAttemptInfo = default;
				dropAttemptInfo.player = closestPlayer;
				dropAttemptInfo.npc = self;
				dropAttemptInfo.IsExpertMode = Main.expertMode;
				dropAttemptInfo.IsMasterMode = Main.masterMode;
				dropAttemptInfo.IsInSimulation = false;
				dropAttemptInfo.rng = Main.rand;
				DropAttemptInfo info = dropAttemptInfo;
				Main.ItemDropSolver.TryDropping(info);
			}
			else
			{
				orig(self, closestPlayer);
			}
		}

		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
		{
			npcLoot.RemoveWhere(x => x is CommonDrop common && DisabledItems.Contains(common.itemId));
			npcLoot.RemoveWhere(x => x is MechBossSpawnersDropRule);
		}
	}
	
	internal class MiscItemDisablerGlobalItem : GlobalItem
	{
		public override bool AppliesToEntity(Item entity, bool lateInstantiation)
		{
			return entity.type == ItemID.PirateMap;
		}

		public override bool CanUseItem(Item item, Player player)
		{
			if (SubworldSystem.Current is null)
			{
				return true;
			}
			
			string key = $"Mods.{PoTMod.ModName}.Misc.SubworldItemDisabler";

			NotificationUtils.ShowNotification(key, Colors.RarityDarkRed);
			
			return false;
		}
	}
}
