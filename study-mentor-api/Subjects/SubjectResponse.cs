using StudyMentorApi.Majors;

namespace StudyMentorApi.Subjects;

public record SubjectResponse(Guid Id, string Name, MajorResponse Major);
