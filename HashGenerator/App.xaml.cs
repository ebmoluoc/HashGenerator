using CsharpHelpers.Helpers;
using System.Windows;

namespace HashGenerator
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var assemblyInfo = new AssemblyInfo(typeof(App).Assembly, "Resources.LICENSE");
            var dataDirectory = new AppDataRoaming(assemblyInfo.Product);
            var logger = new TextFileLogger(dataDirectory.GetFilePath($"{assemblyInfo.FileName}.log"));

            AppHelper.AssemblyInfo = assemblyInfo;
            AppHelper.DataDirectory = dataDirectory;
            AppHelper.Logger = logger;
            AppHelper.CatchUnhandledException = true;
            AppHelper.SetAppMutex($"Global\\{assemblyInfo.Guid}");
        }
    }
}
