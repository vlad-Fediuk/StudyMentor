import arrowToLeft from "../../../assets/ArrowToLeft.svg";
import arrowToRight from "../../../assets/ArrowToRight.svg";

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
      style={{
        width: 38,
        height: 38,
        marginTop: "auto",
        marginBottom: 15,
        marginLeft: isOpen ? "auto" : "auto",
        marginRight: isOpen ? 15 : "auto",
        padding: 0,
        border: "none",
        background: "transparent",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        alignSelf: isOpen ? "stretch" : "center",
        cursor: "pointer",
      }}
    >
      <img
        src={isOpen ? arrowToLeft : arrowToRight}
        alt=""
        aria-hidden="true"
        style={{ width: 30, height: 30, display: "block" }}
      />
    </button>
  );
};

export default SidebarVisibilityToggle;
