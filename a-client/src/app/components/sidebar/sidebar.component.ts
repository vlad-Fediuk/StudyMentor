import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
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

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {
  @Output() collapsedChange = new EventEmitter<boolean>();

  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly apiUrl = 'http://localhost:5132';

  subjects: SubjectResponse[] = [];
  lectures: LectureResponse[] = [];
  subjectLectureMap: Record<string, LectureResponse[]> = {};
  selectedSubjectId: string | null = null;
  selectedLectureId: string | null = null;
  activeTab: 'subjects' | 'lectures' = 'subjects';
  isCollapsed = false;
  isLoading = true;
  error = '';

  async ngOnInit(): Promise<void> {
    this.route.queryParamMap.subscribe((params) => {
      const lectureId = params.get('lectureId');
      this.selectedLectureId = lectureId;
      this.syncSelectionFromUrl();
    });

    await this.loadSubjectsAndLectures();
  }

  get cardTitle(): string {
    return this.activeTab === 'subjects' ? 'Subject' : 'Lecture';
  }

  get lecturesForSelectedSubject(): LectureResponse[] {
    if (!this.selectedSubjectId) {
      return [];
    }
    return this.subjectLectureMap[this.selectedSubjectId] ?? [];
  }

  async loadSubjectsAndLectures(): Promise<void> {
    this.isLoading = true;
    this.error = '';

    try {
      const subjects = await firstValueFrom(
        this.http.get<SubjectResponse[]>(`${this.apiUrl}/subjects/`)
      );
      const lectures = await firstValueFrom(
        this.http.get<LectureResponse[]>(`${this.apiUrl}/lectures/`)
      );

      this.subjects = subjects ?? [];
      this.lectures = lectures ?? [];
      this.buildLectureMap();

      if (!this.selectedSubjectId && this.subjects.length > 0) {
        this.selectedSubjectId = this.subjects[0].id;
      }
    } catch {
      this.error = 'Не вдалося завантажити предмети та лекції';
    } finally {
      this.isLoading = false;
    }
  }

  private buildLectureMap(): void {
    this.subjectLectureMap = this.subjects.reduce((map, subject) => {
      map[subject.id] = [];
      return map;
    }, {} as Record<string, LectureResponse[]>);

    for (const lecture of this.lectures) {
      if (lecture.subject?.id) {
        this.subjectLectureMap[lecture.subject.id] = [
          ...(this.subjectLectureMap[lecture.subject.id] ?? []),
          lecture
        ];
      }
    }
  }

  private syncSelectionFromUrl(): void {
    if (!this.selectedLectureId || !this.lectures.length) {
      return;
    }

    const lecture = this.lectures.find((item) => item.id === this.selectedLectureId);
    if (lecture) {
      this.selectedSubjectId = lecture.subject?.id ?? this.selectedSubjectId;
    }
  }

  selectTab(tab: 'subjects' | 'lectures'): void {
    if (tab === 'lectures' && !this.selectedSubjectId && this.subjects.length > 0) {
      this.selectedSubjectId = this.subjects[0].id;
    }
    this.activeTab = tab;
  }

  selectSubject(subjectId: string): void {
    this.selectedSubjectId = subjectId;
    this.activeTab = 'lectures';
  }

  selectLecture(lecture: LectureResponse): void {
    this.selectedLectureId = lecture.id;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { lectureId: lecture.id },
      queryParamsHandling: 'merge'
    });
  }

  toggleCollapse(): void {
    this.isCollapsed = !this.isCollapsed;
    this.collapsedChange.emit(this.isCollapsed);
  }
}
