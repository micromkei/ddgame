using System;
using Box2D.Interop;

namespace Box2D
{
	/// A Box2D simulation world. This wraps a b2WorldId handle and is the entry point for
	/// creating bodies, joints, and running the simulation.
	public sealed class B2World
	{
		public B2WorldId Id { get; }

		private B2CustomFilterFcn _customFilter;
		private B2PreSolveFcn _preSolve;
		private B2FrictionCallback _frictionCallback;
		private B2RestitutionCallback _restitutionCallback;

		internal B2World(B2WorldId id)
		{
			Id = id;
		}

		/// Creates a world. Throws if the native library was built with BOX2D_DOUBLE_PRECISION -
		/// this binding assumes a single-precision build.
		public static B2World Create(B2WorldDef def)
		{
			if (B2Native.b2IsDoublePrecision())
			{
				throw new InvalidOperationException(
					"Box2D.NET targets a single-precision native build, but the loaded box2d library was built with BOX2D_DOUBLE_PRECISION.");
			}

			return new B2World(B2Native.b2CreateWorld(in def));
		}

		public static B2World Create() => Create(B2WorldDef.Default());

		public void Destroy() => B2Native.b2DestroyWorld(Id);

		public bool IsValid => B2Native.b2World_IsValid(Id);

		public void Step(float timeStep, int subStepCount = 4) => B2Native.b2World_Step(Id, timeStep, subStepCount);

		/// Draws the world using the given debug draw target (e.g. a MonoBehaviour that overrides
		/// the Draw* methods to call Unity's Gizmos/Debug.DrawLine).
		public void Draw(B2DebugDraw draw)
		{
			var bridge = new B2DebugDrawBridge(draw);
			B2NativeDebugDraw native = bridge.BuildNative();
			B2Native.b2World_Draw(Id, ref native);
			GC.KeepAlive(bridge);
		}

		public B2AABB Bounds => B2Native.b2World_GetBounds(Id);

		public B2BodyMoveEvent[] GetBodyEvents()
		{
			B2BodyEvents events = B2Native.b2World_GetBodyEvents(Id);
			return B2Marshal.ToArray<B2BodyMoveEvent>(events.moveEvents, events.moveCount);
		}

		public void GetSensorEvents(out B2SensorBeginTouchEvent[] begin, out B2SensorEndTouchEvent[] end)
		{
			B2SensorEvents events = B2Native.b2World_GetSensorEvents(Id);
			begin = B2Marshal.ToArray<B2SensorBeginTouchEvent>(events.beginEvents, events.beginCount);
			end = B2Marshal.ToArray<B2SensorEndTouchEvent>(events.endEvents, events.endCount);
		}

		public void GetContactEvents(out B2ContactBeginTouchEvent[] begin, out B2ContactEndTouchEvent[] end, out B2ContactHitEvent[] hit)
		{
			B2ContactEvents events = B2Native.b2World_GetContactEvents(Id);
			begin = B2Marshal.ToArray<B2ContactBeginTouchEvent>(events.beginEvents, events.beginCount);
			end = B2Marshal.ToArray<B2ContactEndTouchEvent>(events.endEvents, events.endCount);
			hit = B2Marshal.ToArray<B2ContactHitEvent>(events.hitEvents, events.hitCount);
		}

		public B2JointEvent[] GetJointEvents()
		{
			B2JointEvents events = B2Native.b2World_GetJointEvents(Id);
			return B2Marshal.ToArray<B2JointEvent>(events.jointEvents, events.count);
		}

		public B2TreeStats OverlapAABB(B2Vec2 origin, B2AABB aabb, B2QueryFilter filter, B2OverlapResultFcn callback) =>
			B2Native.b2World_OverlapAABB(Id, origin, aabb, filter, callback, IntPtr.Zero);

		public B2TreeStats OverlapShape(B2Vec2 origin, B2ShapeProxy proxy, B2QueryFilter filter, B2OverlapResultFcn callback) =>
			B2Native.b2World_OverlapShape(Id, origin, in proxy, filter, callback, IntPtr.Zero);

		public B2TreeStats CastRay(B2Vec2 origin, B2Vec2 translation, B2QueryFilter filter, B2CastResultFcn callback) =>
			B2Native.b2World_CastRay(Id, origin, translation, filter, callback, IntPtr.Zero);

		public B2RayResult CastRayClosest(B2Vec2 origin, B2Vec2 translation, B2QueryFilter filter) =>
			B2Native.b2World_CastRayClosest(Id, origin, translation, filter);

