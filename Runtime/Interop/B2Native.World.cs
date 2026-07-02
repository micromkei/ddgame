using System;
using System.Runtime.InteropServices;

namespace Box2D.Interop
{
	/// P/Invoke declarations for the World section of box2d.h. The Recording, Snapshot, and
	/// Replay APIs (b2CreateRecording.., b2World_Snapshot/Restore, b2RecPlayer_*) are
	/// intentionally not bound - see the package README.
	internal static partial class B2Native
	{
		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2WorldId b2CreateWorld(in B2WorldDef def);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DestroyWorld(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2World_IsValid(B2WorldId id);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_Step(B2WorldId worldId, float timeStep, int subStepCount);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_Draw(B2WorldId worldId, ref B2NativeDebugDraw draw);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2AABB b2World_GetBounds(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2BodyEvents b2World_GetBodyEvents(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2SensorEvents b2World_GetSensorEvents(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ContactEvents b2World_GetContactEvents(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2JointEvents b2World_GetJointEvents(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2TreeStats b2World_OverlapAABB(B2WorldId worldId, B2Vec2 origin, B2AABB aabb, B2QueryFilter filter,
			B2OverlapResultFcn fcn, IntPtr context);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2TreeStats b2World_OverlapShape(B2WorldId worldId, B2Vec2 origin, in B2ShapeProxy proxy, B2QueryFilter filter,
			B2OverlapResultFcn fcn, IntPtr context);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2TreeStats b2World_CastRay(B2WorldId worldId, B2Vec2 origin, B2Vec2 translation, B2QueryFilter filter,
			B2CastResultFcn fcn, IntPtr context);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2RayResult b2World_CastRayClosest(B2WorldId worldId, B2Vec2 origin, B2Vec2 translation, B2QueryFilter filter);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2TreeStats b2World_CastShape(B2WorldId worldId, B2Vec2 origin, in B2ShapeProxy proxy, B2Vec2 translation,
			B2QueryFilter filter, B2CastResultFcn fcn, IntPtr context);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_EnableSleeping(B2WorldId worldId, [MarshalAs(UnmanagedType.I1)] bool flag);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2World_IsSleepingEnabled(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_EnableContinuous(B2WorldId worldId, [MarshalAs(UnmanagedType.I1)] bool flag);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2World_IsContinuousEnabled(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_SetRestitutionThreshold(B2WorldId worldId, float value);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2World_GetRestitutionThreshold(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_SetHitEventThreshold(B2WorldId worldId, float value);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2World_GetHitEventThreshold(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_SetCustomFilterCallback(B2WorldId worldId, B2CustomFilterFcn fcn, IntPtr context);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_SetPreSolveCallback(B2WorldId worldId, B2PreSolveFcn fcn, IntPtr context);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_SetGravity(B2WorldId worldId, B2Vec2 gravity);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2World_GetGravity(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_Explode(B2WorldId worldId, in B2ExplosionDef explosionDef);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_SetContactTuning(B2WorldId worldId, float hertz, float dampingRatio, float pushSpeed);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_SetContactRecycleDistance(B2WorldId worldId, float recycleDistance);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2World_GetContactRecycleDistance(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_SetMaximumLinearSpeed(B2WorldId worldId, float maximumLinearSpeed);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2World_GetMaximumLinearSpeed(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_EnableWarmStarting(B2WorldId worldId, [MarshalAs(UnmanagedType.I1)] bool flag);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2World_IsWarmStartingEnabled(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2World_GetAwakeBodyCount(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Profile b2World_GetProfile(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Counters b2World_GetCounters(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Capacity b2World_GetMaxCapacity(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_SetUserData(B2WorldId worldId, IntPtr userData);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr b2World_GetUserData(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_SetFrictionCallback(B2WorldId worldId, B2FrictionCallback callback);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_SetRestitutionCallback(B2WorldId worldId, B2RestitutionCallback callback);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2World_SetWorkerCount(B2WorldId worldId, int count);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2World_GetWorkerCount(B2WorldId worldId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2HexColor b2GetGraphColor(int index);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Contact_IsValid(B2ContactId id);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2ContactData b2Contact_GetData(B2ContactId contactId);
	}
}
