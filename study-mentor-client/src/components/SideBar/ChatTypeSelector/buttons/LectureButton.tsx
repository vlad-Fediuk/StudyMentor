import LectureIcon from '../icons/LectureIcon';
import IconToggleButton from './IconToggleButton';

interface LectureButtonProps {
  isActive?: boolean;
  onClick?: () => void;
}

const LectureButton = ({ isActive = false, onClick }: LectureButtonProps) => {
  return <IconToggleButton icon={<LectureIcon />} isActive={isActive} onClick={onClick} />;
};

export default LectureButton;
