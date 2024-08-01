﻿using System.Reflection;
using PathOfTerraria.Core.Items;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.TreeSystem;

internal abstract class Jewel : ModItem
{
	public virtual string EquppedTooltip
	{
		get
		{
			EntityModifier thisItemModifier = new EntityModifier();
			PoTItemHelper.ApplyAffixes(Item, thisItemModifier, null);

			string tooltip = "";
			EntityModifier.GetChangeOnlyStrings(thisItemModifier).ForEach(s => tooltip += s + "\n");

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