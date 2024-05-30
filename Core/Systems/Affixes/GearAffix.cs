using PathOfTerraria.Content.Items.Gear;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.Affixes;

internal abstract class GearAffix : Affix
{
	public virtual ModifierType ModifierType => ModifierType.Passive;
	public virtual bool IsFlat => true; // alternative is percent
	public virtual bool Round => false;
	public virtual GearInfluence RequiredInfluence => GearInfluence.None;

	public abstract GearType PossibleTypes { get; }

	public virtual void BuffPassive(Player player, Gear gear) { }

	public abstract string Tooltip { get; }

	public string GetTooltip(Gear gear)
	{
		float value = GetModifierValue(gear);
		bool positive = value >= 0;
		string text = MathF.Abs(value).ToString();

		string range = "";
		if (Main.keyState.PressingShift())
		{
			float oVal = Value;

			Value = MinValue;
			float valueMin = GetModifierValue(gear);
			string textMin = valueMin.ToString();

			Value = MaxValue;
			float valueMax = GetModifierValue(gear);
			string textMax = valueMax.ToString();

			range = $" [{textMin} - {textMax}]";

			Value = oVal;
		}

		text += range;

		if (IsFlat && ModifierType == ModifierType.Multiplier)
		{
			text = (value * 100f).ToString("0.0") + range + "%";
		}

		if (!IsFlat)
		{
			text += "%";
		}

		text = ModifierType switch
		{
			ModifierType.Added => positive ? "+" + text : "-" + text,
			ModifierType.Multiplier => text + (positive ? " increased" : " decreased"),
			_ => text
		};

		return Tooltip.Replace("#", text);
	}

	protected abstract float InternalModifierCalculation(Gear gear);

	public float GetModifierValue(Gear gear)
	{
		float v = InternalModifierCalculation(gear) * ExternalMultiplier;

		if (Round)
		{
			v = (float)Math.Round(v);
		}
		else
		{
			v = (float)Math.Round(v, 2);
		}

		return v;
	}

	protected int GetModifierIValue(Gear gear) { return (int)GetModifierValue(gear); }
	
	/// <summary>
	/// Generates an affix from a tag, used on load to re-populate affixes
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public static GearAffix FromTag(TagCompound tag)
	{
		var affix = (GearAffix)Activator.CreateInstance(typeof(GearAffix).Assembly.GetType(tag.GetString("type")));

		if (affix is null)
		{
			PathOfTerraria.Instance.Logger.Error($"Could not load affix {tag.GetString("type")}, was it removed?");
			return null;
		}

		affix.Load(tag);
		return affix;
	}
}