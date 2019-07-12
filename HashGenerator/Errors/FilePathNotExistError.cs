using CsharpHelpers.Helpers;
using CsharpHelpers.WindowServices;
using System.Runtime.CompilerServices;

namespace HashGenerator.Errors
{

    public sealed class FilePathNotExistError : NotifyDataErrorEditInfo
    {
        public FilePathNotExistError(string value, [CallerMemberName] string propertyName = null)
        {
            HasError = !PathHelper.FileExists(value);
            ErrorMessage = "This value points to a file that does not exist.";
            PropertyName = propertyName;
        }
    }

}
