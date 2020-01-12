using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace BFAMExercise.Test
{
    [TestClass]
    class SetupAssemblyInitializer
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Log.CloseAndFlush();
        }
    }
}
