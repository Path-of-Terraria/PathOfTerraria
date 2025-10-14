using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Classing;

internal class ClassingPlayer : ModPlayer
{
	public StarterClasses Class = StarterClasses.None;

	/// <summary>
	/// Gets a random class noun, i.e. Warrior or Marksman, 
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public static string GetRandomClassNoun(Player player)
	{
		ClassingPlayer plr = player.GetModPlayer<ClassingPlayer>();
		return Main.rand.Next(plr.Class.GetNouns().Split(' '));
	}

	public override void SaveData(TagCompound tag)
	{
		tag.Add("class", (byte)Class);
	}

	public override void LoadData(TagCompound tag)
	{
		Class = (StarterClasses)tag.GetByte("class");
	}
}
