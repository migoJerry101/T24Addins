using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using T24AddIns.Handlers.AddColorToTags;
using T24AddIns.Handlers.AddDoorTagHandler;
using T24AddIns.Handlers.AddScheduleHandler;
using T24AddIns.Handlers.AddScheduleToSheetHandler;
using T24AddIns.Handlers.AddTagParamHandler;
using T24AddIns.Handlers.AddWallTagHandler;
using T24AddIns.Handlers.AddWindowTagHandler;
using T24AddIns.Handlers.CalculateAreaHandler;
using T24AddIns.Handlers.ImportTagHandler;
using T24AddIns.Handlers.MoveDoorTagHandler;
using T24AddIns.Handlers.MoveTagFromWallHandler;
using T24AddIns.Handlers.MoveWallTagHandler;
using T24AddIns.Handlers.MoveWindowTagHandler;

namespace T24AddIns.Features.Tags.Form
{
    public partial class TagForm : System.Windows.Forms.Form
    {
        private UIApplication _uiApp;
        private UIDocument _uiDoc;
        private Document _document;

        private readonly ExternalEvent _externalEventAddTagParam;
        private readonly AddTagParameter _addTagParameter = new AddTagParameter();

        private readonly ExternalEvent _externalEventAddWallTagParam;
        private readonly AddWallTagParameter _addWallTagParameter = new AddWallTagParameter();

        private readonly ExternalEvent _externalImportTag;
        private readonly ImportTagHandler _importTagHandler = new ImportTagHandler();

        private readonly ExternalEvent _externalAddDoorTag;
        private readonly AddDoorTagHandler _addDoorTagHandler = new AddDoorTagHandler();

        private readonly ExternalEvent _externalAddWindowTag;
        private readonly AddWindowTagHandler _addWindowTagHandler = new AddWindowTagHandler();

        private readonly ExternalEvent _externalAddWallTag;
        private readonly AddWallTagHandler _addWallTagHandler = new AddWallTagHandler();

        private readonly ExternalEvent _externalMoveDoorTag;
        private readonly MoveDoorTagHandler _moveDoorTagHandler = new MoveDoorTagHandler();

        private readonly ExternalEvent _externalMoveWindowTag;
        private readonly MoveWindowTagHandler _moveWindowTagHandler = new MoveWindowTagHandler();

        private readonly ExternalEvent _externalMoveWallTag;
        private readonly MoveWallTagHandler _moveWallTagHandler = new MoveWallTagHandler();

        private readonly ExternalEvent _externalAddColor;
        private readonly AddColorToTags _addColorToTags = new AddColorToTags();

        private readonly ExternalEvent _externalAddSchedule;
        private readonly AddScheduleHandler _addScheduleHandler = new AddScheduleHandler();

        private readonly ExternalEvent _externalMoveTagFromWall;
        private readonly MoveTagFromWallHandler _moveTagFromWall = new MoveTagFromWallHandler();

        private readonly ExternalEvent _externalAddScheduleToSheet;
        private readonly AddScheduleToSheetHandler _addScheduleToSheetHandler = new AddScheduleToSheetHandler();

        private readonly ExternalEvent _externalCalculateArea;
        private readonly CalculateAreaHandler _calculateAreaHandler = new CalculateAreaHandler();

        public List<string> Properties = new List<string>()
        {
            "North", "South", "East", "West", "Group 1", "Group 2", "Group 3", "Group 4", "Group 5", "Group 6",
            "Group 7", "Group 8", "All"
        };

        public TagForm(UIApplication uiApp)
        {
            InitializeComponent();

            _uiApp = uiApp;
            _uiDoc = uiApp.ActiveUIDocument;
            _document = _uiDoc.Document;

            _externalEventAddTagParam = ExternalEvent.Create(_addTagParameter);
            _externalEventAddWallTagParam = ExternalEvent.Create(_addWallTagParameter);
            _externalImportTag = ExternalEvent.Create(_importTagHandler);
            _externalAddDoorTag = ExternalEvent.Create(_addDoorTagHandler);
            _externalAddWindowTag = ExternalEvent.Create(_addWindowTagHandler);
            _externalAddWallTag = ExternalEvent.Create(_addWallTagHandler);

            _externalMoveDoorTag = ExternalEvent.Create(_moveDoorTagHandler);
            _externalMoveWindowTag = ExternalEvent.Create(_moveWindowTagHandler);
            _externalMoveWallTag = ExternalEvent.Create(_moveWallTagHandler);
            _externalAddColor = ExternalEvent.Create(_addColorToTags);
            _externalAddSchedule = ExternalEvent.Create(_addScheduleHandler);
            _externalMoveTagFromWall = ExternalEvent.Create(_moveTagFromWall);
            _externalAddScheduleToSheet = ExternalEvent.Create(_addScheduleToSheetHandler);
            _externalCalculateArea = ExternalEvent.Create(_calculateAreaHandler);

            PropSelect.DataSource = Properties.Where(x => x != "All").ToList();

            TagType.DataSource = new List<string> { "Doors", "Walls", "Windows" };
            ScheduleTagTypeComboBox.DataSource = new List<string> { "Doors", "Walls", "Windows" };

            ScheduleGroupComboBox.DataSource = Properties;
            AddViewDataSource();
        }

