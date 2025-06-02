using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using System.Collections.Generic;

namespace PathOfTerraria.Common.UI.PassiveTree;

internal class PassiveTreeInnerPanel : AllocatableInnerPanel
{
	public override string TabName => "PassiveTree";
	public override IEnumerable<Edge> Connections => Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>().Edges;
}