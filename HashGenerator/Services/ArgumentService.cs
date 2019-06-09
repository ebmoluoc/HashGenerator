using CsharpHelpers.Helpers;
using System;

namespace HashGenerator.Services
{

    public interface IArgumentService
    {
        string FilePath { get; }
        string HashName { get; }
        bool ComputeNow { get; }
    }


    public class ArgumentService : IArgumentService
    {

        private readonly string[] _args = Environment.GetCommandLineArgs();


        public string FilePath
        {
            get { return EnvironmentHelper.GetArgument("-f:", _args); }
        }


        public string HashName
        {
            get { return EnvironmentHelper.GetArgument("-h:", _args); }
        }


        public bool ComputeNow
        {
            get { return EnvironmentHelper.GetArgument("-c", _args) == ""; }
        }

    }

}
