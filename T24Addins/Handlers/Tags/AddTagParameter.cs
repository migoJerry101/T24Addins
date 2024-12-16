using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace T24AddIns.Handlers.Tags
{
    internal class AddTagParameter : IExternalEventHandler
    {
        public List<Element> Elements { get; set; }
        public Document Document { get; set; }

        public List<string> Properties = new List<string>()
        {
            "North", "South", "East", "West", "Group 1", "Group 2", "Group 3", "Group 4", "Group 5"
        };

        public void Execute(UIApplication app)
        {
            try
            {
                foreach (var element in Elements)
                {
                    if (element is FamilySymbol familySymbol)
                    {
                        var familyDoc = Document.EditFamily(familySymbol.Family);

                        using (var transaction = new Transaction(familyDoc, "Add Tag Parameter"))
                        {
                            transaction.Start();

                            FamilyManager familyManager = familyDoc.FamilyManager;

                            if (familyManager != null)
                            {
                                foreach (var propName in Properties)
                                {
                                    var existingParam = familyManager.get_Parameter(propName);

                                    if (existingParam == null)
                                    {
                                        familyManager.AddParameter(propName, BuiltInParameterGroup.PG_IDENTITY_DATA, ParameterType.YesNo, true);
                                    }
                                }

                            }

                            transaction.Commit();
                        }

                        familyDoc.LoadFamily(Document, new FamilyOptions());

                        familyDoc.Close(false);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public string GetName()
        {
            return "AddTagParameter";
        }

        private bool IsFamilyAlreadyLoaded(Document doc, string familyName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            return collector.OfClass(typeof(Family))
                .Cast<Family>()
                .Any(f => f.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
