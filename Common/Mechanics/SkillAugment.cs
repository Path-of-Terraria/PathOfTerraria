using PathOfTerraria.Common.Systems.Skills;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Localization;

namespace PathOfTerraria.Common.Mechanics;

public abstract class SkillAugment : ILoadable
{
	/// <summary> All skill augments, keyed by name, added during load. </summary>
	public static readonly Dictionary<string, SkillAugment> LoadedAugments = [];

	private Asset<Texture2D> _texture;

	public Asset<Texture2D> Texture //Cache the texture to avoid constant requests
	{
		get
		{
			if (_texture != null)
			{
				return _texture;
			}

			return _texture = ModContent.Request<Texture2D>(TexturePath, AssetRequestMode.ImmediateLoad);
		}
	}

	public string Name => GetType().Name;

	public virtual string TexturePath => $"{PoTMod.ModName}/Assets/SkillAugments/" + Name;
	public virtual string DisplayName => Language.GetTextValue("Mods.PathOfTerraria.SkillAugments." + Name + ".Name");
	public virtual string Tooltip => Language.GetTextValue("Mods.PathOfTerraria.SkillAugments." + Name + ".Tooltip");

	public virtual void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
	{
		Texture2D texture = Texture.Value;
		spriteBatch.Draw(texture, position - texture.Size() / 2, color);
	}

	public virtual void AugmentEffects(ref SkillBuff buff) { }

	public void Load(Mod mod)
	{
		LoadedAugments.Add(Name, this);
	}

	public void Unload() { }
}