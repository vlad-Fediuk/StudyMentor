import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Component, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

type ChatRole = 'user' | 'assistant';

interface ChatMessage {
  id?: string;
  role: ChatRole;
  content: string;
  status?: string;
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

  messages: ChatMessage[] = [];
  isSending = false;
  errorMessage = '';

  async ngOnInit(): Promise<void> {
    await this.loadMessages();
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

    if (urlChatId && /^[a-f\d]{24}$/i.test(urlChatId)) {
      localStorage.setItem(this.chatIdStorageKey, urlChatId);
      return urlChatId;
    }

    const existingChatId = localStorage.getItem(this.chatIdStorageKey);

    if (existingChatId && /^[a-f\d]{24}$/i.test(existingChatId)) {
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

    return await this.createChatSession();
  }

  private async createChatSession(): Promise<string> {
    const session = await firstValueFrom(
      this.http.post<ChatSessionResponse>(`${this.apiUrl}/chat-sessions/`, {
        userId: 'default-user',
        lectureId: 'default-lecture'
      })
    );

    if (this.isBrowser) {
      localStorage.setItem(this.chatIdStorageKey, session.id);
    }

    return session.id;
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
          return 'AI не відповів вчасно. Запит до зовнішнього AI-сервісу на бекенді перевищив 100 секунд.';
        }

        return apiMessage;
      }

      if (error.status === 0) {
        return 'API недоступний. Перевірте, чи запущений бекенд на localhost:5132.';
      }
    }

    return 'Не вдалося отримати відповідь від AI.';
  }
}
