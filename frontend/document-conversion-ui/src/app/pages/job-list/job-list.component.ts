import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { JobApiService, JobSummary } from '../../services/job-api.service';
import { StatusBadgeComponent } from '../../components/status-badge/status-badge.component';
import { formatHttpError } from '../../job-ui.helpers';

@Component({
  selector: 'app-job-list',
  standalone: true,
  imports: [RouterLink, DatePipe, StatusBadgeComponent],
  templateUrl: './job-list.component.html',
  styleUrl: './job-list.component.css'
})
export class JobListComponent implements OnInit {
  private readonly api = inject(JobApiService);

  jobs: JobSummary[] = [];
  page = 1;
  pageSize = 10;
  totalCount = 0;
  loading = true;
  refreshing = false;
  error = '';

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  ngOnInit(): void {
    this.load();
  }

  load(page = this.page): void {
    this.error = '';
    this.refreshing = !this.loading;
    this.api.list(page, this.pageSize).subscribe({
      next: (res) => {
        this.jobs = res.items;
        this.totalCount = res.totalCount;
        this.page = page;
        this.loading = false;
        this.refreshing = false;
      },
      error: (err) => {
        this.error = formatHttpError(err);
        this.loading = false;
        this.refreshing = false;
      }
    });
  }

  prev(): void {
    if (this.page > 1) this.load(this.page - 1);
  }

  next(): void {
    if (this.page < this.totalPages) this.load(this.page + 1);
  }
}
