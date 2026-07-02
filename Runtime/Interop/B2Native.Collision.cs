using System.Runtime.InteropServices;

namespace Box2D.Interop
{
	/// P/Invoke declarations for the geometry subset of collision.h that is in scope for this
	/// binding (shape construction, mass/AABB computation, point/ray/shape queries per shape
	/// type, convex hulls). The dynamic tree, GJK distance/TOI, and character-mover APIs in
	/// collision.h are intentionally not bound - see the package README for rationale.
	internal static partial class B2Native
	{
		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2IsValidRay(in B2RayCastInput input);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Polygon b2MakePolygon(in B2Hull hull, float radius);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Polygon b2MakeOffsetPolygon(in B2Hull hull, B2Vec2 position, B2Rot rotation);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Polygon b2MakeOffsetRoundedPolygon(in B2Hull hull, B2Vec2 position, B2Rot rotation, float radius);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Polygon b2MakeSquare(float halfWidth);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Polygon b2MakeBox(float halfWidth, float halfHeight);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Polygon b2MakeRoundedBox(float halfWidth, float halfHeight, float radius);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Polygon b2MakeOffsetBox(float halfWidth, float halfHeight, B2Vec2 center, B2Rot rotation);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Polygon b2MakeOffsetRoundedBox(float halfWidth, float halfHeight, B2Vec2 center, B2Rot rotation, float radius);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Polygon b2TransformPolygon(B2Transform transform, in B2Polygon polygon);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2MassData b2ComputeCircleMass(in B2Circle shape, float density);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2MassData b2ComputeCapsuleMass(in B2Capsule shape, float density);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2MassData b2ComputePolygonMass(in B2Polygon shape, float density);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2AABB b2ComputeCircleAABB(in B2Circle shape, B2Transform transform);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2AABB b2ComputeCapsuleAABB(in B2Capsule shape, B2Transform transform);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2AABB b2ComputePolygonAABB(in B2Polygon shape, B2Transform transform);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2AABB b2ComputeSegmentAABB(in B2Segment shape, B2Transform transform);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2PointInCircle(in B2Circle shape, B2Vec2 point);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2PointInCapsule(in B2Capsule shape, B2Vec2 point);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2PointInPolygon(in B2Polygon shape, B2Vec2 point);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2CastOutput b2RayCastCircle(in B2Circle shape, in B2RayCastInput input);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2CastOutput b2RayCastCapsule(in B2Capsule shape, in B2RayCastInput input);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2CastOutput b2RayCastSegment(in B2Segment shape, in B2RayCastInput input, [MarshalAs(UnmanagedType.I1)] bool oneSided);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2CastOutput b2RayCastPolygon(in B2Polygon shape, in B2RayCastInput input);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2CastOutput b2ShapeCastCircle(in B2Circle shape, in B2ShapeCastInput input);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2CastOutput b2ShapeCastCapsule(in B2Capsule shape, in B2ShapeCastInput input);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2CastOutput b2ShapeCastSegment(in B2Segment shape, in B2ShapeCastInput input);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2CastOutput b2ShapeCastPolygon(in B2Polygon shape, in B2ShapeCastInput input);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Hull b2ComputeHull(B2Vec2[] points, int count);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2ValidateHull(in B2Hull hull);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ShapeProxy b2MakeProxy(B2Vec2[] points, int count, float radius);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ShapeProxy b2MakeOffsetProxy(B2Vec2[] points, int count, float radius, B2Vec2 position, B2Rot rotation);
	}
}