        private void AddViewDataSource()
        {
            var view = new FilteredElementCollector(_document)
                .OfClass(typeof(ViewSheet)).WhereElementIsNotElementType()
                .Cast<ViewSheet>()
                .Select(x => x.Name)
                .ToList();

            view.Add("None");

            ViewComboBox.DataSource = view;
        }

        private void AddPropertiesBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var elements = new List<Element>();

                _addTagParameter.Document = _document;
                _addTagParameter.Elements = elements;
                _externalEventAddTagParam.Raise();

                //calculate Area
                _externalCalculateArea.Raise();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                TaskDialog.Show("Error", $"An unexpected error occurred: {exception.Message}");
            }
        }

        private void ExitBtn_Click(object sender, EventArgs e)
        {
            _externalImportTag.Raise();
        }

        private void TagDoorBtn_Click(object sender, EventArgs e)
        {
            _externalAddDoorTag.Raise();
            _externalMoveDoorTag.Raise();


            _moveTagFromWall.Category = BuiltInCategory.OST_DoorTags;
            _externalMoveTagFromWall.Raise();
        }

        private void TagWindowBtn_Click(object sender, EventArgs e)
        {
            _externalAddWindowTag.Raise();
            _externalMoveWindowTag.Raise();

            _moveTagFromWall.Category = BuiltInCategory.OST_WindowTags;
            _externalMoveTagFromWall.Raise();
        }

        private void TagWallBtn_Click(object sender, EventArgs e)
        {
            _externalAddWallTag.Raise();
            _externalMoveWallTag.Raise();

            _moveTagFromWall.Category = BuiltInCategory.OST_WallTags;
            _externalMoveTagFromWall.Raise();
        }

        public ColorConfig Config { get; set; } = new ColorConfig();
        private void button1_Click(object sender, EventArgs e)
        {
            _addColorToTags.Config = Config;
            _externalAddColor.Raise();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                var color = colorDialog1.Color;
                Config.Color = new Color(color.R, color.G, color.B);
            }

        }

        private void PropSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config.Group = PropSelect.SelectedItem.ToString();
        }

        private void TagType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var element = TagType.SelectedItem.ToString();

            switch (element)
            {
                case "Doors":

                    Config.TagType = BuiltInCategory.OST_DoorTags;
                    Config.Element = BuiltInCategory.OST_Doors;
                    break;
                case "Walls":
                    Config.TagType = BuiltInCategory.OST_WallTags;
                    Config.Element = BuiltInCategory.OST_Walls;
                    break;
                case "Windows":
                    Config.TagType = BuiltInCategory.OST_WindowTags;
                    Config.Element = BuiltInCategory.OST_Windows;
                    break;
            }
        }

        private void ScheduleGroupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
           _addScheduleHandler.Group = ScheduleGroupComboBox.SelectedItem.ToString();
        }

        private void ScheduleTagTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _addScheduleHandler.TagType = ScheduleTagTypeComboBox.SelectedItem.ToString();
        }

        private void ViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _addScheduleHandler.ViewName = ViewComboBox.SelectedItem.ToString();
            _addScheduleToSheetHandler.SheetName = ViewComboBox.SelectedItem.ToString();
        }

        private void GenerateScheduleBtn_Click(object sender, EventArgs e)
        {
            var name = $"{_addScheduleHandler.Group} {_addScheduleHandler.TagType} Schedule";
            _addScheduleToSheetHandler.ScheduleName = name;

            _externalAddSchedule.Raise();
            _externalAddScheduleToSheet.Raise();

            //calculate Area
            _externalCalculateArea.Raise();
        }
    }
}
    