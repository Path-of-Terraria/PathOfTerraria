using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Data.Models;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.TreeSystem;

internal abstract class Jewel : ModItem
{
	public virtual string InternalIdentifier => "NONE";

	/// <summary>
	/// Name to be used in ALL display situations. This is automatically populated by <see cref="Language.GetOrRegister(string, Func{string})"/>.
	/// </summary>
	public virtual string DisplayName => Language.GetTextValue("Mods.PathOfTerraria.Jewels." + InternalIdentifier + ".Name");

	/// <summary>
	/// Tooltip to be used in ALL display situations. This is automatically populated by <see cref="Language.GetOrRegister(string, Func{string})"/>.
	/// </summary>
	public virtual string DisplayTooltip => Language.GetTextValue("Mods.PathOfTerraria.Jewels." + InternalIdentifier + ".Tooltip");

	public virtual void BuffPlayer(Player player) { }

	public virtual void OnLoad() { }

	public static Jewel LoadFrom(TagCompound tag)
	{
		return null;
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 center)
	{
		
	}
}