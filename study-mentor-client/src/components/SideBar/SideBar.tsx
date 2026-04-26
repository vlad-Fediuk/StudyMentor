import { useState } from "react";
import AccountCard from "./AccountCard/AccountCard";
import ChatTypeSelectorGroup from "./ChatTypeSelector/ChatTypeSelectorGroup";
import SidebarVisibilityToggle from "./SidebarVisibilityToggle/SidebarVisibilityToggle";
import SideSearchBlock from "./SideSearchBlock/SideSearchBlock";

const SideBar = () => {
  const [isOpen, setIsOpen] = useState(true);

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
        defaultActiveId="subject"
        isCollapsed={!isOpen}
        onChange={() => {
          if (!isOpen) {
            setIsOpen(true);
          }
        }}
      />
      {isOpen ? (
        <SideSearchBlock
          items={[
            "Mathematics",
            "Computer Science",
            "World History",
            "Linear Algebra",
            "Organic Chemistry",
          ]}
          placeholder="Search chats"
          onItemClick={() => setIsOpen(false)}
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
