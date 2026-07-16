import type { CSSProperties, ReactNode } from 'react';

interface FieldProps {
  label: string;
  htmlFor?: string;
  hint?: string;
  children: ReactNode;
  className?: string;
  style?: CSSProperties;
}

export function Field({ label, htmlFor, hint, children, className, style }: FieldProps) {
  return (
    <div className={className ? `field ${className}` : 'field'} style={style}>
      <label htmlFor={htmlFor}>{label}</label>
      {children}
      {hint && <span className="hint">{hint}</span>}
    </div>
  );
}
