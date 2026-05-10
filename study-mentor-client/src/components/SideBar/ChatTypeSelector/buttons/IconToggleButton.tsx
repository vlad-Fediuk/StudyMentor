import type { ReactNode } from 'react';
import "./IconToggleButton.css";

interface IconToggleButtonProps {
  icon: ReactNode;
  isActive?: boolean;
  onClick?: () => void;
}

const IconToggleButton = ({ icon, isActive = false, onClick }: IconToggleButtonProps) => {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-pressed={isActive}
      className={`icon-toggle-button ${isActive ? "icon-toggle-button--active" : ""}`}
    >
      <span className="icon-toggle-button__icon">{icon}</span>
    </button>
  );
};

export default IconToggleButton;
