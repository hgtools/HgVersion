using NUnit.Framework;
using System;
using VCSVersion;

namespace HgVersionTests
{
    [SetUpFixture]
    public class ModuleInitializer
    {
        [OneTimeSetUp]
        public static void Initialize()
        {
            Logger.SetLoggers(
                s => Console.WriteLine(s),
                s => Console.WriteLine(s),
                s => Console.WriteLine(s),
                s => Console.WriteLine(s));
        }
    }
}
