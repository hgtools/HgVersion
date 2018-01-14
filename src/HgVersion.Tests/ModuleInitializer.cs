using System;
using NUnit.Framework;
using VCSVersion;

namespace HgVersion.Tests
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
