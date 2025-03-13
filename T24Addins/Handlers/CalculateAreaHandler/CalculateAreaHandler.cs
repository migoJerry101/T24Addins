using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace T24AddIns.Handlers.CalculateAreaHandler
{
    internal class CalculateAreaHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            //walls
            try
            {
                var doc = app.ActiveUIDocument.Document;
                var windowCollector = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Windows)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .ToList();

                var doorCollector = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Doors)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .ToList();


                //var wallCollection = new FilteredElementCollector(doc)
                //    .OfCategory(BuiltInCategory.OST_Walls)
                //    .OfClass(typeof(Wall))
                //    .WhereElementIsNotElementType()
                //    .ToList();

                //foreach (var wall in wallCollection)
                //{
                //    Parameter heightParam = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                //    Parameter widthParam = wall.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM);

                //    var test2 = wall.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsDouble();

                //    Parameter areaParam = wall.LookupParameter("Area");

                //    double doorArea = areaParam.AsDouble();


                //    if (widthParam != null && widthParam.HasValue && heightParam != null && heightParam.HasValue)
                //    {


                //        double windowHeight = heightParam.AsDouble(); // Height in feet
                //        double windowWidth = widthParam.AsDouble(); // Convert feet to meters if needed

                //        double windowArea = windowHeight * windowWidth; // Area in square feet

                //        using (Transaction trans = new Transaction(doc, "Set Door Area"))
                //        {
                //            trans.Start();
                //            areaParam.Set(windowArea); // Set area in square feet
                //            trans.Commit();
                //        }
                //    }
                //}

                foreach (var door in doorCollector)
                {
                    FamilyInstance test = door as FamilyInstance;
                    FamilySymbol windowType = test.Symbol;
                    Parameter heightParam = windowType.get_Parameter(BuiltInParameter.DOOR_HEIGHT);
                    Parameter widthParam = windowType.get_Parameter(BuiltInParameter.DOOR_WIDTH);
 


                    if (widthParam != null && widthParam.HasValue && heightParam != null && heightParam.HasValue)
                    {
                        Parameter areaParam = door.LookupParameter("Area1");

                        double windowHeight = heightParam.AsDouble(); // Height in feet
                        double windowWidth = widthParam.AsDouble(); // Convert feet to meters if needed

                        double windowArea = windowHeight * windowWidth; // Area in square feet

                        using (Transaction trans = new Transaction(doc, "Set Door Area"))
                        {
                            trans.Start();
                            areaParam.Set(windowArea); // Set area in square feet
                            trans.Commit();
                        }
                    }
                    else
                    {

                        FamilyInstance doorInstance = door as FamilyInstance;
                        Element host = doorInstance.Host;

                        if (host is Wall wall)
                        {
                            Parameter widthParam1 = doorInstance.get_Parameter(BuiltInParameter.DOOR_WIDTH);
                            Parameter heightParam1 = doorInstance.get_Parameter(BuiltInParameter.DOOR_HEIGHT);

                            if (widthParam1 != null && widthParam1.HasValue && heightParam1 != null && heightParam1.HasValue)
                            {
                                Parameter areaParam = door.LookupParameter("Area1");

                                double windowHeight = heightParam1.AsDouble(); // Height in feet
                                double windowWidth = widthParam1.AsDouble(); // Convert feet to meters if needed

                                double windowArea = windowHeight * windowWidth; // Area in square feet

                                using (Transaction trans = new Transaction(doc, "Set Door Area"))
                                {
                                    trans.Start();
                                    areaParam.Set(windowArea); // Set area in square feet
                                    trans.Commit();
                                }
                            }

                        }

                    }
                }

                foreach (var window in windowCollector)
                {
                    FamilyInstance test = window as FamilyInstance;
                    FamilySymbol windowType = test.Symbol;
                    Parameter heightParam = windowType.get_Parameter(BuiltInParameter.WINDOW_HEIGHT);
                    Parameter widthParam = windowType.get_Parameter(BuiltInParameter.WINDOW_WIDTH);

                    if (widthParam != null && widthParam.HasValue && heightParam != null && heightParam.HasValue)
                    {
                        Parameter areaParam = window.LookupParameter("Area1");

                        double windowHeight = heightParam.AsDouble(); // Height in feet
                        double windowWidth = widthParam.AsDouble(); // Convert feet to meters if needed

                        double windowArea = windowHeight * windowWidth; // Area in square feet

                        using (Transaction trans = new Transaction(doc, "Set Window Area"))
                        {
                            trans.Start();
                            areaParam.Set(windowArea); // Set area in square feet
                            trans.Commit();
                        }
                    }
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
            return "CalculateAreaHandler";
        }
    }
}
