import { useEffect, useState } from "react";
import AccountCard from "./AccountCard/AccountCard";
import ChatTypeSelectorGroup from "./ChatTypeSelector/ChatTypeSelectorGroup";
import SidebarVisibilityToggle from "./SidebarVisibilityToggle/SidebarVisibilityToggle";
import SideSearchBlock from "./SideSearchBlock/SideSearchBlock";

const apiBaseUrl = "http://localhost:5132";
const accountGroupNumber = 51;
const majorNameByFirstDigit: Record<string, string> = {
  "5": "ProgramIngineer",
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

const lectureItems = [
  "Lecture 1",
  "Lecture 2",
  "Lecture 3",
  "Lecture 4",
];

const SideBar = () => {
  const [isOpen, setIsOpen] = useState(true);
  const [activeChatTypeId, setActiveChatTypeId] = useState("subject");
  const [subjectItems, setSubjectItems] = useState<string[]>([]);

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
          ? items.filter((item) => item.major.name === selectedMajorName)
          : items;
        const visibleSubjectItems =
          majorFilteredItems.length > 0 ? majorFilteredItems : items;

        setSubjectItems(visibleSubjectItems.map((item) => item.name));
      } catch (error) {
        if (abortController.signal.aborted) {
          return;
        }

        setSubjectItems([]);
      }
    };

    void loadSubjects();

    return () => {
      abortController.abort();
    };
  }, [activeChatTypeId]);

  const handleChatTypeChange = (id: string) => {
    setActiveChatTypeId(id);

    if (!isOpen) {
      setIsOpen(true);
    }
  };

  const handleSearchItemClick = () => {
    if (activeChatTypeId === "subject") {
      setActiveChatTypeId("lecture");
      return;
    }

    setIsOpen(false);
  };

  const visibleItems =
    activeChatTypeId === "subject" ? subjectItems : lectureItems;

  return (
    <section
      style={{
        minHeight: "100vh",
        width: isOpen ? 330 : 50,
        display: "flex",
        flexDirection: "column",
        backgroundColor: isOpen ? "#EEE7DE" : "#B55252",
        borderRight: "1px solid #4D463C",
        boxSizing: "border-box",
        overflow: "hidden",
        transition: "width 0.2s ease",
      }}
    >
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
