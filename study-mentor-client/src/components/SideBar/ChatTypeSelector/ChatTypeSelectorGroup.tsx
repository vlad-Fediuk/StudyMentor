import { useState } from "react";
import LectureButton from "./buttons/LectureButton";
import SubjectButton from "./buttons/SubjectButton";

type ChatTypeSelectorItem = {
  id: string;
  type: "subject" | "lecture";
};

interface ChatTypeSelectorGroupProps {
  items: ChatTypeSelectorItem[];
  defaultActiveId?: string;
  onChange?: (id: string) => void;
  isCollapsed?: boolean;
}

const buttonWidth = 38;
const horizontalMargin = 20;

const ChatTypeSelectorGroup = ({
  items,
  defaultActiveId,
  onChange,
  isCollapsed = false,
}: ChatTypeSelectorGroupProps) => {
  const [activeId, setActiveId] = useState(
    defaultActiveId ?? items[0]?.id ?? "",
  );

  const handleClick = (id: string) => {
    setActiveId(id);
    onChange?.(id);
  };

  const visibleItems = isCollapsed
    ? items.filter((item) => item.id === activeId)
    : items;

  return (
    <div
      style={{
        width: isCollapsed ? "100%" : items.length * (buttonWidth + horizontalMargin),
        marginTop: 30,
        minHeight: buttonWidth,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
      }}
    >
      {visibleItems.map((item) => (
        <div key={item.id} style={{ margin: isCollapsed ? 0 : "0 10px" }}>
          {item.type === "subject" ? (
            <SubjectButton
              isActive={item.id === activeId}
              onClick={() => handleClick(item.id)}
            />
          ) : (
            <LectureButton
              isActive={item.id === activeId}
              onClick={() => handleClick(item.id)}
            />
          )}
        </div>
      ))}
    </div>
  );
};

export default ChatTypeSelectorGroup;
