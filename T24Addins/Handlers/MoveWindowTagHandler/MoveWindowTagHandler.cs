using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace T24AddIns.Handlers.MoveWindowTagHandler
{
    internal class MoveWindowTagHandler: IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;

            var tagCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_WindowTags)
                .WhereElementIsNotElementType()
                .Cast<IndependentTag>()
                .ToList();

            foreach (var doorTag in tagCollector)
            {

                using (var trans = new Transaction(doc, "Add Tags to Doors in All Views"))
                {
                    trans.Start();

                    var tagLocation = doorTag.TagHeadPosition;
                    var movementVector = GetNonOverlappingDistance(doorTag, doc);

                    var newOrigin = new XYZ(tagLocation.X + movementVector.X, tagLocation.Y + movementVector.Y, tagLocation.Z);

                    ElementId taggedElementId = doorTag.TaggedElementId.HostElementId;

                    Element doorElement = doc.GetElement(taggedElementId);
                    var doorLocation = doorElement.Location as LocationPoint;


                    if (doorLocation != null)
                    {
                        var doorPosition = doorLocation.Point;

                        // Set the tag head position to avoid overlap
                        doorTag.TagHeadPosition = newOrigin;

                        // Calculate the midpoint between the tag's new position and the door's location
                        var midpoint = new XYZ(
                            (doorPosition.X),
                            (newOrigin.Y + doorPosition.Y) / 2,
                            (newOrigin.Z + doorPosition.Z) / 2
                        );

                        // Set the leader elbow to the midpoint
                        doorTag.LeaderElbow = midpoint;
                    }

                    trans.Commit();
                }
            }
        }

        public string GetName()
        {
            return "MoveWindowTagHandler";
        }

        private XYZ GetNonOverlappingDistance(IndependentTag currentTag, Document doc)
        {
            // Collect all door tags except the current tag
            var otherTags = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_WindowTags)
                .WhereElementIsNotElementType()
                .Cast<IndependentTag>()
                .Where(t => t.Id != currentTag.Id)
                .ToList();

            // Get the bounding box and position of the current tag
            var currentTagHeaderPosition = currentTag.TagHeadPosition;


            BoundingBoxXYZ currentTagBoundingBox = currentTag.get_BoundingBox(doc.ActiveView);

            if (currentTagBoundingBox == null) return new XYZ(0, 0, 0);

            double currentTagHalfWidth = (currentTagBoundingBox.Max.X - currentTagBoundingBox.Min.X) / 2;
            double currentTagHalfHeight = (currentTagBoundingBox.Max.Y - currentTagBoundingBox.Min.Y) / 2;

            // Initialize the required distance to zero
            double minOffsetX = 0;
            double minOffsetY = 0;

            ElementId taggedElementId = currentTag.TaggedElementId.HostElementId;

            var doorElement = doc.GetElement(taggedElementId);
            var doorLocation = doorElement.Location as LocationPoint;

            var doorPosition = doorLocation.Point;

            // Check for overlap with other tags
            foreach (var tag in otherTags)
            {
                var tagHeaderPosition = tag.TagHeadPosition;
                BoundingBoxXYZ tagBoundingBox = tag.get_BoundingBox(doc.ActiveView);

                if (tagBoundingBox == null) continue;

                double tagHalfWidth = (tagBoundingBox.Max.X - tagBoundingBox.Min.X) / 2;
                double tagHalfHeight = (tagBoundingBox.Max.Y - tagBoundingBox.Min.Y) / 2;

                // Calculate overlap distances along X and Y axes
                double overlapX = (currentTagHalfWidth + tagHalfWidth) - Math.Abs(currentTagHeaderPosition.X - tagHeaderPosition.X);
                double overlapY = (currentTagHalfHeight + tagHalfHeight) - Math.Abs(currentTagHeaderPosition.Y - tagHeaderPosition.Y);

                if (overlapX > 0 && overlapY > 0)
                {
                    // Move away from the door (assuming 'doorPosition' is a known point)
                    if (doorPosition.X < currentTagHeaderPosition.X)
                        minOffsetX = Math.Max(minOffsetX, overlapX); // Move to the right, away from the door if door is on the left
                    else
                        minOffsetX = Math.Min(minOffsetX, -overlapX); // Move to the left, away from the door if door is on the right

                    if (doorPosition.Y < currentTagHeaderPosition.Y)
                        minOffsetY = Math.Max(minOffsetY, overlapY); // Move up, away from the door if door is below
                    else
                        minOffsetY = Math.Min(minOffsetY, -overlapY); // Move down, away from the door if door is above
                }
            }

            // Return the required movement vector as an XYZ
            return new XYZ(minOffsetX, minOffsetY, 0);
        }
    }
}
