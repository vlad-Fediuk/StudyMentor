import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, HostListener, OnInit, PLATFORM_ID, inject } from '@angular/core';
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

interface TestAnswerVariantResponse {
  id: string;
  text: string;
}

interface TestQuestionResponse {
  id: string;
  prompt: string;
  answerVariants: TestAnswerVariantResponse[];
}

interface TestResponse {
  id: string;
  name: string;
  chatMessageId: string;
  sourceFlashcardId: string | null;
  questions: TestQuestionResponse[];
}

interface TestAnswerResultResponse {
  questionId: string;
  selectedAnswerVariantId: string;
  correctAnswerVariantId: string;
  isCorrect: boolean;
}

interface TestRunResult {
  correct: number;
  total: number;
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
  private readonly hiddenExerciseMessageContent = '__study_mentor_exercise_source__';
  private readonly legacyExerciseMessageContents = new Set([
    'Flashcard exercise',
    'Вправа з картками',
    'Упражнение с карточками'
  ]);
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
  isDeletingExercise = false;
  isCardStudyOpen = false;
  isTestPromptVisible = false;
  flashcardsErrorMessage = '';

  tests: TestResponse[] = [];
  selectedTest: TestResponse | null = null;
  currentQuestionIndex = 0;
  selectedAnswerVariantId: string | null = null;
  answerResult: TestAnswerResultResponse | null = null;
  isTestRunOpen = false;
  isTestResultOpen = false;
  isCheckingAnswer = false;
  isSuccessFeedbackVisible = false;
  testCorrectAnswers = 0;
  completedTestResults: Record<string, TestRunResult> = {};
  testsErrorMessage = '';

  selectedExerciseType: 'flashcards' | 'tests' = 'flashcards';

