interface BadgeProps {
  value: number;
  isInverted?: boolean;
}

const Badge = ({ value, isInverted = false }: BadgeProps) => {
  return (
    <div
      style={{
        width: 50,
        height: 40,
        backgroundColor: isInverted ? '#B55252' : '#EEE7DE',
        borderRadius: 10,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        fontFamily: 'Montserrat, sans-serif',
        fontSize: 24,
        fontWeight: 500,
        color: isInverted ? '#EEE7DE' : '#B55252',
        flexShrink: 0,
      }}
    >
      {value}
    </div>
  );
};

export default Badge;
