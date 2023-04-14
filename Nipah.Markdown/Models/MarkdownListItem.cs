using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nipah.Markdown.Models;

public record MarkdownListItem(MarkdownElement Item) : MarkdownElement
{
}
