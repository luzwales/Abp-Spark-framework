using Abp.Spark.Users;

namespace Abp.Spark.Pages.Components;

public class PageState
{
	public UserDto User { get; set; } = new();
	public string AppName { get; set; } = "Abp.Spark";
}