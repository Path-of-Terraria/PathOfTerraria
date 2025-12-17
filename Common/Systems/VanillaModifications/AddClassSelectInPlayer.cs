using PathOfTerraria.Common.Classing;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using System.Runtime.CompilerServices;
using Terraria.GameContent.UI.States;

namespace PathOfTerraria.Common.Systems.VanillaModifications;

internal class AddClassSelectInPlayer : ModSystem
{
	public override void Load()
	{
		On_UICharacterCreation.FinishCreatingCharacter += FinishCreatingChar;
		On_UICharacterCreation.SetupPlayerStatsAndInventoryBasedOnDifficulty += ReplaceWeaponWithClassItem;
	}

	private void ReplaceWeaponWithClassItem(On_UICharacterCreation.orig_SetupPlayerStatsAndInventoryBasedOnDifficulty orig, UICharacterCreation self)
	{
		orig(self);

		Player player = GetPlayer(self);
		StarterClass classType = player.GetModPlayer<ClassingPlayer>().Class;
		StarterClassInfo info = StarterClasses.GetInfo(classType);

		foreach (Type skillType in info.SkillTypes)
		{
			player.GetModPlayer<SkillCombatPlayer>().TryAddSkill(Activator.CreateInstance(skillType) as Skill, true);
		}

		player.inventory[0].SetDefaults(info.WeaponItemId);
	}

	private void FinishCreatingChar(On_UICharacterCreation.orig_FinishCreatingCharacter orig, UICharacterCreation self)
	{
		Player player = GetPlayer(self);

		if (player.GetModPlayer<ClassingPlayer>().Class == StarterClass.None)
		{
			void Reset()
			{
				Main.MenuUI.SetState(self);
			}

			Main.MenuUI.SetState(new ClassUIState(Reset, GetPlayer(self)));
			return;
		}

		orig(self);
	}

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_player")]
	private static extern ref Player GetPlayer(UICharacterCreation ui);
}
