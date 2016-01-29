using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OneCog.Io.Owl.Intuition.Command.Response
{
    public interface IBuilder
    {
        IResponse Build(Match match);

        string Name { get; }
        string Regex { get; }
    }
}
