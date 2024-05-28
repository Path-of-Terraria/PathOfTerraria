using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Items;

public abstract class Affix
{
	protected float _minValue;
	protected float _maxValue = 1;
	protected float Value = 1;
	
	public virtual void Roll()
	{
		Value = Main.rand.Next((int)(_minValue * 10), (int)(_maxValue * 10)) / 10f;
	}

	public virtual void Save(TagCompound tag)
	{
		tag["type"] = GetType().FullName;
		tag["value"] = Value;
	}

	public virtual void Load(TagCompound tag)
	{
		Value = tag.GetFloat("value");
	}
}