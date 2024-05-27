using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.ModPlayers;

public class ClassModPlayer : ModPlayer
{
	public string SelectedClass;

	public override void Initialize()
	{
		SelectedClass = "";
	}

	public override void SaveData(TagCompound tag)
	{
		tag["selectedClass"] = SelectedClass;
	}

	public override void LoadData(TagCompound tag)
	{
		SelectedClass = tag.GetString("selectedClass");
	}

	public bool HasSelectedClass()
	{
		return !string.IsNullOrEmpty(SelectedClass);
	}
}