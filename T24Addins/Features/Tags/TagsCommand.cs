using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using T24AddIns.Features.Tags.Form;

namespace T24AddIns.Features.Tags
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class TagsCommand : IExternalCommand
    {
        private static TagForm _form;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;

            if (_form == null || _form.IsDisposed)
            {
                _form = new TagForm(uiApp);
                _form.Show();
            }
            else
            {
                _form.BringToFront();
            }

            return Result.Succeeded;
        }
    }
}
