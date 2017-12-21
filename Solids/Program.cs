using System;
using System.Collections;
using System.Linq;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace Solids
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var model = new Model();
			var graphicsDrawer = new GraphicsDrawer();
			var picker = new Picker();
			var red = new Color(1, 0, 0);
			var white = new Color(1, 1, 1);
			var blue = new Color(0, 0, 1);
			var green = new Color(0, 1, 0);
			var black = new Color(0, 0, 0);

			try
			{
				var savedPlane = model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
				model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());

				var part1 = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick Part") as Part;
				var solid = part1.GetSolid();

				var maximumPoint = solid.MaximumPoint;
				var minimumPoint = solid.MinimumPoint;

				graphicsDrawer.DrawText(maximumPoint, maximumPoint.ToString(), white);
				graphicsDrawer.DrawText(minimumPoint, maximumPoint.ToString(), white);

				var part2 = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick Part") as Part;

				var part2CoordinateSystem = part2.GetCoordinateSystem();
				var origin = part2CoordinateSystem.Origin;
				var xDirection = origin + part2CoordinateSystem.AxisX;
				var yDirection = origin + part2CoordinateSystem.AxisY;

				var xAxis = new LineSegment(origin, xDirection);
				var yAxis = new LineSegment(origin, yDirection);

				graphicsDrawer.DrawLineSegment(xAxis, blue);
				graphicsDrawer.DrawLineSegment(yAxis, red);

				graphicsDrawer.DrawText(origin, "ORIGIN", white);
				graphicsDrawer.DrawText(xDirection, "XAXIS", blue);
				graphicsDrawer.DrawText(yDirection, "YAXIS", red);


				var intersectionPoints = solid.GetAllIntersectionPoints(part2CoordinateSystem.Origin, part2CoordinateSystem.AxisX, part2CoordinateSystem.AxisY);

				while (intersectionPoints.MoveNext())
				{
					var point = intersectionPoints.Current as Point;
					graphicsDrawer.DrawText(point, point.ToString(), green);
				}

				var plane = new Plane()
				{
					Origin = origin,
					AxisX = part2CoordinateSystem.AxisX,
					AxisY = part2CoordinateSystem.AxisY,
				};

				var cut = new CutPlane()
				{
					Father = part1,
					Identifier = new Identifier(new Guid()),
					Plane = plane,
				};

				cut.Insert();
				model.CommitChanges();

				var part2Centerline = part2.GetCenterLine(false);
				var intersect = solid.Intersect(new LineSegment(part2Centerline[0] as Point, part2Centerline[1] as Point));

				var point1 = intersect[0] as Point;
				var point2 = intersect[1] as Point;

				graphicsDrawer.DrawText(point1, "POINT1", red);
				graphicsDrawer.DrawText(point2, "POINT2", blue);

				model.GetWorkPlaneHandler().SetCurrentTransformationPlane(savedPlane);

				var currentViews = ViewHandler.GetAllViews();
				while (currentViews.MoveNext())
				{
					var currentView = currentViews.Current;
					currentView.Select();
					currentView.VisibilitySettings.WeldsVisible = false;
					currentView.VisibilitySettings.WeldsVisibleInComponents = false;
					currentView.VisibilitySettings.ComponentsVisible = false;
					currentView.VisibilitySettings.FittingsVisible = false;
					currentView.VisibilitySettings.FittingsVisibleInComponents = false;
					currentView.Modify();
				}

				var faces = picker.PickFace("Pick Face");
				var vertices = faces.OfType<InputItem>().ToList()[1].GetData() as ArrayList;
				foreach (Point vertex in vertices)
				{
					graphicsDrawer.DrawText(vertex, vertex.ToString(), black);
				}

				plane = new Plane
				{
					Origin = vertices[0] as Point,
					AxisX = new Vector((vertices[1] as Point) - (vertices[0] as Point)),
					AxisY = new Vector((vertices[2] as Point) - (vertices[0] as Point)),
				};

				cut = new CutPlane
				{
					Father = part1,
					Identifier = new Identifier(new Guid()),
					Plane = plane,
				};

				cut.Insert();
				model.CommitChanges();
			}
			catch (Exception ex)
			{
				// ignored
			}
		}
	}
}
