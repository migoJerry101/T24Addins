using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using T24AddIns.Features.Tags;
using T24AddIns.Features.Tags.Form;

namespace T24AddIns
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Application : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            var panel = RibbonPanel(application);
            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            if (panel.AddItem(new PushButtonData("T24 Tagging", "T24 Tagging", assemblyPath, typeof(TagsCommand).ToString()))
                is PushButton button)
            {
                var uri = new Uri(Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "Select.ico"));
                var bitmap = new BitmapImage(uri);

                button.LargeImage = bitmap;
            }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public RibbonPanel RibbonPanel(UIControlledApplication application)
        {
            const string tabName = "T24";
            const string panelName = "T24 Tool";

            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            try
            {
                application.CreateRibbonPanel(tabName, panelName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            var ribbonPanel = application
                .GetRibbonPanels(tabName)
                .FirstOrDefault(x => x.Name == panelName);

            return ribbonPanel;
        }
    }
}
