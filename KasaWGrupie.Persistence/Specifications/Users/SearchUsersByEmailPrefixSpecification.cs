using Ardalis.Specification;
using KasaWGrupie.Core.Entities;
using System.Text.RegularExpressions;

namespace KasaWGrupie.Persistence.Specifications.Users;

public partial class SearchUsersByEmailPrefixSpecification : Specification<User>
{
	[GeneratedRegex(@"[\[\]%_]")]
	private static partial Regex SpecialCharsRegex();

	private static string EscapeSpecialChars(string value) =>
        SpecialCharsRegex().Replace(value, "[$0]");

	public SearchUsersByEmailPrefixSpecification(string email)
	{
		var pattern = EscapeSpecialChars(email) + "%";
		Query.Search(u => u.Email, pattern);
	}
}
