import { highlightMarkdownLines } from './highlightMarkdown';

interface CodeCardProps {
  title: string;
  content: string;
  lang?: string;
}

export function CodeCard({ title, content, lang }: CodeCardProps) {
  const lines = highlightMarkdownLines(content);
  return (
    <div className="code-card">
      <div className="code-head">
        <span className="code-title">{title}</span>
        {lang && <span className="code-lang">{lang}</span>}
      </div>
      <pre className="code-body">
        {lines.map((line) => (
          <span key={line.key} className={line.className}>
            {line.text}
          </span>
        ))}
      </pre>
    </div>
  );
}
