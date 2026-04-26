import { useState } from "react";

interface SearchResultsTableProps {
  items: string[];
  onItemClick?: (item: string) => void;
}

interface SearchResultButtonProps {
  item: string;
  isDisabled?: boolean;
  onClick?: () => void;
}

const SearchResultButton = ({
  item,
  isDisabled = false,
  onClick,
}: SearchResultButtonProps) => {
  const [isHovered, setIsHovered] = useState(false);
  const [isPressed, setIsPressed] = useState(false);

  return (
    <button
      type="button"
      disabled={isDisabled}
      onClick={onClick}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => {
        setIsHovered(false);
        setIsPressed(false);
      }}
      onMouseDown={() => setIsPressed(true)}
      onMouseUp={() => setIsPressed(false)}
      style={{
        width: "100%",
        minHeight: 40,
        height: 40,
        backgroundColor: "#F9F4ED",
        display: "flex",
        alignItems: "center",
        paddingLeft: 15,
        paddingRight: 15,
        boxSizing: "border-box",
        borderTop: "none",
        borderLeft: "none",
        borderRight: "none",
        borderBottom: "1px solid #4D463C",
        flexShrink: 0,
        cursor: isDisabled ? "default" : "pointer",
        textAlign: "left",
        transition: "box-shadow 0.2s ease, transform 0.12s ease",
        outline: "none",
        boxShadow: isDisabled
          ? "none"
          : isHovered
            ? "0 0 0 2px #D0D0D0 inset"
            : "none",
        transform:
          !isDisabled && isPressed
            ? "translateY(1px) scale(0.99)"
            : "translateY(0) scale(1)",
      }}
    >
      <span
        style={{
          fontFamily: "SF Compact, sans-serif",
          fontSize: 16,
          color: "#4D463C",
        }}
      >
        {item}
      </span>
    </button>
  );
};

const SearchResultsTable = ({
  items,
  onItemClick,
}: SearchResultsTableProps) => {
  const visibleItems = items.length > 0 ? items : ["Nothing here"];

  return (
    <div
      style={{
        marginTop: 20,
        paddingBottom: 10,
        boxSizing: "border-box",
        maxHeight: 330,
        overflowY: "auto",
        overflowX: "hidden",
        display: "flex",
        flexDirection: "column",
        scrollbarWidth: "none",
        msOverflowStyle: "none",
        borderBottom: "1px solid #4D463C",
      }}
    >
      {visibleItems.map((item, index) => (
        <SearchResultButton
          key={`${item}-${index}`}
          item={item}
          isDisabled={items.length === 0}
          onClick={() => onItemClick?.(item)}
        />
      ))}
    </div>
  );
};

export default SearchResultsTable;
