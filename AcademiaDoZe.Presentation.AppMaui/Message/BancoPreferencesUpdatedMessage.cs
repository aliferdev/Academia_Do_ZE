using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDoZe.Presentation.AppMaui.Message;

public sealed class BancoPreferencesUpdatedMessage(string value)
    : ValueChangedMessage<string>(value)
{
}