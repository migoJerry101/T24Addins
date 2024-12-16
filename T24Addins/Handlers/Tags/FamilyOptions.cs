using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace T24AddIns.Handlers.Tags
{
    internal class FamilyOptions : IFamilyLoadOptions
    {
        // This method is called when a family is found during the LoadFamily operation
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            // If the family is already in use, decide whether to overwrite parameter values
            overwriteParameterValues = true; // Set to true if you want to overwrite existing parameter values
            return true; // Return true to proceed with loading the family
        }

        // This method is called when a shared family is found during the LoadFamily operation
        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            // Specify the source for the shared family
            source = FamilySource.Project; // Use the version of the family in the project (alternatively, FamilySource.Family can be used)

            // Decide whether to overwrite parameter values
            overwriteParameterValues = true; // Set to true if you want to overwrite existing parameter values
            return true; // Return true to proceed with loading the shared family
        }
    }
}
