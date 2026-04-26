interface BadgeProps {
  value: number;
}

const Badge = ({ value }: BadgeProps) => {
  return (
    <div
      style={{
        width: 50,
        height: 40,
        backgroundColor: "#EEE7DE",
        borderRadius: 10,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        fontFamily: "Montserrat, sans-serif",
        fontSize: 24,
        fontWeight: 500,
        color: "#B55252",
        flexShrink: 0,
      }}
    >
      {value}
    </div>
  );
};

export default Badge;
