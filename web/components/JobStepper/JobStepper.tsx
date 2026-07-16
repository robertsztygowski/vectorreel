export type JobStageKey = 'A' | 'B' | 'C' | 'D';
export type JobStatusState = 'queued' | 'processing' | 'done' | 'failed';

const STAGE_LABELS: Record<JobStageKey, string> = {
  A: 'ingest & chunk',
  B: 'transcribe audio',
  C: 'read the screen',
  D: 'compose document',
};

const STATUS_LABELS: Record<JobStatusState, string> = {
  queued: 'queued',
  processing: 'processing',
  done: 'done',
  failed: 'failed',
};

interface JobStepperProps {
  stages: JobStageKey[];
  status: JobStatusState;
  activeStage?: JobStageKey;
  progress: number;
}

export function JobStepper({ stages, status, activeStage, progress }: JobStepperProps) {
  const activeIndex = activeStage ? stages.indexOf(activeStage) : -1;

  function stateFor(index: number): 'pending' | 'active' | 'done' | 'failed' {
    if (status === 'done') return 'done';
    if (status === 'failed') return index <= activeIndex ? 'failed' : 'pending';
    if (status === 'queued') return 'pending';
    if (index < activeIndex) return 'done';
    if (index === activeIndex) return 'active';
    return 'pending';
  }

  return (
    <div className="stages">
      {stages.map((stage, index) => {
        const state = stateFor(index);
        return (
          <div key={stage} className="stage">
            <div className="stage-head">
              <span className="name">
                {stage}
                <span style={{ color: 'var(--accent)' }}> / </span>
                {STAGE_LABELS[stage]}
              </span>
              <span className={`is-${state}`}>
                {state === 'done' ? STATUS_LABELS.done : state === 'active' ? STATUS_LABELS.processing : state === 'failed' ? STATUS_LABELS.failed : 'pending'}
              </span>
            </div>
            <div className="stage-bar">
              <i
                className={`is-${state}`}
                style={{
                  width: state === 'done' ? '100%' : state === 'active' ? `${progress}%` : '0%',
                  backgroundColor: state === 'failed' ? 'var(--err)' : state === 'done' ? 'var(--ok)' : 'var(--accent)',
                }}
              />
            </div>
          </div>
        );
      })}
    </div>
  );
}
