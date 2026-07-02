using System.Runtime.InteropServices;

namespace Box2D.Interop
{
	/// P/Invoke declarations for the b2Default*() factory functions. These are the only correct
	/// source of Box2D's tuning constants - never hand-roll these defaults in C#.
	internal static partial class B2Native
	{
		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2WorldDef b2DefaultWorldDef();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2BodyDef b2DefaultBodyDef();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Filter b2DefaultFilter();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2QueryFilter b2DefaultQueryFilter();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2SurfaceMaterial b2DefaultSurfaceMaterial();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ShapeDef b2DefaultShapeDef();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ChainDef b2DefaultChainDef();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ExplosionDef b2DefaultExplosionDef();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2DistanceJointDef b2DefaultDistanceJointDef();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2MotorJointDef b2DefaultMotorJointDef();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2FilterJointDef b2DefaultFilterJointDef();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2PrismaticJointDef b2DefaultPrismaticJointDef();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2RevoluteJointDef b2DefaultRevoluteJointDef();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2WeldJointDef b2DefaultWeldJointDef();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2WheelJointDef b2DefaultWheelJointDef();
	}
}
