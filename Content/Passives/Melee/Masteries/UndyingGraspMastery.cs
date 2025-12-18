using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using System.Collections.Generic;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

internal class UndyingGraspMastery : Passive
{
	internal class UndyingGraspPlayer : ModPlayer
	{
		private record struct Healing(int Heal, int Time = HealTime * 60)
		{
			public int Heal = Heal;
			public int Time = Time;
		}

		private readonly List<Healing> _heals = [];

		public override void PostHurt(Player.HurtInfo info)
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<UndyingGraspMastery>(out float value))
			{
				_heals.Add(new Healing((int)(info.Damage * value / 100f)));
			}
		}

		public override void PostUpdateEquips()
		{
			for (int i = 0; i < _heals.Count; ++i)
			{
				Healing heal = _heals[i];
				heal.Time--;

				if (heal.Time <= 0)
				{
					Player.Heal(heal.Heal);
				}

				_heals[i] = heal;
			}

			_heals.RemoveAll(static x => x.Time <= 0);
		}
	}

	public const int HealTime = 2;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, HealTime);
}
