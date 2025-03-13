using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace T24AddIns.Handlers.MoveTagFromWallHandler
{
    internal class MoveTagFromWallHandler : IExternalEventHandler
    {
        public BuiltInCategory Category { get; set; }
        public void Execute(UIApplication app)
        {
            try
            {
                var doc = app.ActiveUIDocument.ActiveView.Document;

                var tags = new FilteredElementCollector(doc)
                    .OfCategory(Category)
                    .WhereElementIsNotElementType()
                    .Cast<IndependentTag>()
                    .ToList();



                foreach (var tag in tags)
                {
                    using (var trans = new Transaction(doc, "Move tag for wall"))
                    {
                        trans.Start();

                        var tagLocation = tag.TagHeadPosition;
                        BoundingBoxXYZ tagBoundingBox = tag.get_BoundingBox(doc.ActiveView);

                        if (tagBoundingBox == null) continue;

                        var movementVector = GetNonOverlappingDistance(tag, doc);

                        // Apply the movement to avoid overlap
                        var newLocation = new XYZ(
                            tagLocation.X + movementVector.X,
                            tagLocation.Y + movementVector.Y,
                            tagLocation.Z
                        );

                        tag.TagHeadPosition = newLocation;

                        trans.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private XYZ GetNonOverlappingDistance(IndependentTag tag, Document doc)
        {
            var walls = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .Cast<Wall>()
                .ToList();

            BoundingBoxXYZ tagBoundingBox = tag.get_BoundingBox(doc.ActiveView);

            Outline tagOutline = new Outline(tagBoundingBox.Min, tagBoundingBox.Max);

            XYZ movementVector = XYZ.Zero;

            foreach (var wall in walls)
            {
                BoundingBoxXYZ wallBoundingBox = wall.get_BoundingBox(doc.ActiveView);

                if (wallBoundingBox != null)
                {
                    Outline wallOutline = new Outline(wallBoundingBox.Min, wallBoundingBox.Max);
                    var shit = tagOutline.Intersects(wallOutline, 0);

                    if (shit) // Check for overlap
                    {
                        // Calculate overlapping distances on both X and Y axes
                        double overlapX = Math.Min(tagBoundingBox.Max.X, wallBoundingBox.Max.X)
                                          - Math.Max(tagBoundingBox.Min.X, wallBoundingBox.Min.X);

                        double overlapY = Math.Min(tagBoundingBox.Max.Y, wallBoundingBox.Max.Y)
                                          - Math.Max(tagBoundingBox.Min.Y, wallBoundingBox.Min.Y);

                        double buffer = 0.1;

                        // Determine which direction requires the least movement
                        if (overlapX < overlapY)
                        {
                            // Move in the X direction if overlap is smaller
                            movementVector = new XYZ(overlapX + buffer, 0, 0);
                        }
                        else
                        {
                            // Move in the Y direction if overlap is smaller
                            movementVector = new XYZ(0, overlapY + buffer, 0);
                        }

                        break; // Exit after handling the first overlap
                    }
                }
            }
            return movementVector;
        }

        public string GetName()
        {
            return "MoveDoorTagFromWallHandler";
        }
    }
}
