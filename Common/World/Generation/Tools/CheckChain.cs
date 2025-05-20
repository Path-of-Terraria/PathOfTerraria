namespace PathOfTerraria.Common.World.Generation.Tools;

internal class CheckChain(CheckChain.GenAction action)
{
	public delegate void GenAction(int x, int y, ref int? checkType);

	public GenAction Action = action;
	public CheckChain Next = null;

	public bool Run(int x, int y)
	{
		int? checkType = null;
		Action(x, y, ref checkType);

		if (Main.tile[x, y].TileType != checkType)
		{
			if (Next is null)
			{
				return Main.tile[x, y].TileType == checkType;
			}

			Next.Run(x, y);
		}

		return Main.tile[x, y].TileType == checkType;
	}

	public CheckChain Chain(CheckChain next)
	{
		if (Next is null)
		{
			Next = next;
		}
		else
		{
			Next.Chain(next);
		}

		return this;
	}

	public CheckChain Chain(GenAction next)
	{
		return Chain(new CheckChain(next));
	}
}
