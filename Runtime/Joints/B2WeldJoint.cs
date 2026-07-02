using System;
using Box2D.Interop;

namespace Box2D
{
	/// Rigidly connects two bodies together, optionally with damped springs for both linear and
	/// angular give. Note: long chains of weld joints may flex since the solver is approximate.
	public readonly struct B2WeldJoint : IEquatable<B2WeldJoint>
	{
		public readonly B2JointId Id;

		public B2WeldJoint(B2JointId id)
		{
			Id = id;
		}

		public static B2WeldJoint Create(B2World world, B2WeldJointDef def) =>
			new B2WeldJoint(B2Native.b2CreateWeldJoint(world.Id, in def));

		public B2Joint Generic => new B2Joint(Id);

		public float LinearHertz
		{
			get => B2Native.b2WeldJoint_GetLinearHertz(Id);
			set => B2Native.b2WeldJoint_SetLinearHertz(Id, value);
		}

		public float LinearDampingRatio
		{
			get => B2Native.b2WeldJoint_GetLinearDampingRatio(Id);
			set => B2Native.b2WeldJoint_SetLinearDampingRatio(Id, value);
		}

		public float AngularHertz
		{
			get => B2Native.b2WeldJoint_GetAngularHertz(Id);
			set => B2Native.b2WeldJoint_SetAngularHertz(Id, value);
		}

		public float AngularDampingRatio
		{
			get => B2Native.b2WeldJoint_GetAngularDampingRatio(Id);
			set => B2Native.b2WeldJoint_SetAngularDampingRatio(Id, value);
		}

		public bool Equals(B2WeldJoint other) => Id.Equals(other.Id);
		public override bool Equals(object obj) => obj is B2WeldJoint other && Equals(other);
		public override int GetHashCode() => Id.GetHashCode();
		public static bool operator ==(B2WeldJoint a, B2WeldJoint b) => a.Equals(b);
		public static bool operator !=(B2WeldJoint a, B2WeldJoint b) => !a.Equals(b);

		public static implicit operator B2JointId(B2WeldJoint joint) => joint.Id;
		public static implicit operator B2Joint(B2WeldJoint joint) => new B2Joint(joint.Id);
	}
}
