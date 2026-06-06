import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

interface IdNameResponse {
  id: string;
  name: string;
}

interface SubjectResponse extends IdNameResponse {
  major: IdNameResponse;
}

interface LectureResponse extends IdNameResponse {
  subject: SubjectResponse;
}

interface LectureChunkResponse {
  id: string;
  content: string;
  order: number;
  lectureId: string;
}

@Component({
  selector: 'app-admin-upload-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-upload-page.component.html',
  styleUrl: './admin-upload-page.component.scss'
})
export class AdminUploadPageComponent {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5132';

  subjects: SubjectResponse[] = [];
  lectures: LectureResponse[] = [];
  chunks: LectureChunkResponse[] = [];

  selectedSubjectId = '';
  selectedLectureId = '';
  selectedFile: File | null = null;

  isLoadingSubjects = false;
  isLoadingLectures = false;
  isLoadingChunks = false;
  isUploading = false;
  isDeleting = false;

  errorMessage = '';
  successMessage = '';

  async ngOnInit(): Promise<void> {
    await this.loadSubjects();
    await this.loadLectures();
  }

  async loadSubjects(): Promise<void> {
    this.isLoadingSubjects = true;
    this.errorMessage = '';
    try {
      this.subjects = await firstValueFrom(
        this.http.get<SubjectResponse[]>(`${this.apiUrl}/subjects/`)
      );
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error);
    } finally {
      this.isLoadingSubjects = false;
    }
  }

  async loadLectures(): Promise<void> {
    this.isLoadingLectures = true;
    this.errorMessage = '';
    try {
      this.lectures = await firstValueFrom(
        this.http.get<LectureResponse[]>(`${this.apiUrl}/lectures/`)
      );
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error);
    } finally {
      this.isLoadingLectures = false;
    }
  }

  async loadChunks(): Promise<void> {
    if (!this.selectedLectureId) {
      this.chunks = [];
      return;
    }

    this.isLoadingChunks = true;
    this.errorMessage = '';
    try {
      this.chunks = await firstValueFrom(
        this.http.get<LectureChunkResponse[]>(
          `${this.apiUrl}/lecture-chunks/lecture/${this.selectedLectureId}`
        )
      );
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error);
      this.chunks = [];
    } finally {
      this.isLoadingChunks = false;
    }
  }

  get filteredLectures(): LectureResponse[] {
    if (!this.selectedSubjectId) {
      return this.lectures;
    }
    return this.lectures.filter(l => l.subject.id === this.selectedSubjectId);
  }

  onSubjectChange(): void {
    this.selectedLectureId = '';
    this.chunks = [];
  }

  onLectureChange(): void {
    this.chunks = [];
    this.loadChunks();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      const extension = file.name.split('.').pop()?.toLowerCase();
      if (extension !== 'txt' && extension !== 'md') {
        this.errorMessage = 'File must be .txt or .md';
        this.selectedFile = null;
        input.value = '';
        return;
      }
      this.selectedFile = file;
      this.errorMessage = '';
    }
  }

  async uploadFile(): Promise<void> {
    if (!this.selectedSubjectId || !this.selectedLectureId || !this.selectedFile) {
      this.errorMessage = 'Please select subject, lecture, and file';
      return;
    }

    this.isUploading = true;
    this.errorMessage = '';
    this.successMessage = '';

    try {
      const formData = new FormData();
      formData.append('SubjectId', this.selectedSubjectId);
      formData.append('LectureId', this.selectedLectureId);
      formData.append('File', this.selectedFile);

      const uploadedChunks = await firstValueFrom(
        this.http.post<LectureChunkResponse[]>(
          `${this.apiUrl}/lecture-chunks/upload`,
          formData
        )
      );

      this.chunks = uploadedChunks;
      this.successMessage = `Successfully uploaded and created ${uploadedChunks.length} chunks`;
      this.selectedFile = null;
      const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
      if (fileInput) {
        fileInput.value = '';
      }
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error);
    } finally {
      this.isUploading = false;
    }
  }

  async deleteChunks(): Promise<void> {
    if (!this.selectedLectureId) {
      this.errorMessage = 'Please select a lecture';
      return;
    }

    this.isDeleting = true;
    this.errorMessage = '';
    this.successMessage = '';

    try {
      await firstValueFrom(
        this.http.delete(`${this.apiUrl}/lecture-chunks/lecture/${this.selectedLectureId}`)
      );
      this.chunks = [];
      this.successMessage = 'Successfully deleted all chunks for this lecture';
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error);
    } finally {
      this.isDeleting = false;
    }
  }

  get canUpload(): boolean {
    return !!(
      this.selectedSubjectId &&
      this.selectedLectureId &&
      this.selectedFile &&
      !this.isUploading
    );
  }

  get canDelete(): boolean {
    return !!(this.selectedLectureId && this.chunks.length > 0 && !this.isDeleting);
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const apiMessage =
        typeof error.error?.message === 'string'
          ? error.error.message
          : typeof error.error?.error === 'string'
            ? error.error.error
            : '';

      if (apiMessage) {
        return apiMessage;
      }

      if (error.status === 0) {
        return 'API is unavailable. Check that the backend is running on localhost:5132.';
      }
    }

    if (error instanceof Error && error.message) {
      return error.message;
    }

    return 'An error occurred';
  }
}
