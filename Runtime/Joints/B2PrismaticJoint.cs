using System;
using Box2D.Interop;

namespace Box2D
{
	/// Body B slides along the local x-axis of frame A with no relative rotation. Useful for
	/// pistons and moving platforms. Also called a slider joint.
	public readonly struct B2PrismaticJoint : IEquatable<B2PrismaticJoint>
	{
		public readonly B2JointId Id;

		public B2PrismaticJoint(B2JointId id)
		{
			Id = id;
		}

		public static B2PrismaticJoint Create(B2World world, B2PrismaticJointDef def) =>
			new B2PrismaticJoint(B2Native.b2CreatePrismaticJoint(world.Id, in def));

		public B2Joint Generic => new B2Joint(Id);

		public bool EnableSpring
		{
			get => B2Native.b2PrismaticJoint_IsSpringEnabled(Id);
			set => B2Native.b2PrismaticJoint_EnableSpring(Id, value);
		}

		public float SpringHertz
		{
			get => B2Native.b2PrismaticJoint_GetSpringHertz(Id);
			set => B2Native.b2PrismaticJoint_SetSpringHertz(Id, value);
		}

		public float SpringDampingRatio
		{
			get => B2Native.b2PrismaticJoint_GetSpringDampingRatio(Id);
			set => B2Native.b2PrismaticJoint_SetSpringDampingRatio(Id, value);
		}

		public float TargetTranslation
		{
			get => B2Native.b2PrismaticJoint_GetTargetTranslation(Id);
			set => B2Native.b2PrismaticJoint_SetTargetTranslation(Id, value);
		}

		public bool EnableLimit
		{
			get => B2Native.b2PrismaticJoint_IsLimitEnabled(Id);
			set => B2Native.b2PrismaticJoint_EnableLimit(Id, value);
		}

		public float LowerLimit => B2Native.b2PrismaticJoint_GetLowerLimit(Id);
		public float UpperLimit => B2Native.b2PrismaticJoint_GetUpperLimit(Id);
		public void SetLimits(float lower, float upper) => B2Native.b2PrismaticJoint_SetLimits(Id, lower, upper);

		public bool EnableMotor
		{
			get => B2Native.b2PrismaticJoint_IsMotorEnabled(Id);
			set => B2Native.b2PrismaticJoint_EnableMotor(Id, value);
		}

		public float MotorSpeed
		{
			get => B2Native.b2PrismaticJoint_GetMotorSpeed(Id);
			set => B2Native.b2PrismaticJoint_SetMotorSpeed(Id, value);
		}

		public float MaxMotorForce
		{
			get => B2Native.b2PrismaticJoint_GetMaxMotorForce(Id);
			set => B2Native.b2PrismaticJoint_SetMaxMotorForce(Id, value);
		}

		public float MotorForce => B2Native.b2PrismaticJoint_GetMotorForce(Id);
		public float Translation => B2Native.b2PrismaticJoint_GetTranslation(Id);
		public float Speed => B2Native.b2PrismaticJoint_GetSpeed(Id);

		public bool Equals(B2PrismaticJoint other) => Id.Equals(other.Id);
		public override bool Equals(object obj) => obj is B2PrismaticJoint other && Equals(other);
		public override int GetHashCode() => Id.GetHashCode();
		public static bool operator ==(B2PrismaticJoint a, B2PrismaticJoint b) => a.Equals(b);
		public static bool operator !=(B2PrismaticJoint a, B2PrismaticJoint b) => !a.Equals(b);

		public static implicit operator B2JointId(B2PrismaticJoint joint) => joint.Id;
		public static implicit operator B2Joint(B2PrismaticJoint joint) => new B2Joint(joint.Id);
	}
}
