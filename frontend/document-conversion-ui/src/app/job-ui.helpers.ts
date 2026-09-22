export function statusBadgeClass(status: string): string {
  switch (status) {
    case 'Completed':
      return 'status-completed';
    case 'Failed':
      return 'status-failed';
    case 'NeedsReview':
      return 'status-needs-review';
    case 'Processing':
    case 'Submitted':
      return 'status-in-progress';
    default:
      return 'status-unknown';
  }
}

export function formatHttpError(error: unknown): string {
  const err = error as { error?: { title?: string; errors?: Record<string, string[]> } };
  const fieldErrors = err.error?.errors;
  if (fieldErrors) {
    return Object.values(fieldErrors).flat().join(' ');
  }
  return err.error?.title ?? 'Request failed';
}

export function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  const kb = bytes / 1024;
  if (kb < 1024) return `${kb < 10 ? kb.toFixed(1) : Math.round(kb)} KB`;
  const mb = kb / 1024;
  return `${mb < 10 ? mb.toFixed(1) : Math.round(mb)} MB`;
}

export function shortJobId(id: string): string {
  return id.replace(/-/g, '').slice(0, 8);
}

export const MAX_UPLOAD_BYTES = 52_428_800;
