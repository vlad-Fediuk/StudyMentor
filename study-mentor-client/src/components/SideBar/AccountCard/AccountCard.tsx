import Avatar from './parts/Avatar';
import UserName from './parts/UserName';
import Badge from './parts/Badge';

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
      <div
        style={{
          width: "100%",
          height: 55,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <Badge value={badgeValue} isInverted />
      </div>
    );
  }

  return (
    <div
      style={{
        width: 330,
        height: 55,
        backgroundColor: '#B55252',
        borderRadius: '0 0 15px 15px',
        display: 'flex',
        alignItems: 'center',
        padding: '0 12px',
        gap: 12,
        boxSizing: 'border-box',
      }}
    >
      <Avatar letter={firstName.charAt(0).toUpperCase()} />
      <UserName firstName={firstName} lastName={lastName} />
      <div style={{ marginLeft: 'auto' }}>
        <Badge value={badgeValue} />
      </div>
    </div>
  );
};

export default AccountCard;
