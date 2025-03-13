using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace T24AddIns.Handlers.AddDoorTagHandler
{
    internal class AddDoorTagHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            try
            {
                var doc = app.ActiveUIDocument.Document;
                var view = app.ActiveUIDocument.ActiveView;

                var doorCollector = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Doors)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .ToList();

                var tags = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_DoorTags)
                    .WhereElementIsNotElementType()
                    .Cast<IndependentTag>()
                    .Select(x => x.Id)
                    .Distinct()
                    .ToList();


                using (Transaction trans = new Transaction(doc, "Delete All Doors"))
                {
                    trans.Start();

                    foreach (var tag in tags)
                    {
                        doc.Delete(tag);
            
                    }
                    app.ActiveUIDocument.RefreshActiveView();

                    trans.Commit();
                }

                // Find a tag type for doors    
                ElementId tagTypeId = FindDefaultTagTypeId(doc);
                if (tagTypeId == null)
                {
                    return;
                }

                var taggedDorDictionary = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_DoorTags)
                    .WhereElementIsNotElementType()
                    .Cast<IndependentTag>()
                    .Select(x => x.TaggedElementId.HostElementId)
                    .Distinct()
                    .ToDictionary(x => x);


                using (var trans = new Transaction(doc, "Add Tags to Doors in All Views"))
                {
                    trans.Start();

                    foreach (var door in doorCollector)
                    {
                        var d = door.GetSubelements();
                        
                        if (IsElementVisibleInView(doc, view, door) || door.Name.Contains("Curtain Wall"))
                        {
                            LocationPoint location = door.Location as LocationPoint;

                            XYZ end = new XYZ();
                            FamilyInstance doorInstance = door as FamilyInstance;
                            if (location == null)
                            {
                                Element host = doorInstance.Host;
                                if (host is Wall wall)
                                {
                                    LocationCurve wallLocation = wall.Location as LocationCurve;
                                    if (wallLocation != null)
                                    {
                                        Line wallLine = wallLocation.Curve as Line;
                                        end = wallLine.GetEndPoint(1);
                                    }
                                }
                            }

                            var hasTag = taggedDorDictionary.TryGetValue(door.Id, out ElementId _);

                            if (!hasTag)
                            {
                                XYZ mainLoc = location == null ? end : location.Point;
                                
                                try
                                {
                                    IndependentTag tag = IndependentTag.Create(
                                        doc,
                                        tagTypeId,
                                        view.Id,
                                        new Reference(door),
                                        true,
                                        TagOrientation.Horizontal,
                                        mainLoc
                                    );

                                    if (tag == null)
                                    {
                                        TaskDialog.Show("Tag Creation Failed", $"Tag creation failed for door {door.Id} in view {view.Name}.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log the exception for debugging
                                    TaskDialog.Show("Error", $"An error occurred while tagging door {door.Id} in view {view.Name}: {ex.Message}");
                                }
                            }
                        }
                    }

                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TaskDialog.Show("Error", $"An unexpected error occurred: {e.Message}");
            }
        }

        public void MoveTagHeader(Document doc)
        {
            var tagCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DoorTags)
                .WhereElementIsNotElementType()
                .Cast<IndependentTag>() // Filters only IndependentTag elements
                .ToList();

            foreach (var doorTag in tagCollector)
            {

                using (var trans = new Transaction(doc, "Add Tags to Doors in All Views"))
                {
                    trans.Start();

                    ElementId taggedElementId = doorTag.TaggedElementId.HostElementId;

                    if (taggedElementId == ElementId.InvalidElementId) continue;

                    Element doorElement = doc.GetElement(taggedElementId);
                    var doorLocation = doorElement.Location as LocationPoint;

                    if (doorLocation == null) continue;

                    var tagLocation = doorTag.TagHeadPosition;
                    var movementVector = GetNonOverlappingDistance(doorTag, doc);

                    var newOrigin = new XYZ(tagLocation.X + movementVector.X + 0.5, tagLocation.Y + movementVector.Y + 0.5, tagLocation.Z);
                    doorTag.TagHeadPosition = newOrigin;

                    trans.Commit();
                }
            }

        }

        public string GetName()
        {
            return "AddDoorTagHandler";
        }

        private bool IsElementVisibleInView(Document doc, View view, Element element)
        {
            BoundingBoxXYZ boundingBox = element.get_BoundingBox(view);
            return boundingBox != null;
        }

        private ElementId FindDefaultTagTypeId(Document doc)
        {
            var famSymbol = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(symbol => symbol.Name == "K2 Door Tag");


            return famSymbol?.Id;    
        }

        private XYZ GetNonOverlappingDistance(IndependentTag currentTag, Document doc)
        {
            // Collect all door tags except the current tag
            var otherTags = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DoorTags)
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

                // If there's overlap, calculate the minimum required offset
                if (overlapX > 0 || overlapY > 0)
                {
                    // Determine the direction of movement to avoid overlap
                    if (currentTagHeaderPosition.X < tagHeaderPosition.X)
                        minOffsetX = Math.Min(minOffsetX, -overlapX); // Move left
                    else
                        minOffsetX = Math.Max(minOffsetX, overlapX); // Move right

                    if (currentTagHeaderPosition.Y < tagHeaderPosition.Y)
                        minOffsetY = Math.Min(minOffsetY, -overlapY); // Move down
                    else
                        minOffsetY = Math.Max(minOffsetY, overlapY); // Move up
                }
            }

            // Return the required movement vector as an XYZ
            return new XYZ(minOffsetX, minOffsetY, 0);
        }
    }
}
