namespace MrDHelper.AppData.Attributes;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ExcludeFromCancellationTokenCheck : Attribute
{
}
