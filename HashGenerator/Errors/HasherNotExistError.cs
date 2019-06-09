using CsharpHelpers.NotifyServices;
using HashGenerator.Services;
using System.Runtime.CompilerServices;

namespace HashGenerator.Errors
{

    public sealed class HasherNotExistError : NotifyDataErrorEditInfo
    {
        public HasherNotExistError(IHasherService svc, string value, [CallerMemberName] string propertyName = null)
        {
            HasError = !svc.HasherExists(value);
            ErrorMessage = "This hasher does not exist.";
            PropertyName = propertyName;
        }
    }

}
