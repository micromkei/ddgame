using System.Runtime.InteropServices;
using Box2D.Interop;

namespace Box2D
{
	public static class B2Constants
	{
		public const int MaxPolygonVertices = 8;
	}

	/// Performance counters returned by dynamic tree / world spatial queries. Matches b2TreeStats.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2TreeStats
	{
		public int nodeVisits;
		public int leafVisits;
	}

	/// This holds the mass data computed for a shape. Matches b2MassData.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2MassData
	{
		public float mass;
		public B2Vec2 center;
		public float rotationalInertia;
	}

	/// Surface material properties for a shape or chain segment. Matches b2SurfaceMaterial.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2SurfaceMaterial
	{
		public float friction;
		public float restitution;
		public float rollingResistance;
		public float tangentSpeed;
		public ulong userMaterialId;
		public uint customColor;

		public static B2SurfaceMaterial Default() => B2Native.b2DefaultSurfaceMaterial();
	}

	/// A solid circle. Matches b2Circle.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Circle
	{
		public B2Vec2 center;
		public float radius;

		public B2Circle(B2Vec2 center, float radius)
		{
			this.center = center;
			this.radius = radius;
		}

		public B2MassData ComputeMass(float density) => B2Native.b2ComputeCircleMass(in this, density);
		public B2AABB ComputeAABB(B2Transform transform) => B2Native.b2ComputeCircleAABB(in this, transform);
		public bool ContainsPoint(B2Vec2 point) => B2Native.b2PointInCircle(in this, point);
		public B2CastOutput RayCast(in B2RayCastInput input) => B2Native.b2RayCastCircle(in this, in input);
		public B2CastOutput ShapeCast(in B2ShapeCastInput input) => B2Native.b2ShapeCastCircle(in this, in input);
	}

	/// A solid capsule: two semicircles connected by a rectangle. Matches b2Capsule.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Capsule
	{
		public B2Vec2 center1;
		public B2Vec2 center2;
		public float radius;

		public B2Capsule(B2Vec2 center1, B2Vec2 center2, float radius)
		{
			this.center1 = center1;
			this.center2 = center2;
			this.radius = radius;
		}

		public B2MassData ComputeMass(float density) => B2Native.b2ComputeCapsuleMass(in this, density);
		public B2AABB ComputeAABB(B2Transform transform) => B2Native.b2ComputeCapsuleAABB(in this, transform);
		public bool ContainsPoint(B2Vec2 point) => B2Native.b2PointInCapsule(in this, point);
		public B2CastOutput RayCast(in B2RayCastInput input) => B2Native.b2RayCastCapsule(in this, in input);
		public B2CastOutput ShapeCast(in B2ShapeCastInput input) => B2Native.b2ShapeCastCapsule(in this, in input);
	}

	/// A line segment with two-sided collision. Matches b2Segment.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Segment
	{
		public B2Vec2 point1;
		public B2Vec2 point2;

		public B2Segment(B2Vec2 point1, B2Vec2 point2)
		{
			this.point1 = point1;
			this.point2 = point2;
		}

		public B2AABB ComputeAABB(B2Transform transform) => B2Native.b2ComputeSegmentAABB(in this, transform);
		public B2CastOutput RayCast(in B2RayCastInput input, bool oneSided) => B2Native.b2RayCastSegment(in this, in input, oneSided);
		public B2CastOutput ShapeCast(in B2ShapeCastInput input) => B2Native.b2ShapeCastSegment(in this, in input);
	}

	/// A one-sided line segment owned by a chain shape (ghost1 -> point1 -> point2 -> ghost2).
	/// Matches b2ChainSegment.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ChainSegment
	{
		public B2Vec2 ghost1;
		public B2Segment segment;
		public B2Vec2 ghost2;
		public int chainId;
	}

