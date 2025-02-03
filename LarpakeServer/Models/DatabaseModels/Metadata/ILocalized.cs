namespace LarpakeServer.Models.DatabaseModels.Metadata;

internal interface ILocalized<T>
{
    List<T> TextData { get; set; }

    long Id { get; set; }

}
