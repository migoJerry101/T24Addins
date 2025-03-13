using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace T24AddIns.Handlers.AddScheduleToSheetHandler
{
    internal class AddScheduleToSheetHandler : IExternalEventHandler
    {
        public string SheetName { get; set; } = string.Empty;
        public string ScheduleName { get; set; } = string.Empty;
        public void Execute(UIApplication app)
        {
            try
            {

                var doc = app.ActiveUIDocument.Document;

                using (Transaction tx = new Transaction(doc, "Add Schedule to Sheet"))
                {
                    tx.Start();

                    if (SheetName == "None") return;

                    ViewSheet sheet = new FilteredElementCollector(doc)
                        .OfClass(typeof(ViewSheet))
                        .Cast<ViewSheet>()
                        .FirstOrDefault(x => x.Name.Equals(SheetName, StringComparison.OrdinalIgnoreCase));

                    if (sheet == null)
                    {
                        TaskDialog.Show("Error", $"Sheet '{SheetName}' not found.");
                        return;
                    }

                    ViewSchedule schedule = new FilteredElementCollector(doc)
                        .OfClass(typeof(ViewSchedule))
                        .Cast<ViewSchedule>()
                        .FirstOrDefault(x => x.Name.Equals(ScheduleName, StringComparison.OrdinalIgnoreCase));

                    if (schedule == null)
                    {
                        TaskDialog.Show("Error", $"Schedule '{ScheduleName}' not found.");
                        return;
                    }

                    XYZ placementPoint = new XYZ(0.5, 0.5, 0);
                    ScheduleSheetInstance scheduleInstance =
                        ScheduleSheetInstance.Create(doc, sheet.Id, schedule.Id, placementPoint);

                    tx.Commit();

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
            return "AddScheduleToSheetHandler";
        }
    }
}
