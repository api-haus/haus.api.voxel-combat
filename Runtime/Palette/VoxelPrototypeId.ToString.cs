namespace VoxelBox.Palette
{
	using System.Runtime.CompilerServices;

	public readonly partial struct VoxelPrototypeId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override unsafe string ToString()
		{
			var chars = stackalloc char[32];

			for (int i = 0; i < 4; i++)
			{
				for (int j = 7; j >= 0; j--)
				{
					uint cur = Value[i];
					cur >>= (j * 4);
					cur &= 0xF;
					chars[i * 8 + j] = KHexToLiteral[cur];
				}
			}

			return new string(chars, 0, 32);
		}

		static readonly char[] KHexToLiteral =
		{
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'
		};
	}
}
