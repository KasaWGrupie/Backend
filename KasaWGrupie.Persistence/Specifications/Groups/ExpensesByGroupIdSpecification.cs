using Ardalis.Specification;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Groups;

public class ExpensesByGroupIdSpecification : Specification<Expense>
{
    public ExpensesByGroupIdSpecification(int groupId)
    {
        Query.Where(expense => expense.GroupId == groupId)
            .Include(expense => expense.ExpenseSplit)
            .Include(expense => expense.ExpenseSplit.SplitRecords);
    }
}