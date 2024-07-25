namespace LRW.Core.Date;

public class SystemDateTimeResolver : IDateTimeResolver
{
    public DateTime Now => DateTime.Now;
}
