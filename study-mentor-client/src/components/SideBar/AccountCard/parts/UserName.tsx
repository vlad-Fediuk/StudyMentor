interface UserNameProps {
  firstName: string;
  lastName: string;
}

const UserName = ({ firstName, lastName }: UserNameProps) => {
  return (
    <div
      style={{
        fontFamily: 'Montserrat, sans-serif',
        fontSize: 20,
        fontWeight: 500,
        color: '#EEE7DE',
      }}
    >
      <span>{firstName}</span>{' '}
      <span>{lastName}</span>
    </div>
  );
};

export default UserName;
