using PathOfTerraria.Common.Classing;
using System.Runtime.CompilerServices;
using Terraria.GameContent.UI.States;

namespace PathOfTerraria.Common.Systems.VanillaModifications;

internal class AddClassSelectInPlayer : ModSystem
{
	public override void Load()
	{
		On_UICharacterCreation.FinishCreatingCharacter += FinishCreatingChar;
	}

	private void FinishCreatingChar(On_UICharacterCreation.orig_FinishCreatingCharacter orig, UICharacterCreation self)
	{
		Player player = GetPlayer(self);

		if (player.GetModPlayer<ClassingPlayer>().Class == StarterClasses.None)
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
