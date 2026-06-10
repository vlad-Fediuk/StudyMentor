import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

type PageLanguage = 'uk' | 'en';

interface GuidanceCard {
  title: string;
  body: string;
}

interface PageCopy {
  languageName: string;
  breadcrumbs: string[];
  kicker: string;
  title: string;
  lead: string;
  calloutLabel: string;
  callout: string;
  whyTitle: string;
  whyParagraphs: string[];
  cards: GuidanceCard[];
  bestTitle: string;
  bestParagraphs: string[];
  backLink: string;
}

const PAGE_COPY: Record<PageLanguage, PageCopy> = {
  uk: {
    languageName: 'Українська',
    breadcrumbs: ['StudyMentor AI', 'Гід безпеки'],
    kicker: 'Як StudyMentor AI відповідає на навчальні питання',
    title: 'Як працює ШІ в StudyMentor і чому відповіді треба перевіряти',
    lead:
      "StudyMentor допомагає розібрати навчальні теми, пояснити складні поняття простішими словами, знайти ідеї для розв'язання задач і підказати наступний крок у навчанні. Відповіді формуються на основі вашого запиту, доступного контексту та мовних закономірностей, які модель вивчила під час навчання.",
    calloutLabel: 'Важливо',
    callout:
      "ШІ може бути дуже корисним помічником, але він не є викладачем, офіційним джерелом або гарантією правильності. Якщо відповідь впливає на оцінювання, безпеку, здоров'я, фінанси чи юридичні рішення, обов'язково звірте її з надійними матеріалами.",
    whyTitle: 'Чому відповіді можуть бути неточними',
    whyParagraphs: [
      "Модель іноді неправильно розуміє запит, пропускає важливі деталі або робить висновок без достатньої опори на факти. Вона може впевнено сформулювати текст, який звучить переконливо, але містить помилку у даті, формулі, терміні, посиланні чи причинно-наслідковому зв'язку.",
      'Також ШІ не завжди має актуальну інформацію про події, зміни в правилах, оновлені навчальні програми або локальні вимоги викладача. Саме тому відповідь варто сприймати як чернетку для мислення: вона допомагає почати, структурувати ідеї та побачити можливий напрям, але фінальну перевірку робить користувач.'
    ],
    cards: [
      {
        title: 'Перевіряйте факти',
        body: 'Звіряйте дати, формули, цитати, визначення та посилання з конспектом, підручником або офіційними матеріалами курсу.'
      },
      {
        title: 'Уточнюйте запит',
        body: 'Якщо відповідь надто загальна, попросіть пояснити кроки, навести приклад, порівняти варіанти або виправити конкретну частину.'
      },
      {
        title: 'Не передавайте приватне',
        body: 'Не вводьте паролі, персональні документи, конфіденційні дані, приватні контакти або іншу інформацію, яку не варто показувати стороннім сервісам.'
      }
    ],
    bestTitle: 'Як використовувати StudyMentor найкраще',
    bestParagraphs: [
      'Питайте не лише "яка відповідь?", а й "чому саме так?", "який перший крок?", "де тут може бути помилка?" або "поясни це на простому прикладі". Так StudyMentor стає не заміною вашого мислення, а навчальним партнером, який допомагає краще зрозуміти матеріал.',
      'Для домашніх завдань і підготовки до контрольних використовуйте відповіді як основу для власної роботи: перепишіть пояснення своїми словами, перевірте розрахунки, порівняйте з матеріалами заняття і поставте додаткове питання, якщо щось залишилось незрозумілим.'
    ],
    backLink: 'Повернутися до чату'
  },
  en: {
    languageName: 'English',
    breadcrumbs: ['StudyMentor AI', 'Safety Guide'],
    kicker: 'How StudyMentor AI answers learning questions',
    title: 'How AI works in StudyMentor and why answers should be checked',
    lead:
      'StudyMentor helps you unpack learning topics, explain difficult concepts in simpler words, find ideas for solving tasks, and choose the next step in your study process. Answers are generated from your prompt, available context, and language patterns learned during model training.',
    calloutLabel: 'Important',
    callout:
      'AI can be a very helpful assistant, but it is not a teacher, an official source, or a guarantee of correctness. If an answer affects grading, safety, health, finances, or legal decisions, always compare it with reliable materials.',
    whyTitle: 'Why answers can be inaccurate',
    whyParagraphs: [
      'The model can misunderstand a prompt, miss important details, or make a conclusion without enough factual support. It may write confidently and sound convincing while still getting a date, formula, term, link, or cause-and-effect relationship wrong.',
      'AI also may not have current information about events, rule changes, updated learning programs, or local teacher requirements. Treat the answer as a draft for thinking: it can help you start, organize ideas, and see a possible direction, but the final check is yours.'
    ],
    cards: [
      {
        title: 'Check the facts',
        body: 'Compare dates, formulas, quotes, definitions, and links with your notes, textbook, or official course materials.'
      },
      {
        title: 'Clarify the prompt',
        body: 'If an answer feels too general, ask for steps, examples, comparisons, or a correction of a specific part.'
      },
      {
        title: 'Keep private data private',
        body: 'Do not enter passwords, personal documents, confidential data, private contacts, or anything you would not share with external services.'
      }
    ],
    bestTitle: 'How to use StudyMentor well',
    bestParagraphs: [
      'Ask not only "what is the answer?", but also "why does it work this way?", "what is the first step?", "where could this be wrong?", or "explain it with a simple example". This makes StudyMentor a learning partner rather than a replacement for your own thinking.',
      'For homework and test preparation, use answers as a base for your own work: rewrite explanations in your own words, check calculations, compare them with class materials, and ask a follow-up question when something is still unclear.'
    ],
    backLink: 'Back to chat'
  }
};

@Component({
  selector: 'app-ai-disclaimer-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './ai-disclaimer-page.component.html',
  styleUrl: './ai-disclaimer-page.component.scss'
})
export class AiDisclaimerPageComponent {
  selectedLanguage: PageLanguage = 'uk';
  languages: PageLanguage[] = ['uk', 'en'];

  get copy(): PageCopy {
    return PAGE_COPY[this.selectedLanguage];
  }

  selectLanguage(language: PageLanguage): void {
    this.selectedLanguage = language;
  }
}
