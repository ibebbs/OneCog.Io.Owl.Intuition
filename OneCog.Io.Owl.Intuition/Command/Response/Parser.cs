using OneCog.Core.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OneCog.Io.Owl.Intuition.Command.Response
{
    public interface IParser
    {
        IEnumerable<IResponse> GetResponses(string value);
    }

    internal class Parser : IParser
    {
        public static readonly IParser Default = new Parser(new IBuilder[] {
            new Builder.DeviceResponse(),
            new Builder.RostaResponse(),
            new Builder.SaveResponse(),
            new Builder.UdpResponse(),
            new Builder.VersionResponse()
        });

        private readonly IBuilder[] _builders;
        private readonly Regex _responseRegex;

        public Parser(IEnumerable<IBuilder> builders)
        {
            _builders = (builders ?? Enumerable.Empty<IBuilder>()).ToArray();

            MultiRegexBuilder regexBuilder = new MultiRegexBuilder(true);
            regexBuilder.AddRange(builders.Select(builder => new KeyValuePair<string, string>(builder.Name, builder.Regex)));

            _responseRegex = regexBuilder.ToRegex();
        }

        public IEnumerable<IResponse> GetResponses(string value)
        {
            var matches = _responseRegex.Matches(value).OfType<Match>().ToArray();

            return _builders.SelectMany(builder => matches.Where(match => match.Groups[builder.Name].Success).Select(builder.Build)).ToArray();
        }
    }
}
