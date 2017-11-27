using HgVersion.SemanticVersions;
using HgVersion.VersionCalculation;
using HgVersion.VersionCalculation.BaseVersionCalculation;

namespace HgVersion
{
    public sealed class VersionEngine
    {
        private readonly IVersionContext _context;

        public VersionEngine(IVersionContext context)
        {
            _context = context;
        }

        public SemanticVersion Execute()
        {
            var baseCalculator = new BaseVersionCalculator(
                new FallbackBaseVersionStrategy());
            
            var versionCalculator = new NextVersionCalculator(baseCalculator);
            return versionCalculator.CalculateVersion(_context);
        }
    }
}