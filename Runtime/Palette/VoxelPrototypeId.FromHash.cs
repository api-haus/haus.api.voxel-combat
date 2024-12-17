namespace VoxelBox.Palette
{
	using Unity.Collections;

	public readonly partial struct VoxelPrototypeId
	{
		public static implicit operator VoxelPrototypeId(string name) => FindByName((FixedString512Bytes)name);

		public static implicit operator VoxelPrototypeId(FixedString32Bytes fStr) => FindByName(fStr);
		public static implicit operator VoxelPrototypeId(FixedString64Bytes fStr) => FindByName(fStr);
		public static implicit operator VoxelPrototypeId(FixedString128Bytes fStr) => FindByName(fStr);
		public static implicit operator VoxelPrototypeId(FixedString512Bytes fStr) => FindByName(fStr);
		public static implicit operator VoxelPrototypeId(FixedString4096Bytes fStr) => FindByName(fStr);

		public static VoxelPrototypeId FindByName(FixedString32Bytes fStr) => FromHash(fStr);
		public static VoxelPrototypeId FindByName(FixedString64Bytes fStr) => FromHash(fStr);
		public static VoxelPrototypeId FindByName(FixedString128Bytes fStr) => FromHash(fStr);
		public static VoxelPrototypeId FindByName(FixedString512Bytes fStr) => FromHash(fStr);
		public static VoxelPrototypeId FindByName(FixedString4096Bytes fStr) => FromHash(fStr);

		public static VoxelPrototypeId FromHash<T>(T data) where T : unmanaged
		{
			var h = new xxHash3.StreamingState(false);

			h.Update(data);

			return h.DigestHash128();
		}
	}
}
