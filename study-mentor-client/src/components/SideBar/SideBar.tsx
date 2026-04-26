import { useState } from "react";
import AccountCard from "./AccountCard/AccountCard";
import ChatTypeSelectorGroup from "./ChatTypeSelector/ChatTypeSelectorGroup";
import SidebarVisibilityToggle from "./SidebarVisibilityToggle/SidebarVisibilityToggle";
import SideSearchBlock from "./SideSearchBlock/SideSearchBlock";

const subjectItems = [
  "Mathematics",
  "Computer Science",
  "World History",
  "Linear Algebra",
  "Organic Chemistry",
];

const lectureItems = [
  "Lecture 1",
  "Lecture 2",
  "Lecture 3",
  "Lecture 4",
];

const SideBar = () => {
  const [isOpen, setIsOpen] = useState(true);
  const [activeChatTypeId, setActiveChatTypeId] = useState("subject");

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
        badgeValue={67}
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