  async ngOnInit(): Promise<void> {
    await Promise.all([this.loadMessages(), this.loadFlashcards(false), this.loadTests(false)]);
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

  get currentQuestion(): TestQuestionResponse | null {
    return this.selectedTest?.questions[this.currentQuestionIndex] ?? null;
  }

  get currentQuestionNumber(): number {
    return this.currentQuestionIndex + 1;
  }

  get totalQuestions(): number {
    return this.selectedTest?.questions.length ?? 0;
  }

  get selectedDeckTests(): TestResponse[] {
    if (!this.selectedDeck) {
      return this.tests;
    }

    return this.tests.filter((test) => test.sourceFlashcardId === this.selectedDeck?.id);
  }

  get testScorePercent(): number {
    if (this.totalQuestions === 0) {
      return 0;
    }

    return Math.round((this.testCorrectAnswers / this.totalQuestions) * 100);
  }

  get isPerfectTestResult(): boolean {
    return this.totalQuestions > 0 && this.testCorrectAnswers === this.totalQuestions;
  }

  displayDeckName(deck: FlashcardResponse | null): string {
    if (!deck) {
      return 'Вправи';
    }

    return this.translateLegacyExerciseName(deck.name);
  }

  displayTestName(test: TestResponse): string {
    return this.translateLegacyExerciseName(test.name);
  }

  displayCardTerm(card: CardResponse): string {
    return this.translateLegacyCardText(card.term);
  }

  displayCardDefinition(card: CardResponse): string {
    return this.translateLegacyCardText(card.definition);
  }

  displayQuestionPrompt(question: TestQuestionResponse): string {
    return this.translateLegacyTestText(question.prompt);
  }

  displayAnswerText(answerVariant: TestAnswerVariantResponse): string {
    return this.translateLegacyTestText(answerVariant.text);
  }

  testResultFor(test: TestResponse): TestRunResult | null {
    return this.completedTestResults[test.id] ?? null;
  }

  testResultPercent(test: TestResponse): number {
    const result = this.testResultFor(test);
    if (!result || result.total === 0) {
      return 0;
    }

    return Math.round((result.correct / result.total) * 100);
  }

  isPerfectTest(test: TestResponse): boolean {
    const result = this.testResultFor(test);
    return Boolean(result && result.total > 0 && result.correct === result.total);
  }

  async openFlashcards(): Promise<void> {
    this.isFlashcardsOpen = !this.isFlashcardsOpen;

    if (this.isFlashcardsOpen) {
      await Promise.all([this.loadFlashcards(), this.loadTests()]);
    }
  }

  closeFlashcards(): void {
    this.isFlashcardsOpen = false;
    this.isCardStudyOpen = false;
    this.closeTestRun();
  }

  selectDeck(deck: FlashcardResponse): void {
    this.selectedDeck = deck;
    this.currentCard = deck.cards[0] ?? null;
    this.isBackVisible = false;
    this.isCardStudyOpen = false;
    this.isTestPromptVisible = false;
    this.flashcardsErrorMessage = '';

    const deckTest = this.selectedDeckTests[0];
    if (deckTest) {
      this.selectTest(deckTest);
    } else if (this.selectedTest?.sourceFlashcardId !== deck.id) {
      this.selectedTest = null;
    }
  }

  selectExerciseType(type: 'flashcards' | 'tests'): void {
    this.selectedExerciseType = type;
    this.closeTestRun();
  }

  selectCard(card: CardResponse): void {
    this.currentCard = card;
    this.isBackVisible = false;
    this.isCardStudyOpen = true;
    this.isTestPromptVisible = false;
  }

  backToExerciseList(): void {
    this.isCardStudyOpen = false;
    this.isBackVisible = false;
    this.isTestPromptVisible = false;
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
    this.isTestPromptVisible = false;
  }

  showNextCard(): void {
    if (!this.selectedDeck || this.selectedDeck.cards.length === 0) {
      return;
    }

    if (this.currentCardIndex === this.selectedDeck.cards.length - 1) {
      this.isTestPromptVisible = true;
      return;
    }

    const index = this.currentCardIndex < 0 ? 0 : this.currentCardIndex + 1;
    this.currentCard = this.selectedDeck.cards[index];
    this.isBackVisible = false;
    this.isTestPromptVisible = false;
  }

  restartDeck(): void {
    if (this.selectedDeck) {
      this.selectDeck(this.selectedDeck);
    }
  }

  selectTest(test: TestResponse): void {
    this.selectedTest = test;
    this.currentQuestionIndex = 0;
    this.selectedAnswerVariantId = null;
    this.answerResult = null;
    this.testCorrectAnswers = 0;
    this.isTestResultOpen = false;
    this.isSuccessFeedbackVisible = false;
    this.testsErrorMessage = '';
  }

  startTest(test: TestResponse): void {
    this.selectTest(test);
    this.isCardStudyOpen = false;
    this.isTestRunOpen = true;
    this.isTestPromptVisible = false;
    this.selectedExerciseType = 'tests';
  }

  startDeckTest(): void {
    const test = this.selectedDeckTests[0];

    if (!test) {
      this.testsErrorMessage = 'Для цього набору тест ще не створено.';
      this.isCardStudyOpen = false;
      this.selectedExerciseType = 'tests';
      return;
    }

    this.startTest(test);
  }

  dismissTestPrompt(): void {
    this.isTestPromptVisible = false;
  }

  closeTestRun(): void {
    this.isTestRunOpen = false;
    this.isTestResultOpen = false;
    this.selectedAnswerVariantId = null;
    this.answerResult = null;
    this.isSuccessFeedbackVisible = false;
  }

  backToTestsList(): void {
    this.isTestRunOpen = false;
    this.isTestResultOpen = false;
    this.selectedExerciseType = 'tests';
  }

  restartTest(): void {
    if (this.selectedTest) {
      this.startTest(this.selectedTest);
    }
  }

  @HostListener('window:keydown')
  continueWrongAnswerWithKeyboard(): void {
    if (this.isTestRunOpen && this.answerResult && !this.answerResult.isCorrect) {
      this.continueAfterWrongAnswer();
    }
  }

  async selectAnswerVariant(answerVariantId: string): Promise<void> {
    if (this.answerResult) {
      if (!this.answerResult.isCorrect && answerVariantId === this.answerResult.correctAnswerVariantId) {
        this.continueAfterWrongAnswer();
      }

      return;
    }

    if (this.isCheckingAnswer) {
      return;
    }

    this.selectedAnswerVariantId = answerVariantId;
    await this.checkAnswer(answerVariantId);
  }

  continueAfterWrongAnswer(): void {
    this.goToNextQuestion();
  }

  async deleteSelectedDeck(): Promise<void> {
    if (!this.selectedDeck || this.isDeletingExercise) {
      return;
    }

    const deck = this.selectedDeck;
    if (this.isBrowser && !window.confirm(`Видалити набір "${this.displayDeckName(deck)}" і пов'язані тести?`)) {
      return;
    }

    this.isDeletingExercise = true;
    this.flashcardsErrorMessage = '';
    this.testsErrorMessage = '';

    try {
      await Promise.all(
        this.tests
          .filter((test) => test.sourceFlashcardId === deck.id)
          .map((test) => firstValueFrom(this.http.delete(`${this.apiUrl}/tests/${test.id}`)))
      );
      await firstValueFrom(this.http.delete(`${this.apiUrl}/flashcards/${deck.id}`));

      this.tests = this.tests.filter((test) => test.sourceFlashcardId !== deck.id);
      this.decks = this.decks.filter((item) => item.id !== deck.id);
      this.selectedDeck = this.decks[0] ?? null;

      if (this.selectedDeck) {
        this.selectDeck(this.selectedDeck);
      } else {
        this.currentCard = null;
        this.selectedTest = null;
        this.isCardStudyOpen = false;
      }
    } catch (error) {
      this.flashcardsErrorMessage = this.getErrorMessage(error);
    } finally {
      this.isDeletingExercise = false;
    }
  }

  async deleteTest(test: TestResponse, event?: Event): Promise<void> {
    event?.stopPropagation();

    if (this.isDeletingExercise) {
      return;
    }

    if (this.isBrowser && !window.confirm(`Видалити тест "${this.displayTestName(test)}"?`)) {
      return;
    }

    this.isDeletingExercise = true;
    this.testsErrorMessage = '';

    try {
      await firstValueFrom(this.http.delete(`${this.apiUrl}/tests/${test.id}`));
      this.tests = this.tests.filter((item) => item.id !== test.id);

      if (this.selectedTest?.id === test.id) {
        this.selectedTest = this.selectedDeckTests[0] ?? null;
        this.closeTestRun();
      }
    } catch (error) {
      this.testsErrorMessage = this.getErrorMessage(error);
    } finally {
      this.isDeletingExercise = false;
    }
  }

  isSelectedAnswer(answerVariant: TestAnswerVariantResponse): boolean {
    return this.selectedAnswerVariantId === answerVariant.id;
  }

  isCorrectAnswer(answerVariant: TestAnswerVariantResponse): boolean {
    return this.answerResult?.correctAnswerVariantId === answerVariant.id;
  }

  isWrongSelectedAnswer(answerVariant: TestAnswerVariantResponse): boolean {
    return Boolean(
      this.answerResult &&
      !this.answerResult.isCorrect &&
      this.answerResult.selectedAnswerVariantId === answerVariant.id
    );
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

  private async checkAnswer(answerVariantId: string): Promise<void> {
    if (!this.selectedTest || !this.currentQuestion) {
      return;
    }

    this.isCheckingAnswer = true;
    this.testsErrorMessage = '';

    try {
      const result = await firstValueFrom(
        this.http.post<TestAnswerResultResponse>(
          `${this.apiUrl}/tests/${this.selectedTest.id}/questions/${this.currentQuestion.id}/answer`,
          { answerVariantId }
        )
      );

      this.answerResult = result;

      if (result.isCorrect) {
        this.testCorrectAnswers += 1;
        this.isSuccessFeedbackVisible = true;
        window.setTimeout(() => {
          this.isSuccessFeedbackVisible = false;
          this.goToNextQuestion();
        }, 750);
      }
    } catch (error) {
      this.selectedAnswerVariantId = null;
      this.testsErrorMessage = this.getErrorMessage(error);
    } finally {
      this.isCheckingAnswer = false;
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

  private async loadTests(shouldSetError = true): Promise<void> {
    try {
      this.tests = await firstValueFrom(this.http.get<TestResponse[]>(`${this.apiUrl}/tests/`));

      if (this.selectedDeckTests.length > 0 && !this.selectedTest) {
        this.selectTest(this.selectedDeckTests[0]);
      }
    } catch (error) {
      if (shouldSetError) {
        this.testsErrorMessage = this.getErrorMessage(error);
      }
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

      this.messages = messages.filter((message) => this.isVisibleChatMessage(message));
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

  private goToNextQuestion(): void {
    if (!this.selectedTest) {
      return;
    }

    if (this.currentQuestionIndex >= this.selectedTest.questions.length - 1) {
      this.completedTestResults = {
        ...this.completedTestResults,
        [this.selectedTest.id]: {
          correct: this.testCorrectAnswers,
          total: this.selectedTest.questions.length
        }
      };
      this.currentQuestionIndex = 0;
      this.isTestRunOpen = false;
      this.isTestResultOpen = true;
      this.selectedAnswerVariantId = null;
      this.answerResult = null;
      this.selectedExerciseType = 'tests';
      return;
    }

    this.currentQuestionIndex += 1;
    this.selectedAnswerVariantId = null;
    this.answerResult = null;
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
          return 'AI не відповів вчасно. Backend перевищив час очікування відповіді зовнішнього AI-сервісу.';
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

  private isVisibleChatMessage(message: ChatMessage): boolean {
    return message.status !== 'failed' &&
      message.content !== this.hiddenExerciseMessageContent &&
      !this.legacyExerciseMessageContents.has(message.content);
  }

  private translateLegacyExerciseName(name: string): string {
    const legacyNames: Record<string, string> = {
      'Programming basics': 'Основи програмування',
      'Основы программирования': 'Основи програмування',
      'Основи програмування': 'Основи програмування',
      'Тест: Основы программирования': 'Тест: Основи програмування',
      'Тест: Основи програмування': 'Тест: Основи програмування',
      'Flashcard exercise': 'Вправа з картками',
      'Упражнение с карточками': 'Вправа з картками',
      'Вправа з картками': 'Вправа з картками',
      'OOP': 'ООП'
    };

    return legacyNames[name] ?? name;
  }

  private translateLegacyCardText(text: string): string {
    const legacyCardTexts: Record<string, string> = {
      'Переменная': 'Змінна',
      'Функция': 'Функція',
      'Цикл': 'Цикл',
      'Именованное место для хранения значения, которое можно использовать позже.':
        'Іменоване місце для зберігання значення, яке можна використати пізніше.',
      'Повторно используемый блок кода, который выполняет конкретную задачу.':
        'Повторно використовуваний блок коду, який виконує конкретну задачу.',
      'Конструкция управления, которая повторяет код, пока условие истинно.':
        'Конструкція керування, яка повторює код, доки умова істинна.',
      'Variable': 'Змінна',
      'Function': 'Функція',
      'Loop': 'Цикл',
      'A named storage location for a value that can be used later.':
        'Іменоване місце для зберігання значення, яке можна використати пізніше.',
      'A reusable block of code that performs a specific task.':
        'Повторно використовуваний блок коду, який виконує конкретну задачу.',
      'A control structure that repeats code while a condition is true.':
        'Конструкція керування, яка повторює код, доки умова істинна.'
    };

    return legacyCardTexts[text] ?? text;
  }

  private translateLegacyTestText(text: string): string {
    const legacyTestTexts: Record<string, string> = {
      'Что такое переменная?': 'Що таке змінна?',
      'Для чего нужна функция?': 'Для чого потрібна функція?',
      'Что делает цикл?': 'Що робить цикл?',
      'Именованное место для хранения значения': 'Іменоване місце для зберігання значення',
      'Ошибка в программе': 'Помилка у програмі',
      'Команда для запуска сервера': 'Команда для запуску сервера',
      'Файл с настройками': 'Файл із налаштуваннями',
      'Чтобы повторно использовать блок кода': 'Щоб повторно використовувати блок коду',
      'Чтобы удалить все данные': 'Щоб видалити всі дані',
      'Чтобы изменить цвет экрана': 'Щоб змінити колір екрана',
      'Чтобы остановить компилятор': 'Щоб зупинити компілятор',
      'Повторяет код, пока выполняется условие': 'Повторює код, доки виконується умова',
      'Сохраняет пароль пользователя': 'Зберігає пароль користувача',
      'Создает новую базу данных': 'Створює нову базу даних',
      'Проверяет интернет-соединение': 'Перевіряє інтернет-з’єднання'
    };

    return legacyTestTexts[text] ?? text;
  }
}
