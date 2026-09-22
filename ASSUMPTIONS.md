# Assumptions & notes

## Scanned PDF rule

Jobs fail with `ScannedDocumentNotSupported` when the PDF has **zero extractable characters** across all pages. Mixed PDFs (some text pages, some image-only pages) **convert**: text is copied and image-only pages contribute images only.

**Demo:** README interview demo **step 3** — submit `samples/mixed-text-and-scan-pages.pdf`.

## Failed vs NeedsReview

| Status | When |
|--------|------|
| **Failed** | Corrupt PDF, scanned-only document, unsupported output format, empty document, unsplittable single page/chunk over size limit, unexpected errors |
| **NeedsReview** | Post-conversion **output validation** failed (part count, page coverage, fingerprint, size checks) — not marked successful per assessment §4.4 |

**Demo:** README interview demo **step 6** — walk unit tests and `ConversionJob.MarkNeedsReview` (live samples usually stay **Completed** or **Failed**).

## Unsupported input

Non-PDF uploads are rejected at submit time via FluentValidation → **400** (no job row). Corrupt PDFs fail during processing with **Failed** and job history.

## Splitting

Page-level packing using pre-serialized per-page DOCX sizes; merged multi-page parts are rebuilt via OpenXml. Part files: `{jobId}_part_{i}_of_{n}.docx` with API/UI **Part i of n** metadata.

## Validation

Fingerprint = ordered `pageIndex:textLength:imageBytesSum` before split; must match after split. Page indices across parts must cover all source pages exactly once.

## Hangfire

SQLite storage (`hangfire.db`), `[AutomaticRetry(Attempts = 0)]` on processing job; terminal jobs skip re-processing if enqueued again.

## Building Blocks

Shared kernel under `backend/src/Shared/` (`BusinessException`, `Guard`, `Error`). MediatR pipeline behaviors and API error handling under `backend/src/BuildingBlocks/`. Document-conversion-specific `DomainErrorCode` and typed exceptions remain in `DocumentConversion.Domain`.

## With more time

- EF migrations instead of `EnsureCreated`  
- SignalR for job updates instead of polling  
- Blob storage (Azure/S3)  
- Dedicated worker host + outbox pattern  
- Stronger DOCX image type handling (JPEG vs PNG)  
- Full merge fidelity for complex PDF layouts
