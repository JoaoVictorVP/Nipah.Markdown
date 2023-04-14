﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nipah.Markdown.Models;

public record MarkdownTitle(string Title, int Level) : MarkdownElement
{
}