	/// A solid convex polygon with up to B2Constants.MaxPolygonVertices vertices. Matches b2Polygon.
	/// Do not fill this out manually - use one of the Make* factory methods, which mirror the
	/// native b2Make*/b2ComputeHull helpers.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Polygon
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = B2Constants.MaxPolygonVertices)]
		public B2Vec2[] vertices;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = B2Constants.MaxPolygonVertices)]
		public B2Vec2[] normals;

		public B2Vec2 centroid;
		public float radius;
		public int count;

		public static B2Polygon MakeBox(float halfWidth, float halfHeight) => B2Native.b2MakeBox(halfWidth, halfHeight);
		public static B2Polygon MakeSquare(float halfWidth) => B2Native.b2MakeSquare(halfWidth);
		public static B2Polygon MakeRoundedBox(float halfWidth, float halfHeight, float radius) => B2Native.b2MakeRoundedBox(halfWidth, halfHeight, radius);

		public static B2Polygon MakeOffsetBox(float halfWidth, float halfHeight, B2Vec2 center, B2Rot rotation) =>
			B2Native.b2MakeOffsetBox(halfWidth, halfHeight, center, rotation);

		public static B2Polygon MakeOffsetRoundedBox(float halfWidth, float halfHeight, B2Vec2 center, B2Rot rotation, float radius) =>
			B2Native.b2MakeOffsetRoundedBox(halfWidth, halfHeight, center, rotation, radius);

		public static B2Polygon MakeFromHull(in B2Hull hull, float radius = 0f) => B2Native.b2MakePolygon(in hull, radius);

		public static B2Polygon MakeOffsetFromHull(in B2Hull hull, B2Vec2 position, B2Rot rotation) =>
			B2Native.b2MakeOffsetPolygon(in hull, position, rotation);

		public static B2Polygon MakeOffsetRoundedFromHull(in B2Hull hull, B2Vec2 position, B2Rot rotation, float radius) =>
			B2Native.b2MakeOffsetRoundedPolygon(in hull, position, rotation, radius);

		public B2Polygon TransformedBy(B2Transform transform) => B2Native.b2TransformPolygon(transform, in this);

		public B2MassData ComputeMass(float density) => B2Native.b2ComputePolygonMass(in this, density);
		public B2AABB ComputeAABB(B2Transform transform) => B2Native.b2ComputePolygonAABB(in this, transform);
		public bool ContainsPoint(B2Vec2 point) => B2Native.b2PointInPolygon(in this, point);
		public B2CastOutput RayCast(in B2RayCastInput input) => B2Native.b2RayCastPolygon(in this, in input);
		public B2CastOutput ShapeCast(in B2ShapeCastInput input) => B2Native.b2ShapeCastPolygon(in this, in input);
	}

	/// A convex hull, used to build a B2Polygon. Do not modify once computed. Matches b2Hull.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Hull
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = B2Constants.MaxPolygonVertices)]
		public B2Vec2[] points;

		public int count;

		public static B2Hull Compute(B2Vec2[] points) => B2Native.b2ComputeHull(points, points.Length);
		public bool Validate() => B2Native.b2ValidateHull(in this);
	}

	/// A point cloud with a radius, used by the GJK-based shape queries. Matches b2ShapeProxy.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ShapeProxy
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = B2Constants.MaxPolygonVertices)]
		public B2Vec2[] points;

		public int count;
		public float radius;

		public static B2ShapeProxy Make(B2Vec2[] points, float radius) => B2Native.b2MakeProxy(points, points.Length, radius);

		public static B2ShapeProxy MakeOffset(B2Vec2[] points, float radius, B2Vec2 position, B2Rot rotation) =>
			B2Native.b2MakeOffsetProxy(points, points.Length, radius, position, rotation);
	}

	/// Low level ray cast input. Matches b2RayCastInput.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2RayCastInput
	{
		public B2Vec2 origin;
		public B2Vec2 translation;
		public float maxFraction;

		public B2RayCastInput(B2Vec2 origin, B2Vec2 translation, float maxFraction = 1f)
		{
			this.origin = origin;
			this.translation = translation;
			this.maxFraction = maxFraction;
		}

		public bool IsValid() => B2Native.b2IsValidRay(in this);
	}

	/// Low level shape cast input. Matches b2ShapeCastInput.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ShapeCastInput
	{
		public B2ShapeProxy proxy;
		public B2Vec2 translation;
		public float maxFraction;
		private byte canEncroach;

		public bool CanEncroach
		{
			get => canEncroach != 0;
			set => canEncroach = B2Bool.From(value);
		}
	}

	/// Low level ray/shape cast output. Matches b2CastOutput (single precision == b2WorldCastOutput).
	[StructLayout(LayoutKind.Sequential)]
	public struct B2CastOutput
	{
		public B2Vec2 normal;
		public B2Vec2 point;
		public float fraction;
		public int iterations;
		private byte hit;

		public bool Hit => hit != 0;
	}
}
