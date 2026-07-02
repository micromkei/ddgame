using System;
using System.Runtime.InteropServices;
using Box2D.Interop;

namespace Box2D
{
	/// Optional world capacities that can be used to avoid run-time allocations. Matches b2Capacity.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Capacity
	{
		public int staticShapeCount;
		public int dynamicShapeCount;
		public int staticBodyCount;
		public int dynamicBodyCount;
		public int contactCount;
	}

	/// World definition used to create a simulation world. Always start from Default() -
	/// several fields are non-trivial tuning constants that only b2DefaultWorldDef() knows how
	/// to compute correctly. Matches b2WorldDef.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2WorldDef
	{
		public B2Vec2 gravity;
		public float restitutionThreshold;
		public float hitEventThreshold;
		public float contactHertz;
		public float contactDampingRatio;
		public float contactSpeed;
		public float maximumLinearSpeed;
		public IntPtr frictionCallback;
		public IntPtr restitutionCallback;
		private byte enableSleep;
		private byte enableContinuous;
		private byte enableContactSoftening;
		public int workerCount;
		public IntPtr enqueueTask;
		public IntPtr finishTask;
		public IntPtr userTaskContext;
		public IntPtr userData;
		public B2Capacity capacity;
		internal int internalValue;

		public bool EnableSleep
		{
			get => enableSleep != 0;
			set => enableSleep = B2Bool.From(value);
		}

		public bool EnableContinuous
		{
			get => enableContinuous != 0;
			set => enableContinuous = B2Bool.From(value);
		}

		public bool EnableContactSoftening
		{
			get => enableContactSoftening != 0;
			set => enableContactSoftening = B2Bool.From(value);
		}

		public static B2WorldDef Default() => B2Native.b2DefaultWorldDef();
	}

	/// A body definition holds all the data needed to construct a rigid body. Always start from
	/// Default(). Matches b2BodyDef.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2BodyDef
	{
		public B2BodyType type;
		public B2Vec2 position;
		public B2Rot rotation;
		public B2Vec2 linearVelocity;
		public float angularVelocity;
		public float linearDamping;
		public float angularDamping;
		public float gravityScale;
		public float sleepThreshold;

		[MarshalAs(UnmanagedType.LPStr)]
		public string name;

		public IntPtr userData;
		public B2MotionLocks motionLocks;

		private byte enableSleep;
		private byte isAwake;
		private byte isBullet;
		private byte isEnabled;
		private byte allowFastRotation;
		private byte enableContactRecycling;
		internal int internalValue;

		public bool EnableSleep
		{
			get => enableSleep != 0;
			set => enableSleep = B2Bool.From(value);
		}

		public bool IsAwake
		{
			get => isAwake != 0;
			set => isAwake = B2Bool.From(value);
		}

		public bool IsBullet
		{
			get => isBullet != 0;
			set => isBullet = B2Bool.From(value);
		}

		public bool IsEnabled
		{
			get => isEnabled != 0;
			set => isEnabled = B2Bool.From(value);
		}

		public bool AllowFastRotation
		{
			get => allowFastRotation != 0;
			set => allowFastRotation = B2Bool.From(value);
		}

		public bool EnableContactRecycling
		{
			get => enableContactRecycling != 0;
			set => enableContactRecycling = B2Bool.From(value);
		}

		public static B2BodyDef Default() => B2Native.b2DefaultBodyDef();
	}

	/// Used to create a shape. Always start from Default(). Matches b2ShapeDef.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ShapeDef
	{
		public IntPtr userData;
		public B2SurfaceMaterial material;
		public float density;
		public B2Filter filter;

		private byte enableCustomFiltering;
		private byte isSensor;
		private byte enableSensorEvents;
		private byte enableContactEvents;
		private byte enableHitEvents;
		private byte enablePreSolveEvents;
		private byte invokeContactCreation;
		private byte updateBodyMass;
		internal int internalValue;

		public bool EnableCustomFiltering
		{
			get => enableCustomFiltering != 0;
			set => enableCustomFiltering = B2Bool.From(value);
		}

		public bool IsSensor
		{
			get => isSensor != 0;
			set => isSensor = B2Bool.From(value);
		}

		public bool EnableSensorEvents
		{
			get => enableSensorEvents != 0;
			set => enableSensorEvents = B2Bool.From(value);
		}

		public bool EnableContactEvents
		{
			get => enableContactEvents != 0;
			set => enableContactEvents = B2Bool.From(value);
		}

		public bool EnableHitEvents
		{
			get => enableHitEvents != 0;
			set => enableHitEvents = B2Bool.From(value);
		}

		public bool EnablePreSolveEvents
		{
			get => enablePreSolveEvents != 0;
			set => enablePreSolveEvents = B2Bool.From(value);
		}

		public bool InvokeContactCreation
		{
			get => invokeContactCreation != 0;
			set => invokeContactCreation = B2Bool.From(value);
		}

		public bool UpdateBodyMass
		{
			get => updateBodyMass != 0;
			set => updateBodyMass = B2Bool.From(value);
		}

		public static B2ShapeDef Default() => B2Native.b2DefaultShapeDef();
	}

	/// Used to create a chain of one-sided line segments. Matches b2ChainDef. The points/materials
	/// pointers are populated internally by B2Chain.Create - do not set them directly.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ChainDef
	{
		public IntPtr userData;
		public IntPtr points;
		public int count;
		public IntPtr materials;
		public int materialCount;
		public B2Filter filter;

		private byte isLoop;
		private byte enableSensorEvents;
		internal int internalValue;

		public bool IsLoop
		{
			get => isLoop != 0;
			set => isLoop = B2Bool.From(value);
		}

		public bool EnableSensorEvents
		{
			get => enableSensorEvents != 0;
			set => enableSensorEvents = B2Bool.From(value);
		}

		public static B2ChainDef Default() => B2Native.b2DefaultChainDef();
	}

	/// Configures a radial explosion applied with B2World.Explode. Matches b2ExplosionDef.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ExplosionDef
	{
		public ulong maskBits;
		public B2Vec2 position;
		public float radius;
		public float falloff;
		public float impulsePerLength;

		public static B2ExplosionDef Default() => B2Native.b2DefaultExplosionDef();
	}
}
