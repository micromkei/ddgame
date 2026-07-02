using System;
using Box2D.Interop;

namespace Box2D
{
	/// Controls the relative velocity and/or transform between two bodies. With a velocity of
	/// zero this acts like top-down friction.
	public readonly struct B2MotorJoint : IEquatable<B2MotorJoint>
	{
		public readonly B2JointId Id;

		public B2MotorJoint(B2JointId id)
		{
			Id = id;
		}

		public static B2MotorJoint Create(B2World world, B2MotorJointDef def) =>
			new B2MotorJoint(B2Native.b2CreateMotorJoint(world.Id, in def));

		public B2Joint Generic => new B2Joint(Id);

		public B2Vec2 LinearVelocity
		{
			get => B2Native.b2MotorJoint_GetLinearVelocity(Id);
			set => B2Native.b2MotorJoint_SetLinearVelocity(Id, value);
		}

		public float AngularVelocity
		{
			get => B2Native.b2MotorJoint_GetAngularVelocity(Id);
			set => B2Native.b2MotorJoint_SetAngularVelocity(Id, value);
		}

		public float MaxVelocityForce
		{
			get => B2Native.b2MotorJoint_GetMaxVelocityForce(Id);
			set => B2Native.b2MotorJoint_SetMaxVelocityForce(Id, value);
		}

		public float MaxVelocityTorque
		{
			get => B2Native.b2MotorJoint_GetMaxVelocityTorque(Id);
			set => B2Native.b2MotorJoint_SetMaxVelocityTorque(Id, value);
		}

		public float LinearHertz
		{
			get => B2Native.b2MotorJoint_GetLinearHertz(Id);
			set => B2Native.b2MotorJoint_SetLinearHertz(Id, value);
		}

		public float LinearDampingRatio
		{
			get => B2Native.b2MotorJoint_GetLinearDampingRatio(Id);
			set => B2Native.b2MotorJoint_SetLinearDampingRatio(Id, value);
		}

		public float MaxSpringForce
		{
			get => B2Native.b2MotorJoint_GetMaxSpringForce(Id);
			set => B2Native.b2MotorJoint_SetMaxSpringForce(Id, value);
		}

		public float AngularHertz
		{
			get => B2Native.b2MotorJoint_GetAngularHertz(Id);
			set => B2Native.b2MotorJoint_SetAngularHertz(Id, value);
		}

		public float AngularDampingRatio
		{
			get => B2Native.b2MotorJoint_GetAngularDampingRatio(Id);
			set => B2Native.b2MotorJoint_SetAngularDampingRatio(Id, value);
		}

		public float MaxSpringTorque
		{
			get => B2Native.b2MotorJoint_GetMaxSpringTorque(Id);
			set => B2Native.b2MotorJoint_SetMaxSpringTorque(Id, value);
		}

		public bool Equals(B2MotorJoint other) => Id.Equals(other.Id);
		public override bool Equals(object obj) => obj is B2MotorJoint other && Equals(other);
		public override int GetHashCode() => Id.GetHashCode();
		public static bool operator ==(B2MotorJoint a, B2MotorJoint b) => a.Equals(b);
		public static bool operator !=(B2MotorJoint a, B2MotorJoint b) => !a.Equals(b);

		public static implicit operator B2JointId(B2MotorJoint joint) => joint.Id;
		public static implicit operator B2Joint(B2MotorJoint joint) => new B2Joint(joint.Id);
	}
}