		public B2TreeStats CastShape(B2Vec2 origin, B2ShapeProxy proxy, B2Vec2 translation, B2QueryFilter filter, B2CastResultFcn callback) =>
			B2Native.b2World_CastShape(Id, origin, in proxy, translation, filter, callback, IntPtr.Zero);

		public bool EnableSleeping
		{
			get => B2Native.b2World_IsSleepingEnabled(Id);
			set => B2Native.b2World_EnableSleeping(Id, value);
		}

		public bool EnableContinuous
		{
			get => B2Native.b2World_IsContinuousEnabled(Id);
			set => B2Native.b2World_EnableContinuous(Id, value);
		}

		public float RestitutionThreshold
		{
			get => B2Native.b2World_GetRestitutionThreshold(Id);
			set => B2Native.b2World_SetRestitutionThreshold(Id, value);
		}

		public float HitEventThreshold
		{
			get => B2Native.b2World_GetHitEventThreshold(Id);
			set => B2Native.b2World_SetHitEventThreshold(Id, value);
		}

		/// Keeps a managed reference to the callback for as long as it is registered with Box2D -
		/// pass null to clear it.
		public void SetCustomFilterCallback(B2CustomFilterFcn callback)
		{
			_customFilter = callback;
			B2Native.b2World_SetCustomFilterCallback(Id, callback, IntPtr.Zero);
		}

		/// Keeps a managed reference to the callback for as long as it is registered with Box2D -
		/// pass null to clear it.
		public void SetPreSolveCallback(B2PreSolveFcn callback)
		{
			_preSolve = callback;
			B2Native.b2World_SetPreSolveCallback(Id, callback, IntPtr.Zero);
		}

		public B2Vec2 Gravity
		{
			get => B2Native.b2World_GetGravity(Id);
			set => B2Native.b2World_SetGravity(Id, value);
		}

		public void Explode(B2ExplosionDef explosionDef) => B2Native.b2World_Explode(Id, in explosionDef);

		public void SetContactTuning(float hertz, float dampingRatio, float pushSpeed) =>
			B2Native.b2World_SetContactTuning(Id, hertz, dampingRatio, pushSpeed);

		public float ContactRecycleDistance
		{
			get => B2Native.b2World_GetContactRecycleDistance(Id);
			set => B2Native.b2World_SetContactRecycleDistance(Id, value);
		}

		public float MaximumLinearSpeed
		{
			get => B2Native.b2World_GetMaximumLinearSpeed(Id);
			set => B2Native.b2World_SetMaximumLinearSpeed(Id, value);
		}

		public bool EnableWarmStarting
		{
			get => B2Native.b2World_IsWarmStartingEnabled(Id);
			set => B2Native.b2World_EnableWarmStarting(Id, value);
		}

		public int AwakeBodyCount => B2Native.b2World_GetAwakeBodyCount(Id);

		public B2Profile Profile => B2Native.b2World_GetProfile(Id);

		public B2Counters Counters => B2Native.b2World_GetCounters(Id);

		public B2Capacity MaxCapacity => B2Native.b2World_GetMaxCapacity(Id);

		/// Wraps an arbitrary managed object in a GCHandle stored as Box2D's userData pointer.
		/// Freed automatically when replaced or when a new value is set to null.
		public object UserData
		{
			get => B2Marshal.GetUserData(B2Native.b2World_GetUserData(Id));
			set => B2Native.b2World_SetUserData(Id, B2Marshal.SetUserData(B2Native.b2World_GetUserData(Id), value));
		}

		/// Keeps a managed reference to the callback for as long as it is registered with Box2D -
		/// pass null to reset to Box2D's default (sqrt(frictionA * frictionB)).
		public void SetFrictionCallback(B2FrictionCallback callback)
		{
			_frictionCallback = callback;
			B2Native.b2World_SetFrictionCallback(Id, callback);
		}

		/// Keeps a managed reference to the callback for as long as it is registered with Box2D -
		/// pass null to reset to Box2D's default (max(restitutionA, restitutionB)).
		public void SetRestitutionCallback(B2RestitutionCallback callback)
		{
			_restitutionCallback = callback;
			B2Native.b2World_SetRestitutionCallback(Id, callback);
		}

		public int WorkerCount
		{
			get => B2Native.b2World_GetWorkerCount(Id);
			set => B2Native.b2World_SetWorkerCount(Id, value);
		}

		public static B2HexColor GetGraphColor(int index) => B2Native.b2GetGraphColor(index);
	}
}
