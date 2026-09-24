public sealed class AirConditionerStateConflictException()
    : DomainException("Stored air conditioner state is invalid; partial updates are not allowed.");
