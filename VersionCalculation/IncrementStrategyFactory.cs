using HgVersion.VersionCalculation.BaseVersionCalculation;

namespace HgVersion.VersionCalculation
{
    public static class IncrementStrategyFactory
    {
        public static IIncrementStrategy GetStrategy(IVersionContext context)
        {
            return new IncrementStrategy(VersionField.Patch);
        }

        public static IIncrementStrategy GetStrategy(IVersionContext context, BaseVersion baseVersion)
        {
            if (baseVersion.ShouldIncrement)
                return GetStrategy(context);
            
            return new IncrementStrategy(VersionField.None);
        }
    }
}