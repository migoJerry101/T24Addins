using Autodesk.Revit.DB;

namespace T24AddIns.Handlers.AddColorToTags
{
    public class ColorConfig
    {
        public string Group { get; set; } = string.Empty;
        public Color Color { get; set; } = new Color(0, 0, 0);
        public BuiltInCategory Element { get; set; }
        public BuiltInCategory TagType { get; set; }

    }
}
