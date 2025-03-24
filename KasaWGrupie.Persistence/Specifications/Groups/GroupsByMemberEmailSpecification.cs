using Ardalis.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using KasaWGrupie.Core.Entities;

namespace KasaWGrupie.Persistence.Specifications.Groups
{
    public class GroupsByMemberEmailSpecification : Specification<Group>
    {
        public GroupsByMemberEmailSpecification(string userEmail)
        {
            Query
                .Include(g => g.Members)
                .Include(g => g.Currency)
                .Include(g => g.Admin)
                .Where(g => g.Members.Any(m => m.Email == userEmail));
        }
    }

}
