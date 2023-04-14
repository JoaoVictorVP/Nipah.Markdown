# Nipah.Markdown
It is a library used for "parsing" markdown, it uses NipahTokenizer under the hood and makes a basic and simple parse of common markdown syntax.

## Usage
```cs
var parser = new MarkdownParser();
var result = parser.Parse("""
	# Title
	### Subtitle
	* Item 1
	* Item 2
	* Item 3
	---
	Text
	""");
var document = result.Expect("Expect a valid result");
// You can use the document to handle the markdown, it has 'Elements' and every of them can be either:
// - A MarkdownTitle
// - A MarkdownText
// - A MarkdownSeparator
// - A MarkdownList -> which by itself has a 'Elements' of either:
//   - A MarkdownTitle
//   - A MarkdownText
//   - A MarkdownSeparator
//   - A MarkdownList -> ... and so on
```

## WIP
We still have some work to do, but we are getting there, we are working on the following:
- [ ] Add support for code blocks
- [ ] Add support for images
- [ ] Add support for links
- [ ] Add support for blockquotes
- [ ] Add support for bold
- [ ] Add support for italic
- [ ] Add support for underline
