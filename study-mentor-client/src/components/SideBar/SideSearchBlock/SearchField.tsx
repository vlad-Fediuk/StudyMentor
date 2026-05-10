import "./SearchField.css";

interface SearchFieldProps {
  value: string;
  placeholder?: string;
  onChange: (value: string) => void;
}

const SearchField = ({
  value,
  placeholder = "Search",
  onChange,
}: SearchFieldProps) => {
  return (
    <div className="search-field">
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="search-field__input"
      />
    </div>
  );
};

export default SearchField;
