using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using T24AddIns.Handlers.Tags;

namespace T24AddIns.Features.Tags.Form
{
    public partial class TagForm : System.Windows.Forms.Form
    {
        private UIApplication _uiApp;
        private UIDocument _uiDoc;
        private Document _doc;

        private readonly ExternalEvent _externalEventAddTagParam;
        private readonly AddTagParameter _addTagParameter = new AddTagParameter();

        public TagForm(UIApplication uiApp)
        {
            InitializeComponent();

            _uiApp = uiApp;
            _uiDoc = uiApp.ActiveUIDocument;
            _doc = _uiDoc.Document;

            _externalEventAddTagParam = ExternalEvent.Create(_addTagParameter);
        }

        private void AddPropertiesBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var doors = new FilteredElementCollector(_doc)
                    .OfCategory(BuiltInCategory.OST_Doors)
                    .OfClass(typeof(FamilySymbol))
                    .ToList();

                var walls = new FilteredElementCollector(_doc)
                    .OfCategory(BuiltInCategory.OST_Walls)
                    .OfClass(typeof(WallType))
                    .ToList();

                var windows = new FilteredElementCollector(_doc)
                    .OfCategory(BuiltInCategory.OST_Windows)
                    .OfClass(typeof(FamilySymbol))
                    .ToList();

                _addTagParameter.Document = _doc;
                _addTagParameter.Elements = doors;

                _externalEventAddTagParam.Raise();

            }
            catch (Exception exception)
            {
                // Log other unexpected exceptions
                Console.WriteLine(exception);
                TaskDialog.Show("Error", $"An unexpected error occurred: {exception.Message}");
            }
        }
    }
}
