using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace T24AddIns.Handlers.AddWindowTagHandler
{
    internal class AddWindowTagHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            try
            {
                var doc = app.ActiveUIDocument.Document;
                var view = app.ActiveUIDocument.ActiveView;

                var windowCollector = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Windows)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .ToList();

                var tags = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_WindowTags)
                    .WhereElementIsNotElementType()
                    .Cast<IndependentTag>()
                    .Distinct()
                    .Select(x => x.Id);

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

                var taggedWindowDictionary = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_WindowTags)
                    .WhereElementIsNotElementType()
                    .Cast<IndependentTag>()
                    .Select(x => x.TaggedElementId.HostElementId)
                    .Distinct()
                    .ToDictionary(x => x);

                using (var trans = new Transaction(doc, "Add Tags to Windows in all view"))
                {
                    trans.Start();

                    foreach (var window in windowCollector)
                    {
                        if (IsElementVisibleInView(doc, view, window))
                        {
                            
                            LocationPoint location = window.Location as LocationPoint;

                            //check location of it overlaps in element
                            var hasTag = taggedWindowDictionary.TryGetValue(window.Id, out ElementId _);

                            if (location != null && !hasTag)
                            {
                                XYZ tagLocation = location.Point;

                                try
                                {
                                    // Attempt to create the tag
                                    IndependentTag tag = IndependentTag.Create(
                                        doc,
                                        tagTypeId,
                                        view.Id,
                                        new Reference(window),
                                        true,
                                        TagOrientation.Horizontal,
                                        tagLocation
                                    );

                                    if (tag == null)
                                    {
                                        TaskDialog.Show("Tag Creation Failed", $"Tag creation failed for door {window.Id} in view {view.Name}.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log the exception for debugging
                                    TaskDialog.Show("Error", $"An error occurred while tagging door {window.Id} in view {view.Name}: {ex.Message}");
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
            return "AddWindowTagHandler";
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
                .FirstOrDefault(symbol => symbol.Name == "K2 Window Tag");


            return famSymbol?.Id;
        }
    }
}
