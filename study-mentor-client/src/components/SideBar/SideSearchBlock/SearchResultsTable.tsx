import "./SearchResultsTable.css";

interface SearchResultsTableProps {
  items: SearchResultItem[];
  onItemClick?: (item: SearchResultItem) => void;
}

export interface SearchResultItem {
  id: string;
  label: string;
}

interface SearchResultButtonProps {
  item: SearchResultItem;
  isDisabled?: boolean;
  onClick?: () => void;
}

const SearchResultButton = ({
  item,
  isDisabled = false,
  onClick,
}: SearchResultButtonProps) => {
  return (
    <button
      type="button"
      disabled={isDisabled}
      onClick={onClick}
      className="search-results-table__button"
    >
      <span className="search-results-table__label">{item.label}</span>
    </button>
  );
};

const SearchResultsTable = ({
  items,
  onItemClick,
}: SearchResultsTableProps) => {
  const visibleItems =
    items.length > 0 ? items : [{ id: "empty", label: "Nothing here" }];

  return (
    <div className="search-results-table">
      {visibleItems.map((item, index) => (
        <SearchResultButton
          key={`${item.id}-${index}`}
          item={item}
          isDisabled={items.length === 0}
          onClick={() => onItemClick?.(item)}
        />
      ))}
    </div>
  );
};

export default SearchResultsTable;
