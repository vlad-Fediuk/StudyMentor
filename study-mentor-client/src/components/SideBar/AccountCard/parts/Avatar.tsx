interface AvatarProps {
  letter: string;
}

const Avatar = ({ letter }: AvatarProps) => {
  return (
    <div
      style={{
        width: 35,
        height: 35,
        borderRadius: '50%',
        backgroundColor: '#EEE7DE',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        color: '#B55252',
        fontSize: 20,
        fontWeight: 500,
        flexShrink: 0,
      }}
    >
      {letter}
    </div>
  );
};

export default Avatar;
