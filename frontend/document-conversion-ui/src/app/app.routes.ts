import { Routes } from '@angular/router';
import { SubmitJobComponent } from './pages/submit-job/submit-job.component';
import { JobListComponent } from './pages/job-list/job-list.component';
import { JobDetailComponent } from './pages/job-detail/job-detail.component';

export const routes: Routes = [
  { path: '', component: SubmitJobComponent },
  { path: 'jobs', component: JobListComponent },
  { path: 'jobs/:id', component: JobDetailComponent }
];
