import { useEffect, useState } from "react";
import AccountCard from "./AccountCard/AccountCard";
import ChatTypeSelectorGroup from "./ChatTypeSelector/ChatTypeSelectorGroup";
import SidebarVisibilityToggle from "./SidebarVisibilityToggle/SidebarVisibilityToggle";
import SideSearchBlock from "./SideSearchBlock/SideSearchBlock";
import type { SearchResultItem } from "./SideSearchBlock/SearchResultsTable";
import "./SideBar.css";

const apiBaseUrl = "http://localhost:5132";
const accountGroupNumber = 531;
const majorNameByFirstDigit: Record<string, string> = {
  "5": "ProgramEngineer",
  "3": "Radiotech",
};

interface SubjectDto {
  id: string;
  name: string;
  major: {
    id: string;
    name: string;
  };
}

interface LectureDto {
  id: string;
  name: string;
  subject: SubjectDto;
}

const SideBar = () => {
  const [isOpen, setIsOpen] = useState(true);
  const [activeChatTypeId, setActiveChatTypeId] = useState("subject");
  const [subjectItems, setSubjectItems] = useState<SearchResultItem[]>([]);
  const [lectureItems, setLectureItems] = useState<SearchResultItem[]>([]);
  const [selectedSubjectId, setSelectedSubjectId] = useState<string | null>(
    null,
  );

  useEffect(() => {
    if (activeChatTypeId !== "subject") {
      return;
    }

    const abortController = new AbortController();
    const firstGroupDigit = String(accountGroupNumber).charAt(0);
    const selectedMajorName = majorNameByFirstDigit[firstGroupDigit];

    const loadSubjects = async () => {
      try {
        const response = await fetch(`${apiBaseUrl}/subjects`, {
          signal: abortController.signal,
        });

        if (!response.ok) {
          throw new Error(`Failed to load subjects: ${response.status}`);
        }

        const items = (await response.json()) as SubjectDto[];
        const majorFilteredItems = selectedMajorName
          ? items.filter(
              (item) =>
                item.major.name.toLowerCase() ===
                selectedMajorName.toLowerCase(),
            )
          : items;
        const mappedSubjectItems = majorFilteredItems.map((item) => ({
          id: item.id,
          label: item.name,
        }));

        setSubjectItems(mappedSubjectItems);

        if (mappedSubjectItems.length > 0) {
          setSelectedSubjectId(
            (current) => current ?? mappedSubjectItems[0].id,
          );
        } else {
          setSelectedSubjectId(null);
        }
      } catch (error) {
        if (abortController.signal.aborted) {
          return;
        }

        console.error("Failed to load subjects", error);
        setSubjectItems([]);
      }
    };

    void loadSubjects();

    return () => {
      abortController.abort();
    };
  }, [activeChatTypeId]);

  useEffect(() => {
    if (activeChatTypeId !== "lecture") {
      return;
    }

    const abortController = new AbortController();
    const firstGroupDigit = String(accountGroupNumber).charAt(0);
    const selectedMajorName = majorNameByFirstDigit[firstGroupDigit];

    const loadLectures = async () => {
      try {
        const response = await fetch(`${apiBaseUrl}/lectures`, {
          signal: abortController.signal,
        });

        if (!response.ok) {
          throw new Error(`Failed to load lectures: ${response.status}`);
        }

        const items = (await response.json()) as LectureDto[];
        const majorFilteredLectureItems = selectedMajorName
          ? items.filter(
              (item) =>
                item.subject.major.name.toLowerCase() ===
                selectedMajorName.toLowerCase(),
            )
          : items;
        const fallbackSubjectId =
          selectedSubjectId ?? subjectItems[0]?.id ?? null;
        const subjectFilteredLectureItems = fallbackSubjectId
          ? majorFilteredLectureItems.filter(
              (item) => item.subject.id === fallbackSubjectId,
            )
          : majorFilteredLectureItems;

        setLectureItems(
          subjectFilteredLectureItems.map((item) => ({
            id: item.id,
            label: item.name,
          })),
        );
      } catch (error) {
        if (abortController.signal.aborted) {
          return;
        }

        console.error("Failed to load lectures", error);
        setLectureItems([]);
      }
    };

    void loadLectures();

    return () => {
      abortController.abort();
    };
  }, [activeChatTypeId, selectedSubjectId, subjectItems]);

  const handleChatTypeChange = (id: string) => {
    if (id === "lecture" && subjectItems.length > 0) {
      setSelectedSubjectId(subjectItems[0].id);
    }

    setActiveChatTypeId(id);

    if (!isOpen) {
      setIsOpen(true);
    }
  };

  const handleSearchItemClick = (item: SearchResultItem) => {
    if (activeChatTypeId === "subject") {
      setSelectedSubjectId(item.id);
      setActiveChatTypeId("lecture");
      return;
    }

    setIsOpen(false);
  };

  const visibleItems =
    activeChatTypeId === "subject" ? subjectItems : lectureItems;

  return (
    <section className={`sidebar ${isOpen ? "sidebar--open" : "sidebar--collapsed"}`}>
      <AccountCard
        firstName="Julia"
        lastName="Fox"
        badgeValue={accountGroupNumber}
        isCollapsed={!isOpen}
      />
      <ChatTypeSelectorGroup
        items={[
          { id: "subject", type: "subject" },
          { id: "lecture", type: "lecture" },
        ]}
        activeId={activeChatTypeId}
        isCollapsed={!isOpen}
        onChange={handleChatTypeChange}
      />
      {isOpen ? (
        <SideSearchBlock
          items={visibleItems}
          placeholder="Search chats"
          onItemClick={handleSearchItemClick}
        />
      ) : null}
      <SidebarVisibilityToggle
        isOpen={isOpen}
        onClick={() => setIsOpen((current) => !current)}
      />
    </section>
  );
};

export default SideBar;
