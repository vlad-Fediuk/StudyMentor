import "./Badge.css";

interface BadgeProps {
  value: number;
  isInverted?: boolean;
}

const Badge = ({ value, isInverted = false }: BadgeProps) => {
  return <div className={`badge ${isInverted ? "badge--inverted" : ""}`}>{value}</div>;
};

export default Badge;
