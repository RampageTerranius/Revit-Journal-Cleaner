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
            // Prepare a list to store all files we want to delete.
            List<string> deleteList = new List<string>();

            // Ask user to confirm they want to delete journals.
            TaskDialog dialogBox = new TaskDialog("WARNING!");
            dialogBox.MainContent = "WARNING! this will delete all .rvt files in your local application data folder\n" +
                                    "Are you sure you wish to proceed?";
            dialogBox.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;

            // If they say no cancel the plugin.
            TaskDialogResult result = dialogBox.Show();
            if (result == TaskDialogResult.No)         
                return Result.Cancelled;
            
            // Get the local app data location.
            string localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Check what version we are currnetly running and prepare to wipe journals for that version.
            string revitVersion = commandData.Application.Application.VersionName;

            // Get a list of all files inside of the journals folder of the current version of revit.
            string[] fileList = Directory.GetFiles(localPath + "\\Autodesk\\Revit\\" + commandData.Application.Application.VersionName + "\\Journals");

            try
            {
                long totalFileSize = 0;

                // Check if a file ends with .rvt and add it to the delete list if it does.
                foreach (string str in fileList)
                    if (str.EndsWith(".rvt"))
                    {
                        totalFileSize += new FileInfo(str).Length;
                        deleteList.Add(str);
                    }

                // If we have nothing to delete dont even bother going further.
                if (deleteList.Count != 0)
                {
                    // Prepare a string.
                    string delete = deleteList.Count.ToString() + " File(s) will be deleted.\n" +
                                    "This will clear up " + GetBytesReadable(totalFileSize) + " of space.\n" +
                                    "Please confirm that you wish to delete these files.";
                                    

                    // Ask user to confirm they want to delete the given journals.
                    TaskDialog deleteDialogBox = new TaskDialog("WARNING!");
                    dialogBox.MainContent = delete;
                    dialogBox.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;

                    // If they say no cancel the plugin.
                    if (dialogBox.Show() == TaskDialogResult.No)
                    {
                        // Show a box confirming cancelation.
                        TaskDialog canceledDialogBox = new TaskDialog("Information");
                        canceledDialogBox.CommonButtons = TaskDialogCommonButtons.Ok;
                        canceledDialogBox.MainContent = "Canceled deleting files.";

                        canceledDialogBox.Show();

                        return Result.Cancelled;
                    }
                        
                    // Finally, go ahead and delete all the files.
                    foreach (string str in deleteList)
                        File.Delete(str);
                }
            }
            catch (Exception e)
            {
                // If there was a problem report it to the user.
                TaskDialog error = new TaskDialog("Error!");
                error.CommonButtons = TaskDialogCommonButtons.Ok;
                error.MainContent = "Error: " + e.Message;

                error.Show();
                return Result.Failed;
            }

            // Show a box confirming the plugin succeeded.
            TaskDialog td = new TaskDialog("Information");
            td.CommonButtons = TaskDialogCommonButtons.Ok;
            td.MainContent = "Success!\n" +
                             deleteList.Count.ToString() + " Journal(s) found and deleted.";

            td.Show();

            return Result.Succeeded;
        }

        // The following code comes from the following location and is not owned or maintained by the creator of this plugin.
        // https://www.somacon.com/p576.php
        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        public string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }
    }
}
