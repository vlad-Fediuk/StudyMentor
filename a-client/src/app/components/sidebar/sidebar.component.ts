import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  OnInit,
  Output,
  inject
} from '@angular/core';
import { FormsModule } from '@angular/forms';
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
  imports: [CommonModule, FormsModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {
  @Output() collapsedChange = new EventEmitter<boolean>();
  @Output() practiceOpen = new EventEmitter<void>();

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
  searchTerm = '';
  isCollapsed = false;
  isLoading = true;
  error = '';
  isDarkTheme = false;

  async ngOnInit(): Promise<void> {
    this.initTheme();
    this.route.queryParamMap.subscribe((params) => {
      const lectureId = params.get('lectureId');
      this.selectedLectureId = lectureId;
      this.syncSelectionFromUrl();
    });

    await this.loadSubjectsAndLectures();
  }

  initTheme(): void {
    if (typeof window !== 'undefined') {
      const savedTheme = localStorage.getItem('theme');
      this.isDarkTheme = savedTheme === 'dark' ||
        (!savedTheme && window.matchMedia('(prefers-color-scheme: dark)').matches);
      this.applyTheme();
    }
  }

  toggleTheme(): void {
    this.isDarkTheme = !this.isDarkTheme;
    if (typeof window !== 'undefined') {
      localStorage.setItem('theme', this.isDarkTheme ? 'dark' : 'light');
    }
    this.applyTheme();
  }

  private applyTheme(): void {
    if (typeof window !== 'undefined') {
      if (this.isDarkTheme) {
        document.documentElement.setAttribute('data-theme', 'dark');
      } else {
        document.documentElement.removeAttribute('data-theme');
      }
    }
  }

  get cardTitle(): string {
    return this.activeTab === 'subjects' ? 'Предмет' : 'Лекція';
  }

  get searchPlaceholder(): string {
    return this.activeTab === 'subjects' ? 'Предмет' : 'Лекція';
  }

  get filteredSubjects(): SubjectResponse[] {
    return this.filterBySearch(this.subjects);
  }

  get lecturesForSelectedSubject(): LectureResponse[] {
    if (!this.selectedSubjectId) {
      return [];
    }
    return this.filterBySearch(this.subjectLectureMap[this.selectedSubjectId] ?? []);
  }

  private filterBySearch<T extends IdNameResponse>(items: T[]): T[] {
    const term = this.searchTerm.trim().toLocaleLowerCase('uk-UA');
    if (!term) {
      return items;
    }

    return items.filter((item) => item.name.toLocaleLowerCase('uk-UA').includes(term));
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
    this.searchTerm = '';
    this.activeTab = tab;
  }

  selectSubject(subjectId: string): void {
    this.selectedSubjectId = subjectId;
    this.searchTerm = '';
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

  openPractice(): void {
    this.practiceOpen.emit();
  }
}
