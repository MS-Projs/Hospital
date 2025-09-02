namespace Domain.Enums;

public enum ErrorEnum : short
{
    InternalServerError = 600,
    PhoneAlreadyExists,
    EmailAlreadyExists,
    SessionNotFound,
    InvalidCode,
    UserHasActiveSubscription,
    PlanNotFound,
    AiModelNotFound,
    ChatNotFound,
    SubscriptionNotFound,
    LimitReached,
    ModelVersionNotFound,
    UserNotFound,
    HynonymAlreadyExist,
    HynonymNotFound,
    LiteraryWordAlreadyExist,
    LiteraryWordNotFound,
    WordNotFound
}