import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface JobSummary {
  id: string;
  status: string;
  outputFormat: string;
  sourceFileName: string;
  createdAt: string;
  completedAt?: string;
  errorCode?: string;
  errorMessage?: string;
}

export interface JobEvent {
  eventType: string;
  message: string;
  occurredAt: string;
}

export interface OutputArtifact {
  partNumber: number;
  totalParts: number;
  displayLabel: string;
  fileName: string;
  sizeBytes: number;
}

export interface JobDetail extends JobSummary {
  events: JobEvent[];
  artifacts: OutputArtifact[];
}

export interface PagedJobs {
  items: JobSummary[];
  totalCount: number;
}

@Injectable({ providedIn: 'root' })
export class JobApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  submit(file: File, outputFormat: string): Observable<{ jobId: string }> {
    const form = new FormData();
    form.append('file', file);
    form.append('outputFormat', outputFormat);
    return this.http.post<{ jobId: string }>(`${this.base}/jobs`, form);
  }

  getById(id: string): Observable<JobDetail> {
    return this.http.get<JobDetail>(`${this.base}/jobs/${id}`);
  }

  list(page = 1, pageSize = 20): Observable<PagedJobs> {
    return this.http.get<PagedJobs>(`${this.base}/jobs`, { params: { page, pageSize } });
  }

  artifactUrl(jobId: string, partIndex: number): string {
    return `${this.base}/jobs/${jobId}/artifacts/${partIndex}`;
  }
}
