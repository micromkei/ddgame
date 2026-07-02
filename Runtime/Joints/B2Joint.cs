using System;
using Box2D.Interop;

namespace Box2D
{
	/// Generic operations shared by every joint type. Get this from a specific joint wrapper's
	/// `.Generic` property, or construct directly from a B2JointId (e.g. one returned by
	/// B2Body.GetJoints()) when you don't know the concrete joint type up front - use `.Type` to
	/// find out and then wrap it in the matching specific struct if you need type-specific access.
	public readonly struct B2Joint : IEquatable<B2Joint>
	{
		public readonly B2JointId Id;

		public B2Joint(B2JointId id)
		{
			Id = id;
		}

		public void Destroy(bool wakeAttached = true) => B2Native.b2DestroyJoint(Id, wakeAttached);

		public bool IsValid => B2Native.b2Joint_IsValid(Id);
		public B2JointType Type => B2Native.b2Joint_GetType(Id);
		public B2Body GetBodyA() => new B2Body(B2Native.b2Joint_GetBodyA(Id));
		public B2Body GetBodyB() => new B2Body(B2Native.b2Joint_GetBodyB(Id));
		public B2World GetWorld() => new B2World(B2Native.b2Joint_GetWorld(Id));

		public B2Transform LocalFrameA
		{
			get => B2Native.b2Joint_GetLocalFrameA(Id);
			set => B2Native.b2Joint_SetLocalFrameA(Id, value);
		}

		public B2Transform LocalFrameB
		{
			get => B2Native.b2Joint_GetLocalFrameB(Id);
			set => B2Native.b2Joint_SetLocalFrameB(Id, value);
		}

		public bool CollideConnected
		{
			get => B2Native.b2Joint_GetCollideConnected(Id);
			set => B2Native.b2Joint_SetCollideConnected(Id, value);
		}

		/// Wraps an arbitrary managed object in a GCHandle stored as Box2D's userData pointer.
		public object UserData
		{
			get => B2Marshal.GetUserData(B2Native.b2Joint_GetUserData(Id));
			set => B2Native.b2Joint_SetUserData(Id, B2Marshal.SetUserData(B2Native.b2Joint_GetUserData(Id), value));
		}

		public void WakeBodies() => B2Native.b2Joint_WakeBodies(Id);

		public B2Vec2 GetConstraintForce() => B2Native.b2Joint_GetConstraintForce(Id);
		public float GetConstraintTorque() => B2Native.b2Joint_GetConstraintTorque(Id);
		public float GetLinearSeparation() => B2Native.b2Joint_GetLinearSeparation(Id);
		public float GetAngularSeparation() => B2Native.b2Joint_GetAngularSeparation(Id);

		public void SetConstraintTuning(float hertz, float dampingRatio) => B2Native.b2Joint_SetConstraintTuning(Id, hertz, dampingRatio);

		public void GetConstraintTuning(out float hertz, out float dampingRatio) =>
			B2Native.b2Joint_GetConstraintTuning(Id, out hertz, out dampingRatio);

		public float ForceThreshold
		{
			get => B2Native.b2Joint_GetForceThreshold(Id);
			set => B2Native.b2Joint_SetForceThreshold(Id, value);
		}

		public float TorqueThreshold
		{
			get => B2Native.b2Joint_GetTorqueThreshold(Id);
			set => B2Native.b2Joint_SetTorqueThreshold(Id, value);
		}

		public bool Equals(B2Joint other) => Id.Equals(other.Id);
		public override bool Equals(object obj) => obj is B2Joint other && Equals(other);
		public override int GetHashCode() => Id.GetHashCode();
		public static bool operator ==(B2Joint a, B2Joint b) => a.Equals(b);
		public static bool operator !=(B2Joint a, B2Joint b) => !a.Equals(b);

		public static implicit operator B2JointId(B2Joint joint) => joint.Id;
	}
}
