using StudyMentorApi.Subjects;

namespace StudyMentorApi.Lectures;

public record LectureResponse(string Id, string Name, SubjectResponse Subject);
