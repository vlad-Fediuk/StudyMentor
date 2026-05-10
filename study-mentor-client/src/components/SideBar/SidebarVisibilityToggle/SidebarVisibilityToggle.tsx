import arrowToLeft from "../../../assets/ArrowToLeft.svg";
import arrowToRight from "../../../assets/ArrowToRight.svg";
import "./SidebarVisibilityToggle.css";

interface SidebarVisibilityToggleProps {
  isOpen: boolean;
  onClick?: () => void;
}

const SidebarVisibilityToggle = ({
  isOpen,
  onClick,
}: SidebarVisibilityToggleProps) => {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-label={isOpen ? "Sidebar opened" : "Sidebar closed"}
      className={`sidebar-visibility-toggle ${isOpen ? "sidebar-visibility-toggle--open" : "sidebar-visibility-toggle--collapsed"}`}
    >
      <img
        src={isOpen ? arrowToLeft : arrowToRight}
        alt=""
        aria-hidden="true"
        className="sidebar-visibility-toggle__icon"
      />
    </button>
  );
};

export default SidebarVisibilityToggle;
