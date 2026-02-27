using Studies.Domain.Enums;

namespace Studies.Domain.Errors;

public sealed record Error(string Code, string Description, ErrorType Type);