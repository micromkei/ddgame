using System;
using System.Runtime.InteropServices;

namespace Box2D.Interop
{
	/// P/Invoke declarations for the Body section of box2d.h.
	internal static partial class B2Native
	{
		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2BodyId b2CreateBody(B2WorldId worldId, in B2BodyDef def);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DestroyBody(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Body_IsValid(B2BodyId id);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2BodyType b2Body_GetType(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetType(B2BodyId bodyId, B2BodyType type);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetName(B2BodyId bodyId, [MarshalAs(UnmanagedType.LPStr)] string name);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		public static extern string b2Body_GetName(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetUserData(B2BodyId bodyId, IntPtr userData);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr b2Body_GetUserData(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2Body_GetPosition(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Rot b2Body_GetRotation(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Transform b2Body_GetTransform(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetTransform(B2BodyId bodyId, B2Vec2 position, B2Rot rotation);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2Body_GetLocalPoint(B2BodyId bodyId, B2Vec2 worldPoint);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2Body_GetWorldPoint(B2BodyId bodyId, B2Vec2 localPoint);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2Body_GetLocalVector(B2BodyId bodyId, B2Vec2 worldVector);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2Body_GetWorldVector(B2BodyId bodyId, B2Vec2 localVector);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2Body_GetLinearVelocity(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Body_GetAngularVelocity(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetLinearVelocity(B2BodyId bodyId, B2Vec2 linearVelocity);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetAngularVelocity(B2BodyId bodyId, float angularVelocity);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetTargetTransform(B2BodyId bodyId, B2Transform target, float timeStep, [MarshalAs(UnmanagedType.I1)] bool wake);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2Body_GetLocalPointVelocity(B2BodyId bodyId, B2Vec2 localPoint);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2Body_GetWorldPointVelocity(B2BodyId bodyId, B2Vec2 worldPoint);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_ApplyForce(B2BodyId bodyId, B2Vec2 force, B2Vec2 point, [MarshalAs(UnmanagedType.I1)] bool wake);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_ApplyForceToCenter(B2BodyId bodyId, B2Vec2 force, [MarshalAs(UnmanagedType.I1)] bool wake);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_ApplyTorque(B2BodyId bodyId, float torque, [MarshalAs(UnmanagedType.I1)] bool wake);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_ClearForces(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_ApplyLinearImpulse(B2BodyId bodyId, B2Vec2 impulse, B2Vec2 point, [MarshalAs(UnmanagedType.I1)] bool wake);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_ApplyLinearImpulseToCenter(B2BodyId bodyId, B2Vec2 impulse, [MarshalAs(UnmanagedType.I1)] bool wake);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_ApplyAngularImpulse(B2BodyId bodyId, float impulse, [MarshalAs(UnmanagedType.I1)] bool wake);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Body_GetMass(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Body_GetRotationalInertia(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2Body_GetLocalCenter(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2Body_GetWorldCenter(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetMassData(B2BodyId bodyId, B2MassData massData);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2MassData b2Body_GetMassData(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_ApplyMassFromShapes(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetLinearDamping(B2BodyId bodyId, float linearDamping);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Body_GetLinearDamping(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetAngularDamping(B2BodyId bodyId, float angularDamping);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Body_GetAngularDamping(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetGravityScale(B2BodyId bodyId, float gravityScale);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Body_GetGravityScale(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Body_IsAwake(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetAwake(B2BodyId bodyId, [MarshalAs(UnmanagedType.I1)] bool awake);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_WakeTouching(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_EnableSleep(B2BodyId bodyId, [MarshalAs(UnmanagedType.I1)] bool enableSleep);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Body_IsSleepEnabled(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetSleepThreshold(B2BodyId bodyId, float sleepThreshold);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Body_GetSleepThreshold(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Body_IsEnabled(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_Disable(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_Enable(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetMotionLocks(B2BodyId bodyId, B2MotionLocks locks);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2MotionLocks b2Body_GetMotionLocks(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_SetBullet(B2BodyId bodyId, [MarshalAs(UnmanagedType.I1)] bool flag);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Body_IsBullet(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_EnableContactRecycling(B2BodyId bodyId, [MarshalAs(UnmanagedType.I1)] bool flag);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Body_IsContactRecyclingEnabled(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_EnableContactEvents(B2BodyId bodyId, [MarshalAs(UnmanagedType.I1)] bool flag);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Body_EnableHitEvents(B2BodyId bodyId, [MarshalAs(UnmanagedType.I1)] bool flag);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2WorldId b2Body_GetWorld(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2Body_GetShapeCount(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2Body_GetShapes(B2BodyId bodyId, [Out] B2ShapeId[] shapeArray, int capacity);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2Body_GetJointCount(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2Body_GetJoints(B2BodyId bodyId, [Out] B2JointId[] jointArray, int capacity);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2Body_GetContactCapacity(B2BodyId bodyId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern int b2Body_GetContactData(B2BodyId bodyId, [Out] B2ContactData[] contactData, int capacity);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2AABB b2Body_ComputeAABB(B2BodyId bodyId);
	}
}
