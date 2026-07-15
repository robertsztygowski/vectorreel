import { highlightMarkdownLines } from './highlightMarkdown';

interface CodeCardProps {
  title: string;
  content: string;
  rotate?: boolean;
}

export function CodeCard({ title, content, rotate }: CodeCardProps) {
  const lines = highlightMarkdownLines(content);
  return (
    <div className={rotate ? 'code-card rotate' : 'code-card'}>
      <div className="code-head">
        <span className="dot" />
        <span className="dot" />
        <span className="dot" />
        <span className="code-title">{title}</span>
      </div>
      <pre className="code-body">
        {lines.map((line) => (
          <span key={line.key} className={line.className}>
            {line.text}
            {'\n'}
          </span>
        ))}
      </pre>
    </div>
  );
}
