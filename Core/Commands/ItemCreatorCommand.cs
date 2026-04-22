#if DEBUG || STAGING
using PathOfTerraria.Common.UI.ItemCreator;
using PathOfTerraria.Core.UI.SmartUI;

#nullable enable

namespace PathOfTerraria.Core.Commands;

public sealed class ItemCreatorCommand : ModCommand
{
	public override string Command => "potItemCreator";
	public override CommandType Type => CommandType.Chat;
	public override string Usage => $"[c/ff6a00:Usage: /{Command}";
	public override string Description => "Opens the Item Creator debug panel for editing item affixes.";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		SmartUiLoader.GetUiState<ItemCreatorUIState>().Toggle();
	}
}
#endif
