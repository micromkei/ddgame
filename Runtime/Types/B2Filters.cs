using System.Runtime.InteropServices;
using Box2D.Interop;

namespace Box2D
{
	/// Collision filtering for shape-vs-shape and shape-vs-query collision. Matches b2Filter.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Filter
	{
		public ulong categoryBits;
		public ulong maskBits;
		public int groupIndex;

		public static B2Filter Default() => B2Native.b2DefaultFilter();
	}

	/// Filter used for world queries (raycasts, overlaps, shape casts). Matches b2QueryFilter.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2QueryFilter
	{
		public ulong categoryBits;
		public ulong maskBits;

		public static B2QueryFilter Default() => B2Native.b2DefaultQueryFilter();
	}

	/// Restricts body movement along specific axes. Matches b2MotionLocks.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2MotionLocks
	{
		private byte linearX;
		private byte linearY;
		private byte angularZ;

		public bool LinearX
		{
			get => linearX != 0;
			set => linearX = B2Bool.From(value);
		}

		public bool LinearY
		{
			get => linearY != 0;
			set => linearY = B2Bool.From(value);
		}

		public bool AngularZ
		{
			get => angularZ != 0;
			set => angularZ = B2Bool.From(value);
		}
	}
}
