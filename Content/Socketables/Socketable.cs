using Ionic.Zlib;
using Newtonsoft.Json.Linq;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Content.Summonables.Vanilla;
using PathOfTerraria.Core.Systems.Affixes;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Socketables;
internal abstract class Socketable : ModItem
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Socketable/Placeholder";
	/// <summary>
	/// will be run when socketed.
	/// </summary>
	/// <param name="player">the player wielding the weapon this is socketed into.</param>
	/// <param name="item">the item this is socketed into.</param>
	public virtual void OnSocket(Player player, Item item) { }

	/// <summary>
	/// will be run when unsocketed.
	/// </summary>
	/// <param name="player">the player wielding the weapon this is socketed into.</param>
	/// <param name="item">the item this is socketed into.</param>
	public virtual void OnUnSocket(Player player, Item item) { }

	/// <summary>
	/// Updates every frame, only activates when socketed
	/// </summary>
	/// <param name="player">the player wielding the weapon this is socketed into.</param>
	/// <param name="item">the item this is socketed into.</param>
	public virtual void UpdateEquip(Player player, Item item) { }

	public virtual string GenerateName() { return "Unknown Socket"; }

	public void Save(TagCompound tag)
	{
		tag["type"] = GetType().FullName;
		SaveData(tag); // if i ever add anything, place it in there
	}

	protected void Load(TagCompound tag)
	{
		LoadData(tag);
	}

	private static readonly MethodInfo _getItemType = typeof(ModContent).GetMethod("ItemType", BindingFlags.Public | BindingFlags.Static);
	public static Socketable FromTag(TagCompound tag)
	{
		Type t = typeof(Socketable).Assembly.GetType(tag.GetString("type"));
		if (t is null)
		{
			PathOfTerraria.Instance.Logger.Error($"Could not load socketable {tag.GetString("type")}, was it removed?");
			return null;
		}

		MethodInfo getItemTypeFromt = _getItemType.MakeGenericMethod([t]);

		var item = new Item();
		item.SetDefaults((int)getItemTypeFromt.Invoke(null, []));

		(item.ModItem as Socketable).Load(tag);
		return item.ModItem as Socketable;
	}
}
