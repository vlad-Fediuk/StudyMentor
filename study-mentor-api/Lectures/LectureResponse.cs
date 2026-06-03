using StudyMentorApi.Subjects;

namespace StudyMentorApi.Lectures;

public record LectureResponse(Guid Id, string Name, SubjectResponse Subject);
