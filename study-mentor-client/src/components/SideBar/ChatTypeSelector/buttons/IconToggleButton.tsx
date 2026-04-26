import { useState, type ReactNode } from 'react';

interface IconToggleButtonProps {
  icon: ReactNode;
  isActive?: boolean;
  onClick?: () => void;
}

const IconToggleButton = ({ icon, isActive = false, onClick }: IconToggleButtonProps) => {
  const [isHovered, setIsHovered] = useState(false);

  return (
    <button
      type="button"
      onClick={onClick}
      aria-pressed={isActive}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      style={{
        width: 38,
        height: 38,
        padding: 0,
        borderRadius: 5,
        backgroundColor: isActive ? '#B55252' : '#EEE7DE',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        cursor: 'pointer',
        border: 'none',
        boxSizing: 'border-box',
        color: isActive ? '#EEE7DE' : '#4D463C',
        boxShadow: isHovered ? '0 0 0 2px #D0D0D0 inset' : 'none',
        outline: 'none',
      }}
    >
      <span style={{ width: 30, height: 30, display: 'inline-flex', color: 'currentColor' }}>
        {icon}
      </span>
    </button>
  );
};

export default IconToggleButton;
