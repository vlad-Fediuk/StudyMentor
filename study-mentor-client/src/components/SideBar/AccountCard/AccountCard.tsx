import Avatar from './parts/Avatar';
import UserName from './parts/UserName';
import Badge from './parts/Badge';
import "./AccountCard.css";

interface AccountCardProps {
  firstName: string;
  lastName: string;
  badgeValue: number;
  isCollapsed?: boolean;
}

const AccountCard = ({
  firstName,
  lastName,
  badgeValue,
  isCollapsed = false,
}: AccountCardProps) => {
  if (isCollapsed) {
    return (
      <div className="account-card--collapsed">
        <Badge value={badgeValue} isInverted />
      </div>
    );
  }

  return (
    <div className="account-card">
      <Avatar letter={firstName.charAt(0).toUpperCase()} />
      <UserName firstName={firstName} lastName={lastName} />
      <div className="account-card__badge">
        <Badge value={badgeValue} />
      </div>
    </div>
  );
};

export default AccountCard;
