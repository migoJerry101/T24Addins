using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace T24AddIns.Handlers.AddColorToTags
{
    internal class AddColorToTags : IExternalEventHandler
    {
        public ColorConfig Config { get; set; }
        public void Execute(UIApplication app)
        {
            try
            {
                var doc = app.ActiveUIDocument.Document;

                var elements = new FilteredElementCollector(doc)
                    .OfCategory(Config.Element)
                    .WhereElementIsNotElementType()
                    .Where(door =>
                    {

                        var props = door.GetParameters(Config.Group);


                        foreach (var prop in props)
                        {
                            var data = prop.AsValueString();

                            return data == "Yes";
                        }

                        return false;


                    })
                    .Select(x => x.Id.IntegerValue);

                var otherTags = new FilteredElementCollector(doc)
                    .OfCategory(Config.TagType)
                    .WhereElementIsNotElementType()
                    .Cast<IndependentTag>()
                    .Where(x => elements.Contains(x.TaggedElementId.HostElementId.IntegerValue));

                var color = Config.Color;
 
                var revitColor = new Color(color.Red, color.Green, color.Blue);

                var overrideSettings = new OverrideGraphicSettings();
                overrideSettings.SetProjectionLineColor(revitColor);

                using (var trans = new Transaction(doc, "Set Tag Colors"))
                {
                    trans.Start();

                    foreach (var tag in otherTags)
                    {
                        doc.ActiveView.SetElementOverrides(tag.Id, overrideSettings);
                    }

                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public string GetName()
        {
            return "AddColorToTags";
        }
    }
}
