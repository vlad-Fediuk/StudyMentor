import "./Avatar.css";

interface AvatarProps {
  letter: string;
}

const Avatar = ({ letter }: AvatarProps) => {
  return <div className="avatar">{letter}</div>;
};

export default Avatar;
