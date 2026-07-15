export interface HighlightedLine {
  key: number;
  className?: string;
  text: string;
}

// Classifies each line of a raw markdown output file for the dark code-card display — a rough
// syntax highlighter, not a full markdown parser (that's lib/corpus.ts's job for the Rendered tab).
export function highlightMarkdownLines(text: string): HighlightedLine[] {
  const lines = text.split('\n');
  let inFrontmatter = false;
  let frontmatterClosed = false;

  return lines.map((line, i) => {
    if (!frontmatterClosed && line.trim() === '---') {
      inFrontmatter = !inFrontmatter;
      if (!inFrontmatter) frontmatterClosed = true;
      return { key: i, className: 'c-fm', text: line };
    }
    if (inFrontmatter) return { key: i, className: 'c-fm', text: line };
    if (/^##\s+/.test(line)) return { key: i, className: 'c-h2', text: line };
    if (/^#\s+/.test(line)) return { key: i, className: 'c-h', text: line };
    if (/^\*\*[^*]+:\*\*/.test(line)) return { key: i, className: 'c-b', text: line };
    if (/^>\s?/.test(line)) return { key: i, className: 'c-q', text: line };
    return { key: i, text: line };
  });
}
