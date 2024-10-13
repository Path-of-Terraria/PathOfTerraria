using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;

namespace PathOfTerraria.Common.Waypoints.UI;

public class UIScalingText : UIText
{
	private static readonly FieldInfo UITextTextScaleInfo = typeof(UIText).GetField("_textScale", BindingFlags.NonPublic | BindingFlags.Instance);

	public float Scale;

	public UIScalingText(string text, float textScale = 1f, bool large = false) : base(text, textScale, large) { }

	public UIScalingText(LocalizedText text, float textScale = 1f, bool large = false) : base(text, textScale, large) { }

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		object value = UITextTextScaleInfo.GetValue(this);

		if (value is float scale && scale == Scale)
		{
			return;
		}

		UITextTextScaleInfo.SetValue(this, Scale);
	}
}