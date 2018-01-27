using System.Collections.ObjectModel;
using System.ComponentModel;
using Mercurial;
using Mercurial.Attributes;

// ReSharper disable CollectionNeverQueried.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace HgVersion.VCS
{
    public sealed class CountCommand : MercurialCommandBase<CountCommand>, IMercurialCommand<int>
    {
        /// <summary>
        /// Gets the result of executing the command as a count of commits from log.
        /// </summary>
        public int Result { get; private set; }
        
        /// <summary>
        /// Gets the log output template to process.
        /// </summary>
        [NullableArgument(NonNullOption = "--template")]
        [DefaultValue("*")]
        public string Template { get; }
        
        /// <summary>
        /// Gets the collection of <see cref="Revisions"/> to process/include.
        /// </summary>
        [RepeatableArgument(Option = "--rev")]
        public Collection<RevSpec> Revisions { get; }

        public CountCommand() : base("log")
        {
            Revisions = new Collection<RevSpec>();
            Template = "*";
        }
        
        /// <summary>
        /// Adds the value to the <see cref="Revisions"/> collection property and
        /// returns this <see cref="CountCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The value to add to the <see cref="Revisions"/> collection property.
        /// </param>
        /// <returns>
        /// This <see cref="CountCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public CountCommand WithRevision(RevSpec value)
        {
            Revisions.Add(value);
            return this;
        }

        protected override void ParseStandardOutputForResults(int exitCode, string standardOutput)
        {
            base.ParseStandardOutputForResults(exitCode, standardOutput);
            Result = standardOutput?.Length ?? 0;
        }
    }
}