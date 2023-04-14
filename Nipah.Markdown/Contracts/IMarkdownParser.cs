using Nipah.Markdown.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nipah.Markdown.Contracts;

public interface IMarkdownParser
{
    ParseResult Parse(string markdown);
}
