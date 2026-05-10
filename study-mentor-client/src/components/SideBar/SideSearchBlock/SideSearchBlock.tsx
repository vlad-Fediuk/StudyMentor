import { useMemo, useState } from "react";
import SearchField from "./SearchField";
import SearchResultsTable, { type SearchResultItem } from "./SearchResultsTable";
import "./SideSearchBlock.css";

interface SearchBlockProps {
  items: SearchResultItem[];
  placeholder?: string;
  onItemClick?: (item: SearchResultItem) => void;
}

const normalize = (value: string) =>
  value.toLowerCase().replace(/\s+/g, " ").trim();

const SideSearchBlock = ({
  items,
  placeholder = "Search",
  onItemClick,
}: SearchBlockProps) => {
  const [query, setQuery] = useState("");

  const filteredItems = useMemo(() => {
    const normalizedQuery = normalize(query);

    if (!normalizedQuery) {
      return items;
    }

    const queryParts = normalizedQuery.split(" ");

    return items.filter((item) => {
      const normalizedItem = normalize(item.label);
      return queryParts.every((part) => normalizedItem.includes(part));
    });
  }, [items, query]);

  return (
    <div className="search-block">
      <SearchField
        value={query}
        onChange={setQuery}
        placeholder={placeholder}
      />
      <SearchResultsTable items={filteredItems} onItemClick={onItemClick} />
    </div>
  );
};

export default SideSearchBlock;
