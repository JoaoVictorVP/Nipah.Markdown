using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nipah.Markdown.Models;

public class ParseResult
{
    private readonly MarkdownDocument? document;
    private readonly string error = "";

    public ParseResult(MarkdownDocument document)
    {
        this.document = document;
    }

    public ParseResult(string error)
    {
        this.error = error;
    }

    public bool IsSuccess => document is not null;

    public MarkdownDocument Expect(string message)
        => document is not null
        ? document
        : throw new Exception(message);

    public void Match(Action<MarkdownDocument> ok, Action<string> error)
    {
        if (document is not null)
            ok(document);
        else
            error(this.error);
    }
}
