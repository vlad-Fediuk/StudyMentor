import Avatar from './Avatar';
import UserName from './UserName';
import Badge from './Badge';

interface AccountCardProps {
  firstName: string;
  lastName: string;
  badgeValue: number;
}

const AccountCard = ({ firstName, lastName, badgeValue }: AccountCardProps) => {
  return (
    <div style={{
      width: 330,
      height: 55,
      backgroundColor: '#B55252',
      borderRadius: '0 0 15px 15px',
      display: 'flex',
      alignItems: 'center',
      padding: '0 12px',
      gap: 12,
      boxSizing: 'border-box',
    }}>
      <Avatar letter={firstName.charAt(0).toUpperCase()} />
      <UserName firstName={firstName} lastName={lastName} />
      <div style={{ marginLeft: 'auto' }}>
        <Badge value={badgeValue} />
      </div>
    </div>
  );
};

export default AccountCard;
