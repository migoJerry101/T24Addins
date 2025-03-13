using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace T24AddIns.Handlers.AddWallTagHandler
{
    internal class AddWallTagHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            try
            {
                var doc = app.ActiveUIDocument.Document;
                var view = app.ActiveUIDocument.ActiveGraphicalView;

                var wallCollection = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Walls)
                    .OfClass(typeof(Wall))
                    .WhereElementIsNotElementType()
                    .ToList();

                var tags = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_WallTags)
                    .WhereElementIsNotElementType()
                    .Cast<IndependentTag>()
                    .Select(x => x.Id)
                    .Distinct()
                    .ToList();

                using (Transaction trans = new Transaction(doc, "Delete All Doors"))
                {
                    trans.Start();

                    foreach (ElementId tag in tags)
                    {
                        doc.Delete(tag);
                    }

                    trans.Commit();
                }

                ElementId tagTypeId = FindDefaultTagTypeId(doc);

                if (tagTypeId == null)
                {
                    return;
                }

                var taggedWallDictionary = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_WallTags)
                    .WhereElementIsNotElementType()
                    .Cast<IndependentTag>()
                    .Select(x => x.TaggedElementId.HostElementId)
                    .Distinct()
                    .ToDictionary(x => x);

                using (var trans = new Transaction(doc, "Add Tags to Walls in All Views"))
                {
                    trans.Start();

                    foreach (var wall in wallCollection)
                    {
                        if (IsElementVisibleInView(doc, view, wall))
                        {
                            LocationCurve location = wall.Location as LocationCurve;
                            Curve curve = location.Curve;

                            var hasTag = taggedWallDictionary.TryGetValue(wall.Id, out ElementId _);

                            if (location != null && !hasTag)
                            {
                                //XYZ startPoint = curve.GetEndPoint();

                                var wallMidpoint = curve.Evaluate(0.3, true);

                                try
                                {
                                    // Attempt to create the tag
                                    IndependentTag tag = IndependentTag.Create(
                                        doc,
                                        tagTypeId,
                                        view.Id,
                                        new Reference(wall),
                                        true,
                                        TagOrientation.Horizontal,
                                        wallMidpoint
                                    );

                                    if (tag == null)
                                    {
                                        TaskDialog.Show("Tag Creation Failed", $"Tag creation failed for door {wall.Id} in view {view.Name}.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log the exception for debugging
                                    TaskDialog.Show("Error", $"An error occurred while tagging door {wall.Id} in view {view.Name}: {ex.Message}");
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

        public string GetName()
        {
            return "AddWallTagHandler";
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
                .OfCategory(BuiltInCategory.OST_WallTags) // Filter for Wall Tags
                .Cast<FamilySymbol>()
                .Where(x => x.FamilyName == "K2 Wall Tag")
                .ToList();


            return famSymbol?.FirstOrDefault().Id;
        }
    }
}
