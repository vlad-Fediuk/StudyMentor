import SubjectIcon from '../icons/SubjectIcon';
import IconToggleButton from './IconToggleButton';

interface SubjectButtonProps {
  isActive?: boolean;
  onClick?: () => void;
}

const SubjectButton = ({ isActive = false, onClick }: SubjectButtonProps) => {
  return <IconToggleButton icon={<SubjectIcon />} isActive={isActive} onClick={onClick} />;
};

export default SubjectButton;
