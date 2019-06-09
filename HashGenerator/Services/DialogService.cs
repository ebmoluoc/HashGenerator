using CsharpHelpers.DialogServices;
using CsharpHelpers.Helpers;
using System;
using System.IO;
using System.Windows;

namespace HashGenerator.Services
{

    public interface IDialogService
    {
        string BrowseFile(Window window, string filePath);
    }


    public class DialogService : IDialogService
    {

        public string BrowseFile(Window owner, string filePath)
        {
            using (var dialog = new FileOpenDialog())
            {
                dialog.SetClientGuid(new Guid("1941C841-398F-486F-8542-A0B2250F406F"));
                dialog.SetDefaultFolder(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                dialog.SetTitle("Location of the file to be computed");
                dialog.SetOkButtonLabel("Select");
                dialog.SetCancelButtonLabel("Cancel");
                dialog.SetFileNameLabel("File name :");
                dialog.DontAddToRecent = true;

                if (PathHelper.FileExists(filePath))
                {
                    dialog.SetFileName(Path.GetFileName(filePath));
                    dialog.SetFolder(Path.GetDirectoryName(filePath));
                }

                return dialog.ShowDialog(owner) == true ? dialog.GetResult() : null;
            }
        }

    }

}
