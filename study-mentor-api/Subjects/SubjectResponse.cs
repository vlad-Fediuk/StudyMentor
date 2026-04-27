using StudyMentorApi.Majors;

namespace StudyMentorApi.Subjects;

public record SubjectResponse(string Id, string Name, MajorResponse Major);
