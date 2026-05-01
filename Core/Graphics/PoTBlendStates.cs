using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfTerraria.Core.Graphics;
public static class PoTBlendStates
{
	public static readonly BlendState Screen = new()
	{
		ColorSourceBlend = Blend.InverseDestinationColor,
		ColorDestinationBlend = Blend.One,
		AlphaDestinationBlend = Blend.One,
		AlphaSourceBlend = Blend.Zero,
	};
}
