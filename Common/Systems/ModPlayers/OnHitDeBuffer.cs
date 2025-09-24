using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PathOfTerraria.Common.Systems.ModPlayers;

#nullable enable

/// <summary>
/// Used to add on-hit [de]buffs for a player to provide.
/// </summary>
public class OnHitDeBuffer : EntityModifierSegment, IEnumerable<KeyValuePair<int, OnHitDeBuffer.DebufferInstance>>
{
	public class DebufferInstance
	{
		public delegate void ApplyDebufferDelegate(Player player, NPC target, NPC.HitInfo info, int damageDone, int time);

		public Dictionary<int, StatModifier> ModifiersByDuration = [];
		public ApplyDebufferDelegate? OnApplyOverride = null;
	}

	private readonly Dictionary<int, DebufferInstance> Buffs = [];
	
	public override Dictionary<string, StatModifier> Modifiers => Buffs
		.SelectMany(outer => outer.Value.ModifiersByDuration
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
	public void Add(int id, int duration, float chance, DebufferInstance.ApplyDebufferDelegate? customBuffAction = null)
	{
		if (!Buffs.TryGetValue(id, out DebufferInstance? value))
		{
			value = new() { OnApplyOverride = customBuffAction };
			Buffs.Add(id, value);
		}

		value.ModifiersByDuration.TryAdd(duration, StatModifier.Default);
		value.ModifiersByDuration[duration] += chance;
	}

	public IEnumerator<KeyValuePair<int, DebufferInstance>> GetEnumerator()
	{
		foreach (KeyValuePair<int, DebufferInstance> val in Buffs)
		{
			yield return val;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
