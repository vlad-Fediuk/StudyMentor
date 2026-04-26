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
    <div
      style={{
        width: 290,
        height: 40,
        backgroundColor: "#F9F4ED",
        borderRadius: 20,
        display: "flex",
        alignItems: "center",
        paddingLeft: 15,
        boxSizing: "border-box",
      }}
    >
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        style={{
          border: "none",
          outline: "none",
          background: "transparent",
          fontFamily: "SF Compact, sans-serif",
          fontSize: 16,
          fontWeight: 400,
          color: "#4D463C",
          width: "100%",
          height: 40,
          lineHeight: "40px",
          padding: 0,
          boxSizing: "border-box",
        }}
      />
    </div>
  );
};

export default SearchField;
