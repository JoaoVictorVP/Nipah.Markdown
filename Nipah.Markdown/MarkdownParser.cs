using Nipah.Markdown.Contracts;
using Nipah.Markdown.Models;
using NipahTokenizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nipah.Markdown;

public class MarkdownParser : IMarkdownParser
{
    static readonly Tokenizer tokenizer = new();

    public ParseResult Parse(string markdown)
    {
        try
        {
            return new ParseResult(ParseDocument(markdown));
        }
        catch (Exception e)
        {
            return new ParseResult(e.Message);
        }
    }
    static MarkdownDocument ParseDocument(string markdown)
    {
        var tokens = CollectionsMarshal.AsSpan(tokenizer.Tokenize(markdown, TokenizerOptions.Default));

        var doc = new MarkdownDocument();

        Span<Token> SkipEOF(Span<Token> tokens)
        {
            while(tokens.Length > 0 && tokens[0] is { Type: TokenType.EOF })
                tokens = tokens[1..];
            return tokens;
        }
        string GetText(Span<Token> tokens, out Span<Token> outTokens)
        {
            var str = "";
            while (tokens.Length > 0 && tokens[0] is not { Type: TokenType.EOF })
            {
                bool IsSep(string tks)
                    => tks.All(c => char.IsSeparator(c));

                if (str is not "" && IsSep(tokens[0].Text) is false)
                    str += " ";

                str += tokens[0].Text;
                tokens = tokens[1..];
            }
            outTokens = tokens;
            return str;
        }
        Continue<MarkdownElement> ParseElementOut(Span<Token> tokens, int possibleListLevel, out Continue<MarkdownElement> parsed)
        {
            parsed = ParseElement(tokens, possibleListLevel);
            return parsed;
        }
        Continue<MarkdownElement> ParseElement(Span<Token> tokens, int possibleListLevel = 1)
        {
            tokens = SkipEOF(tokens);
            return ParseTitle(tokens, 1)
                    .Or(ParseCitation(tokens))
                    .Or(ParseSeparator(tokens))
                    .Or(ParseList(tokens, possibleListLevel))
                    .Or(ParseText(tokens));
        }

        Continue<MarkdownElement> ParseTitle(Span<Token> tokens, int fromLevel = 1)
            => tokens switch
            {
                [{ Type: TokenType.Hashtag }, { Type: TokenType.Hashtag }, ..]
                    => ParseTitle(tokens[1..], fromLevel + 1),
                [{ Type: TokenType.Hashtag }, ..]
                    => new MarkdownTitle(GetText(tokens[1..], out var rest), fromLevel) is var title
                        ? Continue.From<MarkdownElement>(rest, title) : default,
                _ => Continue.Not(tokens)
            };
        Continue<MarkdownElement> ParseCitation(Span<Token> tokens)
            => tokens switch
            {
                [{ Type: TokenType.Larger }, ..]
                    => ParseElement(tokens[1..]) is var parsed and { Ok: true }
                    ? Continue.From<MarkdownElement>(parsed.Tokens, new MarkdownCitation(parsed.Value!))
                    : Continue.Not(tokens),
                _ => Continue.Not(tokens)
            };
        Continue<MarkdownElement> ParseSeparator(Span<Token> tokens)
            => tokens switch
            {
                [{ Type: TokenType.Minus }, { Type: TokenType.Minus }, { Type: TokenType.Minus}, { Type: TokenType.EOF }, ..]
                    => Continue.From<MarkdownElement>(tokens[4..], new MarkdownSeparator()),
                _ => Continue.Not(tokens)
            };
        Continue<MarkdownElement> ParseList(Span<Token> tokens, int level)
        {
            level = level < 2 ? level : 2;
            Continue<MarkdownListItem> ParseListItem(Span<Token> tokens)
            {
                var og = tokens;

                tokens = SkipEOF(tokens);
                for(int i = 0; i < level; i++)
                {
                    if(tokens is { IsEmpty: true }
                        or [{ Type: not (TokenType.Multiply or TokenType.Minus) }, ..])
                        return Continue.Not(tokens);
                    tokens = tokens[1..];
                }

                if (tokens[0] is { Type: TokenType.Multiply or TokenType.Minus })
                    return Continue.Not(og);

                var item = ParseElement(tokens, level + 1);

                if (item.Ok)
                    return item.Then(item.Tokens, new MarkdownListItem(item.Value!));
                return Continue.Not(og);
            }

            var list = new List<MarkdownListItem>();

        parse_next:
            var item = ParseListItem(tokens);
            if (item.Ok is false)
                return list.Count is 0
                    ? Continue.Not(tokens)
                    : Continue.From<MarkdownElement>(item.Tokens, new MarkdownList(list));
            else
            {
                list.Add(item.Value!);
                tokens = item.Tokens;
                goto parse_next;
            }
        }
        Continue<MarkdownElement> ParseText(Span<Token> tokens)
            => tokens switch
            {
                { Length: > 0 } and [{ Type: not TokenType.EOF }, ..]
                    => GetText(tokens, out var rest) is var text and not (null or "")
                        ? Continue.From<MarkdownElement>(rest, new MarkdownText(text))
                        : Continue.Not(tokens),
                _ => Continue.Not(tokens)
            };

    next:
        var parsed = ParseElement(tokens);
        if(parsed.Ok)
        {
            doc.Elements.Add(parsed.Value!);
            tokens = parsed.Tokens;
            goto next;
        }

        return doc;
    }

    [DebuggerStepThrough]
    public readonly ref struct Continue<T>
    {
        public readonly Span<Token> Tokens;
        public readonly T? Value;
        public readonly bool Ok;

        public Continue<TNext> Then<TNext>(Span<Token> tokens, TNext next)
            => new Continue<TNext>(tokens, next, true);

        public Continue<T> Then(Continue<T> then)
            => then;

        public Continue<T> Or(Continue<T> other)
            => Ok ? this : other;

        public Continue(Span<Token> tokens, T? value, bool ok)
        {
            Tokens = tokens;
            Value = value;
            Ok = ok;
        }

        public static implicit operator Continue<T>(Continue.NotDef def)
            => new(def.Tokens, default, false);
    }
    [DebuggerStepThrough]
    public struct Continue
    {
        public static Continue<T> From<T>(Span<Token> tokens, T value)
            => new(tokens, value, true);

        public static NotDef Not(Span<Token> tokens)
            => new(tokens);

        public readonly ref struct NotDef
        {
            public readonly Span<Token> Tokens;

            public NotDef(Span<Token> tokens)
            {
                Tokens = tokens;
            }
        }
    }
}
