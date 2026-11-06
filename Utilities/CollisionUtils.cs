using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.DataStructures;

namespace OrchidMod.Utilities;

public struct Circle(Vector2 origin, float radius)
{
	public Vector2 Origin = origin;
	public float Radius = radius;

	public static float X(Circle value) => value.Origin.X;
	public static float Y(Circle value) => value.Origin.Y;
	public static (Vector2, float) ToTuple(Circle value) => (value.Origin, value.Radius);

	public static float Circumference(Circle value) => 2 * MathHelper.Pi * value.Radius;

	public static float Area(Circle value) => MathHelper.Pi * value.Radius * value.Radius;

	public static bool HasPointInCircle(Circle circle, Vector2 point) => point.DistanceSQ(circle.Origin) <= circle.Radius * circle.Radius;
	
	public static Vector2 ClosestPointInCircle(Circle circle, Vector2 point)
	{
		if (point.DistanceSQ(circle.Origin) <= circle.Radius * circle.Radius)
			return point;
		return circle.Origin + circle.Origin.DirectionTo(point) * circle.Radius;
	}
	
	public static Circle CircularizeHitbox(Entity entity, float lenience = 1, bool useEntity1LargestDim = true)
	{
		float radius = entity.GetDimension(useEntity1LargestDim) * 0.5f;
		return new Circle(entity.Center, radius * lenience);
	}

	public static Circle operator +(Circle circle, Vector2 point) => new (circle.Origin + point, circle.Radius);
	public static Circle operator +(Circle circle, float radius) => new (circle.Origin, circle.Radius + radius);
	public static Circle operator -(Circle circle, Vector2 point) => circle + -point;
	public static Circle operator -(Circle circle, float radius) => circle + -radius;
	
	public static Circle operator *(Circle circle, float amount) => new (circle.Origin, circle.Radius * amount);
	public static Circle operator /(Circle circle, float amount) => new (circle.Origin, circle.Radius / amount);
}
	

public static partial class OrchidUtils
{
	public static Vector2 getGFXOffY(this Player player) => Vector2.UnitY * player.gfxOffY;
		
	public static Vector2 VisualCenter(this Player player, bool floor = true)
	{
		Vector2 pos = player.Center + player.getGFXOffY();
		return floor ? pos.Floor() : pos;
	}
	public static Vector2 VisualMountedCenter(this Player player, bool floor = true)
	{
		Vector2 pos = player.MountedCenter + player.getGFXOffY();
		return floor ? pos.Floor() : pos;
	}
	
	public static int GetLargestDimension(this Entity entity) => Math.Max(entity.width, entity.height);
	public static int GetSmallestDimension(this Entity entity) => Math.Min(entity.width, entity.height);
	public static int GetDimension(this Entity entity, bool useLargestDim = true) => useLargestDim ? entity.GetLargestDimension() : entity.GetSmallestDimension();

	public static (float, float) XYTuple(this Vector2 vector) => (vector.X, vector.Y);
	public static (double, double) XYTuple(this Vector2D vector) => (vector.X, vector.Y);
	public static (int, int) XYTuple(this Point point) => (point.X, point.Y);
	public static (short, short) XYTuple(this Point16 point) => (point.X, point.Y);

	public static Vector4 Vector4(Vector2 vector1, Vector2 vector2) => new Vector4(vector1.X, vector1.Y, vector2.X, vector2.Y);
	
	public static bool HasPointInCircle(this Circle circle, Vector2 point) => Circle.HasPointInCircle(circle, point);	
	public static Vector2 ClosestPointInCircle(this Circle circle, Vector2 point) => Circle.ClosestPointInCircle(circle, point);
	public static Circle CircularizeHitbox(this Entity entity, float lenience = 1, bool useEntity1LargestDim = true) => Circle.CircularizeHitbox(entity, lenience, useEntity1LargestDim);
	
	public static bool CheckCircularvCircularCollision(Vector2 origin1, float radius1, Vector2 origin2, float radius2) => origin1.Distance(origin2) <= radius1 + radius2;
	public static bool CheckCircularvCircularCollision(Circle circle1, Circle circle2) => CheckCircularvCircularCollision(circle1.Origin, circle1.Radius, circle2.Origin, circle2.Radius);
	public static bool CheckCircularvCircularCollision(Entity entity1, Entity entity2, bool useEntity1LargestDim = true, bool useEntity2LargestDim = true) => CheckCircularvCircularCollision(entity1.Center, entity1.GetDimension(useEntity1LargestDim) * 0.5f, entity2.Center, entity2.GetDimension(useEntity2LargestDim) * 0.5f);

	// https://stackoverflow.com/questions/401847/circle-rectangle-collision-detection-intersection
	public static bool CheckAABBvCircularCollision(Rectangle rectangle, Circle circle)
	{
		float closestX = MathHelper.Clamp(circle.Origin.X, rectangle.Left, rectangle.Right);
		float closestY = MathHelper.Clamp(circle.Origin.Y, rectangle.Top, rectangle.Bottom);

		float distanceX = circle.Origin.X - closestX;
		float distanceY = circle.Origin.Y - closestY;

		float distanceSquared = distanceX * distanceX + distanceY * distanceY;
		return distanceSquared < circle.Radius * circle.Radius;
	}
	
	public static bool CheckAABBvCircularCollision(Vector2 rectPosition, Vector2 rectDimensions, Vector2 circOrigin, float circRadius)
	{
		Rectangle rectangle = new ((int)rectPosition.X, (int)rectPosition.Y, (int)rectDimensions.X, (int)rectDimensions.Y);
		Circle circle = new (circOrigin, circRadius);
		return CheckAABBvCircularCollision(rectangle, circle);
	}

	public static Vector2 Scale(this Vector2 value, float factorX = 1f, float factorY = 1f) => new (value.X * factorX, value.Y * factorY);
	public static void Scale(ref Vector2 original, out Vector2 result, float factorX = 1f, float factorY = 1f) => result = new Vector2(original.X * factorX, original.Y * factorY);

}