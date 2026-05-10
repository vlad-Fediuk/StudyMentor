import "./UserName.css";

interface UserNameProps {
  firstName: string;
  lastName: string;
}

const UserName = ({ firstName, lastName }: UserNameProps) => {
  return (
    <div className="user-name">
      <span>{firstName}</span>{' '}
      <span>{lastName}</span>
    </div>
  );
};

export default UserName;
