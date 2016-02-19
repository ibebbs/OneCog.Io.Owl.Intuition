using OneCog.Core.Text;
using System;

namespace OneCog.Io.Owl.Intuition.Network.Command.Response.Builder
{
    internal class SaveResponse : IBuilder
    {
        private const string StatusGroup = "Status";
        private const string SaveResponseName = "SaveResponse";
        private const string SaveResponsePattern = @"(?<Status>OK|ERROR),SAVE(?:,)?";

        public IResponse Build(System.Text.RegularExpressions.Match match)
        {
            Status status = match.ReadGroupAs(StatusGroup, value => (Status) Enum.Parse(typeof(Status), value, true));

            return new Save(status);
        }

        public string Name
        {
            get { return SaveResponseName; }
        }

        public string Regex
        {
            get { return SaveResponsePattern; }
        }
    }
}
