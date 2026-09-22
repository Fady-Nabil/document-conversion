import { Component, Input } from '@angular/core';
import { statusBadgeClass } from '../../job-ui.helpers';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `<span [class]="'status-badge ' + statusBadgeClass(status)">{{ label }}</span>`,
  styles: [`
    :host { display: inline-flex; }
    .status-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      padding: 0.2rem 0.65rem;
      border-radius: 999px;
      font-size: 0.78rem;
      font-weight: 600;
      letter-spacing: 0.01em;
    }
    .status-completed { background: var(--ok-soft); color: var(--ok); }
    .status-failed { background: var(--bad-soft); color: var(--bad); }
    .status-needs-review { background: var(--warn-soft); color: var(--warn); }
    .status-in-progress { background: var(--accent-soft); color: var(--accent); }
    .status-unknown { background: var(--paper-sunken); color: var(--ink-soft); }
  `]
})
export class StatusBadgeComponent {
  @Input({ required: true }) status = '';

  get label(): string {
    return this.status === 'NeedsReview' ? 'Needs review' : this.status;
  }

  statusBadgeClass = statusBadgeClass;
}
