import { useMemo, useState } from "react";
import SearchField from "./SearchField";
import SearchResultsTable from "./SearchResultsTable";

interface SearchBlockProps {
  items: string[];
  placeholder?: string;
  onItemClick?: (item: string) => void;
}

const searchFieldHeight = 40;
const searchResultsGap = 20;
const searchBlockTopPadding = 10;
const searchBlockBottomPadding = 10;
const resultRowHeight = 40;
const resultRowsLimit = 8;

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
      const normalizedItem = normalize(item);
      return queryParts.every((part) => normalizedItem.includes(part));
    });
  }, [items, query]);

  const visibleRowsCount = Math.max(
    1,
    Math.min(filteredItems.length, resultRowsLimit),
  );
  const resultsHeight =
    visibleRowsCount * resultRowHeight + searchBlockBottomPadding;
  const searchBlockHeight =
    searchBlockTopPadding +
    searchFieldHeight +
    searchResultsGap +
    resultsHeight;

  return (
    <div
      style={{
        width: 310,
        height: searchBlockHeight,
        maxHeight:
          searchBlockTopPadding +
          searchFieldHeight +
          searchResultsGap +
          resultRowsLimit * resultRowHeight +
          searchBlockBottomPadding,
        marginTop: 15,
        marginLeft: 10,
        marginRight: 10,
        backgroundColor: "#B55252",
        borderRadius: 10,
        paddingLeft: 10,
        paddingRight: 10,
        boxSizing: "border-box",
        display: "flex",
        flexDirection: "column",
        paddingTop: searchBlockTopPadding,
        overflow: "hidden",
      }}
    >
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
