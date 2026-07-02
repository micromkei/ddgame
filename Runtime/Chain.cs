using System;
using System.Runtime.InteropServices;
using Box2D.Interop;

namespace Box2D
{
	/// A chain of one-sided line segments attached to a static body. Thin, cheap-to-copy wrapper
	/// around a b2ChainId handle.
	public readonly struct B2Chain : IEquatable<B2Chain>
	{
		public readonly B2ChainId Id;

		public B2Chain(B2ChainId id)
		{
			Id = id;
		}

		/// Creates a chain shape. `points` needs at least 4 entries; `materials` must have either
		/// 1 entry (applied to every segment) or one entry per segment. Box2D clones both arrays
		/// synchronously, so they only need to stay pinned for the duration of this call.
		public static B2Chain Create(B2Body body, B2ChainDef def, B2Vec2[] points, B2SurfaceMaterial[] materials)
		{
			if (points == null || points.Length < 4)
			{
				throw new ArgumentException("A chain needs at least 4 points.", nameof(points));
			}

			if (materials == null || materials.Length == 0)
			{
				throw new ArgumentException("A chain needs at least 1 surface material.", nameof(materials));
			}

			GCHandle pointsHandle = GCHandle.Alloc(points, GCHandleType.Pinned);
			GCHandle materialsHandle = GCHandle.Alloc(materials, GCHandleType.Pinned);
			try
			{
				def.points = pointsHandle.AddrOfPinnedObject();
				def.count = points.Length;
				def.materials = materialsHandle.AddrOfPinnedObject();
				def.materialCount = materials.Length;
				return new B2Chain(B2Native.b2CreateChain(body.Id, in def));
			}
			finally
			{
				pointsHandle.Free();
				materialsHandle.Free();
			}
		}

		public static B2Chain Create(B2Body body, B2ChainDef def, B2Vec2[] points, B2SurfaceMaterial material) =>
			Create(body, def, points, new[] { material });

		public void Destroy() => B2Native.b2DestroyChain(Id);

		public bool IsValid => B2Native.b2Chain_IsValid(Id);

		public B2World GetWorld() => new B2World(B2Native.b2Chain_GetWorld(Id));

		public int SegmentCount => B2Native.b2Chain_GetSegmentCount(Id);

		public B2Shape[] GetSegments()
		{
			var ids = new B2ShapeId[SegmentCount];
			int n = B2Native.b2Chain_GetSegments(Id, ids, ids.Length);
			var shapes = new B2Shape[n];
			for (int i = 0; i < n; i++) shapes[i] = new B2Shape(ids[i]);
			return shapes;
		}

		public int SurfaceMaterialCount => B2Native.b2Chain_GetSurfaceMaterialCount(Id);

		public void SetSurfaceMaterial(B2SurfaceMaterial material, int materialIndex) =>
			B2Native.b2Chain_SetSurfaceMaterial(Id, in material, materialIndex);

		public B2SurfaceMaterial GetSurfaceMaterial(int materialIndex) => B2Native.b2Chain_GetSurfaceMaterial(Id, materialIndex);

		public bool Equals(B2Chain other) => Id.Equals(other.Id);
		public override bool Equals(object obj) => obj is B2Chain other && Equals(other);
		public override int GetHashCode() => Id.GetHashCode();
		public static bool operator ==(B2Chain a, B2Chain b) => a.Equals(b);
		public static bool operator !=(B2Chain a, B2Chain b) => !a.Equals(b);

		public static implicit operator B2ChainId(B2Chain chain) => chain.Id;
	}
}
