import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { JobApiService } from '../../services/job-api.service';
import { formatBytes, formatHttpError, MAX_UPLOAD_BYTES } from '../../job-ui.helpers';

@Component({
  selector: 'app-submit-job',
  standalone: true,
  templateUrl: './submit-job.component.html',
  styleUrl: './submit-job.component.css'
})
export class SubmitJobComponent {
  private readonly api = inject(JobApiService);
  private readonly router = inject(Router);

  file: File | null = null;
  outputFormat = 'Docx';
  submitting = false;
  error = '';
  dragOver = false;

  readonly formatBytes = formatBytes;

  onFileInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.takeFile(input.files?.[0] ?? null);
    input.value = '';
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    const next = event.relatedTarget as Node | null;
    if (next && (event.currentTarget as Node).contains(next)) return;
    this.dragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = false;
    this.takeFile(event.dataTransfer?.files?.[0] ?? null);
  }

  clearFile(): void {
    this.file = null;
    this.error = '';
  }

  submit(): void {
    if (!this.file) return;
    this.submitting = true;
    this.error = '';
    this.api.submit(this.file, this.outputFormat).subscribe({
      next: (res) => this.router.navigate(['/jobs', res.jobId]),
      error: (err) => {
        this.error = formatHttpError(err);
        this.submitting = false;
      },
      complete: () => (this.submitting = false)
    });
  }

  private takeFile(file: File | null): void {
    this.error = '';
    if (!file) {
      this.file = null;
      return;
    }
    const isPdf = file.type === 'application/pdf' || file.name.toLowerCase().endsWith('.pdf');
    if (!isPdf) {
      this.file = null;
      this.error = 'Only PDF files can be converted.';
      return;
    }
    if (file.size > MAX_UPLOAD_BYTES) {
      this.file = null;
      this.error = `File is too large. Maximum upload size is ${formatBytes(MAX_UPLOAD_BYTES)}.`;
      return;
    }
    this.file = file;
  }
}
