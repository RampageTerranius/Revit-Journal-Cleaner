using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.IO;

namespace Revit_Journal_Cleaner
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //prepare a list to store all files we want to delete
            List<string> deleteList = new List<string>();

            //ask user to confirm they want to delete journals
            TaskDialog dialogBox = new TaskDialog("WARNING!");
            dialogBox.MainContent = "WARNING! this will delete all .rvt files in your local application data folder\n" +
                                    "Are you sure you wish to proceed?";
            dialogBox.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;

            //if they say no cancel the plugin
            TaskDialogResult result = dialogBox.Show();
            if (result == TaskDialogResult.No)         
                return Result.Cancelled;
            
            //get the local app data location
            string localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            //get a list of all files inside of the journals folder
            string[] fileList = Directory.GetFiles(localPath + "\\Autodesk\\Revit\\Autodesk Revit 2019\\Journals");

            try
            {
                

                //check if a file ends with .rvt and add it to the delete list if it does
                foreach (string str in fileList)
                    if (str.EndsWith(".rvt"))
                        deleteList.Add(str);

                //if we have nothing to delete dont even bother going further
                if (deleteList.Count != 0)
                {
                    //prepare a string
                    string delete = "Please confirm that you wish to delete the following files.\n" +
                                    "The following files will be deleted:\n";

                    //check if a file ends with .rvt and add it to the delete list if it does
                    foreach (string str in deleteList)
                        delete += str + "\n";

                    //ask user to confirm they want to delete the given journals
                    TaskDialog deleteDialogBox = new TaskDialog("WARNING!");
                    dialogBox.MainContent = delete;
                    dialogBox.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;

                    //if they say no cancel the plugin
                    TaskDialogResult deleteResult = dialogBox.Show();
                    if (deleteResult == TaskDialogResult.No)
                    {
                        //show a box confirming cancelation
                        TaskDialog canceledDialogBox = new TaskDialog("Information");
                        canceledDialogBox.CommonButtons = TaskDialogCommonButtons.Ok;
                        canceledDialogBox.MainContent = "Canceled deleting files.";

                        canceledDialogBox.Show();

                        return Result.Cancelled;
                    }
                        
                    //finally, go ahead and delete all the files
                    foreach (string str in deleteList)
                        File.Delete(str);
                }
            }
            catch (Exception e)
            {
                //if there was a problem report it to the user
                TaskDialog error = new TaskDialog("Error!");
                error.CommonButtons = TaskDialogCommonButtons.Ok;
                error.MainContent = "Error: " + e.Message;

                error.Show();
                return Result.Failed;
            }

            //show a box confirming the plugin succeeded
            TaskDialog td = new TaskDialog("Information");
            td.CommonButtons = TaskDialogCommonButtons.Ok;
            td.MainContent = "Success!\n" +
                             deleteList.Count.ToString() + " Journal(s) found and deleted.";

            td.Show();

            return Result.Succeeded;
        }
    }
}
