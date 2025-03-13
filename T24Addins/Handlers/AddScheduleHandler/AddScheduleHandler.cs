using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace T24AddIns.Handlers.AddScheduleHandler
{
    internal class AddScheduleHandler : IExternalEventHandler
    {
        public string Group { get; set; }
        public string ViewName { get; set; }
        public string TagType { get; set; }
        public void Execute(UIApplication app)
        {
            try
            {
                BuiltInCategory cat = BuiltInCategory.INVALID;
                BuiltInCategory tagCat = BuiltInCategory.INVALID;

                switch (TagType)
                {
                    case "Doors":

                        cat = BuiltInCategory.OST_Doors;
                        tagCat= BuiltInCategory.OST_DoorTags;
                        break;
                    case "Walls":
                        cat = BuiltInCategory.OST_Walls;
                        tagCat = BuiltInCategory.OST_WallTags;
                        break;
                    case "Windows":
                        cat = BuiltInCategory.OST_Windows;
                        tagCat = BuiltInCategory.OST_WindowTags;
                        break;
                }

                var doc = app.ActiveUIDocument.Document;

                using (var trans = new Transaction(doc, "Create schedule"))
                {
                    trans.Start();

                    var groupViewSchedule =
                        ViewSchedule.CreateSchedule(doc, new ElementId(cat));
                    //ViewSchedule groupViewSchedule = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Windows));
                    groupViewSchedule.Name = $"{Group} {TagType} Schedule";
                    var def = groupViewSchedule.Definition;
                    doc.Regenerate();

                    //var availableFields = groupViewSchedule.Definition.GetSchedulableFields();
                    //string fieldNames = string.Join("\n", availableFields.Select(sf => sf.GetName(doc)));
                    //TaskDialog.Show("Schedulable Fields", fieldNames);
                    FilteredElementCollector collector = new FilteredElementCollector(doc);

                    switch (TagType)
                    {
                        case "Doors":

                            AddRegularFieldToSchedule(groupViewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_MARK), "Door #");
                            AddRegularFieldToSchedule(groupViewSchedule, new ElementId(BuiltInParameter.DOOR_HEIGHT), "Height (ft)");
                            AddRegularFieldToSchedule(groupViewSchedule, new ElementId(BuiltInParameter.DOOR_WIDTH), "Width (ft)"); 
                            break;
                        case "Walls":
                            AddRegularFieldToSchedule(groupViewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_MARK), "Walls #");
                            AddRegularFieldToSchedule(groupViewSchedule, new ElementId(BuiltInParameter.WALL_USER_HEIGHT_PARAM), "Height (ft)"); ; ;
                            AddRegularFieldToSchedule(groupViewSchedule, new ElementId(BuiltInParameter.CURVE_ELEM_LENGTH), "Width (ft)"); 
                            break;
                        case "Windows":
                            AddRegularFieldToSchedule(groupViewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_MARK), "Window #");
                            AddRegularFieldToSchedule(groupViewSchedule, new ElementId(BuiltInParameter.WINDOW_HEIGHT), "Height (ft)");
                            AddRegularFieldToSchedule(groupViewSchedule, new ElementId(BuiltInParameter.WINDOW_WIDTH), "Width (ft)");
                            break;
                    }

                    var schedulableField = def.GetSchedulableFields().FirstOrDefault(sf => sf.GetName(doc) == Group);

                    var schedulableFieldArea = def.GetSchedulableFields().FirstOrDefault(sf => sf.GetName(doc) == "Area1");
                    var schedulableFieldAreaWall= def.GetSchedulableFields().FirstOrDefault(sf => sf.GetName(doc) == "Area");

                    if (schedulableFieldAreaWall != null & TagType == "Walls")
                    {
                        ScheduleField field = groupViewSchedule.Definition.AddField(schedulableFieldAreaWall);

                        field.ColumnHeading = "Area (sq. ft)";
                    }


                    if (schedulableFieldArea != null & TagType != "Walls")
                    {
                        ScheduleField field = groupViewSchedule.Definition.AddField(schedulableFieldArea);

                        field.ColumnHeading = "Area (sq. ft)";
                    }

                    if (schedulableField != null && Group != "All")
                    {
                        ScheduleField field = groupViewSchedule.Definition.AddField(schedulableField);

                        field.ColumnHeading = Group;
                        field.IsHidden = true;

                        ScheduleFilter filter = new ScheduleFilter(field.FieldId, ScheduleFilterType.Equal, 1);

                        groupViewSchedule.Definition.AddFilter(filter);
                    }

                    ScheduleField field0 = def.GetField(0);
                    ScheduleField field1 = def.GetField(1);
                    ScheduleField field2 = def.GetField(2);
                    ScheduleField field3 = def.GetField(3);


                    var group = new FilteredElementCollector(doc)
                        .OfCategory(cat)
                        .OfClass(typeof(FamilyInstance))
                        .WhereElementIsNotElementType()
                        .ToList();

                    var wallGroup = new FilteredElementCollector(doc)
                        .OfCategory(cat) // Ensure 'cat' is set to BuiltInCategory.OST_Walls
                        .OfClass(typeof(Wall))
                        .WhereElementIsNotElementType()
                        .ToList();

                    var filteredGroup = group
                        .Where(fi => (fi.LookupParameter(Group)?.AsInteger() ?? 0) == 1)
                        .FirstOrDefault();

                    // If filteredGroup is null, try filtering from group1
                    if (filteredGroup == null)
                    {
                        filteredGroup = wallGroup
                            .Where(w => (w.LookupParameter(Group)?.AsInteger() ?? 0) == 1)
                            .FirstOrDefault();
                    }

                    Color color = new Color(0,0,0);

                    if (filteredGroup != null)
                    {
                        // Find the associated tag for the FamilyInstance
                        var tag = new FilteredElementCollector(doc)
                            .OfCategory(tagCat)
                            .WhereElementIsNotElementType()
                            .Cast<IndependentTag>()
                            .FirstOrDefault(t => t.TaggedElementId.HostElementId == filteredGroup.Id);
                        if (tag != null)
                        {
                            var view = doc.ActiveView;
                            var overrideGraphics = view.GetElementOverrides(tag.Id);

                            if (overrideGraphics != null)
                            { 
                                var projectionLineColor = overrideGraphics.ProjectionLineColor;

                                color = projectionLineColor.IsValid
                                    ? overrideGraphics.ProjectionLineColor
                                    : new Color(255, 255, 255);
                            }
                        }
                    }

                    FormatOptions options = field1.GetFormatOptions();
                    options.UseDefault = true;

                    // Set the number of decimal places (accuracy)
                    //options.Accuracy = 2; // Set to 2 decimals

                    TableCellStyle style = field1.GetStyle();

                    TableCellStyleOverrideOptions overrideOptions = style.GetCellStyleOverrideOptions();
                    overrideOptions.BackgroundColor = true;

                    if (Group != "All")
                    {
                        style.BackgroundColor = new Color(color.Red, color.Green, color.Blue);
                        overrideOptions.FontColor = true;
                    }


              
                    //style.TextColor = new Color(color.Red, color.Green, color.Blue);
                    //overrideOptions.Italics = true;
                    //style.IsFontItalic = true;

                    style.SetCellStyleOverrideOptions(overrideOptions);

                    FormatOptions options0 = field0.GetFormatOptions();
                    options0.UseDefault = false;
                    TableCellStyle style0 = field0.GetStyle();

                    TableCellStyleOverrideOptions overrideOptions0 = style0.GetCellStyleOverrideOptions();

                    if (color.Red != 0 && color.Green != 0 && color.Blue !=0 )
                    {
                        overrideOptions0.BackgroundColor = true;
                    }
                    else
                    {
                        overrideOptions0.BackgroundColor = false;
                    }

                    style0.BackgroundColor = new Color(color.Red, color.Green, color.Blue);

                    field0.SetStyle(style);
                    //field0.SetFormatOptions(options0);
                    field1.SetStyle(style);
                    field1.SetFormatOptions(options);
                    field2.SetStyle(style);
                    field2.SetFormatOptions(options);
                    field3.SetStyle(style);
                    field3.SetFormatOptions(options);

                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TaskDialog.Show("Schedulable Fields", e.Message);
                throw;
            }
        }

        public string GetName()
        {
            return "AddScheduleHandler";
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="parameterName"></param>
        /// <returns>ParameterElement for schedule</returns>
        private static ParameterElement GetCustomParameter(Document doc, string parameterName)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(ParameterElement))
                .Cast<ParameterElement>()
                .FirstOrDefault(param => param.Name == parameterName);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="paramId"></param>
        /// <param name="headingName"></param>
        public static void AddRegularFieldToSchedule(ViewSchedule schedule, ElementId paramId, string headingName)
        {
            ScheduleDefinition definition = schedule.Definition;

            SchedulableField schedulableField =
                definition.GetSchedulableFields().FirstOrDefault(sf => sf.ParameterId == paramId);

            if (schedulableField != null)
            {
                // Add the found field
                var field = definition.AddField(schedulableField);
                field.ColumnHeading = headingName;
            }
        }

    }
}
