using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PathOfTerraria.Common.Systems.ModPlayers;

/// <summary>
/// Used to add on-hit [de]buffs for a player to provide.
/// </summary>
public class OnHitDeBuffer : EntityModifierSegment, IEnumerable<KeyValuePair<int, Dictionary<int, StatModifier>>>
{
	private readonly Dictionary<int, Dictionary<int, StatModifier>> Buffs = [];
	
	public override Dictionary<string, StatModifier> Modifiers => Buffs
		.SelectMany(outer => outer.Value
			.Select(inner => new KeyValuePair<string, StatModifier>("+" + Lang.GetBuffName(outer.Key) + " (" + MathF.Round(inner.Key / 60f, 2) + " s)", inner.Value)))
		.ToDictionary(v => v.Key, v => v.Value);

	/// <summary>
	/// Adds the given <paramref name="id"/> to the buffer in the given tick <paramref name="duration"/> and <paramref name="chance"/>.<br/>
	/// Chance is additive per duration - the chance is cumulative, so if multiple places apply, it'll add.<br/>
	/// Each duration is exclusive, i.e. 1 second and 2 seconds have different chances, and both have a chance to proc.<br/>
	/// Both procing at the same time will use reapply behaviour for the buff (usually just taking the higher time).
	/// </summary>
	/// <param name="id">Buff ID to use.</param>
	/// <param name="duration">Duration to use. This also creates its own tracker per duration.</param>
	/// <param name="chance">Additive chance for the buff to proc.</param>
	public void Add(int id, int duration, float chance)
	{
		if (!Buffs.ContainsKey(id))
		{
			Buffs.Add(id, []);
		}

		if (!Buffs[id].ContainsKey(duration))
		{
			Buffs[id].Add(duration, StatModifier.Default);
		}

		Buffs[id][duration] += chance;
	}

	public IEnumerator<KeyValuePair<int, Dictionary<int, StatModifier>>> GetEnumerator()
	{
		foreach (KeyValuePair<int, Dictionary<int, StatModifier>> val in Buffs)
		{
			yield return val;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
