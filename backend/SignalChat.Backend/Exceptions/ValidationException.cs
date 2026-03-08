using FluentValidation.Results;

namespace SignalChat.Backend.Exceptions;

public class ValidationException(IReadOnlyList<ValidationFailure> failures)
    : AppException("Ошибка валидации", StatusCodes.Status400BadRequest)
{
    public IReadOnlyList<ValidationFailure> Failures { get; } = failures;
}
