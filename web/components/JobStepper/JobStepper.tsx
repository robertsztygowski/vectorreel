import styles from './JobStepper.module.css';

export type JobStageKey = 'A' | 'B' | 'C' | 'D';
export type JobStatusState = 'queued' | 'processing' | 'done' | 'failed';

const STAGE_LABELS: Record<JobStageKey, string> = {
  A: 'Probe & prepare',
  B: 'Segment analysis',
  C: 'Fusion',
  D: 'Persist & notify',
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
    if (status === 'failed') {
      return index <= activeIndex ? 'failed' : 'pending';
    }
    if (status === 'queued') return 'pending';
    if (index < activeIndex) return 'done';
    if (index === activeIndex) return 'active';
    return 'pending';
  }

  return (
    <div className={styles.stepper}>
      <div className={styles.row}>
        <span className={`${styles.dot} ${status === 'queued' ? styles.active : styles.done}`}>
          <span className={styles.marker}>{status === 'queued' ? '…' : '✓'}</span>
          Queued
        </span>
        {stages.map((stage, i) => {
          const state = stateFor(i);
          return (
            <span key={stage} className={`${styles.dot} ${styles[state]}`}>
              <span className={styles.marker}>{state === 'done' ? '✓' : state === 'failed' ? '!' : i + 1}</span>
              {STAGE_LABELS[stage]}
            </span>
          );
        })}
      </div>
      <div className={styles.bar}>
        <div className={status === 'failed' ? styles.fillFailed : styles.fill} style={{ width: `${progress}%` }} />
      </div>
    </div>
  );
}
