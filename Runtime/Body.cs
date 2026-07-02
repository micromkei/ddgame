using System;
using Box2D.Interop;

namespace Box2D
{
	/// A rigid body. This is a thin, cheap-to-copy wrapper around a b2BodyId handle - it does not
	/// own any resources itself, so copying it around freely is fine.
	public readonly struct B2Body : IEquatable<B2Body>
	{
		public readonly B2BodyId Id;

		public B2Body(B2BodyId id)
		{
			Id = id;
		}

		public static B2Body Create(B2World world, B2BodyDef def) => new B2Body(B2Native.b2CreateBody(world.Id, in def));

		public void Destroy() => B2Native.b2DestroyBody(Id);

		public bool IsValid => B2Native.b2Body_IsValid(Id);

		public B2BodyType Type
		{
			get => B2Native.b2Body_GetType(Id);
			set => B2Native.b2Body_SetType(Id, value);
		}

		public string Name
		{
			get => B2Native.b2Body_GetName(Id);
			set => B2Native.b2Body_SetName(Id, value);
		}

		/// Wraps an arbitrary managed object in a GCHandle stored as Box2D's userData pointer.
		public object UserData
		{
			get => B2Marshal.GetUserData(B2Native.b2Body_GetUserData(Id));
			set => B2Native.b2Body_SetUserData(Id, B2Marshal.SetUserData(B2Native.b2Body_GetUserData(Id), value));
		}

		public B2Vec2 Position => B2Native.b2Body_GetPosition(Id);
		public B2Rot Rotation => B2Native.b2Body_GetRotation(Id);
		public B2Transform Transform => B2Native.b2Body_GetTransform(Id);

		/// Teleports the body. Prefer creating the body at the desired transform instead - this
		/// is fairly expensive.
		public void SetTransform(B2Vec2 position, B2Rot rotation) => B2Native.b2Body_SetTransform(Id, position, rotation);

		public B2Vec2 GetLocalPoint(B2Vec2 worldPoint) => B2Native.b2Body_GetLocalPoint(Id, worldPoint);
		public B2Vec2 GetWorldPoint(B2Vec2 localPoint) => B2Native.b2Body_GetWorldPoint(Id, localPoint);
		public B2Vec2 GetLocalVector(B2Vec2 worldVector) => B2Native.b2Body_GetLocalVector(Id, worldVector);
		public B2Vec2 GetWorldVector(B2Vec2 localVector) => B2Native.b2Body_GetWorldVector(Id, localVector);

		public B2Vec2 LinearVelocity
		{
			get => B2Native.b2Body_GetLinearVelocity(Id);
			set => B2Native.b2Body_SetLinearVelocity(Id, value);
		}

		public float AngularVelocity
		{
			get => B2Native.b2Body_GetAngularVelocity(Id);
			set => B2Native.b2Body_SetAngularVelocity(Id, value);
		}

		/// Sets the velocity needed to reach `target` after `timeStep` seconds. Intended for
		/// kinematic bodies.
		public void SetTargetTransform(B2Transform target, float timeStep, bool wake = true) =>
			B2Native.b2Body_SetTargetTransform(Id, target, timeStep, wake);

		public B2Vec2 GetLocalPointVelocity(B2Vec2 localPoint) => B2Native.b2Body_GetLocalPointVelocity(Id, localPoint);
		public B2Vec2 GetWorldPointVelocity(B2Vec2 worldPoint) => B2Native.b2Body_GetWorldPointVelocity(Id, worldPoint);

		public void ApplyForce(B2Vec2 force, B2Vec2 point, bool wake = true) => B2Native.b2Body_ApplyForce(Id, force, point, wake);
		public void ApplyForceToCenter(B2Vec2 force, bool wake = true) => B2Native.b2Body_ApplyForceToCenter(Id, force, wake);
		public void ApplyTorque(float torque, bool wake = true) => B2Native.b2Body_ApplyTorque(Id, torque, wake);
		public void ClearForces() => B2Native.b2Body_ClearForces(Id);

		public void ApplyLinearImpulse(B2Vec2 impulse, B2Vec2 point, bool wake = true) =>
			B2Native.b2Body_ApplyLinearImpulse(Id, impulse, point, wake);

		public void ApplyLinearImpulseToCenter(B2Vec2 impulse, bool wake = true) =>
			B2Native.b2Body_ApplyLinearImpulseToCenter(Id, impulse, wake);

		public void ApplyAngularImpulse(float impulse, bool wake = true) => B2Native.b2Body_ApplyAngularImpulse(Id, impulse, wake);

		public float Mass => B2Native.b2Body_GetMass(Id);
		public float RotationalInertia => B2Native.b2Body_GetRotationalInertia(Id);
		public B2Vec2 LocalCenter => B2Native.b2Body_GetLocalCenter(Id);
		public B2Vec2 WorldCenter => B2Native.b2Body_GetWorldCenter(Id);

		public B2MassData MassData
		{
			get => B2Native.b2Body_GetMassData(Id);
			set => B2Native.b2Body_SetMassData(Id, value);
		}

		public void ApplyMassFromShapes() => B2Native.b2Body_ApplyMassFromShapes(Id);

		public float LinearDamping
		{
			get => B2Native.b2Body_GetLinearDamping(Id);
			set => B2Native.b2Body_SetLinearDamping(Id, value);
		}

		public float AngularDamping
		{
			get => B2Native.b2Body_GetAngularDamping(Id);
			set => B2Native.b2Body_SetAngularDamping(Id, value);
		}

		public float GravityScale
		{
			get => B2Native.b2Body_GetGravityScale(Id);
			set => B2Native.b2Body_SetGravityScale(Id, value);
		}

		public bool IsAwake
		{
			get => B2Native.b2Body_IsAwake(Id);
			set => B2Native.b2Body_SetAwake(Id, value);
		}

		/// Wakes bodies touching this body. Works for static bodies.
		public void WakeTouching() => B2Native.b2Body_WakeTouching(Id);

		public bool EnableSleep
		{
			get => B2Native.b2Body_IsSleepEnabled(Id);
			set => B2Native.b2Body_EnableSleep(Id, value);
		}

		public float SleepThreshold
		{
			get => B2Native.b2Body_GetSleepThreshold(Id);
			set => B2Native.b2Body_SetSleepThreshold(Id, value);
		}

		public bool IsEnabled => B2Native.b2Body_IsEnabled(Id);
		public void Disable() => B2Native.b2Body_Disable(Id);
		public void Enable() => B2Native.b2Body_Enable(Id);

		public B2MotionLocks MotionLocks
		{
			get => B2Native.b2Body_GetMotionLocks(Id);
			set => B2Native.b2Body_SetMotionLocks(Id, value);
		}

		public bool IsBullet
		{
			get => B2Native.b2Body_IsBullet(Id);
			set => B2Native.b2Body_SetBullet(Id, value);
		}

		public bool EnableContactRecycling
		{
			get => B2Native.b2Body_IsContactRecyclingEnabled(Id);
			set => B2Native.b2Body_EnableContactRecycling(Id, value);
		}

		public void EnableContactEvents(bool flag) => B2Native.b2Body_EnableContactEvents(Id, flag);
		public void EnableHitEvents(bool flag) => B2Native.b2Body_EnableHitEvents(Id, flag);

		public B2World GetWorld() => new B2World(B2Native.b2Body_GetWorld(Id));

		public int ShapeCount => B2Native.b2Body_GetShapeCount(Id);

		public B2Shape[] GetShapes()
		{
			var ids = new B2ShapeId[ShapeCount];
			int n = B2Native.b2Body_GetShapes(Id, ids, ids.Length);
			var shapes = new B2Shape[n];
			for (int i = 0; i < n; i++) shapes[i] = new B2Shape(ids[i]);
			return shapes;
		}

		public int JointCount => B2Native.b2Body_GetJointCount(Id);

		public B2JointId[] GetJoints()
		{
			var ids = new B2JointId[JointCount];
			int n = B2Native.b2Body_GetJoints(Id, ids, ids.Length);
			if (n != ids.Length) Array.Resize(ref ids, n);
			return ids;
		}

		public int ContactCapacity => B2Native.b2Body_GetContactCapacity(Id);

		public B2ContactData[] GetContactData()
		{
			var data = new B2ContactData[ContactCapacity];
			int n = B2Native.b2Body_GetContactData(Id, data, data.Length);
			if (n != data.Length) Array.Resize(ref data, n);
			return data;
		}

		public B2AABB ComputeAABB() => B2Native.b2Body_ComputeAABB(Id);

		public bool Equals(B2Body other) => Id.Equals(other.Id);
		public override bool Equals(object obj) => obj is B2Body other && Equals(other);
		public override int GetHashCode() => Id.GetHashCode();
		public static bool operator ==(B2Body a, B2Body b) => a.Equals(b);
		public static bool operator !=(B2Body a, B2Body b) => !a.Equals(b);

		public static implicit operator B2BodyId(B2Body body) => body.Id;
	}
}
