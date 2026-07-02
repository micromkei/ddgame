using System;
using System.Runtime.InteropServices;

namespace Box2D.Interop
{
	/// P/Invoke declarations for the Shape and Chain Shape sections of box2d.h.
	internal static partial class B2Native
	{
		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ShapeId b2CreateCircleShape(B2BodyId bodyId, in B2ShapeDef def, in B2Circle circle);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ShapeId b2CreateSegmentShape(B2BodyId bodyId, in B2ShapeDef def, in B2Segment segment);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ShapeId b2CreateChainSegmentShape(B2BodyId bodyId, in B2ShapeDef def, in B2ChainSegment chainSegment);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ShapeId b2CreateCapsuleShape(B2BodyId bodyId, in B2ShapeDef def, in B2Capsule capsule);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ShapeId b2CreatePolygonShape(B2BodyId bodyId, in B2ShapeDef def, in B2Polygon polygon);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DestroyShape(B2ShapeId shapeId, [MarshalAs(UnmanagedType.I1)] bool updateBodyMass);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Shape_IsValid(B2ShapeId id);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ShapeType b2Shape_GetType(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2BodyId b2Shape_GetBody(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2WorldId b2Shape_GetWorld(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Shape_IsSensor(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_SetUserData(B2ShapeId shapeId, IntPtr userData);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr b2Shape_GetUserData(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_SetDensity(B2ShapeId shapeId, float density, [MarshalAs(UnmanagedType.I1)] bool updateBodyMass);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Shape_GetDensity(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_SetFriction(B2ShapeId shapeId, float friction);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Shape_GetFriction(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_SetRestitution(B2ShapeId shapeId, float restitution);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Shape_GetRestitution(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_SetUserMaterial(B2ShapeId shapeId, ulong material);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong b2Shape_GetUserMaterial(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_SetSurfaceMaterial(B2ShapeId shapeId, in B2SurfaceMaterial surfaceMaterial);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2SurfaceMaterial b2Shape_GetSurfaceMaterial(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Filter b2Shape_GetFilter(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_SetFilter(B2ShapeId shapeId, B2Filter filter);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_EnableSensorEvents(B2ShapeId shapeId, [MarshalAs(UnmanagedType.I1)] bool flag);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Shape_AreSensorEventsEnabled(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_EnableContactEvents(B2ShapeId shapeId, [MarshalAs(UnmanagedType.I1)] bool flag);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Shape_AreContactEventsEnabled(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_EnablePreSolveEvents(B2ShapeId shapeId, [MarshalAs(UnmanagedType.I1)] bool flag);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Shape_ArePreSolveEventsEnabled(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_EnableHitEvents(B2ShapeId shapeId, [MarshalAs(UnmanagedType.I1)] bool flag);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Shape_AreHitEventsEnabled(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Shape_TestPoint(B2ShapeId shapeId, B2Vec2 point);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2CastOutput b2Shape_RayCast(B2ShapeId shapeId, B2Vec2 origin, B2Vec2 translation);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Circle b2Shape_GetCircle(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Segment b2Shape_GetSegment(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ChainSegment b2Shape_GetChainSegment(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Capsule b2Shape_GetCapsule(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Polygon b2Shape_GetPolygon(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_SetCircle(B2ShapeId shapeId, in B2Circle circle);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_SetCapsule(B2ShapeId shapeId, in B2Capsule capsule);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_SetSegment(B2ShapeId shapeId, in B2Segment segment);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_SetPolygon(B2ShapeId shapeId, in B2Polygon polygon);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_SetChainSegment(B2ShapeId shapeId, in B2ChainSegment chainSegment);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ChainId b2Shape_GetParentChain(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2Shape_GetContactCapacity(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2Shape_GetContactData(B2ShapeId shapeId, [Out] B2ContactData[] contactData, int capacity);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2Shape_GetSensorCapacity(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2Shape_GetSensorData(B2ShapeId shapeId, [Out] B2ShapeId[] visitorIds, int capacity);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2AABB b2Shape_GetAABB(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2MassData b2Shape_ComputeMassData(B2ShapeId shapeId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2Shape_GetClosestPoint(B2ShapeId shapeId, B2Vec2 target);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Shape_ApplyWind(B2ShapeId shapeId, B2Vec2 wind, float drag, float lift, [MarshalAs(UnmanagedType.I1)] bool wake);

		// ---- Chain Shape ----

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ChainId b2CreateChain(B2BodyId bodyId, in B2ChainDef def);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DestroyChain(B2ChainId chainId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2WorldId b2Chain_GetWorld(B2ChainId chainId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2Chain_GetSegmentCount(B2ChainId chainId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2Chain_GetSegments(B2ChainId chainId, [Out] B2ShapeId[] segmentArray, int capacity);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2Chain_GetSurfaceMaterialCount(B2ChainId chainId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Chain_SetSurfaceMaterial(B2ChainId chainId, in B2SurfaceMaterial material, int materialIndex);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2SurfaceMaterial b2Chain_GetSurfaceMaterial(B2ChainId chainId, int materialIndex);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Chain_IsValid(B2ChainId id);
	}
}
