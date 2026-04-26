interface AvatarProps {
  letter: string;
}

const Avatar = ({ letter }: AvatarProps) => {
  return (
    <div style={{
      width: 35,
      height: 35,
      borderRadius: '50%',
      backgroundColor: '#EEE7DE',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      fontFamily: 'Montserrat, sans-serif',
      fontSize: 24,
      fontWeight: 500,
      color: '#4D463C',
      flexShrink: 0,
    }}>
      {letter}
    </div>
  );
};

export default Avatar;