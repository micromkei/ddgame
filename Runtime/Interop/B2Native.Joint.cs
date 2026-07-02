using System;
using System.Runtime.InteropServices;

namespace Box2D.Interop
{
	/// P/Invoke declarations for the generic Joint API and all 7 joint type APIs in box2d.h.
	internal static partial class B2Native
	{
		// ---- generic joint ----

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DestroyJoint(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool wakeAttached);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Joint_IsValid(B2JointId id);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2JointType b2Joint_GetType(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2BodyId b2Joint_GetBodyA(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2BodyId b2Joint_GetBodyB(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2WorldId b2Joint_GetWorld(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Joint_SetLocalFrameA(B2JointId jointId, B2Transform localFrame);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Transform b2Joint_GetLocalFrameA(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Joint_SetLocalFrameB(B2JointId jointId, B2Transform localFrame);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Transform b2Joint_GetLocalFrameB(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Joint_SetCollideConnected(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool shouldCollide);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2Joint_GetCollideConnected(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Joint_SetUserData(B2JointId jointId, IntPtr userData);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr b2Joint_GetUserData(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Joint_WakeBodies(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2Joint_GetConstraintForce(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Joint_GetConstraintTorque(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Joint_GetLinearSeparation(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Joint_GetAngularSeparation(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Joint_SetConstraintTuning(B2JointId jointId, float hertz, float dampingRatio);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Joint_GetConstraintTuning(B2JointId jointId, out float hertz, out float dampingRatio);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Joint_SetForceThreshold(B2JointId jointId, float threshold);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Joint_GetForceThreshold(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Joint_SetTorqueThreshold(B2JointId jointId, float threshold);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Joint_GetTorqueThreshold(B2JointId jointId);

		// ---- distance joint ----

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2JointId b2CreateDistanceJoint(B2WorldId worldId, in B2DistanceJointDef def);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DistanceJoint_SetLength(B2JointId jointId, float length);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2DistanceJoint_GetLength(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DistanceJoint_EnableSpring(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool enableSpring);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2DistanceJoint_IsSpringEnabled(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DistanceJoint_SetSpringForceRange(B2JointId jointId, float lowerForce, float upperForce);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DistanceJoint_GetSpringForceRange(B2JointId jointId, out float lowerForce, out float upperForce);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DistanceJoint_SetSpringHertz(B2JointId jointId, float hertz);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DistanceJoint_SetSpringDampingRatio(B2JointId jointId, float dampingRatio);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2DistanceJoint_GetSpringHertz(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2DistanceJoint_GetSpringDampingRatio(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DistanceJoint_EnableLimit(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool enableLimit);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2DistanceJoint_IsLimitEnabled(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DistanceJoint_SetLengthRange(B2JointId jointId, float minLength, float maxLength);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2DistanceJoint_GetMinLength(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2DistanceJoint_GetMaxLength(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2DistanceJoint_GetCurrentLength(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DistanceJoint_EnableMotor(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool enableMotor);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2DistanceJoint_IsMotorEnabled(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DistanceJoint_SetMotorSpeed(B2JointId jointId, float motorSpeed);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2DistanceJoint_GetMotorSpeed(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2DistanceJoint_SetMaxMotorForce(B2JointId jointId, float force);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2DistanceJoint_GetMaxMotorForce(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2DistanceJoint_GetMotorForce(B2JointId jointId);

		// ---- motor joint ----

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2JointId b2CreateMotorJoint(B2WorldId worldId, in B2MotorJointDef def);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2MotorJoint_SetLinearVelocity(B2JointId jointId, B2Vec2 velocity);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Vec2 b2MotorJoint_GetLinearVelocity(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2MotorJoint_SetAngularVelocity(B2JointId jointId, float velocity);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2MotorJoint_GetAngularVelocity(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2MotorJoint_SetMaxVelocityForce(B2JointId jointId, float maxForce);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2MotorJoint_GetMaxVelocityForce(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2MotorJoint_SetMaxVelocityTorque(B2JointId jointId, float maxTorque);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2MotorJoint_GetMaxVelocityTorque(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2MotorJoint_SetLinearHertz(B2JointId jointId, float hertz);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2MotorJoint_GetLinearHertz(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2MotorJoint_SetLinearDampingRatio(B2JointId jointId, float damping);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2MotorJoint_GetLinearDampingRatio(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2MotorJoint_SetAngularHertz(B2JointId jointId, float hertz);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2MotorJoint_GetAngularHertz(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2MotorJoint_SetAngularDampingRatio(B2JointId jointId, float damping);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2MotorJoint_GetAngularDampingRatio(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2MotorJoint_SetMaxSpringForce(B2JointId jointId, float maxForce);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2MotorJoint_GetMaxSpringForce(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2MotorJoint_SetMaxSpringTorque(B2JointId jointId, float maxTorque);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2MotorJoint_GetMaxSpringTorque(B2JointId jointId);

		// ---- filter joint ----

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2JointId b2CreateFilterJoint(B2WorldId worldId, in B2FilterJointDef def);

		// ---- prismatic joint ----

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2JointId b2CreatePrismaticJoint(B2WorldId worldId, in B2PrismaticJointDef def);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2PrismaticJoint_EnableSpring(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool enableSpring);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2PrismaticJoint_IsSpringEnabled(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2PrismaticJoint_SetSpringHertz(B2JointId jointId, float hertz);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2PrismaticJoint_GetSpringHertz(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2PrismaticJoint_SetSpringDampingRatio(B2JointId jointId, float dampingRatio);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2PrismaticJoint_GetSpringDampingRatio(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2PrismaticJoint_SetTargetTranslation(B2JointId jointId, float translation);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2PrismaticJoint_GetTargetTranslation(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2PrismaticJoint_EnableLimit(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool enableLimit);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2PrismaticJoint_IsLimitEnabled(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2PrismaticJoint_GetLowerLimit(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2PrismaticJoint_GetUpperLimit(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2PrismaticJoint_SetLimits(B2JointId jointId, float lower, float upper);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2PrismaticJoint_EnableMotor(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool enableMotor);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2PrismaticJoint_IsMotorEnabled(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2PrismaticJoint_SetMotorSpeed(B2JointId jointId, float motorSpeed);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2PrismaticJoint_GetMotorSpeed(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2PrismaticJoint_SetMaxMotorForce(B2JointId jointId, float force);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2PrismaticJoint_GetMaxMotorForce(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2PrismaticJoint_GetMotorForce(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2PrismaticJoint_GetTranslation(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2PrismaticJoint_GetSpeed(B2JointId jointId);

		// ---- revolute joint ----

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2JointId b2CreateRevoluteJoint(B2WorldId worldId, in B2RevoluteJointDef def);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2RevoluteJoint_EnableSpring(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool enableSpring);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2RevoluteJoint_IsSpringEnabled(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2RevoluteJoint_SetSpringHertz(B2JointId jointId, float hertz);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2RevoluteJoint_GetSpringHertz(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2RevoluteJoint_SetSpringDampingRatio(B2JointId jointId, float dampingRatio);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2RevoluteJoint_GetSpringDampingRatio(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2RevoluteJoint_SetTargetAngle(B2JointId jointId, float angle);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2RevoluteJoint_GetTargetAngle(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2RevoluteJoint_GetAngle(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2RevoluteJoint_EnableLimit(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool enableLimit);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2RevoluteJoint_IsLimitEnabled(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2RevoluteJoint_GetLowerLimit(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2RevoluteJoint_GetUpperLimit(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2RevoluteJoint_SetLimits(B2JointId jointId, float lower, float upper);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2RevoluteJoint_EnableMotor(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool enableMotor);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2RevoluteJoint_IsMotorEnabled(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2RevoluteJoint_SetMotorSpeed(B2JointId jointId, float motorSpeed);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2RevoluteJoint_GetMotorSpeed(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2RevoluteJoint_GetMotorTorque(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2RevoluteJoint_SetMaxMotorTorque(B2JointId jointId, float torque);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2RevoluteJoint_GetMaxMotorTorque(B2JointId jointId);

		// ---- weld joint ----

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2JointId b2CreateWeldJoint(B2WorldId worldId, in B2WeldJointDef def);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2WeldJoint_SetLinearHertz(B2JointId jointId, float hertz);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2WeldJoint_GetLinearHertz(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2WeldJoint_SetLinearDampingRatio(B2JointId jointId, float dampingRatio);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2WeldJoint_GetLinearDampingRatio(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2WeldJoint_SetAngularHertz(B2JointId jointId, float hertz);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2WeldJoint_GetAngularHertz(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2WeldJoint_SetAngularDampingRatio(B2JointId jointId, float dampingRatio);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2WeldJoint_GetAngularDampingRatio(B2JointId jointId);

		// ---- wheel joint ----

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2JointId b2CreateWheelJoint(B2WorldId worldId, in B2WheelJointDef def);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2WheelJoint_EnableSpring(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool enableSpring);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2WheelJoint_IsSpringEnabled(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2WheelJoint_SetSpringHertz(B2JointId jointId, float hertz);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2WheelJoint_GetSpringHertz(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2WheelJoint_SetSpringDampingRatio(B2JointId jointId, float dampingRatio);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2WheelJoint_GetSpringDampingRatio(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2WheelJoint_EnableLimit(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool enableLimit);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2WheelJoint_IsLimitEnabled(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2WheelJoint_GetLowerLimit(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2WheelJoint_GetUpperLimit(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2WheelJoint_SetLimits(B2JointId jointId, float lower, float upper);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2WheelJoint_EnableMotor(B2JointId jointId, [MarshalAs(UnmanagedType.I1)] bool enableMotor);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2WheelJoint_IsMotorEnabled(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2WheelJoint_SetMotorSpeed(B2JointId jointId, float motorSpeed);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2WheelJoint_GetMotorSpeed(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2WheelJoint_SetMaxMotorTorque(B2JointId jointId, float torque);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2WheelJoint_GetMaxMotorTorque(B2JointId jointId);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2WheelJoint_GetMotorTorque(B2JointId jointId);
	}
}
