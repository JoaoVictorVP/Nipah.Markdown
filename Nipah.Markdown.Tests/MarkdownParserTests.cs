using FluentAssertions;
using Nipah.Markdown.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nipah.Markdown.Tests;

public class MarkdownParserTests
{
    [Fact]
    public void CanParseSimpleMarkdown()
    {
        // Prepare
        var md = """
            # Title
            > Citation

            * Item 1
            * Item 2
            * Item 3

            ---
            ### Sub Title
            Text
            """;

        var parser = new MarkdownParser();

        // Act
        var result = parser.Parse(md);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        var document = result.Expect("No error");

        document.Elements.Should().HaveCount(6);

        document.Elements[0].As<MarkdownTitle>().Title.Should().Be("Title");
        document.Elements[1].As<MarkdownCitation>()
            .Citation.As<MarkdownText>().Text.Should().Be("Citation");

        var list = document.Elements[2].As<MarkdownList>();

        list.Should().NotBeNull();
        
        list.Elements.Should().HaveCount(3);

        list.Elements[0].As<MarkdownListItem>().Item
            .As<MarkdownText>().Text.Should().Be("Item 1");
        list.Elements[1].As<MarkdownListItem>().Item
            .As<MarkdownText>().Text.Should().Be("Item 2");
        list.Elements[2].As<MarkdownListItem>().Item
            .As<MarkdownText>().Text.Should().Be("Item 3");

        document.Elements[3].Should().BeOfType<MarkdownSeparator>();

        var subtitle = document.Elements[4].As<MarkdownTitle>();
        subtitle.Title.Should().Be("Sub Title");
        subtitle.Level.Should().Be(3);

        document.Elements[5].As<MarkdownText>()
            .Text.Should().Be("Text");
    }

    [Fact]
    public void CanParseSubPatterns()
    {
        // Prepare
        var md = """
            # Title
            > # Sub Title in Citation
            * > Item As Citation
            * > # Item as Title in Citation
            """;

        var parser = new MarkdownParser();

        // Act
        var result = parser.Parse(md);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var document = result.Expect("No error");
        document.Should().NotBeNull();

        document.Elements.Should().HaveCount(3);

        var title = document.Elements[0].As<MarkdownTitle>();
        title.Should().NotBeNull();
        title.Title.Should().Be("Title");
        title.Level.Should().Be(1);

        var citation = document.Elements[1].As<MarkdownCitation>();
        citation.Should().NotBeNull();
        var citTitle = citation.Citation.As<MarkdownTitle>();
        citTitle.Should().NotBeNull();
        citTitle.Title.Should().Be("Sub Title in Citation");
        citTitle.Level.Should().Be(1);

        var list = document.Elements[2].As<MarkdownList>();
        list.Should().NotBeNull();
        list.Elements.Should().HaveCount(2);

        var listItem1 = list.Elements[0].As<MarkdownListItem>();
        listItem1.Should().NotBeNull();
        var listItem1Citation = listItem1.Item.As<MarkdownCitation>();
        listItem1Citation.Should().NotBeNull();
        listItem1Citation.Citation.As<MarkdownText>().Text.Should().Be("Item As Citation");

        var listItem2 = list.Elements[1].As<MarkdownListItem>();
        listItem2.Should().NotBeNull();
        var listItem2Citation = listItem2.Item.As<MarkdownCitation>();
        listItem2Citation.Should().NotBeNull();
        var listItem2CitationTitle = listItem2Citation.Citation.As<MarkdownTitle>();
        listItem2CitationTitle.Should().NotBeNull();
        listItem2CitationTitle.Title.Should().Be("Item as Title in Citation");
        listItem2CitationTitle.Level.Should().Be(1);
    }
}
