namespace LarpakeServer.Models.GetDtos.Templates;

internal interface IMappable<FromType, ToType> 
    where ToType : IMappable<FromType, ToType>
{
    static abstract ToType From(FromType record);
}
