using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Data.Models;
using Terraria.Chat.Commands;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.TreeSystem;

internal abstract class Jewel : PoTItem
{
	public virtual string EquppedTooltip
	{
		get
		{
			EntityModifier thisItemModifier = new EntityModifier();
			ApplyAffixes(thisItemModifier);

			string tooltip = "";
			EntityModifier.GetChange(thisItemModifier).ForEach(s =>  tooltip += s + "\n");

			Console.WriteLine(EntityModifier.GetChange(thisItemModifier).Count);
			return tooltip;
		}
	}

	private static readonly MethodInfo _getItemType = typeof(ModContent).GetMethod("ItemType", BindingFlags.Public | BindingFlags.Static);
	public static Jewel LoadFrom(TagCompound tag)
	{
		Type t = typeof(Jewel).Assembly.GetType(tag.GetString("jewelType"));
		if (t is null)
		{
			PathOfTerraria.Instance.Logger.Error($"Could not load jewel {tag.GetString("type")}, was it removed?");
			return null;
		}

		MethodInfo getItemTypeFromT = _getItemType.MakeGenericMethod([t]);

		var item = new Item();
		item.SetDefaults((int)getItemTypeFromT.Invoke(null, []));

		(item.ModItem as Jewel)?.LoadData(tag);
		return item.ModItem as Jewel;
	}

	public override void SaveData(TagCompound tag)
	{
		tag["jewelType"] = GetType().FullName;
		base.SaveData(tag);
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 center)
	{
		Texture2D tex = (Texture2D)ModContent.Request<Texture2D>(Texture);

		spriteBatch.Draw(tex, center, null, Color.White, 0, tex.Size() / 2, 1f, SpriteEffects.None, 0f);
	}
}