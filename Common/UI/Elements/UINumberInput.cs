using Terraria.UI;

namespace PathOfTerraria.Common.UI.Elements;

/// <summary>
///		A reusable numeric text input that exposes a nullable integer value. An empty field maps to
///		<see langword="null"/>; non-numeric characters are rejected by the underlying
///		<see cref="UIEditableText"/>. Optional <see cref="Min"/>/<see cref="Max"/> clamp the parsed value.
/// </summary>
public sealed class UINumberInput : UIEditableText
{
	public int? Min { get; set; }
	public int? Max { get; set; }

	/// <summary>Fired when the parsed value changes (including transitions to/from null).</summary>
	public event Action<int?> OnValueChanged;

	private int? _lastReportedValue;

	public int? Value
	{
		get
		{
			if (string.IsNullOrWhiteSpace(CurrentValue))
			{
				return null;
			}

			if (!int.TryParse(CurrentValue, out int parsed))
			{
				return null;
			}

			if (Min is { } min && parsed < min)
			{
				parsed = min;
			}

			if (Max is { } max && parsed > max)
			{
				parsed = max;
			}

			return parsed;
		}
		set => CurrentValue = value?.ToString() ?? string.Empty;
	}

	public UINumberInput(string placeholder = "", int maxChars = 6)
		: base(InputType.Integer, placeholder, maxChars)
	{
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		int? current = Value;

		if (current == _lastReportedValue)
		{
			return;
		}

		_lastReportedValue = current;
		OnValueChanged?.Invoke(current);
	}
}
