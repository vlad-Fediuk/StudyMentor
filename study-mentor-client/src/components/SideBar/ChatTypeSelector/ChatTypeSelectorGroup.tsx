import LectureButton from "./buttons/LectureButton";
import SubjectButton from "./buttons/SubjectButton";
import "./ChatTypeSelectorGroup.css";

type ChatTypeSelectorItem = {
  id: string;
  type: "subject" | "lecture";
};

interface ChatTypeSelectorGroupProps {
  items: ChatTypeSelectorItem[];
  activeId: string;
  onChange?: (id: string) => void;
  isCollapsed?: boolean;
}

const ChatTypeSelectorGroup = ({
  items,
  activeId,
  onChange,
  isCollapsed = false,
}: ChatTypeSelectorGroupProps) => {
  const handleClick = (id: string) => {
    onChange?.(id);
  };

  const visibleItems = isCollapsed
    ? items.filter((item) => item.id === activeId)
    : items;

  return (
    <div className="chat-type-selector">
      {visibleItems.map((item) => (
        <div
          key={item.id}
          className={`chat-type-selector__item ${isCollapsed ? "chat-type-selector__item--collapsed" : ""}`}
        >
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
