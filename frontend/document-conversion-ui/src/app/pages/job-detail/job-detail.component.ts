import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { JobApiService, JobDetail } from '../../services/job-api.service';
import { StatusBadgeComponent } from '../../components/status-badge/status-badge.component';
import { formatBytes, shortJobId } from '../../job-ui.helpers';

const PIPELINE = ['Queued', 'Converting', 'Splitting', 'Validating'] as const;

@Component({
  selector: 'app-job-detail',
  standalone: true,
  imports: [RouterLink, DatePipe, StatusBadgeComponent],
  templateUrl: './job-detail.component.html',
  styleUrl: './job-detail.component.css'
})
export class JobDetailComponent implements OnInit, OnDestroy {
  private readonly api = inject(JobApiService);
  private readonly route = inject(ActivatedRoute);

  job: JobDetail | null = null;
  notFound = false;
  loadError = '';
  readonly pipeline = PIPELINE;
  readonly formatBytes = formatBytes;
  readonly shortJobId = shortJobId;

  private jobId = '';
  private timer?: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    this.jobId = this.route.snapshot.paramMap.get('id') ?? '';
    this.refresh();
    this.timer = setInterval(() => this.refresh(), 2500);
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  get isLive(): boolean {
    return this.job?.status === 'Submitted' || this.job?.status === 'Processing';
  }

  get pipelineIndex(): number {
    const events = new Set(this.job?.events.map((e) => e.eventType) ?? []);
    if (events.has('ValidationPassed') || events.has('ValidationFailed') || events.has('NeedsReview')) return 3;
    if (events.has('SplitIntoParts') || events.has('ConversionSucceeded')) return 2;
    if (events.has('Started') || this.job?.status === 'Processing') return 1;
    return 0;
  }

  artifactUrl(partIndex: number): string {
    return this.api.artifactUrl(this.jobId, partIndex);
  }

  private refresh(): void {
    if (!this.jobId) return;
    this.api.getById(this.jobId).subscribe({
      next: (job) => {
        this.job = job;
        this.notFound = false;
        this.loadError = '';
        if (!this.isLive) this.stopPolling();
      },
      error: (err) => {
        if (err.status === 404) {
          this.notFound = true;
          this.job = null;
          this.stopPolling();
          return;
        }
        if (!this.job) {
          this.loadError = 'Could not load this job. Check that the API is running.';
        }
      }
    });
  }

  private stopPolling(): void {
    if (this.timer) {
      clearInterval(this.timer);
      this.timer = undefined;
    }
  }
}
