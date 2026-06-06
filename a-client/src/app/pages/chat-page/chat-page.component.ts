import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

type ChatRole = 'user' | 'assistant';

interface ChatMessage {
  id?: string;
  role: ChatRole;
  content: string;
  status?: string;
}

interface IdNameResponse {
  id: string;
  name: string;
}

interface UserResponse extends IdNameResponse {
  groupId: string;
}

interface SubjectResponse extends IdNameResponse {
  major: IdNameResponse;
}

interface LectureResponse extends IdNameResponse {
  subject: SubjectResponse;
}

interface ChatSessionResponse {
  id: string;
  userId: string;
  lectureId: string;
}

interface AiChatSendMessageResponse {
  status: string;
  userMessage: ChatMessage;
  assistantMessage: ChatMessage;
}

interface ChatMessageResponse {
  id: string;
}

interface CardResponse {
  id: string;
  term: string;
  definition: string;
}

interface FlashcardResponse {
  id: string;
  name: string;
  chatMessageId: string;
  cards: CardResponse[];
}

@Component({
  selector: 'app-chat-page',
  standalone: true,
  imports: [],
  templateUrl: './chat-page.component.html',
  styleUrl: './chat-page.component.scss'
})
export class ChatPageComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);
  private readonly apiUrl = 'http://localhost:5132';
  private readonly chatIdStorageKey = 'studyMentor.chatId';
  private readonly guidPattern =
    /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

  messages: ChatMessage[] = [];
  isSending = false;
  errorMessage = '';
  decks: FlashcardResponse[] = [];
  selectedDeck: FlashcardResponse | null = null;
  currentCard: CardResponse | null = null;
  isBackVisible = false;
  isFlashcardsOpen = false;
  isLoadingFlashcards = false;
  isCreatingSampleDeck = false;
  isCardStudyOpen = false;
  flashcardsErrorMessage = '';
  selectedExerciseType = 'flashcards';

  async ngOnInit(): Promise<void> {
    await Promise.all([this.loadMessages(), this.loadFlashcards(false)]);
  }

  get currentCardIndex(): number {
    if (!this.selectedDeck || !this.currentCard) {
      return -1;
    }

    return this.selectedDeck.cards.findIndex((card) => card.id === this.currentCard?.id);
  }

  get totalCount(): number {
    return this.selectedDeck?.cards.length ?? 0;
  }

  async openFlashcards(): Promise<void> {
    this.isFlashcardsOpen = !this.isFlashcardsOpen;

    if (this.isFlashcardsOpen && this.decks.length === 0) {
      await this.loadFlashcards();
    }
  }

  closeFlashcards(): void {
    this.isFlashcardsOpen = false;
    this.isCardStudyOpen = false;
  }

  selectDeck(deck: FlashcardResponse): void {
    this.selectedDeck = deck;
    this.currentCard = deck.cards[0] ?? null;
    this.isBackVisible = false;
    this.isCardStudyOpen = false;
    this.flashcardsErrorMessage = '';
  }

  selectDeckById(event: Event): void {
    const deckId = (event.target as HTMLSelectElement).value;
    const deck = this.decks.find((item) => item.id === deckId);
    if (deck) {
      this.selectDeck(deck);
    }
  }

  selectExerciseType(event: Event): void {
    this.selectedExerciseType = (event.target as HTMLSelectElement).value;
  }

  selectCard(card: CardResponse): void {
    this.currentCard = card;
    this.isBackVisible = false;
    this.isCardStudyOpen = true;
  }

  backToExerciseList(): void {
    this.isCardStudyOpen = false;
    this.isBackVisible = false;
  }

  flipCard(): void {
    this.isBackVisible = !this.isBackVisible;
  }

  showPreviousCard(): void {
    if (!this.selectedDeck || this.selectedDeck.cards.length === 0) {
      return;
    }

    const index = this.currentCardIndex <= 0
      ? this.selectedDeck.cards.length - 1
      : this.currentCardIndex - 1;
    this.currentCard = this.selectedDeck.cards[index];
    this.isBackVisible = false;
  }

  showNextCard(): void {
    if (!this.selectedDeck || this.selectedDeck.cards.length === 0) {
      return;
    }

    const index = this.currentCardIndex < 0 || this.currentCardIndex === this.selectedDeck.cards.length - 1
      ? 0
      : this.currentCardIndex + 1;
    this.currentCard = this.selectedDeck.cards[index];
    this.isBackVisible = false;
  }

  restartDeck(): void {
    if (this.selectedDeck) {
      this.selectDeck(this.selectedDeck);
    }
  }

  async createSampleDeck(): Promise<void> {
    if (this.isCreatingSampleDeck) {
      return;
    }

    this.isCreatingSampleDeck = true;
    this.flashcardsErrorMessage = '';

    try {
      const chatId = await this.getOrCreateChatId();
      const message = await firstValueFrom(
        this.http.post<ChatMessageResponse>(`${this.apiUrl}/chat-messages/`, {
          chatSessionId: chatId,
          content: 'Вправа з картками',
          timestamp: null,
          role: 1,
          sequenceNumber: 0
        })
      );

      const deck = await firstValueFrom(
        this.http.post<FlashcardResponse>(`${this.apiUrl}/flashcards/`, {
          name: 'Основи програмування',
          chatMessageId: message.id,
          cards: [
            {
              term: 'Змінна',
              definition: 'Іменоване місце для зберігання значення, яке можна використати пізніше.'
            },
            {
              term: 'Функція',
              definition: 'Повторно використовуваний блок коду, який виконує конкретну задачу.'
            },
            {
              term: 'Цикл',
              definition: 'Конструкція керування, яка повторює код, доки умова істинна.'
            }
          ]
        })
      );

      this.decks = [...this.decks, deck];
      this.selectDeck(deck);
    } catch (error) {
      this.flashcardsErrorMessage = this.getErrorMessage(error);
    } finally {
      this.isCreatingSampleDeck = false;
    }
  }

  async sendMessage(input: HTMLInputElement, event: SubmitEvent): Promise<void> {
    event.preventDefault();

    const content = input.value.trim();
    if (!content || this.isSending) {
      return;
    }

    input.value = '';
    this.errorMessage = '';
    this.isSending = true;

    const pendingMessage: ChatMessage = {
      role: 'user',
      content,
      status: 'pending'
    };
    this.messages = [...this.messages, pendingMessage];

    try {
      const response = await this.sendMessageToApi(content);

      this.messages = [
        ...this.messages.filter((message) => message !== pendingMessage),
        response.userMessage,
        response.assistantMessage
      ];
    } catch (error) {
      this.messages = this.messages.filter((message) => message !== pendingMessage);
      input.value = content;
      this.errorMessage = this.getErrorMessage(error);
      await this.loadMessages(false);
    } finally {
      this.isSending = false;
    }
  }

  private getStoredChatId(): string | null {
    if (!this.isBrowser) {
      return null;
    }

    const urlChatId = new URLSearchParams(window.location.search).get('chatId');
    if (urlChatId && this.guidPattern.test(urlChatId)) {
      localStorage.setItem(this.chatIdStorageKey, urlChatId);
      return urlChatId;
    }

    const existingChatId = localStorage.getItem(this.chatIdStorageKey);
    if (existingChatId && this.guidPattern.test(existingChatId)) {
      return existingChatId;
    }

    localStorage.removeItem(this.chatIdStorageKey);
    return null;
  }

  private async getOrCreateChatId(): Promise<string> {
    const existingChatId = this.getStoredChatId();
    if (existingChatId) {
      return existingChatId;
    }

    const user = await this.getOrCreateUser();
    const lecture = await this.getOrCreateLecture();
    const session = await firstValueFrom(
      this.http.post<ChatSessionResponse>(`${this.apiUrl}/chat-sessions/`, {
        userId: user.id,
        lectureId: lecture.id
      })
    );

    if (this.isBrowser) {
      localStorage.setItem(this.chatIdStorageKey, session.id);
    }

    return session.id;
  }

  private async getOrCreateUser(): Promise<UserResponse> {
    const users = await firstValueFrom(this.http.get<UserResponse[]>(`${this.apiUrl}/users/`));
    if (users.length > 0) {
      return users[0];
    }

    const group = await this.getOrCreateGroup();
    return await firstValueFrom(
      this.http.post<UserResponse>(`${this.apiUrl}/users/`, {
        name: 'Default user',
        password: '',
        groupId: group.id
      })
    );
  }

  private async getOrCreateGroup(): Promise<IdNameResponse> {
    const groups = await firstValueFrom(this.http.get<IdNameResponse[]>(`${this.apiUrl}/groups/`));
    if (groups.length > 0) {
      return groups[0];
    }

    return await firstValueFrom(
      this.http.post<IdNameResponse>(`${this.apiUrl}/groups/`, {
        name: 'Default group'
      })
    );
  }

  private async getOrCreateLecture(): Promise<LectureResponse> {
    const lectures = await firstValueFrom(
      this.http.get<LectureResponse[]>(`${this.apiUrl}/lectures/`)
    );
    if (lectures.length > 0) {
      return lectures[0];
    }

    const subject = await this.getOrCreateSubject();
    return await firstValueFrom(
      this.http.post<LectureResponse>(`${this.apiUrl}/lectures/`, {
        name: 'Default lecture',
        subjectId: subject.id
      })
    );
  }

  private async getOrCreateSubject(): Promise<SubjectResponse> {
    const subjects = await firstValueFrom(
      this.http.get<SubjectResponse[]>(`${this.apiUrl}/subjects/`)
    );
    if (subjects.length > 0) {
      return subjects[0];
    }

    const major = await this.getOrCreateMajor();
    return await firstValueFrom(
      this.http.post<SubjectResponse>(`${this.apiUrl}/subjects/`, {
        name: 'Default subject',
        majorId: major.id
      })
    );
  }

  private async getOrCreateMajor(): Promise<IdNameResponse> {
    const majors = await firstValueFrom(this.http.get<IdNameResponse[]>(`${this.apiUrl}/majors/`));
    if (majors.length > 0) {
      return majors[0];
    }

    return await firstValueFrom(
      this.http.post<IdNameResponse>(`${this.apiUrl}/majors/`, {
        name: 'Default major'
      })
    );
  }

  private async sendMessageToApi(content: string): Promise<AiChatSendMessageResponse> {
    const chatId = await this.getOrCreateChatId();

    return await firstValueFrom(
      this.http.post<AiChatSendMessageResponse>(
        `${this.apiUrl}/api/ai-chat/messages`,
        { chatId, content }
      )
    );
  }

  private async loadFlashcards(shouldSetError = true): Promise<void> {
    this.isLoadingFlashcards = true;
    this.flashcardsErrorMessage = '';

    try {
      this.decks = await firstValueFrom(
        this.http.get<FlashcardResponse[]>(`${this.apiUrl}/flashcards/`)
      );

      if (this.decks.length > 0) {
        const existingDeck = this.selectedDeck
          ? this.decks.find((deck) => deck.id === this.selectedDeck?.id)
          : null;
        this.selectDeck(existingDeck ?? this.decks[0]);
      }
    } catch (error) {
      if (shouldSetError) {
        this.flashcardsErrorMessage = this.getErrorMessage(error);
      }
    } finally {
      this.isLoadingFlashcards = false;
    }
  }

  private async loadMessages(shouldSetError = true): Promise<void> {
    const chatId = this.getStoredChatId();
    if (!chatId) {
      return;
    }

    try {
      const messages = await firstValueFrom(
        this.http.get<ChatMessage[]>(`${this.apiUrl}/api/ai-chat/messages`, {
          params: { chatId }
        })
      );

      this.messages = messages.filter((message) => message.status !== 'failed');
    } catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 404) {
        if (this.isBrowser) {
          localStorage.removeItem(this.chatIdStorageKey);
        }

        this.messages = [];
        return;
      }

      if (shouldSetError) {
        this.errorMessage = this.getErrorMessage(error);
      }
    }
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
        if (apiMessage.includes('HttpClient.Timeout')) {
          return 'ШІ не відповів вчасно. Backend перевищив час очікування відповіді зовнішнього AI-сервісу.';
        }

        return apiMessage;
      }

      if (error.status === 0) {
        return 'API недоступний. Перевір, що backend запущений на localhost:5132.';
      }
    }

    if (error instanceof Error && error.message) {
      return error.message;
    }

    return 'Не вдалося отримати відповідь.';
  }
}
