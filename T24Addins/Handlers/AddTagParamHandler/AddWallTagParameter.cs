using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace T24AddIns.Handlers.AddTagParamHandler
{
    internal class AddWallTagParameter : IExternalEventHandler
    {
        public Document Document { get; set; }

        public List<string> Properties = new List<string>()
        {
            "North", "South", "East", "West", "Group 1", "Group 2", "Group 3", "Group 4", "Group 5", "Group 6", "Group 7", "Group 8"
        };

        public void Execute(UIApplication app)
        {
            try
            {
                // Open shared parameter file if using shared parameters
                DefinitionFile sharedParamFile = Document.Application.OpenSharedParameterFile();

                if (sharedParamFile == null)
                {
                    string sharedParamFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SharedParameters.txt");

                    // Create a new shared parameter file if it doesn't exist
                    if (!File.Exists(sharedParamFilePath))
                    {
                        File.Create(sharedParamFilePath).Dispose(); // Create and release the file
                    }

                    // Assign the newly created shared parameter file to Revit
                    Document.Application.SharedParametersFilename = sharedParamFilePath;

                    // Re-open the shared parameter file
                    sharedParamFile = Document.Application.OpenSharedParameterFile();

                    if (sharedParamFile == null)
                    {
                        TaskDialog.Show("Error", "Failed to create or open the shared parameter file.");
                        return;
                    }
                }

                // Get or create the shared parameter group for the walls
                DefinitionGroup definitionGroup = sharedParamFile.Groups.get_Item("Tag Parameters");
                if (definitionGroup == null)
                {
                    definitionGroup = sharedParamFile.Groups.Create("Tag Parameters");
                }

                // Get the Wall category (OST_Walls)
                var doc = app.ActiveUIDocument.Document;
                Category wallCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Walls);

                // Begin a transaction for the document to add parameters
                using (var transaction = new Transaction(doc, "Add Wall Tag Parameter"))
                {
                    transaction.Start();

                    // Check and create the shared parameters
                    foreach (var propName in Properties)
                    {
                        // Check if the shared parameter already exists
                        Definition definition = definitionGroup.Definitions.get_Item(propName);
                        if (definition == null)
                        {
                            // Create the shared parameter if it does not exist
                            ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions(propName, ParameterType.YesNo);
                            definition = definitionGroup.Definitions.Create(options);
                        }

                        // Bind the shared parameter to the Wall category
                        CategorySet categorySet = new CategorySet();
                        categorySet.Insert(wallCategory);

                        InstanceBinding binding = doc.Application.Create.NewInstanceBinding(categorySet);

                        // Add the shared parameter to the project
                        if (!doc.ParameterBindings.Contains(definition))
                        {
                            doc.ParameterBindings.Insert(definition, binding, BuiltInParameterGroup.PG_IDENTITY_DATA);
                        }
                    }

                    // Commit the transaction
                    transaction.Commit();
                }

                TaskDialog.Show("Success", "Tag parameters added to walls successfully.");
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
            }
        }

        public string GetName()
        {
            return "AddWallTagParameter";
        }
    }
}
