using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nipah.Markdown.Models;

[DebuggerStepThrough]
public record MarkdownContainer : MarkdownElement
{
    public List<MarkdownElement> Elements { get; } = new(32);
}
