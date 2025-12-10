using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

#nullable enable

internal class WideSwingsMastery : Passive
{
	internal class WideSwingsPlayer : ModPlayer
	{
		public static Asset<Texture2D> PoisonIcon = null!;

		internal int StacksCount => _stacks.Count;

		private readonly List<int> _stacks = [];

		/// <summary>
		/// Adds a stack of poison to the current NPC.
		/// </summary>
		public void AddStack(int stack)
		{
			_stacks.Add(stack);
		}

		public override void Load()
		{
			PoisonIcon = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/PoisonIcon");
		}

		public override void PostUpdateEquips()
		{
			for (int i = 0; i < _stacks.Count; i++)
			{
				_stacks[i]--;
			}

			_stacks.RemoveAll(x => x <= 0);
		}

		public override void ModifyItemScale(Item item, ref float scale)
		{
			scale += _stacks.Count / 10f;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (hit.Crit && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<WideSwingsMastery>(out float value))
			{
				Player.AddBuff(ModContent.BuffType<WideSwingsBuff>(), (int)value * 60);
			}
		}
	}

	internal const int MaxStacks = 5;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, MaxStacks);
}
