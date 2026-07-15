// The hero's line-numbered "actual output" panel from the design comp. A static, decorative
// facsimile of a generated .md file (aria-hidden in the Hero), so it's expressed as literal lines
// rather than run through the markdown highlighter.
interface Line {
  ln: number;
  code?: React.ReactNode;
  className?: string;
}

const LINES: Line[] = [
  { ln: 1, code: '---', className: 'dim' },
  { ln: 2, code: <><span className="k">title:</span> &quot;Q3 Platform Demo — Billing Module&quot;</> },
  { ln: 3, code: <><span className="k">duration:</span> &quot;00:47:12&quot;</> },
  { ln: 4, code: <><span className="k">language:</span> &quot;en&quot;</> },
  { ln: 5, code: <><span className="k">tags:</span> [demo, billing]</> },
  { ln: 6, code: '---', className: 'dim' },
  { ln: 7, code: '\u00A0' },
  { ln: 8, code: '# Q3 Platform Demo — Billing Module', className: 'bold' },
  { ln: 9, code: '\u00A0' },
  { ln: 10, code: <>## <span className="ts">[00:03:40]</span> Invoice workflow</>, className: 'bold' },
  { ln: 11, code: <><span className="tag-spoken">Spoken:</span> We open the invoice editor and</> },
  { ln: 12, code: 'apply proration before sending…' },
  { ln: 13, code: <><span className="tag-screen">On screen:</span></> },
  { ln: 14, code: '> Invoices → Create → "Apply proration"', className: 'soft' },
  { ln: 15, code: '> code: InvoiceService.CreateAsync(...)', className: 'soft' },
];

export function ArtifactCard() {
  return (
    <div className="artifact" aria-hidden="true">
      <div className="artifact-head">
        <span className="name">demo-billing.md</span>
        <span className="meta">4.2 KB · utf-8 · 47:12</span>
      </div>
      <div className="artifact-body">
        {LINES.map((line) => (
          <div className="artifact-line" key={line.ln}>
            <span className="ln">{line.ln}</span>
            <span className={line.className ? `code ${line.className}` : 'code'}>{line.code}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
